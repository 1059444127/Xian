﻿#region License

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

namespace ClearCanvas.Ris.Application.Common.RegistrationWorkflow
{
    [DataContract]
    public class LoadSearchPatientFormDataResponse : DataContractBase
    {
        public LoadSearchPatientFormDataResponse(List<EnumValueInfo> sexChoices)
        {
            this.SexChoices = sexChoices;
        }

        [DataMember]
        public List<EnumValueInfo> SexChoices;
    }
}
