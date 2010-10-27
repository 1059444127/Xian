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

    public partial class DatabaseVersionSelectCriteria : EntitySelectCriteria
    {
        public DatabaseVersionSelectCriteria()
        : base("DatabaseVersion_")
        {}
        public DatabaseVersionSelectCriteria(DatabaseVersionSelectCriteria other)
        : base(other)
        {}
        public override object Clone()
        {
            return new DatabaseVersionSelectCriteria(this);
        }
        [EntityFieldDatabaseMappingAttribute(TableName="DatabaseVersion_", ColumnName="Major_")]
        public ISearchCondition<String> Major
        {
            get
            {
              if (!SubCriteria.ContainsKey("Major_"))
              {
                 SubCriteria["Major_"] = new SearchCondition<String>("Major_");
              }
              return (ISearchCondition<String>)SubCriteria["Major_"];
            } 
        }
        [EntityFieldDatabaseMappingAttribute(TableName="DatabaseVersion_", ColumnName="Minor_")]
        public ISearchCondition<String> Minor
        {
            get
            {
              if (!SubCriteria.ContainsKey("Minor_"))
              {
                 SubCriteria["Minor_"] = new SearchCondition<String>("Minor_");
              }
              return (ISearchCondition<String>)SubCriteria["Minor_"];
            } 
        }
        [EntityFieldDatabaseMappingAttribute(TableName="DatabaseVersion_", ColumnName="Build_")]
        public ISearchCondition<String> Build
        {
            get
            {
              if (!SubCriteria.ContainsKey("Build_"))
              {
                 SubCriteria["Build_"] = new SearchCondition<String>("Build_");
              }
              return (ISearchCondition<String>)SubCriteria["Build_"];
            } 
        }
        [EntityFieldDatabaseMappingAttribute(TableName="DatabaseVersion_", ColumnName="Revision_")]
        public ISearchCondition<String> Revision
        {
            get
            {
              if (!SubCriteria.ContainsKey("Revision_"))
              {
                 SubCriteria["Revision_"] = new SearchCondition<String>("Revision_");
              }
              return (ISearchCondition<String>)SubCriteria["Revision_"];
            } 
        }
    }
}
