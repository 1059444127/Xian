#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using ClearCanvas.Enterprise.Core;
using ClearCanvas.ImageServer.Common.CommandProcessor;
using ClearCanvas.ImageServer.Enterprise;
using ClearCanvas.ImageServer.Model;
using ClearCanvas.ImageServer.Model.EntityBrokers;

namespace ClearCanvas.ImageServer.Services.ServiceLock.FilesystemStudyProcess
{
    /// <summary>
    /// <see cref="ServerDatabaseCommand"/> derived class for deleting <see cref="FilesystemQueue"/> entries for a
    /// specific study from the database.
    /// </summary>
    public class DeleteFilesystemQueueCommand : ServerDatabaseCommand
    {
        private readonly ServerEntityKey _storageLocationKey;
    	private readonly ServerRuleApplyTimeEnum _applyTime = null;

		public DeleteFilesystemQueueCommand(ServerEntityKey storageLocationKey, ServerRuleApplyTimeEnum applyTime)
			: base("Delete FilesystemQueue", false)
		{
			_storageLocationKey = storageLocationKey;
			_applyTime = applyTime;
		}

        protected override void OnExecute(ServerCommandProcessor theProcessor, IUpdateContext updateContext)
        {
            IFilesystemQueueEntityBroker filesystemQueueBroker= updateContext.GetBroker<IFilesystemQueueEntityBroker>();
            FilesystemQueueSelectCriteria criteria = new FilesystemQueueSelectCriteria();
            criteria.StudyStorageKey.EqualTo(_storageLocationKey);
            IList<FilesystemQueue> filesystemQueueItems = filesystemQueueBroker.Find(criteria);

            IWorkQueueEntityBroker workQueueBroker = updateContext.GetBroker<IWorkQueueEntityBroker>();
            WorkQueueSelectCriteria workQueueCriteria = new WorkQueueSelectCriteria();
            workQueueCriteria.StudyStorageKey.EqualTo(_storageLocationKey);
			workQueueCriteria.WorkQueueTypeEnum.In(new WorkQueueTypeEnum[] { WorkQueueTypeEnum.PurgeStudy, WorkQueueTypeEnum.DeleteStudy, WorkQueueTypeEnum.CompressStudy, WorkQueueTypeEnum.MigrateStudy });
            IList<Model.WorkQueue> workQueueItems = workQueueBroker.Find(workQueueCriteria);

            foreach (FilesystemQueue queue in filesystemQueueItems)
            {
            	bool delete = false;
				if (_applyTime.Equals(ServerRuleApplyTimeEnum.StudyArchived))
				{
					if (queue.FilesystemQueueTypeEnum.Equals(FilesystemQueueTypeEnum.PurgeStudy))
						delete = true;
				}
				else
				{
					delete = true;
				}

				if (delete)
				{
					if (!filesystemQueueBroker.Delete(queue.GetKey()))
						throw new ApplicationException("Unable to delete items in the filesystem queue");
				}
            }

			if (!_applyTime.Equals(ServerRuleApplyTimeEnum.StudyArchived))
			{
				// delete work queue
				foreach (Model.WorkQueue item in workQueueItems)
				{
					if (!item.Delete(updateContext))
						throw new ApplicationException("Unable to delete items in the work queue");
				}
			}
        }
    }
}
