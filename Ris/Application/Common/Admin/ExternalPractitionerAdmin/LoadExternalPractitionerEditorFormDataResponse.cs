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

namespace ClearCanvas.Ris.Application.Common.Admin.ExternalPractitionerAdmin
{
    [DataContract]
    public class LoadExternalPractitionerEditorFormDataResponse : DataContractBase
    {
        public LoadExternalPractitionerEditorFormDataResponse(
            List<EnumValueInfo> addressTypeChoices,
            List<EnumValueInfo> phoneTypeChoices,
            List<EnumValueInfo> resultCommunicationModeChoices)
        {
            this.AddressTypeChoices = addressTypeChoices;
            this.PhoneTypeChoices = phoneTypeChoices;
            this.ResultCommunicationModeChoices = resultCommunicationModeChoices;
        }

        [DataMember]
        public List<EnumValueInfo> AddressTypeChoices;

        [DataMember]
        public List<EnumValueInfo> PhoneTypeChoices;

        [DataMember]
        public List<EnumValueInfo> ResultCommunicationModeChoices;
    }
}
