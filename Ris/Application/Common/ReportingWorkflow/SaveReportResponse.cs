﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
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

namespace ClearCanvas.Ris.Application.Common.ReportingWorkflow
{
    [DataContract]
    public class SaveReportResponse : DataContractBase
    {
        public SaveReportResponse(EntityRef reportingStepRef)
        {
            this.ReportingStepRef = reportingStepRef;
        }

        [DataMember]
        public EntityRef ReportingStepRef;
    }
}
