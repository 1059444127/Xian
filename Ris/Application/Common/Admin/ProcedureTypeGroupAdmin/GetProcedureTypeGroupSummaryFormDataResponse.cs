#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System.Collections.Generic;
using ClearCanvas.Enterprise.Common;
using System.Runtime.Serialization;

namespace ClearCanvas.Ris.Application.Common.Admin.ProcedureTypeGroupAdmin
{
	[DataContract]
	public class GetProcedureTypeGroupSummaryFormDataResponse : DataContractBase
	{
		public GetProcedureTypeGroupSummaryFormDataResponse(List<EnumValueInfo> categoryChoices)
		{
			this.CategoryChoices = categoryChoices;
		}

		[DataMember]
		public List<EnumValueInfo> CategoryChoices;
	}
}
