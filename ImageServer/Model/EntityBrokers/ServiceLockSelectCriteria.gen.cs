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

    public partial class ServiceLockSelectCriteria : EntitySelectCriteria
    {
        public ServiceLockSelectCriteria()
        : base("ServiceLock")
        {}
        public ServiceLockSelectCriteria(ServiceLockSelectCriteria other)
        : base(other)
        {}
        public override object Clone()
        {
            return new ServiceLockSelectCriteria(this);
        }
        [EntityFieldDatabaseMappingAttribute(TableName="ServiceLock", ColumnName="ServiceLockTypeEnum")]
        public ISearchCondition<ServiceLockTypeEnum> ServiceLockTypeEnum
        {
            get
            {
              if (!SubCriteria.ContainsKey("ServiceLockTypeEnum"))
              {
                 SubCriteria["ServiceLockTypeEnum"] = new SearchCondition<ServiceLockTypeEnum>("ServiceLockTypeEnum");
              }
              return (ISearchCondition<ServiceLockTypeEnum>)SubCriteria["ServiceLockTypeEnum"];
            } 
        }
        [EntityFieldDatabaseMappingAttribute(TableName="ServiceLock", ColumnName="Lock")]
        public ISearchCondition<Boolean> Lock
        {
            get
            {
              if (!SubCriteria.ContainsKey("Lock"))
              {
                 SubCriteria["Lock"] = new SearchCondition<Boolean>("Lock");
              }
              return (ISearchCondition<Boolean>)SubCriteria["Lock"];
            } 
        }
        [EntityFieldDatabaseMappingAttribute(TableName="ServiceLock", ColumnName="ScheduledTime")]
        public ISearchCondition<DateTime> ScheduledTime
        {
            get
            {
              if (!SubCriteria.ContainsKey("ScheduledTime"))
              {
                 SubCriteria["ScheduledTime"] = new SearchCondition<DateTime>("ScheduledTime");
              }
              return (ISearchCondition<DateTime>)SubCriteria["ScheduledTime"];
            } 
        }
        [EntityFieldDatabaseMappingAttribute(TableName="ServiceLock", ColumnName="Enabled")]
        public ISearchCondition<Boolean> Enabled
        {
            get
            {
              if (!SubCriteria.ContainsKey("Enabled"))
              {
                 SubCriteria["Enabled"] = new SearchCondition<Boolean>("Enabled");
              }
              return (ISearchCondition<Boolean>)SubCriteria["Enabled"];
            } 
        }
        [EntityFieldDatabaseMappingAttribute(TableName="ServiceLock", ColumnName="State")]
        public ISearchCondition<XmlDocument> State
        {
            get
            {
              if (!SubCriteria.ContainsKey("State"))
              {
                 SubCriteria["State"] = new SearchCondition<XmlDocument>("State");
              }
              return (ISearchCondition<XmlDocument>)SubCriteria["State"];
            } 
        }
        [EntityFieldDatabaseMappingAttribute(TableName="ServiceLock", ColumnName="FilesystemGUID")]
        public ISearchCondition<ServerEntityKey> FilesystemKey
        {
            get
            {
              if (!SubCriteria.ContainsKey("FilesystemKey"))
              {
                 SubCriteria["FilesystemKey"] = new SearchCondition<ServerEntityKey>("FilesystemKey");
              }
              return (ISearchCondition<ServerEntityKey>)SubCriteria["FilesystemKey"];
            } 
        }
        [EntityFieldDatabaseMappingAttribute(TableName="ServiceLock", ColumnName="ProcessorId")]
        public ISearchCondition<String> ProcessorId
        {
            get
            {
              if (!SubCriteria.ContainsKey("ProcessorId"))
              {
                 SubCriteria["ProcessorId"] = new SearchCondition<String>("ProcessorId");
              }
              return (ISearchCondition<String>)SubCriteria["ProcessorId"];
            } 
        }
    }
}
