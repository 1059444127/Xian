#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using ClearCanvas.Enterprise.Common.ServiceConfiguration.Server;

namespace ClearCanvas.Enterprise.Common
{
	/// <summary>
	/// Arguments for configuration of a service host.
	/// </summary>
	public struct ServiceHostConfigurationArgs
	{
		public ServiceHostConfigurationArgs(Type serviceContract, Uri hostUri, bool authenticated,
			int maxReceivedMessageSize, CertificateSearchDirective certificateSearchParams)
		{
			ServiceContract = serviceContract;
			HostUri = hostUri;
			Authenticated = authenticated;
			MaxReceivedMessageSize = maxReceivedMessageSize;
			CertificateSearchDirective = certificateSearchParams;
		}

		/// <summary>
		/// The parameters used for finding the certificate
		/// </summary>
		public CertificateSearchDirective CertificateSearchDirective;

		/// <summary>
		/// The service contract for which the host is created.
		/// </summary>
		public Type ServiceContract;

		/// <summary>
		/// The URI on which the service is being exposed.
		/// </summary>
		public Uri HostUri;

		/// <summary>
		/// A value indicating whether the service is authenticated, or allows anonymous access.
		/// </summary>
		public bool Authenticated;

		/// <summary>
		/// The maximum allowable size of received messages, in bytes.
		/// </summary>
		public int MaxReceivedMessageSize;
	}
}
