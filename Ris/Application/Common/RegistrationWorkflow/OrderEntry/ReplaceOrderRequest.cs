﻿#region License

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
    public class ReplaceOrderRequest : DataContractBase
    {
        public ReplaceOrderRequest(EntityRef orderRef, EnumValueInfo cancelReason, OrderRequisition newRequisition)
        {
            this.OrderRef = orderRef;
            this.CancelReason = cancelReason;
            this.Requisition = newRequisition;
        }

        /// <summary>
        /// Order to replace.
        /// </summary>
        [DataMember]
        public EntityRef OrderRef;

        /// <summary>
        /// Reason the order is being cancelled.
        /// </summary>
        [DataMember]
        public EnumValueInfo CancelReason;

        /// <summary>
        /// Requisition for replacement order.
        /// </summary>
        [DataMember]
        public OrderRequisition Requisition;
    }
}
