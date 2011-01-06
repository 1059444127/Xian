#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System.Collections.Generic;
using System.Runtime.Serialization;
using ClearCanvas.Enterprise.Common;

namespace ClearCanvas.Ris.Application.Common.BrowsePatientData
{
    [DataContract]
    public class GetOrderDetailResponse : DataContractBase
    {
        public GetOrderDetailResponse(OrderDetail orderDetail)
        {
            this.Order = orderDetail;
        }

        public GetOrderDetailResponse()
        {
        }

        [DataMember]
        public OrderDetail Order;

        [DataMember]
        public List<AlertNotificationDetail> OrderAlerts;
    }
}
