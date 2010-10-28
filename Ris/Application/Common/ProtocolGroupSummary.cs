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

namespace ClearCanvas.Ris.Application.Common
{
    [DataContract]
    public class ProtocolGroupSummary : DataContractBase
    {
        public ProtocolGroupSummary(EntityRef entityRef, string name, string description)
        {
			ProtocolGroupRef = entityRef;
            Name = name;
            Description = description;
        }

        [DataMember]
        public EntityRef ProtocolGroupRef;

        [DataMember]
        public string Name;

        [DataMember]
        public string Description;
    }
}
