#region License

// Copyright (c) 2011, ClearCanvas Inc.
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

namespace ClearCanvas.Ris.Application.Common.RegistrationWorkflow.OrderEntry
{
    [DataContract]
    public class ModifyOrderRequest : DataContractBase
    {
        public ModifyOrderRequest(EntityRef orderRef, OrderRequisition requisition)
        {
            this.OrderRef = orderRef;
            this.Requisition = requisition;
        }

        /// <summary>
        /// Order to modify.
        /// </summary>
        [DataMember]
        public EntityRef OrderRef;

        /// <summary>
        /// Requisition specifying details of the modified order.
        /// </summary>
        [DataMember]
        public OrderRequisition Requisition;
    }
}