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
	public class MergeOrderResponse : DataContractBase
	{
		/// <summary>
		/// If a dry-run was requested and succeeded, specifies what the merged order would look like.
		/// </summary>
		[DataMember]
		public OrderDetail DryRunMergedOrder;

		/// <summary>
		/// If a dry-run was requested and failed, specifies the failure reason.
		/// </summary>
		[DataMember]
		public string DryRunFailureReason;
	}
}
