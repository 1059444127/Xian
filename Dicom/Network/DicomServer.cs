namespace ClearCanvas.Dicom.Network
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using ClearCanvas.Dicom.OffisWrapper;
    using System.Runtime.InteropServices;    
    using MySR = ClearCanvas.Dicom.SR;
    using ClearCanvas.Common.Utilities;

    /// <summary>
    /// This class's implementation is not yet complete. Use at your own risk.
    /// </summary>
    public partial class DicomServer
    {
        /// <summary>
        /// Fires when a C-FIND query has been received
        /// </summary>
        private event EventHandler<FindScpEventArgs> _findScpEvent;
        /// <summary>
        /// Fires when a new SOP Instance has arrived and is starting to be written to the local filesystem.
        /// </summary>
        private event EventHandler<StoreScpProgressUpdateEventArgs> _storeScpBeginEvent;
        /// <summary>
        /// Fires when more data of the new SOP Instance has arrived
        /// </summary>
        private event EventHandler<StoreScpProgressUpdateEventArgs> _storeScpProgressingEvent;
        /// <summary>
        /// Fires when a new SOP Instance has been successfully written to the local filesystem.
        /// </summary>
        private event EventHandler<StoreScpImageReceivedEventArgs> _storeScpEndEvent;

        public event EventHandler<FindScpEventArgs> FindScpEvent
        {
            add { _findScpEvent += value; }
            remove { _findScpEvent -= value; }
        }
        public event EventHandler<StoreScpProgressUpdateEventArgs> StoreScpBeginEvent
        {
            add { _storeScpBeginEvent += value; }
            remove { _storeScpBeginEvent -= value; }
        }
        public event EventHandler<StoreScpProgressUpdateEventArgs> StoreScpProgressingEvent
        {
            add { _storeScpProgressingEvent += value; }
            remove { _storeScpProgressingEvent -= value; }
        }
        public event EventHandler<StoreScpImageReceivedEventArgs> StoreScpEndEvent
        {
            add { _storeScpEndEvent += value; }
            remove { _storeScpEndEvent -= value; }
        }

        protected void OnFindScpEvent(FindScpEventArgs e)
        {
            EventsHelper.Fire(_findScpEvent, this, e);
        }
        protected void OnStoreScpBeginEvent(StoreScpProgressUpdateEventArgs e)
        {
            EventsHelper.Fire(_storeScpBeginEvent, this, e);
        }
        protected void OnStoreScpProgressingEvent(StoreScpProgressUpdateEventArgs e)
        {
            EventsHelper.Fire(_storeScpProgressingEvent, this, e);
        }
        protected void OnStoreScpEndEvent(StoreScpImageReceivedEventArgs e)
        {
            EventsHelper.Fire(_storeScpEndEvent, this, e);
        }

        /// <summary>
        /// Mandatory constructor.
        /// </summary>
        /// <param name="ownAEParameters">The AE parameters of the DICOM server. That is,
        /// the user's AE parameters that will be passed to the server as the source
        /// of the DICOM commands, and will also become the destination for the receiving
        /// of DICOM data.</param>
        public DicomServer(ApplicationEntity ownAEParameters, String saveDirectory)
        {
            SocketManager.InitializeSockets();
            _myOwnAE = ownAEParameters;
            _state = ServerState.STOPPED;
            _stopRequestedFlag = false;
            _saveDirectory = DicomHelper.NormalizeDirectory(saveDirectory);

            _findScpCallbackHelper = new FindScpCallbackHelper(this);
            _storeScpCallbackHelper = new StoreScpCallbackHelper(this);
        }


        public void Start()
        {
            // TODO
            if (ServerState.INITIALIZING == State)
                throw new System.Exception("Server is already in the process of initialization: cannot start again.");

            if (ServerState.STARTED == State)
                throw new System.Exception("Server is already started: cannot start again");

            Thread t = new Thread(new ThreadStart(WaitForIncomingAssociations));
            t.IsBackground = true;
            t.Start();
        }

        public String SaveDirectory
        {
            get { return _saveDirectory; }
        }

        protected void WaitForIncomingAssociations()
        {
            State = ServerState.INITIALIZING;

            try
            {
                T_ASC_Network network = new T_ASC_Network(T_ASC_NetworkRole.NET_ACCEPTOR, _myOwnAE.Port, _timeout);

                State = ServerState.STARTED;

                do
                {
                    T_ASC_Association association =  network.AcceptAssociation(_myOwnAE.AE, _operationTimeout, _threadList.NumberOfAliveThreads, MaximumAssociations);

                    if (null != association)
                    {

                        // spawn a thread to process the association
                        AssociationProcessor processor = new AssociationProcessor(this, association, _threadList);
                        Thread t = new Thread(new ThreadStart(processor.Process));
                        t.IsBackground = true;
                        // put the thread into a data structure that keeps track of all the threads
                        _threadList.Add(t);

                        t.Start();
                    }

                    // association returned was null, this is the semantic meaning 
                    // that the timeout has expired, and we should check to see whether
                    // a stop request has occurred.
                    if (StopRequestedFlag)
                    {
                        // wait for all the child processing threads to complete
                        foreach (Thread t in _threadList)
                        {
                            if (t.IsAlive)
                                t.Join();
                        }
                        break;
                    }
                } while (true);

                State = ServerState.STOPPED;

            }
            catch (DicomRuntimeApplicationException e)
            {
                State = ServerState.STOPPED;
                throw new NetworkDicomException(OffisConditionParser.GetTextString(_myOwnAE, e), e);
            }
        }

        public void Stop()
        {
            // TODO
            if (ServerState.STARTED != State)
                throw new System.Exception("Cannot stop server while it has not yet been started.");

            StopRequestedFlag = true;
        }

        ~DicomServer()
        {
            SocketManager.DeinitializeSockets();
        }


        /// <summary>
        /// Set the overall connection timeout period in the underlying OFFIS DICOM
        /// library.
        /// </summary>
        /// <param name="timeout">Timeout period in seconds.</param>
        public static void SetGlobalConnectionTimeout(ushort timeout)
        {
            if (timeout < 1)
                throw new System.ArgumentOutOfRangeException("timeout", MySR.ExceptionDicomConnectionTimeoutOutOfRange);
            OffisDcm.SetGlobalConnectionTimeout(timeout);
        }

        public short MaximumAssociations
        {
            get { return 25; }
        }

        protected enum ServerState
        {
            STOPPED,
            INITIALIZING,
            STARTED
        }

        protected class AssociationProcessor
        {
            public AssociationProcessor(DicomServer parent, T_ASC_Association association, ThreadList threadList)
            {
                _parent = parent;
                _association = association;
                _threadList = threadList;
            }

            public void Process()
            {
                OFCondition cond = _association.ProcessServerCommands(-1, _parent.SaveDirectory);
                if (DicomHelper.CompareConditions(cond, OffisDcm.DUL_PEERREQUESTEDRELEASE))
                {
                    _association.RespondToReleaseRequest();
                }
                else
                {
                    try
                    {
                        bool releaseSuccessful = _association.Release();
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
            }

            private T_ASC_Association _association;
            private ThreadList _threadList;
            private DicomServer _parent;
        }

        protected class ThreadList : IList<Thread>
        {
            public short NumberOfAliveThreads
            {
                get
                {
                    short count = 0;
                    foreach (Thread t in _threadList)
                    {
                        if (t.IsAlive)
                            count++;
                    }

                    return count;
                }
            }

            #region IList<Thread> Members

            public int IndexOf(Thread item)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public void Insert(int index, Thread item)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public void RemoveAt(int index)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public Thread this[int index]
            {
                get
                {
                    throw new Exception("The method or operation is not implemented.");
                }
                set
                {
                    throw new Exception("The method or operation is not implemented.");
                }
            }

            #endregion

            #region ICollection<Thread> Members

            public void Add(Thread item)
            {
                lock (_monitorLock)
                {
                    _threadList.Add(item);
                }
            }

            public void Clear()
            {
                lock (_monitorLock)
                {
                    _threadList.Clear();
                }
            }

            public bool Contains(Thread item)
            {
                lock (_monitorLock)
                {
                    return _threadList.Contains(item);
                }
            }

            public void CopyTo(Thread[] array, int arrayIndex)
            {
                lock (_monitorLock)
                {
                    _threadList.CopyTo(array, arrayIndex);
                }
            }

            public int Count
            {
                get
                {
                    lock (_monitorLock)
                    {
                        return _threadList.Count;
                    }
                }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(Thread item)
            {
                lock (_monitorLock)
                {
                    return _threadList.Remove(item);
                }
            }

            #endregion

            #region IEnumerable<Thread> Members

            public IEnumerator<Thread> GetEnumerator()
            {
                return (_threadList as IEnumerable<Thread>).GetEnumerator();
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return _threadList.GetEnumerator();
            }

            #endregion

            private object _monitorLock = new object();
            private List<Thread> _threadList = new List<Thread>();
        }

        public bool IsRunning
        {
            get { return _state == ServerState.STARTED; }
        }

        #region Private members
        private ServerState State
        {
            get
            {
                lock (_monitorLock)
                {
                    return _state;
                }
            }

            set
            {
                lock (_monitorLock)
                {
                    _state = value;
                }
            }
        }
        private bool StopRequestedFlag
        {
            get
            {
                lock (_monitorLock)
                {
                    return _stopRequestedFlag;
                }
            }

            set
            {
                lock (_monitorLock)
                {
                    _stopRequestedFlag = value;
                }
            }
        }

        private ApplicationEntity _myOwnAE;
        private int _timeout = 500;
        private int _operationTimeout = 5;
        private ServerState _state;
        private bool _stopRequestedFlag;
        private object _monitorLock = new object();
        private ThreadList _threadList = new ThreadList();
        private String _saveDirectory;
        private FindScpCallbackHelper _findScpCallbackHelper;
        private StoreScpCallbackHelper _storeScpCallbackHelper;

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            SocketManager.DeinitializeSockets();
            _findScpCallbackHelper.Dispose();
            _storeScpCallbackHelper.Dispose();
        }

        #endregion

    }
}
