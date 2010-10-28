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
    public class ListProtocolGroupsForProcedureResponse : DataContractBase
    {
        public ListProtocolGroupsForProcedureResponse(List<ProtocolGroupSummary> protocolGroups, ProtocolGroupSummary initialProtocolGroup)
        {
            ProtocolGroups = protocolGroups;
            InitialProtocolGroup = initialProtocolGroup;
        }

        [DataMember]
        public List<ProtocolGroupSummary> ProtocolGroups;

        [DataMember]
        public ProtocolGroupSummary InitialProtocolGroup;
    }
}