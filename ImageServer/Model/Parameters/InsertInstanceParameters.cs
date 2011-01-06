#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using ClearCanvas.Dicom;
using ClearCanvas.ImageServer.Enterprise;

namespace ClearCanvas.ImageServer.Model.Parameters
{
    public class InsertInstanceParameters : ProcedureParameters
    {
        public InsertInstanceParameters()
            : base("InsertInstance")
        {
        }

        public ServerEntityKey ServerPartitionKey
        {
            set { SubCriteria["ServerPartitionKey"] = new ProcedureParameter<ServerEntityKey>("ServerPartitionKey", value); }
        }

		public ServerEntityKey StudyStorageKey
		{
			set { SubCriteria["StudyStorageKey"] = new ProcedureParameter<ServerEntityKey>("StudyStorageKey", value); }
		}

        [DicomField(DicomTags.PatientId, DefaultValue = DicomFieldDefault.Null)]
        public string PatientId
        {
            set { SubCriteria["PatientId"] = new ProcedureParameter<string>("PatientId", value); }
        }

        [DicomField(DicomTags.PatientsName, DefaultValue = DicomFieldDefault.Null)]
        public string PatientsName
        {
            set { SubCriteria["PatientsName"] = new ProcedureParameter<string>("PatientsName", value); }
        }

        [DicomField(DicomTags.IssuerOfPatientId, DefaultValue = DicomFieldDefault.Null)]
        public string IssuerOfPatientId
        {
            set { SubCriteria["IssuerOfPatientId"] = new ProcedureParameter<string>("IssuerOfPatientId", value); }
        }

        [DicomField(DicomTags.StudyInstanceUid, DefaultValue = DicomFieldDefault.Null)]
        public string StudyInstanceUid
        {
            set { SubCriteria["StudyInstanceUid"] = new ProcedureParameter<string>("StudyInstanceUid", value); }
        }

        [DicomField(DicomTags.PatientsBirthDate, DefaultValue = DicomFieldDefault.Null)]
        public string PatientsBirthDate
        {
            set { SubCriteria["PatientsBirthDate"] = new ProcedureParameter<string>("PatientsBirthDate", value); }
        }

        [DicomField(DicomTags.PatientsSex, DefaultValue = DicomFieldDefault.Null)]
        public string PatientsSex
        {
            set { SubCriteria["PatientsSex"] = new ProcedureParameter<string>("PatientsSex", value); }
        }

		[DicomField(DicomTags.PatientsAge, DefaultValue = DicomFieldDefault.Null)]
		public string PatientsAge
		{
			set { SubCriteria["PatientsAge"] = new ProcedureParameter<string>("PatientsAge", value); }
		}

        [DicomField(DicomTags.StudyDate, DefaultValue = DicomFieldDefault.Null)]
        public string StudyDate
        {
            set { SubCriteria["StudyDate"] = new ProcedureParameter<string>("StudyDate", value); }
        }

        [DicomField(DicomTags.StudyTime, DefaultValue = DicomFieldDefault.Null)]
        public string StudyTime
        {
            set { SubCriteria["StudyTime"] = new ProcedureParameter<string>("StudyTime", value); }
        }

        [DicomField(DicomTags.AccessionNumber, DefaultValue = DicomFieldDefault.Null)]
        public string AccessionNumber
        {
            set { SubCriteria["AccessionNumber"] = new ProcedureParameter<string>("AccessionNumber", value); }
        }

        [DicomField(DicomTags.StudyId, DefaultValue = DicomFieldDefault.Null)]
        public string StudyId
        {
            set { SubCriteria["StudyId"] = new ProcedureParameter<string>("StudyId", value); }
        }

        [DicomField(DicomTags.StudyDescription, DefaultValue = DicomFieldDefault.Null)]
        public string StudyDescription
        {
            set { SubCriteria["StudyDescription"] = new ProcedureParameter<string>("StudyDescription", value); }
        }

        [DicomField(DicomTags.ReferringPhysiciansName, DefaultValue = DicomFieldDefault.Null)]
        public string ReferringPhysiciansName
        {
            set { SubCriteria["ReferringPhysiciansName"] = new ProcedureParameter<string>("ReferringPhysiciansName", value); }
        }

        [DicomField(DicomTags.SeriesInstanceUid, DefaultValue = DicomFieldDefault.Null)]
        public string SeriesInstanceUid
        {
            set { SubCriteria["SeriesInstanceUid"] = new ProcedureParameter<string>("SeriesInstanceUid", value); }
        }

        [DicomField(DicomTags.Modality, DefaultValue = DicomFieldDefault.Null)]
        public string Modality
        {
            set { SubCriteria["Modality"] = new ProcedureParameter<string>("Modality", value); }
        }

        [DicomField(DicomTags.SeriesNumber, DefaultValue = DicomFieldDefault.Null)]
        public string SeriesNumber
        {
            set { SubCriteria["SeriesNumber"] = new ProcedureParameter<string>("SeriesNumber", value); }
        }

        [DicomField(DicomTags.SeriesDescription, DefaultValue = DicomFieldDefault.Null)]
        public string SeriesDescription
        {
            set { SubCriteria["SeriesDescription"] = new ProcedureParameter<string>("SeriesDescription", value); }
        }

        [DicomField(DicomTags.PerformedProcedureStepStartDate, DefaultValue = DicomFieldDefault.Null)]
        public string PerformedProcedureStepStartDate
        {
            set { SubCriteria["PerformedProcedureStepStartDate"] = new ProcedureParameter<string>("PerformedProcedureStepStartDate", value); }
        }

        [DicomField(DicomTags.PerformedProcedureStepStartTime, DefaultValue = DicomFieldDefault.Null)]
        public string PerformedProcedureStepStartTime
        {
            set { SubCriteria["PerformedProcedureStepStartTime"] = new ProcedureParameter<string>("PerformedProcedureStepStartTime", value); }
        }

        [DicomField(DicomTags.SourceApplicationEntityTitle, DefaultValue = DicomFieldDefault.Null)]
        public string SourceApplicationEntityTitle
        {
            set { SubCriteria["SourceApplicationEntityTitle"] = new ProcedureParameter<string>("SourceApplicationEntityTitle", value); }
        }

        [DicomField(DicomTags.SpecificCharacterSet, DefaultValue = DicomFieldDefault.Null)]
        public string SpecificCharacterSet
        {
            set { SubCriteria["SpecificCharacterSet"] = new ProcedureParameter<string>("SpecificCharacterSet", value); }
        }
    }
}
