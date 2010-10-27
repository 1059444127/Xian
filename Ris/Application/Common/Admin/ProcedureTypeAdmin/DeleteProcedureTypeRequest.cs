﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using ClearCanvas.Enterprise.Common;
using System.Runtime.Serialization;

namespace ClearCanvas.Ris.Application.Common.Admin.ProcedureTypeAdmin
{
	[DataContract]
	public class DeleteProcedureTypeRequest : DataContractBase
	{
		public DeleteProcedureTypeRequest(EntityRef preocedureTypeRef)
		{
			this.ProcedureTypeRef = preocedureTypeRef;
		}

		[DataMember]
		public EntityRef ProcedureTypeRef;
	}
}
