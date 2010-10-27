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
    using ClearCanvas.ImageServer.Enterprise;

   public class StudyIntegrityQueueUpdateColumns : EntityUpdateColumns
   {
       public StudyIntegrityQueueUpdateColumns()
       : base("StudyIntegrityQueue")
       {}
        [EntityFieldDatabaseMappingAttribute(TableName="StudyIntegrityQueue", ColumnName="ServerPartitionGUID")]
        public ServerEntityKey ServerPartitionKey
        {
            set { SubParameters["ServerPartitionKey"] = new EntityUpdateColumn<ServerEntityKey>("ServerPartitionKey", value); }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="StudyIntegrityQueue", ColumnName="StudyStorageGUID")]
        public ServerEntityKey StudyStorageKey
        {
            set { SubParameters["StudyStorageKey"] = new EntityUpdateColumn<ServerEntityKey>("StudyStorageKey", value); }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="StudyIntegrityQueue", ColumnName="InsertTime")]
        public DateTime InsertTime
        {
            set { SubParameters["InsertTime"] = new EntityUpdateColumn<DateTime>("InsertTime", value); }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="StudyIntegrityQueue", ColumnName="StudyData")]
        public XmlDocument StudyData
        {
            set { SubParameters["StudyData"] = new EntityUpdateColumn<XmlDocument>("StudyData", value); }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="StudyIntegrityQueue", ColumnName="StudyIntegrityReasonEnum")]
        public StudyIntegrityReasonEnum StudyIntegrityReasonEnum
        {
            set { SubParameters["StudyIntegrityReasonEnum"] = new EntityUpdateColumn<StudyIntegrityReasonEnum>("StudyIntegrityReasonEnum", value); }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="StudyIntegrityQueue", ColumnName="GroupID")]
        public String GroupID
        {
            set { SubParameters["GroupID"] = new EntityUpdateColumn<String>("GroupID", value); }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="StudyIntegrityQueue", ColumnName="Details")]
        public XmlDocument Details
        {
            set { SubParameters["Details"] = new EntityUpdateColumn<XmlDocument>("Details", value); }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="StudyIntegrityQueue", ColumnName="Description")]
        public String Description
        {
            set { SubParameters["Description"] = new EntityUpdateColumn<String>("Description", value); }
        }
    }
}
