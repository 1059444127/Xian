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
using System.Text;
using ClearCanvas.Enterprise.Common;
using System.Runtime.Serialization;

namespace ClearCanvas.Ris.Application.Common.Admin.EnumerationAdmin
{
    [DataContract]
    public class EditValueRequest : DataContractBase
    {
        public EditValueRequest()
        {

        }

		public EditValueRequest(string enumerationName, EnumValueAdminInfo value, EnumValueAdminInfo insertAfter)
        {
            this.AssemblyQualifiedClassName = enumerationName;
            this.Value = value;
            this.InsertAfter = insertAfter;
		}

        [DataMember]
        public string AssemblyQualifiedClassName;

        [DataMember]
		public EnumValueAdminInfo Value;

        [DataMember]
		public EnumValueAdminInfo InsertAfter;
	}
}
