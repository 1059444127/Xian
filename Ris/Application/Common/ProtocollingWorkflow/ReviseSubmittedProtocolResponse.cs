#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System.Runtime.Serialization;
using ClearCanvas.Enterprise.Common;
using ClearCanvas.Ris.Application.Common.ReportingWorkflow;

namespace ClearCanvas.Ris.Application.Common.ProtocollingWorkflow
{
	[DataContract]
	public class ReviseSubmittedProtocolResponse : DataContractBase
	{
		public ReviseSubmittedProtocolResponse(ReportingWorklistItemSummary replacementStep)
		{
			this.ReplacementStep = replacementStep;
		}

		[DataMember]
		public ReportingWorklistItemSummary ReplacementStep;
	}
}