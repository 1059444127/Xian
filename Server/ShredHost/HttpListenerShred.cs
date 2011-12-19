#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Net;
using System.Threading;
using ClearCanvas.Common;
using ClearCanvas.Common.Shreds;

namespace ClearCanvas.Server.ShredHost
{
    /// <summary>
    /// Represents the state of the asynchronous operation that occurs when a http request is processed.
    /// </summary>
    public class HttpListenerAsyncState
    {
        #region Private Members
        private HttpListener _listener;
        private ManualResetEvent _waitEvent;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the wait event associated with the operation
        /// </summary>
        public ManualResetEvent WaitEvent
        {
            get { return _waitEvent; }
        }

        /// <summary>
        /// Gets the http listener object
        /// </summary>
        public HttpListener Listener
        {
            get { return _listener; }
        }

        #endregion

        #region Constructors
        
        /// <summary>
        /// Creates an instance of <see cref="HttpListenerAsyncState"/> for the specified http listener.
        /// </summary>
        /// <param name="listener"></param>
        public HttpListenerAsyncState(HttpListener listener)
        {
            _listener = listener;
            _waitEvent = new ManualResetEvent(false);
        }

        #endregion
    }

    /// <summary>
    /// Represents a shred that listens to and handles http requests.
    /// </summary>
    public abstract class HttpListenerShred : Shred
    {
        #region Private Members
        private HttpListener _listener;
        private HttpListenerAsyncState _syncState;
        
        #endregion

        #region Constructors

        /// <summary>
        /// Creates an instance of <see cref="HttpListenerShred"/> to listern at the specified address.
        /// </summary>
        protected HttpListenerShred(int port, string uriSubPath)
        {
            Port = port;
            UriSubPath = uriSubPath;
        }

        
        #endregion


        #region Protected Properties


        /// <summary>
        /// Gets the URI where the shred is listening at for incoming http requests.
        /// </summary>
        protected String BaseUri
        {
            get { return String.Format("{0}://+:{1}{2}", 
                    Uri.UriSchemeHttp, Port, UriSubPath); }
        }


        /// <summary>
        /// Gets or sets the name of the shred.
        /// </summary>
        protected string Name { get; set; }

        protected int Port { get; set; }

        protected string UriSubPath { get; set; }


        #endregion

        #region Protected Methods

        protected void StartListening(AsyncCallback callback)
        {
            Platform.Log(LogLevel.Info, "Started listening at {0}", BaseUri);

            _listener = new HttpListener(); ;
            _listener.Prefixes.Add(BaseUri);
            _listener.Start();

            
            while (_listener.IsListening)
            {
                Platform.Log(LogLevel.Debug, "Waiting for request at {0}", BaseUri);

                _syncState = new HttpListenerAsyncState(_listener);

                _listener.BeginGetContext(callback, _syncState);

                _syncState.WaitEvent.WaitOne();

            }
        }

        #endregion


        #region Overridden Public Methods

        public override void Stop()
        {
            _listener.Stop();
            _syncState.WaitEvent.Set();

        }

        #endregion

    }
}
