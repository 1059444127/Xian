#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.ServiceModel;
using ClearCanvas.Common;

namespace ClearCanvas.ImageViewer.Common.DicomServer
{
    [ExtensionOf(typeof(ServiceProviderExtensionPoint))]
    internal class ServiceProvider : IServiceProvider
    {
        #region IServiceProvider Members

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IDicomServer))
                return new DicomServerClient();
            return null;
        }

        #endregion
    }

	internal class DicomServerClient : ClientBase<IDicomServer>, IDicomServer
	{
        public RestartListenerResult RestartListener(RestartListenerRequest request)
        {
            return base.Channel.RestartListener(request);
        }
    }
}