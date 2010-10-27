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
    using ClearCanvas.Enterprise.Core;
    using ClearCanvas.ImageServer.Enterprise;

    public partial class WorkQueueTypePropertiesSelectCriteria : EntitySelectCriteria
    {
        public WorkQueueTypePropertiesSelectCriteria()
        : base("WorkQueueTypeProperties")
        {}
        public WorkQueueTypePropertiesSelectCriteria(WorkQueueTypePropertiesSelectCriteria other)
        : base(other)
        {}
        public override object Clone()
        {
            return new WorkQueueTypePropertiesSelectCriteria(this);
        }
        [EntityFieldDatabaseMappingAttribute(TableName="WorkQueueTypeProperties", ColumnName="WorkQueueTypeEnum")]
        public ISearchCondition<WorkQueueTypeEnum> WorkQueueTypeEnum
        {
            get
            {
              if (!SubCriteria.ContainsKey("WorkQueueTypeEnum"))
              {
                 SubCriteria["WorkQueueTypeEnum"] = new SearchCondition<WorkQueueTypeEnum>("WorkQueueTypeEnum");
              }
              return (ISearchCondition<WorkQueueTypeEnum>)SubCriteria["WorkQueueTypeEnum"];
            } 
        }
        [EntityFieldDatabaseMappingAttribute(TableName="WorkQueueTypeProperties", ColumnName="WorkQueuePriorityEnum")]
        public ISearchCondition<WorkQueuePriorityEnum> WorkQueuePriorityEnum
        {
            get
            {
              if (!SubCriteria.ContainsKey("WorkQueuePriorityEnum"))
              {
                 SubCriteria["WorkQueuePriorityEnum"] = new SearchCondition<WorkQueuePriorityEnum>("WorkQueuePriorityEnum");
              }
              return (ISearchCondition<WorkQueuePriorityEnum>)SubCriteria["WorkQueuePriorityEnum"];
            } 
        }
        [EntityFieldDatabaseMappingAttribute(TableName="WorkQueueTypeProperties", ColumnName="MemoryLimited")]
        public ISearchCondition<Boolean> MemoryLimited
        {
            get
            {
              if (!SubCriteria.ContainsKey("MemoryLimited"))
              {
                 SubCriteria["MemoryLimited"] = new SearchCondition<Boolean>("MemoryLimited");
              }
              return (ISearchCondition<Boolean>)SubCriteria["MemoryLimited"];
            } 
        }
        [EntityFieldDatabaseMappingAttribute(TableName="WorkQueueTypeProperties", ColumnName="AlertFailedWorkQueue")]
        public ISearchCondition<Boolean> AlertFailedWorkQueue
        {
            get
            {
              if (!SubCriteria.ContainsKey("AlertFailedWorkQueue"))
              {
                 SubCriteria["AlertFailedWorkQueue"] = new SearchCondition<Boolean>("AlertFailedWorkQueue");
              }
              return (ISearchCondition<Boolean>)SubCriteria["AlertFailedWorkQueue"];
            } 
        }
        [EntityFieldDatabaseMappingAttribute(TableName="WorkQueueTypeProperties", ColumnName="MaxFailureCount")]
        public ISearchCondition<Int32> MaxFailureCount
        {
            get
            {
              if (!SubCriteria.ContainsKey("MaxFailureCount"))
              {
                 SubCriteria["MaxFailureCount"] = new SearchCondition<Int32>("MaxFailureCount");
              }
              return (ISearchCondition<Int32>)SubCriteria["MaxFailureCount"];
            } 
        }
        [EntityFieldDatabaseMappingAttribute(TableName="WorkQueueTypeProperties", ColumnName="ProcessDelaySeconds")]
        public ISearchCondition<Int32> ProcessDelaySeconds
        {
            get
            {
              if (!SubCriteria.ContainsKey("ProcessDelaySeconds"))
              {
                 SubCriteria["ProcessDelaySeconds"] = new SearchCondition<Int32>("ProcessDelaySeconds");
              }
              return (ISearchCondition<Int32>)SubCriteria["ProcessDelaySeconds"];
            } 
        }
        [EntityFieldDatabaseMappingAttribute(TableName="WorkQueueTypeProperties", ColumnName="FailureDelaySeconds")]
        public ISearchCondition<Int32> FailureDelaySeconds
        {
            get
            {
              if (!SubCriteria.ContainsKey("FailureDelaySeconds"))
              {
                 SubCriteria["FailureDelaySeconds"] = new SearchCondition<Int32>("FailureDelaySeconds");
              }
              return (ISearchCondition<Int32>)SubCriteria["FailureDelaySeconds"];
            } 
        }
        [EntityFieldDatabaseMappingAttribute(TableName="WorkQueueTypeProperties", ColumnName="DeleteDelaySeconds")]
        public ISearchCondition<Int32> DeleteDelaySeconds
        {
            get
            {
              if (!SubCriteria.ContainsKey("DeleteDelaySeconds"))
              {
                 SubCriteria["DeleteDelaySeconds"] = new SearchCondition<Int32>("DeleteDelaySeconds");
              }
              return (ISearchCondition<Int32>)SubCriteria["DeleteDelaySeconds"];
            } 
        }
        [EntityFieldDatabaseMappingAttribute(TableName="WorkQueueTypeProperties", ColumnName="PostponeDelaySeconds")]
        public ISearchCondition<Int32> PostponeDelaySeconds
        {
            get
            {
              if (!SubCriteria.ContainsKey("PostponeDelaySeconds"))
              {
                 SubCriteria["PostponeDelaySeconds"] = new SearchCondition<Int32>("PostponeDelaySeconds");
              }
              return (ISearchCondition<Int32>)SubCriteria["PostponeDelaySeconds"];
            } 
        }
        [EntityFieldDatabaseMappingAttribute(TableName="WorkQueueTypeProperties", ColumnName="ExpireDelaySeconds")]
        public ISearchCondition<Int32> ExpireDelaySeconds
        {
            get
            {
              if (!SubCriteria.ContainsKey("ExpireDelaySeconds"))
              {
                 SubCriteria["ExpireDelaySeconds"] = new SearchCondition<Int32>("ExpireDelaySeconds");
              }
              return (ISearchCondition<Int32>)SubCriteria["ExpireDelaySeconds"];
            } 
        }
        [EntityFieldDatabaseMappingAttribute(TableName="WorkQueueTypeProperties", ColumnName="MaxBatchSize")]
        public ISearchCondition<Int32> MaxBatchSize
        {
            get
            {
              if (!SubCriteria.ContainsKey("MaxBatchSize"))
              {
                 SubCriteria["MaxBatchSize"] = new SearchCondition<Int32>("MaxBatchSize");
              }
              return (ISearchCondition<Int32>)SubCriteria["MaxBatchSize"];
            } 
        }
        [EntityFieldDatabaseMappingAttribute(TableName="WorkQueueTypeProperties", ColumnName="ReadLock")]
        public ISearchCondition<Boolean> ReadLock
        {
            get
            {
              if (!SubCriteria.ContainsKey("ReadLock"))
              {
                 SubCriteria["ReadLock"] = new SearchCondition<Boolean>("ReadLock");
              }
              return (ISearchCondition<Boolean>)SubCriteria["ReadLock"];
            } 
        }
        [EntityFieldDatabaseMappingAttribute(TableName="WorkQueueTypeProperties", ColumnName="WriteLock")]
        public ISearchCondition<Boolean> WriteLock
        {
            get
            {
              if (!SubCriteria.ContainsKey("WriteLock"))
              {
                 SubCriteria["WriteLock"] = new SearchCondition<Boolean>("WriteLock");
              }
              return (ISearchCondition<Boolean>)SubCriteria["WriteLock"];
            } 
        }
        [EntityFieldDatabaseMappingAttribute(TableName="WorkQueueTypeProperties", ColumnName="QueueStudyStateEnum")]
        public ISearchCondition<QueueStudyStateEnum> QueueStudyStateEnum
        {
            get
            {
              if (!SubCriteria.ContainsKey("QueueStudyStateEnum"))
              {
                 SubCriteria["QueueStudyStateEnum"] = new SearchCondition<QueueStudyStateEnum>("QueueStudyStateEnum");
              }
              return (ISearchCondition<QueueStudyStateEnum>)SubCriteria["QueueStudyStateEnum"];
            } 
        }
        [EntityFieldDatabaseMappingAttribute(TableName="WorkQueueTypeProperties", ColumnName="QueueStudyStateOrder")]
        public ISearchCondition<Int16> QueueStudyStateOrder
        {
            get
            {
              if (!SubCriteria.ContainsKey("QueueStudyStateOrder"))
              {
                 SubCriteria["QueueStudyStateOrder"] = new SearchCondition<Int16>("QueueStudyStateOrder");
              }
              return (ISearchCondition<Int16>)SubCriteria["QueueStudyStateOrder"];
            } 
        }
    }
}
