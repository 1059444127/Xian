#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using ClearCanvas.Common;
using ClearCanvas.Enterprise.Core;
using ClearCanvas.ImageServer.Common.Exceptions;
using ClearCanvas.ImageServer.Common.Utilities;
using ClearCanvas.ImageServer.Enterprise;
using ClearCanvas.ImageServer.Model;
using ClearCanvas.ImageServer.Model.Brokers;
using ClearCanvas.ImageServer.Model.Parameters;

namespace ClearCanvas.ImageServer.Core.Edit
{
    /// <summary>
    /// Helper class to perform update on a study
    /// </summary>
    public static class StudyEditorHelper
    {
        /// <summary>
        /// Inserts delete request(s) to delete a series in a study.
        /// </summary>
        /// <param name="context">The persistence context used for database connection.</param>
        /// <param name="partition">The <see cref="ServerPartition"/> where the study resides</param>
        /// <param name="studyInstanceUid">The Study Instance Uid of the study</param>
        /// <param name="seriesInstanceUids">The Series Instance Uid of the series to be deleted.</param>
        /// <param name="reason">The reason for deleting the series.</param>
        /// <returns>A list of DeleteSeries <see cref="WorkQueue"/> entries inserted into the system.</returns>
        /// <exception cref="InvalidStudyStateOperationException"></exception>
        public static IList<WorkQueue> DeleteSeries(IUpdateContext context, ServerPartition partition, string studyInstanceUid, List<string> seriesInstanceUids, string reason)
        {
            // Find all location of the study in the system and insert series delete request
            IList<StudyStorageLocation> storageLocations = StudyStorageLocation.FindStorageLocations(partition.Key, studyInstanceUid);
            IList<WorkQueue> entries = new List<WorkQueue>();

            foreach (StudyStorageLocation location in storageLocations)
            {
                try
                {
                    string failureReason;
                    if (ServerHelper.LockStudy(location.Key, QueueStudyStateEnum.WebDeleteScheduled, out failureReason))
                    {
                        // insert a delete series request
                        WorkQueue request = InsertDeleteSeriesRequest(context, location, seriesInstanceUids, reason);
                        Debug.Assert(request.WorkQueueTypeEnum.Equals(WorkQueueTypeEnum.WebDeleteStudy));
                        entries.Add(request);
                    }
                    else
                    {
                        throw new ApplicationException(String.Format("Unable to lock storage location {0} for deletion : {1}", location.Key, failureReason));
                    }
                }
                catch(Exception ex)
                {
                    Platform.Log(LogLevel.Error, ex, "Errors occurred when trying to insert delete request");
                    if (!ServerHelper.UnlockStudy(location.Key))
                        throw new ApplicationException("Unable to unlock the study");
                }
            }

            return entries;
        }

        /// <summary>
        /// Inserts a move request to move one or more series in a study.
        /// </summary>
        /// <param name="context">The persistence context used for database connection.</param>
        /// <param name="partition">The <see cref="ServerPartition"/> where the study resides</param>
        /// <param name="studyInstanceUid">The Study Instance Uid of the study</param>
        /// <param name="deviceKey">The Key of the device to move the series to.</param> 
        /// <param name="seriesInstanceUids">The Series Instance Uid of the series to be move.</param>
        /// <returns>A MoveSeries <see cref="WorkQueue"/> entry inserted into the system.</returns>
        /// <exception cref="InvalidStudyStateOperationException"></exception>
        public static IList<WorkQueue> MoveSeries(IUpdateContext context, ServerPartition partition, string studyInstanceUid, ServerEntityKey deviceKey, List<string> seriesInstanceUids)
        {
            // Find all location of the study in the system and insert series delete request
			IList<StudyStorageLocation> storageLocations = StudyStorageLocation.FindStorageLocations(partition.Key, studyInstanceUid);
			IList<WorkQueue> entries = new List<WorkQueue>();

            foreach (StudyStorageLocation location in storageLocations)
            {
                try
                {
                    // insert a move series request
                    WorkQueue request = InsertMoveSeriesRequest(context, location, seriesInstanceUids, deviceKey);
                    Debug.Assert(request.WorkQueueTypeEnum.Equals(WorkQueueTypeEnum.WebMoveStudy));
                    entries.Add(request);
                }
                catch (Exception ex)
                {
                    Platform.Log(LogLevel.Error, ex, "Errors occurred when trying to insert move request");
                    if (!ServerHelper.UnlockStudy(location.Key))
                        throw new ApplicationException("Unable to unlock the study");
                }
            }

            return entries;
        }


        /// <summary>
        /// Inserts edit request(s) to update a study.
        /// </summary>
        /// <param name="context">The persistence context used for database connection.</param>
        /// <param name="studyStorageKey">The StudyStorage record key</param>
        /// <param name="reason">The reason the study is being editted</param>
        /// <param name="userId">The ID of the user requesting the study edit</param> 
        /// <param name="editType">The request is a web edit request</param>
        /// <exception cref="InvalidStudyStateOperationException"></exception>
        /// <param name="updateItems"></param>
        public static IList<WorkQueue> EditStudy(IUpdateContext context, ServerEntityKey studyStorageKey, List<UpdateItem> updateItems, string reason, string userId, EditType editType)
        {
            // Find all location of the study in the system and insert series delete request
			IList<StudyStorageLocation> storageLocations = StudyStorageLocation.FindStorageLocations(studyStorageKey);
			IList<WorkQueue> entries = new List<WorkQueue>();

            foreach (StudyStorageLocation location in storageLocations)
            {
                if (location.StudyStatusEnum.Equals(StudyStatusEnum.OnlineLossy))
                {
                    if (location.IsLatestArchiveLossless)
                    {
                        throw new InvalidStudyStateOperationException("Study is lossy but was archived as lossless. It must be restored before editing.");                
                    }
                }

                try
                {
                    string failureReason;
                    if (ServerHelper.LockStudy(location.Key, QueueStudyStateEnum.EditScheduled, out failureReason))
                    {
                        // insert an edit request
                        WorkQueue request = InsertEditStudyRequest(context, location.Key, location.ServerPartitionKey, WorkQueueTypeEnum.WebEditStudy, updateItems, reason, userId, editType);
                        entries.Add(request);
                    }
                    else
                    {
                        throw new ApplicationException(String.Format("Unable to lock storage location {0} for edit : {1}", location.Key, failureReason));
                    }
                }
                catch (Exception ex)
                {
                    Platform.Log(LogLevel.Error, ex, "Errors occured when trying to insert edit request");
                    if (!ServerHelper.UnlockStudy(location.Key))
                        throw new ApplicationException("Unable to unlock the study");
                }
            }

            return entries;
        }

        /// <summary>
        /// Inserts an External edit request(s) to update a study.
        /// </summary>
        /// <remarks>
        /// The External Edit request can be for a study in any state.  The study could be offline/nearline/etc.
        /// </remarks>
        /// <param name="context">The persistence context used for database connection.</param>
        /// <param name="studyStorageKey">The StudyStorage record key</param>
        /// <param name="reason">The reason the study is being editted</param>
        /// <param name="user">A string identifying the user that triggered the edit and is stored with the history for the edit.</param>
        /// <exception cref="InvalidStudyStateOperationException"></exception>
        /// <param name="updateItems"></param>
        /// <param name="editType">The request is a web edit request </param>
        public static IList<WorkQueue> ExternalEditStudy(IUpdateContext context, ServerEntityKey studyStorageKey, List<UpdateItem> updateItems, string reason, string user, EditType editType)
        {
            // Find all location of the study in the system and insert series delete request
            StudyStorage s = StudyStorage.Load(studyStorageKey);
            IList<WorkQueue> entries = new List<WorkQueue>();

            // insert an edit request
            WorkQueue request = InsertEditStudyRequest(context, s.Key, s.ServerPartitionKey,
                                                       WorkQueueTypeEnum.ExternalEdit, updateItems, reason, user,
                                                       editType);
            entries.Add(request);

            return entries;
        }

        /// <summary>
        /// Insert an EditStudy request.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="studyStorageKey"></param>
        /// <param name="serverPartitionKey"></param>
        /// <param name="type"></param>
        /// <param name="updateItems"></param>
        /// <param name="reason"></param>
        /// <param name="user"></param>
        /// <param name="editType"></param>
        /// <returns></returns>
        private static WorkQueue InsertEditStudyRequest(IUpdateContext context, ServerEntityKey studyStorageKey, ServerEntityKey serverPartitionKey, WorkQueueTypeEnum type, List<UpdateItem> updateItems, string reason, string user, EditType editType)
        {
        	var broker = context.GetBroker<IInsertWorkQueue>();
            InsertWorkQueueParameters criteria = new EditStudyWorkQueueParameters(studyStorageKey, serverPartitionKey, type, updateItems, reason, user, editType);
            WorkQueue editEntry = broker.FindOne(criteria);
            if (editEntry == null)
            {
                throw new ApplicationException(string.Format("Unable to insert an Edit request of type {0} for study for user {1}",type.Description, user));
            }
            return editEntry;
        }


        /// <summary>
        /// Inserts a DeleteSeries work queue entry
        /// </summary>
        /// <param name="context"></param>
        /// <param name="location"></param>
        /// <param name="seriesInstanceUids"></param>
        /// <param name="reason"></param>
        /// <exception cref="ApplicationException">If the "DeleteSeries" Work Queue entry cannot be inserted.</exception>
        private static WorkQueue InsertDeleteSeriesRequest(IUpdateContext context, StudyStorageLocation location, IEnumerable<string> seriesInstanceUids, string reason)
        {
            // Create a work queue entry and append the series instance uid into the WorkQueueUid table

            WorkQueue deleteSeriesEntry = null;
            foreach(string uid in seriesInstanceUids)
            {
                var broker = context.GetBroker<IInsertWorkQueue>();
                InsertWorkQueueParameters criteria = new DeleteSeriesWorkQueueParameters(location, uid, reason);
                deleteSeriesEntry = broker.FindOne(criteria);
                if (deleteSeriesEntry == null)
                {
                    throw new ApplicationException(
                        String.Format("Unable to insert a Delete Series request for series {0} in study {1}",
                                      uid, location.StudyInstanceUid));
                }
            }

            return deleteSeriesEntry;
        }

        /// <summary>
        /// Inserts a MoveStudy work queue entry
        /// </summary>
        /// <param name="context"></param>
        /// <param name="location"></param>
        /// <param name="seriesInstanceUids"></param>
        /// <param name="deviceKey"></param>
        /// <exception cref="ApplicationException">If the "DeleteSeries" Work Queue entry cannot be inserted.</exception>
        private static WorkQueue InsertMoveSeriesRequest(IUpdateContext context, StudyStorageLocation location, IEnumerable<string> seriesInstanceUids, ServerEntityKey deviceKey)
        {
            // Create a work queue entry and append the series instance uid into the WorkQueueUid table

            WorkQueue moveSeriesEntry = null;
            var broker = context.GetBroker<IInsertWorkQueue>();
			foreach (string series in seriesInstanceUids)
			{
				InsertWorkQueueParameters criteria = new MoveSeriesWorkQueueParameters(location, series, deviceKey);
				moveSeriesEntry = broker.FindOne(criteria);
				if (moveSeriesEntry == null)
				{
					throw new ApplicationException(
						String.Format("Unable to insert a Move Series request for study {0}", location.StudyInstanceUid));
				}
			}

        	return moveSeriesEntry;
        }
    }

    internal class EditStudyWorkQueueParameters : InsertWorkQueueParameters
    {
        public EditStudyWorkQueueParameters(ServerEntityKey studyStorageKey, ServerEntityKey serverPartitionKey, WorkQueueTypeEnum type, List<UpdateItem> updateItems, string reason, string user, EditType editType)
        {
            DateTime now = Platform.Time;
            var data = new EditStudyWorkQueueData
                           {
                               EditRequest =
                                   {
                                       TimeStamp = now,
                                       UserId = user,
                                       UpdateEntries = updateItems,
                                       Reason = reason,
                                       EditType = editType
                                   }
                           };

            WorkQueueTypeEnum = type;
            StudyStorageKey = studyStorageKey;
            ServerPartitionKey = serverPartitionKey;
            ScheduledTime = now;
            WorkQueueData = XmlUtils.SerializeAsXmlDoc(data); 
        }
    }

    class DeleteSeriesWorkQueueParameters : InsertWorkQueueParameters
    {
        public DeleteSeriesWorkQueueParameters(StudyStorageLocation studyStorageLocation, string seriesInstanceUid, string reason)
        {
            DateTime now = Platform.Time;
            var data = new WebDeleteSeriesLevelQueueData
                           {
                               Reason = reason,
                               Timestamp = now,
                               UserId = ServerHelper.CurrentUserName
                           };

        	WorkQueueTypeEnum = WorkQueueTypeEnum.WebDeleteStudy;
            StudyStorageKey = studyStorageLocation.Key;
            ServerPartitionKey = studyStorageLocation.ServerPartitionKey;
            ScheduledTime = now;
            SeriesInstanceUid = seriesInstanceUid;
            WorkQueueData = XmlUtils.SerializeAsXmlDoc(data);
        }
    }

    class MoveSeriesWorkQueueParameters : InsertWorkQueueParameters
    {
        public MoveSeriesWorkQueueParameters(StudyStorageLocation studyStorageLocation, string seriesInstanceUid, ServerEntityKey deviceKey)
        {
            DateTime now = Platform.Time;
            var data = new WebMoveSeriesLevelQueueData
                           {
                               Timestamp = now,
                               UserId = ServerHelper.CurrentUserName
                           };
			//data.SeriesInstanceUids = new List<string> {seriesInstanceUid};

        	WorkQueueTypeEnum = WorkQueueTypeEnum.WebMoveStudy;
            StudyStorageKey = studyStorageLocation.Key;
            ServerPartitionKey = studyStorageLocation.ServerPartitionKey;
            ScheduledTime = now;
        	SeriesInstanceUid = seriesInstanceUid;
            WorkQueueData = XmlUtils.SerializeAsXmlDoc(data);
            DeviceKey = deviceKey;
        }
    }
}
