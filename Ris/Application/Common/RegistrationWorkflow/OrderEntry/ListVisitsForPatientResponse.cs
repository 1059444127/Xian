#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using ClearCanvas.Enterprise.Common;
using ClearCanvas.Ris.Application.Common.Admin.VisitAdmin;

namespace ClearCanvas.Ris.Application.Common.RegistrationWorkflow.OrderEntry
{
    [DataContract]
    public class ListVisitsForPatientResponse : DataContractBase
    {
        public ListVisitsForPatientResponse(List<VisitSummary> visits)
        {
            this.Visits = visits;
        }

        [DataMember]
        public List<VisitSummary> Visits;
    }
}
