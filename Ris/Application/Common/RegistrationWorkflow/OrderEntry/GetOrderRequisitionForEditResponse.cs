#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System.Runtime.Serialization;
using ClearCanvas.Enterprise.Common;

namespace ClearCanvas.Ris.Application.Common.RegistrationWorkflow.OrderEntry
{
	[DataContract]
	public class GetOrderRequisitionForEditResponse : DataContractBase
	{
		public GetOrderRequisitionForEditResponse(EntityRef orderRef, OrderRequisition requisition, bool IsCompleted)
		{
			this.OrderRef = orderRef;
			this.Requisition = requisition;
			this.IsCompleted = IsCompleted;
		}

		[DataMember]
		public OrderRequisition Requisition;

		[DataMember]
		public EntityRef OrderRef;

		[DataMember]
		public bool IsCompleted;
	}
}
