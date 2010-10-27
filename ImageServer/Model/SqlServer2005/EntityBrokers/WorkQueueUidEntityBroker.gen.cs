﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

// This file is auto-generated by the ClearCanvas.Model.SqlServer2005.CodeGenerator project.

namespace ClearCanvas.ImageServer.Model.SqlServer2005.EntityBrokers
{
    using System;
    using System.Xml;
    using ClearCanvas.Common;
    using ClearCanvas.ImageServer.Enterprise;
    using ClearCanvas.ImageServer.Model.EntityBrokers;
    using ClearCanvas.ImageServer.Enterprise.SqlServer2005;

    [ExtensionOf(typeof(BrokerExtensionPoint))]
    public class WorkQueueUidBroker : EntityBroker<WorkQueueUid, WorkQueueUidSelectCriteria, WorkQueueUidUpdateColumns>, IWorkQueueUidEntityBroker
    {
        public WorkQueueUidBroker() : base("WorkQueueUid")
        { }
    }
}
