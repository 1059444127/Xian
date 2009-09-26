﻿using ClearCanvas.Dicom.Iod;
using System.Runtime.Serialization;

namespace ClearCanvas.Dicom.ServiceModel
{
	[DataContract]
	public class DicomServerApplicationEntity : ApplicationEntity, IDicomServerApplicationEntity
	{
		public DicomServerApplicationEntity()
		{}

		public DicomServerApplicationEntity(string aeTitle, string hostName, int port)
			: this(aeTitle, hostName, port, "", "", "")
		{
		}

		public DicomServerApplicationEntity(string aeTitle, string hostName, int port, string name, string description, string location)
			: base(aeTitle, name, description, location)
		{
			HostName = hostName;
			Port = port;
		}

		#region IDicomServerApplicationEntity Members

		[DataMember(IsRequired = true)]
		public string HostName { get; set; }

		[DataMember(IsRequired = true)]
		public int Port { get; set; }

		#endregion
	}
}
