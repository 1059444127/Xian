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

   public class StudyStorageUpdateColumns : EntityUpdateColumns
   {
       public StudyStorageUpdateColumns()
       : base("StudyStorage")
       {}
        [EntityFieldDatabaseMappingAttribute(TableName="StudyStorage", ColumnName="ServerPartitionGUID")]
        public ServerEntityKey ServerPartitionKey
        {
            set { SubParameters["ServerPartitionKey"] = new EntityUpdateColumn<ServerEntityKey>("ServerPartitionKey", value); }
        }
       [DicomField(DicomTags.StudyInstanceUid, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="StudyStorage", ColumnName="StudyInstanceUid")]
        public String StudyInstanceUid
        {
            set { SubParameters["StudyInstanceUid"] = new EntityUpdateColumn<String>("StudyInstanceUid", value); }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="StudyStorage", ColumnName="InsertTime")]
        public DateTime InsertTime
        {
            set { SubParameters["InsertTime"] = new EntityUpdateColumn<DateTime>("InsertTime", value); }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="StudyStorage", ColumnName="LastAccessedTime")]
        public DateTime LastAccessedTime
        {
            set { SubParameters["LastAccessedTime"] = new EntityUpdateColumn<DateTime>("LastAccessedTime", value); }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="StudyStorage", ColumnName="WriteLock")]
        public Boolean WriteLock
        {
            set { SubParameters["WriteLock"] = new EntityUpdateColumn<Boolean>("WriteLock", value); }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="StudyStorage", ColumnName="ReadLock")]
        public Int16 ReadLock
        {
            set { SubParameters["ReadLock"] = new EntityUpdateColumn<Int16>("ReadLock", value); }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="StudyStorage", ColumnName="StudyStatusEnum")]
        public StudyStatusEnum StudyStatusEnum
        {
            set { SubParameters["StudyStatusEnum"] = new EntityUpdateColumn<StudyStatusEnum>("StudyStatusEnum", value); }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="StudyStorage", ColumnName="QueueStudyStateEnum")]
        public QueueStudyStateEnum QueueStudyStateEnum
        {
            set { SubParameters["QueueStudyStateEnum"] = new EntityUpdateColumn<QueueStudyStateEnum>("QueueStudyStateEnum", value); }
        }
    }
}
