using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using ClearCanvas.Dicom.DataStore;
using ClearCanvas.Dicom;
using ClearCanvas.Dicom.OffisWrapper;

namespace ClearCanvas.Utilities.RebuildDatabase
{
    public class DatabaseRebuilder
    {
        public event EventHandler<ImageInsertCompletingEventArgs> ImageInsertCompletingEvent;
        public event EventHandler<ImageInsertCompletedEventArgs> ImageInsertCompletedEvent;
        public event EventHandler<DatabaseRebuildCompletedEventArgs> DatabaseRebuildCompletedEvent;
       
        public DatabaseRebuilder(String imageStoragePath, Boolean isSearchRecursive)
        {
            _state = RebuilderState.Stopped;
            _imageStoragePath = imageStoragePath;
            _isSearchRecursive = isSearchRecursive;
            _fileList = Directory.GetFiles(_imageStoragePath,
                "*",
                _isSearchRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        public String ImageStoragePath
        {
            get { return _imageStoragePath; }
            private set { _imageStoragePath = value; }
        }

        public Boolean IsSearchRecursive
        {
            get { return _isSearchRecursive; }
            private set { _isSearchRecursive = value; }
        }

        public Int32 NumberOfFiles
        {
            get { return _fileList.Length; }
        }

        public Boolean IsRebuilding
        {
            get { return (RebuilderState.Rebuilding == State); }
        }

        private RebuilderState State
        {
            get
            {
                lock (_synchronizationLock)
                {
                    return _state;
                }
            }

            set
            {
                lock (_synchronizationLock)
                {
                    _state = value;
                }
            }
        }

        private Boolean StopRequested
        {
            get
            {
                lock (_synchronizationLock)
                {
                    return _stopRequested;
                }
            }

            set
            {
                lock (_synchronizationLock)
                {
                    _stopRequested = value;
                }
            }
        }

        public void StartRebuild()
        {
            // TODO
            if (RebuilderState.Stopped != _state)
                throw new System.InvalidOperationException("Cannot start a rebuild while the rebuilder object is not in the stopped state");

            State = RebuilderState.Rebuilding;
            Thread t = new Thread(new ThreadStart(DoRebuild));
            t.IsBackground = true;
            t.Start();
        }

        public void StopRebuild()
        {
            // TODO
            if (RebuilderState.Rebuilding != _state)
                throw new System.InvalidOperationException("Cannot stop a rebuild while the rebuilder object is not in the rebuilding state");

            StopRequested = true;
        }

        protected void OnImageInsertCompletedEvent(ImageInsertCompletedEventArgs args)
        {
            ClearCanvas.Common.EventsHelper.Fire(ImageInsertCompletedEvent, this, args);
        }

        protected void OnImageInsertCompletingEvent(ImageInsertCompletingEventArgs args)
        {
            ClearCanvas.Common.EventsHelper.Fire(ImageInsertCompletingEvent, this, args);
        }

        protected void OnDatabaseRebuildCompletedEvent(DatabaseRebuildCompletedEventArgs args)
        {
            ClearCanvas.Common.EventsHelper.Fire(DatabaseRebuildCompletedEvent, this, args);
        }

        protected void DoRebuild()
        {
            StopRequested = false;
            bool rebuildWasAborted = false;

            foreach (string file in _fileList)
            {
                OnImageInsertCompletingEvent(new ImageInsertCompletingEventArgs(file));
                
                InsertSopInstance(file);
                
                OnImageInsertCompletedEvent(new ImageInsertCompletedEventArgs(file));

                if (StopRequested)
                {
                    State = RebuilderState.Stopped;
                    rebuildWasAborted = true;
                    break;
                }

                DatabaseFlush();
            }

            FinalDatabaseFlush();

            OnDatabaseRebuildCompletedEvent(new DatabaseRebuildCompletedEventArgs(rebuildWasAborted));
        }

        private void DatabaseFlush()
        {
            if (this.DicomStore.GetCachedStudiesCount() > 10)
                this.DicomStore.Flush();
        }

        private void FinalDatabaseFlush()
        {
            this.DicomStore.Flush();
        }

        protected void InsertSopInstance(string fileName)
        {
            DcmFileFormat file = new DcmFileFormat();
            OFCondition condition = file.loadFile(fileName);
            if (!condition.good())
            {
                // there was an error reading the file, possibly it's not a DICOM file
                return;
            }

            DcmMetaInfo metaInfo = file.getMetaInfo();
            DcmDataset dataset = file.getDataset();
            this.DicomStore.InsertSopInstance(metaInfo, dataset, fileName);

            // keep the file object alive until the end of this scope block
            // otherwise, it'll be GC'd and metaInfo and dataset will be gone
            // as well, even though they are needed in the InsertSopInstance
            // and sub methods
            GC.KeepAlive(file);
        }

        private DatabaseRebuilder() { }
        
        private IDicomPersistentStore DicomStore
        {
            get { return _dicomStore; }
        }

        private String _imageStoragePath;
        private Boolean _isSearchRecursive;
        private String[] _fileList;
        private Boolean _stopRequested = false;
        private Object _synchronizationLock = new Object();
        private enum RebuilderState { Stopped, Rebuilding };
        private RebuilderState _state;
        private IDicomPersistentStore _dicomStore = new DicomImageStore();
    }
}
