#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using ClearCanvas.Common;
using ClearCanvas.ImageServer.Common.Utilities;
using ClearCanvas.ImageServer.Core.Edit;
using ClearCanvas.ImageServer.Core.Reconcile;
using ClearCanvas.ImageServer.Model;

namespace ClearCanvas.ImageServer.Web.Application.Pages.Studies.StudyDetails.Code
{
    /// <summary>
    /// Provides methods to decode the information in a <see cref="StudyHistory"/> record.
    /// </summary>
    static class StudyHistoryRecordDecoder
    {
        public static ReconcileHistoryRecord ReadReconcileRecord(StudyHistory historyRecord)
        {
            Platform.CheckTrue(historyRecord.StudyHistoryTypeEnum == StudyHistoryTypeEnum.StudyReconciled,
                               "History record has invalid history record type");

            ReconcileHistoryRecord record = new ReconcileHistoryRecord();
            record.InsertTime = historyRecord.InsertTime;
            record.StudyStorageLocation = StudyStorageLocation.FindStorageLocations(StudyStorage.Load(historyRecord.StudyStorageKey))[0];
            StudyReconcileDescriptorParser parser = new StudyReconcileDescriptorParser();
            record.UpdateDescription = parser.Parse(historyRecord.ChangeDescription);
            return record;
        }

        public static WebEditStudyHistoryRecord ReadEditRecord(StudyHistory historyRecord)
        {
            Platform.CheckTrue(historyRecord.StudyHistoryTypeEnum == StudyHistoryTypeEnum.WebEdited,
                               "History record has invalid history record type");

            WebEditStudyHistoryRecord record = new WebEditStudyHistoryRecord();
            record.InsertTime = historyRecord.InsertTime;
            record.StudyStorageLocation = StudyStorageLocation.FindStorageLocations(StudyStorage.Load(historyRecord.StudyStorageKey))[0];
            record.UpdateDescription = XmlUtils.Deserialize<WebEditStudyHistoryChangeDescription>(historyRecord.ChangeDescription);
            return record;
        }
    }
    
    internal class StudyHistoryRecordBase
    {
        public DateTime InsertTime;
        public StudyStorageLocation StudyStorageLocation;
    }
}
