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

namespace ClearCanvas.Ris.Application.Common.ProtocollingWorkflow
{
	[DataContract]
	public class SubmitProtocolForApprovalRequest : UpdateProtocolRequest
	{
		public SubmitProtocolForApprovalRequest(EntityRef protocolAssignmentStepRef, ProtocolDetail protocol, List<OrderNoteDetail> orderNotes)
			: base(protocolAssignmentStepRef, protocol, orderNotes)
		{
		}

		public SubmitProtocolForApprovalRequest(EntityRef protocolAssignmentStepRef, EntityRef supervisorRef)
			: base(protocolAssignmentStepRef, supervisorRef)
		{
		}
	}
}