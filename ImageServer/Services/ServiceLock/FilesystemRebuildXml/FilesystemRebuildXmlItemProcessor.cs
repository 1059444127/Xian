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
using System.IO;
using ClearCanvas.Common;
using ClearCanvas.Dicom;
using ClearCanvas.Enterprise.Core;
using ClearCanvas.ImageServer.Common;
using ClearCanvas.ImageServer.Common.CommandProcessor;
using ClearCanvas.ImageServer.Common.Exceptions;
using ClearCanvas.ImageServer.Common.Utilities;
using ClearCanvas.ImageServer.Core.Rebuild;
using ClearCanvas.ImageServer.Model;
using ClearCanvas.ImageServer.Model.EntityBrokers;

namespace ClearCanvas.ImageServer.Services.ServiceLock.FilesystemRebuildXml
{
	/// <summary>
	/// Class for processing 'FilesystemRebuildXml' <see cref="Model.ServiceLock"/> rows.
	/// </summary>
	public class FilesystemRebuildXmlItemProcessor : BaseServiceLockItemProcessor, IServiceLockItemProcessor, ICancelable
	{
		#region Private Members

		private IList<ServerPartition> _partitions;
		#endregion

		#region Private Methods
		/// <summary>
		/// Traverse the filesystem directories for studies to rebuild the XML for.
		/// </summary>
		/// <param name="filesystem"></param>
		private void TraverseFilesystemStudies(Filesystem filesystem)
		{
			List<StudyStorageLocation> lockFailures = new List<StudyStorageLocation>();
			ServerPartition partition;

			DirectoryInfo filesystemDir = new DirectoryInfo(filesystem.FilesystemPath);

			foreach (DirectoryInfo partitionDir in filesystemDir.GetDirectories())
			{
				if (GetServerPartition(partitionDir.Name, out partition) == false)
					continue;

				foreach (DirectoryInfo dateDir in partitionDir.GetDirectories())
				{
					if (dateDir.FullName.EndsWith("Deleted")
						|| dateDir.FullName.EndsWith(ServerPlatform.ReconcileStorageFolder))
						continue;

					foreach (DirectoryInfo studyDir in dateDir.GetDirectories())
					{
						// Check for Cancel message
						if (CancelPending) return;

						String studyInstanceUid = studyDir.Name;
						
						StudyStorageLocation location;
						try
						{
							FilesystemMonitor.Instance.GetWritableStudyStorageLocation(partition.Key, studyInstanceUid,
							                                                           StudyRestore.False, StudyCache.False, out location);
						}
						catch (StudyNotFoundException)
						{
							List<FileInfo> fileList = LoadSopFiles(studyDir, true);
							if (fileList.Count == 0)
							{
								Platform.Log(LogLevel.Warn, "Found empty study folder: {0}\\{1}", dateDir.Name, studyDir.Name);
								continue;
							}

							DicomFile file = LoadFileFromList(fileList);
							if (file == null)
							{
								Platform.Log(LogLevel.Warn, "Found directory with no readable files: {0}\\{1}", dateDir.Name, studyDir.Name);
								continue;
							}

							// Do a second check, using the study instance uid from a file in the directory.
							// had an issue with trailing periods on uids causing us to not find the 
							// study storage, and insert a new record into the database.
							studyInstanceUid = file.DataSet[DicomTags.StudyInstanceUid].ToString();
							if (!studyInstanceUid.Equals(studyDir.Name))
								try
								{
									FilesystemMonitor.Instance.GetWritableStudyStorageLocation(partition.Key, studyInstanceUid,
									                                                           StudyRestore.False, StudyCache.False, out location);
								}
								catch (Exception e)
								{
									Platform.Log(LogLevel.Warn, "Study {0} on filesystem partition {1} not found {2}: {3}", studyInstanceUid,
									             partition.Description, studyDir.ToString(), e.Message);
									continue;
								}
							else
							{
								Platform.Log(LogLevel.Warn, "Study {0} on filesystem partition {1} not found {2}", studyInstanceUid,
											 partition.Description, studyDir.ToString());
								continue;
							}
						}
						catch (Exception e)
						{
							Platform.Log(LogLevel.Warn, "Study {0} on filesystem partition {1} not found {2}: {3}", studyInstanceUid,
										 partition.Description, studyDir.ToString(), e.Message);
							continue;
						}

						try
						{
							if (!location.AcquireWriteLock())
							{
								Platform.Log(LogLevel.Warn, "Unable to lock study: {0}, delaying rebuild", location.StudyInstanceUid);
								lockFailures.Add(location);
								continue;
							}

							StudyXmlRebuilder rebuilder = new StudyXmlRebuilder(location);
							rebuilder.RebuildXml();

							location.ReleaseWriteLock();
						}
						catch (Exception e)
						{
							Platform.Log(LogLevel.Error, e, "Unexpected exception when rebuilding study xml for study: {0}",
							             location.StudyInstanceUid);
							lockFailures.Add(location);
						}
					}


					// Cleanup the parent date directory, if its empty
					DirectoryUtility.DeleteIfEmpty(dateDir.FullName);
				}
			}

			// Re-do all studies that failed locks one time
			foreach (StudyStorageLocation location in lockFailures)
			{
				try
				{
					if (!location.AcquireWriteLock())
					{
						Platform.Log(LogLevel.Warn, "Unable to lock study: {0}, skipping rebuild", location.StudyInstanceUid);
						continue;
					}

					StudyXmlRebuilder rebuilder = new StudyXmlRebuilder(location);
					rebuilder.RebuildXml();

					location.ReleaseWriteLock();
				}
				catch (Exception e)
				{
					Platform.Log(LogLevel.Error, e, "Unexpected exception on retry when rebuilding study xml for study: {0}",
										 location.StudyInstanceUid);
				}
			}
		}

		/// <summary>
		/// Get the server partition
		/// </summary>
		/// <param name="partitionFolderName"></param>
		/// <param name="partition"></param>
		/// <returns></returns>
		private bool GetServerPartition(string partitionFolderName, out ServerPartition partition)
		{
			foreach (ServerPartition part in _partitions)
			{
				if (part.PartitionFolder == partitionFolderName)
				{
					partition = part;
					return true;
				}
			}

			partition = null;
			return false;
		}

		#endregion

		#region Public Methods
		/// <summary>
		/// Process the <see cref="ServiceLock"/> item.
		/// </summary>
		/// <param name="item"></param>
        protected override void OnProcess(Model.ServiceLock item)
		{
			PersistentStoreRegistry.GetDefaultStore();

			using (ExecutionContext context = new ExecutionContext())
			{
				IServerPartitionEntityBroker broker = context.ReadContext.GetBroker<IServerPartitionEntityBroker>();
				ServerPartitionSelectCriteria criteria = new ServerPartitionSelectCriteria();
				criteria.AeTitle.SortAsc(0);

				_partitions = broker.Find(criteria);
			}

			ServerFilesystemInfo info = FilesystemMonitor.Instance.GetFilesystemInfo(item.FilesystemKey);

			Platform.Log(LogLevel.Info, "Starting rebuilding of Study XML files for filesystem: {0}", info.Filesystem.Description);

			TraverseFilesystemStudies(info.Filesystem);

			item.ScheduledTime = item.ScheduledTime.AddDays(1);

			if (CancelPending)
			{
				Platform.Log(LogLevel.Info,
							 "FilesystemRebuildXml of {0} has been canceled, rescheduling.  Note that the entire Filesystem will be rebuilt again.",
							 info.Filesystem.Description);
				UnlockServiceLock(item, true, Platform.Time.AddMinutes(1));
			}
			else
				UnlockServiceLock(item, false, Platform.Time.AddDays(1));

			Platform.Log(LogLevel.Info, "Completed rebuilding of the Study XML files for filesystem: {0}", info.Filesystem.Description);
		}


		#endregion

		public new void Dispose()
		{
			base.Dispose();
		}
	}
}
