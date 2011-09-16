#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;

namespace ClearCanvas.ImageViewer.Web.Client.Silverlight
{
	// TODO: The purpose of having this class was to pull out application-level functions in ServerEventDispatcher so that
    // it will only responsible for communication with the server.
    // 
    // Update: Some of the functions (eg, timeout, error handling) have been moved into CC.ImageViewer.Web.Client.Silverlight.ImageViewer.
    // Either remove this class or pull the stuff out of ImageViewer.
    public class ApplicationContext : IDisposable
    {
        internal ServerEventMediator ServerEventBroker { get; private set; }

        // TODO: Review this
        // [ThreadStatic]   Commented out.. why did it have to be thread static?
        private static ApplicationContext _current;
        // TODO: Review this
        // [ThreadStatic]   Commented out.. why did it have to be thread static?
        private static object _syncLock = new object();

        public Guid ID { get; set; }

        public string ViewerVersion { get; set; }

        public  ApplicationStartupParameters Parameters { get; set; }

        static public ApplicationContext Current
        {
            get
            {
                lock (_syncLock)
                {
                    return _current;
                }
            }
            set
            {
                lock (_syncLock)
                {
                    if (_current != null)
                    {
                        _current.Dispose();
                    }

                    _current = value;
                }
            }
        }

        static public ApplicationContext Initialize()
        {
            ApplicationContext instance = new ApplicationContext();
            Current = instance;
            return instance;
        }

        private ApplicationContext()
        {
            Parameters = ApplicationStartupParameters.Current;
            
            ServerEventBroker = new ServerEventMediator(this);
            
            ServerEventBroker.Initialize(Parameters);
        }

        public void Dispose()
        {
            if (ServerEventBroker != null)
            {
                ServerEventBroker.Dispose();
                ServerEventBroker = null;
            }
        }
    }
}
