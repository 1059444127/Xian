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
using System.ServiceModel.Description;
using ClearCanvas.Common;

namespace ClearCanvas.Enterprise.Common.ServiceConfiguration.Server
{
	/// <summary>
	/// Configures a TCP service host.
	/// </summary>
	public class NetTcpConfiguration : IServiceHostConfiguration
	{
		#region IServiceHostConfiguration Members

		/// <summary>
		/// Configures the specified service host, according to the specified arguments.
		/// </summary>
		/// <param name="host"></param>
		/// <param name="args"></param>
		public void ConfigureServiceHost(ServiceHost host, ServiceHostConfigurationArgs args)
		{
            NetTcpBinding binding = new NetTcpBinding();
			binding.MaxReceivedMessageSize = args.MaxReceivedMessageSize;
            binding.ReaderQuotas.MaxStringContentLength = args.MaxReceivedMessageSize;
            binding.ReaderQuotas.MaxArrayLength = args.MaxReceivedMessageSize;
			binding.Security.Mode = args.Authenticated ? SecurityMode.TransportWithMessageCredential : SecurityMode.Transport;
			binding.Security.Message.ClientCredentialType = args.Authenticated ?
				MessageCredentialType.UserName : MessageCredentialType.None;

			// turn off transport security altogether
			binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;

			// establish endpoint
			host.AddServiceEndpoint(args.ServiceContract, binding, "");

#if DEBUG
			// We need to expose the metadata in order to generate client proxy code for some service
			// used in applications that cannot reference any CC assemblies (e.g utilities for installer).
            if (host.Description.Behaviors.Find<ServiceMetadataBehavior>() == null)
            {
                
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                smb.HttpGetUrl = new Uri(string.Format("http://localhost:{0}/{1}/mex", args.HostUri.Port+1, args.ServiceContract.Name));
                Platform.Log(LogLevel.Debug, "Service Metadata endpoint: {0}", smb.HttpGetUrl);
                host.Description.Behaviors.Add(smb);
            }
            var endpoint = host.AddServiceEndpoint(typeof(IMetadataExchange), binding, args.ServiceContract.Name);
            Platform.Log(LogLevel.Debug, "MetadataExchange Endpoint for {0}: {1}", args.ServiceContract.Name, endpoint.ListenUri);
            
#endif

			// set up the certificate - required for transmitting custom credentials
            host.Credentials.ServiceCertificate.SetCertificate(
		        args.CertificateSearchDirective.StoreLocation, args.CertificateSearchDirective.StoreName,
		        args.CertificateSearchDirective.FindType, args.CertificateSearchDirective.FindValue);
		}

		#endregion
	}
}
