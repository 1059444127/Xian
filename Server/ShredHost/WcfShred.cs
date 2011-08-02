#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using ClearCanvas.Common;
using ClearCanvas.Common.Shreds;

namespace ClearCanvas.Server.ShredHost
{
    public abstract class WcfShred : Shred, IWcfShred
    {
        public WcfShred()
        {
            _serviceEndpointDescriptions = new Dictionary<string, ServiceEndpointDescription>();
        }


        public override object InitializeLifetimeService()
        {
            // I can't find any documentation yet, that says that returning null 
            // means that the lifetime of the object should not expire after a timeout
            // but the initial solution comes from this page: http://www.dotnet247.com/247reference/msgs/13/66416.aspx
            return null;
        }


        public ServiceEndpointDescription StartHttpHost<TServiceType, TServiceInterfaceType>(string name, string description)
        {
            Platform.Log(LogLevel.Info, "Starting WCF Shred {0}...", name);

            if (_serviceEndpointDescriptions.ContainsKey(name))
                throw new Exception(String.Format("The service endpoint '{0}' already exists.", name));

            ServiceEndpointDescription sed =
				WcfHelper.StartHttpHost<TServiceType, TServiceInterfaceType>(name, description, SharedHttpPort, ServiceAddressBase);
            _serviceEndpointDescriptions[name] = sed;

			Platform.Log(LogLevel.Info, "WCF Shred {0} is listening at {1}.", name, sed.ServiceHost.Description.Endpoints[0]);

            return sed;
        }

        public ServiceEndpointDescription StartBasicHttpHost<TServiceType, TServiceInterfaceType>(string name, string description)
        {
            Platform.Log(LogLevel.Info, "Starting WCF Shred {0}...", name);

            if (_serviceEndpointDescriptions.ContainsKey(name))
                throw new Exception(String.Format("The service endpoint '{0}' already exists.", name));

            ServiceEndpointDescription sed =
				WcfHelper.StartBasicHttpHost<TServiceType, TServiceInterfaceType>(name, description, SharedHttpPort, ServiceAddressBase);
            _serviceEndpointDescriptions[name] = sed;

			Platform.Log(LogLevel.Info, "WCF Shred {0} is listening at {1}.", name, sed.ServiceHost.Description.Endpoints[0]);

            return sed;
        }

        public ServiceEndpointDescription StartHttpDualHost<TServiceType, TServiceInterfaceType>(string name,
                                                                                                 string description)
        {
            Platform.Log(LogLevel.Info, "Starting WCF Shred {0}...", name);

            if (_serviceEndpointDescriptions.ContainsKey(name))
                throw new Exception(String.Format("The service endpoint '{0}' already exists.", name));

            ServiceEndpointDescription sed =
				WcfHelper.StartHttpDualHost<TServiceType, TServiceInterfaceType>(name, description, SharedHttpPort, ServiceAddressBase);
            _serviceEndpointDescriptions[name] = sed;

			Platform.Log(LogLevel.Info, "WCF Shred {0} is listening at {1}.", name, sed.ServiceHost.Description.Endpoints[0]);

            return sed;
        }

        public ServiceEndpointDescription StartNetTcpHost<TServiceType, TServiceInterfaceType>(string name,
                                                                                               string description)
        {
            Platform.Log(LogLevel.Info, "Starting WCF Shred {0}...", name);

            if (_serviceEndpointDescriptions.ContainsKey(name))
                throw new Exception(String.Format("The service endpoint '{0}' already exists.", name));

            ServiceEndpointDescription sed =
				WcfHelper.StartNetTcpHost<TServiceType, TServiceInterfaceType>(name, description, SharedTcpPort, SharedHttpPort, ServiceAddressBase);
            _serviceEndpointDescriptions[name] = sed;

			Platform.Log(LogLevel.Info, "WCF Shred {0}is listening at {1}.", name, sed.ServiceHost.Description.Endpoints[0]);


            return sed;
        }

        public ServiceEndpointDescription StartNetPipeHost<TServiceType, TServiceInterfaceType>(string name,
                                                                                                string description)
        {
            Platform.Log(LogLevel.Info, "Starting WCF Shred {0}...", name);

            if (_serviceEndpointDescriptions.ContainsKey(name))
                throw new Exception(String.Format("The service endpoint '{0}' already exists.", name));

            ServiceEndpointDescription sed =
				WcfHelper.StartNetPipeHost<TServiceType, TServiceInterfaceType>(name, description, SharedHttpPort, ServiceAddressBase);
            _serviceEndpointDescriptions[name] = sed;


            Platform.Log(LogLevel.Info, "WCF Shred {0} is listening at {1}.", name, sed.ServiceHost.Description.Endpoints[0]);

            return sed;
        }

        protected void StopHost(string name)
        {
            Platform.Log(LogLevel.Info, "Stopping WCF Shred {0}...", name);

            if (_serviceEndpointDescriptions.ContainsKey(name))
            {
                _serviceEndpointDescriptions[name].ServiceHost.Close();
                _serviceEndpointDescriptions.Remove(name);

                Platform.Log(LogLevel.Info, "WCF Shred {0} Stopped", name);
            }
            else
            {
                // TODO: throw an exception, since a name of a service endpoint that is
                // passed in here that doesn't exist should be considered a programming error
                Platform.Log(LogLevel.Debug, "Attempt to stop WCF Shred {0} failed: shred doesn't exist.", name);
            }
        }

        #region Private Members

        private Dictionary<string, ServiceEndpointDescription> _serviceEndpointDescriptions;

        #endregion

        #region IWcfShred Members

        private int _httpPort;
        private int _tcpPort;
    	private string _serviceAddressBase;

        public int SharedHttpPort
        {
            get { return _httpPort; }
            set { _httpPort = value; }
        }

        public int SharedTcpPort
        {
            get { return _tcpPort; }
            set { _tcpPort = value; }
        }

		public string ServiceAddressBase
    	{
			get { return _serviceAddressBase; }
			set { _serviceAddressBase = value; }
    	}

        #endregion
    }
}
