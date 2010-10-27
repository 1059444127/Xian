﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using ClearCanvas.ImageServer.Enterprise;
using ClearCanvas.ImageServer.Model;

namespace ClearCanvas.ImageServer.Web.Application.Pages.Queues.WorkQueue.Edit
{
    /// <summary>
    /// Base class encapsulating the detailed information of a <see cref="WorkQueue"/> item in the context of a WorkQueue details page.
    /// </summary>
    public class WorkQueueDetails
    {
        #region Private members

        #endregion Private members

        #region Public Properties

        public DateTime ScheduledDateTime { get; set; }

        public DateTime ExpirationTime { get; set; }

        public DateTime InsertTime { get; set; }

        public int FailureCount { get; set; }

        public WorkQueueTypeEnum Type { get; set; }

        public WorkQueueStatusEnum Status { get; set; }

        public StudyDetails Study { get; set; }

        public string ServerDescription { get; set; }

        public int NumInstancesPending { get; set; }

        public int NumSeriesPending { get; set; }

        public ServerEntityKey Key { get; set; }

        public WorkQueuePriorityEnum Priority { get; set; }

        public string FailureDescription { get; set; }

        public string StorageLocationPath { get; set; }

        public string DuplicateStorageLocationPath { get; set; }

        #endregion Public Properties
    }
}