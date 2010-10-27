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

namespace ClearCanvas.Ris.Application.Common.Admin.WorklistAdmin
{
    [DataContract]
    public class UpdateWorklistRequest : DataContractBase
    {
        public UpdateWorklistRequest(EntityRef entityRef, WorklistAdminDetail detail)
        {
            EntityRef = entityRef;
            Detail = detail;
        }

        [DataMember]
        public EntityRef EntityRef;

        [DataMember]
        public WorklistAdminDetail Detail;
    }
}