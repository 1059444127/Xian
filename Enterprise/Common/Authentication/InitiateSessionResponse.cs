﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using ClearCanvas.Enterprise.Common;
using System.Runtime.Serialization;
using ClearCanvas.Enterprise.Common.Admin.UserAdmin;

namespace ClearCanvas.Enterprise.Common.Authentication
{
	[DataContract]
	public class InitiateSessionResponse : DataContractBase
	{
		public InitiateSessionResponse(SessionToken sessionToken, string[] authorityTokens, string displayName)
		{
			this.SessionToken = sessionToken;
			this.AuthorityTokens = authorityTokens;
		    this.DisplayName = displayName;
		}

		/// <summary>
		/// Session token that identifies newly created session.
		/// </summary>
		[DataMember]
		public SessionToken SessionToken;

		/// <summary>
		/// User authority tokens, if requested.
		/// </summary>
		[DataMember]
		public string[] AuthorityTokens;

        /// <summary>
        /// Name of the user.
        /// </summary>
	    [DataMember] 
        public string DisplayName;
	}
}
