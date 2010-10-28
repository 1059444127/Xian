#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System.Collections.Generic;
using System.Runtime.Serialization;
using ClearCanvas.Enterprise.Common;

namespace ClearCanvas.Ris.Application.Common.Admin.WorkQueueAdmin
{
	[DataContract]
	public class GetWorkQueueFormDataResponse : DataContractBase
	{
		public GetWorkQueueFormDataResponse(List<EnumValueInfo> statuses, List<string> types)
		{
			this.Statuses = statuses;
			this.Types = types;
		}

		[DataMember]
		public List<EnumValueInfo> Statuses;

		[DataMember]
		public List<string> Types;
	}
}