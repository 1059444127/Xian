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

namespace ClearCanvas.Ris.Application.Common.ProtocollingWorkflow
{
    [DataContract]
    public class GetProtocolResponse : DataContractBase
    {
        public GetProtocolResponse(EntityRef protocolRef, ProtocolDetail protocolDetail)
        {
            ProtocolRef = protocolRef;
            ProtocolDetail = protocolDetail;
        }

        [DataMember]
        public EntityRef ProtocolRef;

        [DataMember]
        public ProtocolDetail ProtocolDetail;
    }
}