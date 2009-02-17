#region License

// Copyright (c) 2006-2009, ClearCanvas Inc.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met:
//
//    * Redistributions of source code must retain the above copyright notice, 
//      this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, 
//      this list of conditions and the following disclaimer in the documentation 
//      and/or other materials provided with the distribution.
//    * Neither the name of ClearCanvas Inc. nor the names of its contributors 
//      may be used to endorse or promote products derived from this software without 
//      specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR 
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
// OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
// ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY 
// OF SUCH DAMAGE.

#endregion

// This file is auto-generated by the ClearCanvas.Model.SqlServer2005.CodeGenerator project.

namespace ClearCanvas.ImageServer.Model
{
    using System;
    using ClearCanvas.Dicom;
    using ClearCanvas.Enterprise.Core;
    using ClearCanvas.ImageServer.Enterprise;
    using ClearCanvas.ImageServer.Model.EntityBrokers;

    [Serializable]
    public partial class StudyDeleteRecord: ServerEntity
    {
        #region Constructors
        public StudyDeleteRecord():base("StudyDeleteRecord")
        {}
        public StudyDeleteRecord(
             System.String _accessionNumber_
            ,System.Xml.XmlDocument _archiveInfo_
            ,System.String _backupPath_
            ,System.String _extendedInfo_
            ,ClearCanvas.ImageServer.Enterprise.ServerEntityKey _filesystemKey_
            ,System.String _patientId_
            ,System.String _patientsName_
            ,System.String _reason_
            ,System.String _serverPartitionAE_
            ,System.String _studyDate_
            ,System.String _studyDescription_
            ,System.String _studyId_
            ,System.String _studyInstanceUid_
            ,System.String _studyTime_
            ,System.DateTime _timestamp_
            ):base("StudyDeleteRecord")
        {
            _accessionNumber = _accessionNumber_;
            _archiveInfo = _archiveInfo_;
            _backupPath = _backupPath_;
            _extendedInfo = _extendedInfo_;
            _filesystemKey = _filesystemKey_;
            _patientId = _patientId_;
            _patientsName = _patientsName_;
            _reason = _reason_;
            _serverPartitionAE = _serverPartitionAE_;
            _studyDate = _studyDate_;
            _studyDescription = _studyDescription_;
            _studyId = _studyId_;
            _studyInstanceUid = _studyInstanceUid_;
            _studyTime = _studyTime_;
            _timestamp = _timestamp_;
        }
        #endregion

        #region Private Members
        private System.String _accessionNumber;
        private System.Xml.XmlDocument _archiveInfo;
        private System.String _backupPath;
        private System.String _extendedInfo;
        private ClearCanvas.ImageServer.Enterprise.ServerEntityKey _filesystemKey;
        private System.String _patientId;
        private System.String _patientsName;
        private System.String _reason;
        private System.String _serverPartitionAE;
        private System.String _studyDate;
        private System.String _studyDescription;
        private System.String _studyId;
        private System.String _studyInstanceUid;
        private System.String _studyTime;
        private System.DateTime _timestamp;
        #endregion

        #region Public Properties
        [DicomField(DicomTags.AccessionNumber, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="StudyDeleteRecord", ColumnName="AccessionNumber")]
        public System.String AccessionNumber
        {
        get { return _accessionNumber; }
        set { _accessionNumber = value; }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="StudyDeleteRecord", ColumnName="ArchiveInfo")]
        public System.Xml.XmlDocument ArchiveInfo
        {
        get { return _archiveInfo; }
        set { _archiveInfo = value; }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="StudyDeleteRecord", ColumnName="BackupPath")]
        public System.String BackupPath
        {
        get { return _backupPath; }
        set { _backupPath = value; }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="StudyDeleteRecord", ColumnName="ExtendedInfo")]
        public System.String ExtendedInfo
        {
        get { return _extendedInfo; }
        set { _extendedInfo = value; }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="StudyDeleteRecord", ColumnName="FilesystemGUID")]
        public ClearCanvas.ImageServer.Enterprise.ServerEntityKey FilesystemKey
        {
        get { return _filesystemKey; }
        set { _filesystemKey = value; }
        }
        [DicomField(DicomTags.PatientId, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="StudyDeleteRecord", ColumnName="PatientId")]
        public System.String PatientId
        {
        get { return _patientId; }
        set { _patientId = value; }
        }
        [DicomField(DicomTags.PatientsName, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="StudyDeleteRecord", ColumnName="PatientsName")]
        public System.String PatientsName
        {
        get { return _patientsName; }
        set { _patientsName = value; }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="StudyDeleteRecord", ColumnName="Reason")]
        public System.String Reason
        {
        get { return _reason; }
        set { _reason = value; }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="StudyDeleteRecord", ColumnName="ServerPartitionAE")]
        public System.String ServerPartitionAE
        {
        get { return _serverPartitionAE; }
        set { _serverPartitionAE = value; }
        }
        [DicomField(DicomTags.StudyDate, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="StudyDeleteRecord", ColumnName="StudyDate")]
        public System.String StudyDate
        {
        get { return _studyDate; }
        set { _studyDate = value; }
        }
        [DicomField(DicomTags.StudyDescription, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="StudyDeleteRecord", ColumnName="StudyDescription")]
        public System.String StudyDescription
        {
        get { return _studyDescription; }
        set { _studyDescription = value; }
        }
        [DicomField(DicomTags.StudyId, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="StudyDeleteRecord", ColumnName="StudyId")]
        public System.String StudyId
        {
        get { return _studyId; }
        set { _studyId = value; }
        }
        [DicomField(DicomTags.StudyInstanceUid, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="StudyDeleteRecord", ColumnName="StudyInstanceUid")]
        public System.String StudyInstanceUid
        {
        get { return _studyInstanceUid; }
        set { _studyInstanceUid = value; }
        }
        [DicomField(DicomTags.StudyTime, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="StudyDeleteRecord", ColumnName="StudyTime")]
        public System.String StudyTime
        {
        get { return _studyTime; }
        set { _studyTime = value; }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="StudyDeleteRecord", ColumnName="Timestamp")]
        public System.DateTime Timestamp
        {
        get { return _timestamp; }
        set { _timestamp = value; }
        }
        #endregion

        #region Static Methods
        static public StudyDeleteRecord Load(ServerEntityKey key)
        {
            using (IReadContext read = PersistentStoreRegistry.GetDefaultStore().OpenReadContext())
            {
                return Load(read, key);
            }
        }
        static public StudyDeleteRecord Load(IReadContext read, ServerEntityKey key)
        {
            IStudyDeleteRecordEntityBroker broker = read.GetBroker<IStudyDeleteRecordEntityBroker>();
            StudyDeleteRecord theObject = broker.Load(key);
            return theObject;
        }
        static public StudyDeleteRecord Insert(StudyDeleteRecord table)
        {
            using (IUpdateContext update = PersistentStoreRegistry.GetDefaultStore().OpenUpdateContext(UpdateContextSyncMode.Flush))
            {
                return Insert(update, table);
            }
        }
        static public StudyDeleteRecord Insert(IUpdateContext update, StudyDeleteRecord table)
        {
            IStudyDeleteRecordEntityBroker broker = update.GetBroker<IStudyDeleteRecordEntityBroker>();
            StudyDeleteRecordUpdateColumns updateColumns = new StudyDeleteRecordUpdateColumns();
            updateColumns.AccessionNumber = table.AccessionNumber;
            updateColumns.ArchiveInfo = table.ArchiveInfo;
            updateColumns.BackupPath = table.BackupPath;
            updateColumns.ExtendedInfo = table.ExtendedInfo;
            updateColumns.FilesystemKey = table.FilesystemKey;
            updateColumns.PatientId = table.PatientId;
            updateColumns.PatientsName = table.PatientsName;
            updateColumns.Reason = table.Reason;
            updateColumns.ServerPartitionAE = table.ServerPartitionAE;
            updateColumns.StudyDate = table.StudyDate;
            updateColumns.StudyDescription = table.StudyDescription;
            updateColumns.StudyId = table.StudyId;
            updateColumns.StudyInstanceUid = table.StudyInstanceUid;
            updateColumns.StudyTime = table.StudyTime;
            updateColumns.Timestamp = table.Timestamp;
            StudyDeleteRecord theObject = broker.Insert(updateColumns);
            update.Commit();
            return theObject;
        }
        #endregion
    }
}
