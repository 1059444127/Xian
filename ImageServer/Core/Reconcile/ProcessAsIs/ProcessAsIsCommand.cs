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
using ClearCanvas.Dicom;
using ClearCanvas.Dicom.Utilities.Xml;
using ClearCanvas.ImageServer.Common;
using ClearCanvas.ImageServer.Common.CommandProcessor;
using ClearCanvas.ImageServer.Core.Process;
using ClearCanvas.ImageServer.Model;
using ClearCanvas.ImageServer.Rules;

namespace ClearCanvas.ImageServer.Core.Reconcile.ProcessAsIs
{
	internal class ProcessAsIsCommand : ReconcileCommandBase
	{
		private StudyStorageLocation _destinationStudyStorage;
		private readonly bool _complete;

		public StudyStorageLocation Location
		{
			get { return _destinationStudyStorage; }
		}
   
		/// <summary>
		/// Creates an instance of <see cref="ProcessAsIsCommand"/>
		/// </summary>
		public ProcessAsIsCommand(ReconcileStudyProcessorContext context, bool complete)
			: base("Process As-is Command", true, context)
		{
			_complete = complete;
		}

		protected override void OnExecute(ServerCommandProcessor theProcessor)
		{
			Platform.CheckForNullReference(Context, "Context");
			
			DetermineTargetLocation();
		
			EnsureStudyCanBeUpdated(_destinationStudyStorage);

            try
            {
                if (Context.WorkQueueUidList.Count>0)
                    ProcessUidList();
            }
            finally
            {
                UpdateHistory(_destinationStudyStorage);
            }
			if (_complete)
			{
				StudyRulesEngine engine = new StudyRulesEngine(_destinationStudyStorage, Context.Partition);
				engine.Apply(ServerRuleApplyTimeEnum.StudyProcessed, theProcessor);
			}
		}

		private void DetermineTargetLocation()
		{
			if (Context.History.DestStudyStorageKey!=null)
			{
				_destinationStudyStorage =
					StudyStorageLocation.FindStorageLocations(StudyStorage.Load(Context.History.DestStudyStorageKey))[0];

			}
			else
			{
				_destinationStudyStorage = Context.WorkQueueItemStudyStorage;
				Context.History.DestStudyStorageKey = _destinationStudyStorage.Key;
			}
		}

		protected override void OnUndo()
		{
			// undo is done  in SaveFile()
		}

		private void ProcessUidList()
		{
			int counter = 0;
			Platform.Log(LogLevel.Info, "Populating new images into study folder.. {0} to go", Context.WorkQueueUidList.Count);

			StudyProcessorContext context = new StudyProcessorContext(_destinationStudyStorage);

			// Load the rules engine
			context.SopProcessedRulesEngine = new ServerRulesEngine(ServerRuleApplyTimeEnum.SopProcessed, Context.WorkQueueItem.ServerPartitionKey);
			context.SopProcessedRulesEngine.AddOmittedType(ServerRuleTypeEnum.SopCompress);
			context.SopProcessedRulesEngine.Load();

			// Load the Study XML File
			StudyXml xml = LoadStudyXml(_destinationStudyStorage);

		    string lastErrorMessage="";

		    foreach (WorkQueueUid uid in Context.WorkQueueUidList)
			{
				string imagePath = GetReconcileUidPath(uid);
				DicomFile file = new DicomFile(imagePath);
			
				try
				{
					file.Load();
					
					string groupID = ServerHelper.GetUidGroup(file, _destinationStudyStorage.ServerPartition, Context.WorkQueueItem.InsertTime);

				    SopInstanceProcessor sopProcessor = new SopInstanceProcessor(context);
                    ProcessingResult result = sopProcessor.ProcessFile(groupID, file, xml, false, true, uid, GetReconcileUidPath(uid));
					if (result.Status != ProcessingStatus.Success)
					{
						throw new ApplicationException(String.Format("Unable to reconcile image {0}", file.Filename));
					}

					counter++;
			
					Platform.Log(ServerPlatform.InstanceLogLevel, "Reconciled SOP {0} [{1} of {2}]",
					             uid.SopInstanceUid, counter, Context.WorkQueueUidList.Count);
				}
				catch (Exception e)
				{
					Platform.Log(LogLevel.Error, e, "Error occurred when processing uid {0}", uid.SopInstanceUid);

                    if (e is InstanceAlreadyExistsException
                        || e.InnerException != null && e.InnerException is InstanceAlreadyExistsException)
                    {
                        DuplicateSopProcessorHelper.CreateDuplicateSIQEntry(file, _destinationStudyStorage, GetReconcileUidPath(uid),
                                                                           Context.WorkQueueItem, uid);
                    }
                    else
                    {
                        lastErrorMessage = e.Message;
                        SopInstanceProcessor.FailUid(uid, true);
                    }
				}
			}

            
            if (counter == 0)
            {
                throw new ApplicationException(lastErrorMessage);
            }
		}
	}
}