﻿#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using ClearCanvas.Common;
using ClearCanvas.Enterprise.Common;
using ClearCanvas.Enterprise.Common.Ping;
using System;

namespace ClearCanvas.Enterprise.Core.Ping
{
	[ExtensionOf(typeof(CoreServiceExtensionPoint))]
	[ServiceImplementsContract(typeof(IPingService))]
	public class PingService : CoreServiceLayer, IPingService
	{
		#region IPingService Members

		[ReadOperation(ChangeSetAuditable = false, PersistenceScopeOption = PersistenceScopeOption.Required)]
		[ResponseCaching("GetPingResponseCacheDirective")]
		public PingResponse Ping(PingRequest request)
		{
			Platform.Log(LogLevel.Debug, "Received ping request.");
			return new PingResponse();
		}

		#endregion

		private ResponseCachingDirective GetPingResponseCacheDirective()
		{
			var settings = new PingServiceSettings();
			return new ResponseCachingDirective(settings.ResponseCachingEnabled,
				TimeSpan.FromSeconds(settings.ResponseCachingTimeToLiveSeconds), ResponseCachingSite.Client);
		}
	}
}
