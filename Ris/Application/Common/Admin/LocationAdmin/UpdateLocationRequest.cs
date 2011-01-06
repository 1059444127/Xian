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
using System.Runtime.Serialization;
using ClearCanvas.Enterprise.Common;

namespace ClearCanvas.Ris.Application.Common.Admin.LocationAdmin
{
    [DataContract]
    public class UpdateLocationRequest : DataContractBase
    {
        public UpdateLocationRequest(LocationDetail detail)
        {
            this.LocationDetail = detail;
        }

        [DataMember]
        public LocationDetail LocationDetail;
    }
}
