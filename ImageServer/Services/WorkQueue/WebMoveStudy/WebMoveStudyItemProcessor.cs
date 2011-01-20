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
using System.IO;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Dicom.Network.Scu;
using ClearCanvas.Dicom.Utilities.Xml;
using ClearCanvas.Enterprise.Core;
using ClearCanvas.ImageServer.Common;
using ClearCanvas.ImageServer.Common.Utilities;
using ClearCanvas.ImageServer.Core;
using ClearCanvas.ImageServer.Core.Edit;
using ClearCanvas.ImageServer.Core.Validation;
using ClearCanvas.ImageServer.Model;
using ClearCanvas.ImageServer.Model.EntityBrokers;
using ClearCanvas.ImageServer.Services.WorkQueue.AutoRoute;

namespace ClearCanvas.ImageServer.Services.WorkQueue.WebMoveStudy
{
    [StudyIntegrityValidation(ValidationTypes = StudyIntegrityValidationModes.None)]
    public class WebMoveStudyItemProcessor : AutoRouteItemProcessor
    {
        
        /// <summary>
        /// Gets the list of instances to be sent from the study xml
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<StorageInstance> GetStorageInstanceList()
        {
        	IList<WorkQueueUid> seriesList = WorkQueueUidList;

            Platform.CheckForNullReference(StorageLocation, "StorageLocation");

            List<StorageInstance> list = new List<StorageInstance>(); 

			// We alread moved the series
			if (WorkQueueItem.Data != null && seriesList.Count == 0)
				return list;

            string studyPath = StorageLocation.GetStudyPath();
            StudyXml studyXml = LoadStudyXml(StorageLocation);
            foreach (SeriesXml seriesXml in studyXml)
            {
                // FOR SERIES LEVEL Move,
                // Check if the series is in the WorkQueueUid list. If it is not in the list then don't include it.
				if (seriesList.Count > 0)
				{
					bool found = false;
					foreach (WorkQueueUid uid in seriesList)
					{
						if (!string.IsNullOrEmpty(uid.SeriesInstanceUid))
							if (uid.SeriesInstanceUid.Equals(seriesXml.SeriesInstanceUid))
							{
								found = true;
								break;
							}
					}
					if (!found) continue; // don't send this series
				}

            	foreach (InstanceXml instanceXml in seriesXml)
                {
                    string seriesPath = Path.Combine(studyPath, seriesXml.SeriesInstanceUid);
                    string instancePath = Path.Combine(seriesPath, instanceXml.SopInstanceUid + ServerPlatform.DicomFileExtension);
                    StorageInstance instance = new StorageInstance(instancePath)
                                               	{
                                               		SopClass = instanceXml.SopClass,
                                               		TransferSyntax = instanceXml.TransferSyntax,
                                               		SopInstanceUid = instanceXml.SopInstanceUid,
                                               		StudyInstanceUid = studyXml.StudyInstanceUid,
													SeriesInstanceUid = seriesXml.SeriesInstanceUid,
                                               		PatientId = studyXml.PatientId,
                                               		PatientsName = studyXml.PatientsName
                                               	};

                	list.Add(instance);
                }
            }
            
            return list;
        }

        protected override void OnComplete()
        {
			if (WorkQueueItem.Data == null)
			{
				AddWorkQueueData();
			}

            // Force the entry to idle and stay for a while
            // Note: the assumption is the code will set ScheduledTime = ExpirationTime = some future time
            // so that the item will be removed when it is processed again.
            PostProcessing(WorkQueueItem,
                           WorkQueueProcessorStatus.CompleteDelayDelete, 
                           WorkQueueProcessorDatabaseUpdate.None);
        
        }

		private void AddWorkQueueData()
		{
			WebMoveWorkQueueEntryData data = new WebMoveWorkQueueEntryData
			{
				Timestamp = DateTime.Now,
				UserId = ServerHelper.CurrentUserName
			};
			using (IUpdateContext update = PersistentStoreRegistry.GetDefaultStore().OpenUpdateContext(UpdateContextSyncMode.Flush))
			{
				IWorkQueueEntityBroker broker = update.GetBroker<IWorkQueueEntityBroker>();
				WorkQueueUpdateColumns cols = new WorkQueueUpdateColumns();
				cols.Data = XmlUtils.SerializeAsXmlDoc(data);
				broker.Update(WorkQueueItem.Key, cols);
				update.Commit();
			}
		}

        protected override bool CanStart()
        {
            IList<Model.WorkQueue> relatedItems = FindRelatedWorkQueueItems(WorkQueueItem,
                new [] { WorkQueueTypeEnum.StudyProcess, WorkQueueTypeEnum.ReconcileStudy },
                new [] { WorkQueueStatusEnum.Idle, WorkQueueStatusEnum.InProgress, WorkQueueStatusEnum.Pending });
            
            if (relatedItems != null && relatedItems.Count > 0)
            {
                // can't do it now. Reschedule it for future
                List<Model.WorkQueue> list = CollectionUtils.Sort(relatedItems,
                                                                  (item1, item2) =>
                                                                  item1.ScheduledTime.CompareTo(item2.ScheduledTime));

                DateTime newScheduledTime = list[0].ScheduledTime.AddSeconds(WorkQueueProperties.PostponeDelaySeconds);
                if (newScheduledTime < Platform.Time.AddMinutes(1))
                    newScheduledTime = Platform.Time.AddMinutes(1);

                PostponeItem(newScheduledTime, newScheduledTime.AddDays(1), "Study is being processed or reconciled.");
                Platform.Log(LogLevel.Info, "{0} postponed to {1}. Study UID={2}", WorkQueueItem.WorkQueueTypeEnum, newScheduledTime, StorageLocation.StudyInstanceUid);
            }

            return base.CanStart();
        }
    }

    
}
