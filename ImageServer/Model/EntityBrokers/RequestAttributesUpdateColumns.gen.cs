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

   public class RequestAttributesUpdateColumns : EntityUpdateColumns
   {
       public RequestAttributesUpdateColumns()
       : base("RequestAttributes")
       {}
        [EntityFieldDatabaseMappingAttribute(TableName="RequestAttributes", ColumnName="SeriesGUID")]
        public ServerEntityKey SeriesKey
        {
            set { SubParameters["SeriesKey"] = new EntityUpdateColumn<ServerEntityKey>("SeriesKey", value); }
        }
       [DicomField(DicomTags.RequestedProcedureId, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="RequestAttributes", ColumnName="RequestedProcedureId")]
        public String RequestedProcedureId
        {
            set { SubParameters["RequestedProcedureId"] = new EntityUpdateColumn<String>("RequestedProcedureId", value); }
        }
       [DicomField(DicomTags.ScheduledProcedureStepId, DefaultValue = DicomFieldDefault.Null)]
        [EntityFieldDatabaseMappingAttribute(TableName="RequestAttributes", ColumnName="ScheduledProcedureStepId")]
        public String ScheduledProcedureStepId
        {
            set { SubParameters["ScheduledProcedureStepId"] = new EntityUpdateColumn<String>("ScheduledProcedureStepId", value); }
        }
    }
}
