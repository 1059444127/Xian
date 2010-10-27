﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

// This file is auto-generated by the ClearCanvas.Model.SqlServer2005.CodeGenerator project.

namespace ClearCanvas.ImageServer.Model.EntityBrokers
{
    using System;
    using System.Xml;
    using ClearCanvas.Dicom;
    using ClearCanvas.ImageServer.Enterprise;

   public class StudyUpdateColumns : EntityUpdateColumns
   {
       public StudyUpdateColumns()
       : base("Study")
       {}
       [DicomField(DicomTags.StudyInstanceUid, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="Study", ColumnName="StudyInstanceUid")]
        public String StudyInstanceUid
        {
            set { SubParameters["StudyInstanceUid"] = new EntityUpdateColumn<String>("StudyInstanceUid", value); }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="Study", ColumnName="ServerPartitionGUID")]
        public ServerEntityKey ServerPartitionKey
        {
            set { SubParameters["ServerPartitionKey"] = new EntityUpdateColumn<ServerEntityKey>("ServerPartitionKey", value); }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="Study", ColumnName="PatientGUID")]
        public ServerEntityKey PatientKey
        {
            set { SubParameters["PatientKey"] = new EntityUpdateColumn<ServerEntityKey>("PatientKey", value); }
        }
       [DicomField(DicomTags.NumberOfStudyRelatedSeries, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="Study", ColumnName="NumberOfStudyRelatedSeries")]
        public Int32 NumberOfStudyRelatedSeries
        {
            set { SubParameters["NumberOfStudyRelatedSeries"] = new EntityUpdateColumn<Int32>("NumberOfStudyRelatedSeries", value); }
        }
       [DicomField(DicomTags.NumberOfStudyRelatedInstances, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="Study", ColumnName="NumberOfStudyRelatedInstances")]
        public Int32 NumberOfStudyRelatedInstances
        {
            set { SubParameters["NumberOfStudyRelatedInstances"] = new EntityUpdateColumn<Int32>("NumberOfStudyRelatedInstances", value); }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="Study", ColumnName="StudySizeInKB")]
        public Decimal StudySizeInKB
        {
            set { SubParameters["StudySizeInKB"] = new EntityUpdateColumn<Decimal>("StudySizeInKB", value); }
        }
       [DicomField(DicomTags.SpecificCharacterSet, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="Study", ColumnName="SpecificCharacterSet")]
        public String SpecificCharacterSet
        {
            set { SubParameters["SpecificCharacterSet"] = new EntityUpdateColumn<String>("SpecificCharacterSet", value); }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="Study", ColumnName="StudyStorageGUID")]
        public ServerEntityKey StudyStorageKey
        {
            set { SubParameters["StudyStorageKey"] = new EntityUpdateColumn<ServerEntityKey>("StudyStorageKey", value); }
        }
       [DicomField(DicomTags.PatientsName, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="Study", ColumnName="PatientsName")]
        public String PatientsName
        {
            set { SubParameters["PatientsName"] = new EntityUpdateColumn<String>("PatientsName", value); }
        }
       [DicomField(DicomTags.PatientId, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="Study", ColumnName="PatientId")]
        public String PatientId
        {
            set { SubParameters["PatientId"] = new EntityUpdateColumn<String>("PatientId", value); }
        }
       [DicomField(DicomTags.IssuerOfPatientId, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="Study", ColumnName="IssuerOfPatientId")]
        public String IssuerOfPatientId
        {
            set { SubParameters["IssuerOfPatientId"] = new EntityUpdateColumn<String>("IssuerOfPatientId", value); }
        }
       [DicomField(DicomTags.PatientsBirthDate, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="Study", ColumnName="PatientsBirthDate")]
        public String PatientsBirthDate
        {
            set { SubParameters["PatientsBirthDate"] = new EntityUpdateColumn<String>("PatientsBirthDate", value); }
        }
       [DicomField(DicomTags.PatientsAge, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="Study", ColumnName="PatientsAge")]
        public String PatientsAge
        {
            set { SubParameters["PatientsAge"] = new EntityUpdateColumn<String>("PatientsAge", value); }
        }
       [DicomField(DicomTags.PatientsSex, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="Study", ColumnName="PatientsSex")]
        public String PatientsSex
        {
            set { SubParameters["PatientsSex"] = new EntityUpdateColumn<String>("PatientsSex", value); }
        }
       [DicomField(DicomTags.StudyDate, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="Study", ColumnName="StudyDate")]
        public String StudyDate
        {
            set { SubParameters["StudyDate"] = new EntityUpdateColumn<String>("StudyDate", value); }
        }
       [DicomField(DicomTags.StudyTime, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="Study", ColumnName="StudyTime")]
        public String StudyTime
        {
            set { SubParameters["StudyTime"] = new EntityUpdateColumn<String>("StudyTime", value); }
        }
       [DicomField(DicomTags.AccessionNumber, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="Study", ColumnName="AccessionNumber")]
        public String AccessionNumber
        {
            set { SubParameters["AccessionNumber"] = new EntityUpdateColumn<String>("AccessionNumber", value); }
        }
       [DicomField(DicomTags.StudyId, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="Study", ColumnName="StudyId")]
        public String StudyId
        {
            set { SubParameters["StudyId"] = new EntityUpdateColumn<String>("StudyId", value); }
        }
       [DicomField(DicomTags.StudyDescription, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="Study", ColumnName="StudyDescription")]
        public String StudyDescription
        {
            set { SubParameters["StudyDescription"] = new EntityUpdateColumn<String>("StudyDescription", value); }
        }
       [DicomField(DicomTags.ReferringPhysiciansName, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="Study", ColumnName="ReferringPhysiciansName")]
        public String ReferringPhysiciansName
        {
            set { SubParameters["ReferringPhysiciansName"] = new EntityUpdateColumn<String>("ReferringPhysiciansName", value); }
        }
    }
}
