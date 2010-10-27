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

namespace ClearCanvas.Ris.Application.Common
{
	[DataContract]
	public class ModalityDetail : DataContractBase
	{
		public ModalityDetail(EntityRef modalityRef, string id, string name, EnumValueInfo dicomModality, bool deactivated)
		{
			this.ModalityRef = modalityRef;
			this.Id = id;
			this.Name = name;
			this.DicomModality = dicomModality;
			this.Deactivated = deactivated;
		}

		public ModalityDetail()
		{
		}

		[DataMember]
		public EntityRef ModalityRef;

		[DataMember]
		public string Id;

		[DataMember]
		public string Name;

		[DataMember]
		public EnumValueInfo DicomModality;

		[DataMember]
		public bool Deactivated;
	}
}
