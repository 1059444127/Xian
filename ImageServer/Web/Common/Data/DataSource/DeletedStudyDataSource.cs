﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Dicom.Utilities;
using ClearCanvas.ImageServer.Common.Utilities;
using ClearCanvas.ImageServer.Model;
using ClearCanvas.ImageServer.Model.EntityBrokers;
using ClearCanvas.ImageServer.Services.WorkQueue.DeleteStudy.Extensions;
using ClearCanvas.ImageServer.Web.Common.Data.Model;

namespace ClearCanvas.ImageServer.Web.Common.Data.DataSource
{
	public class DeletedStudyDataSource 
	{
		private IList<DeletedStudyInfo> _studies;

		public string AccessionNumber { get; set; }

		public DeletedStudyInfo  Find(object key)
		{
			return CollectionUtils.SelectFirst(_studies,
			                                   info => info.RowKey.Equals(key));
		}

		public string PatientId { get; set; }

		public DateTime? StudyDate { get; set; }

		public string DeletedBy { get; set; }

		public string PatientsName { get; set; }

		public string StudyDescription { get; set; }

		private StudyDeleteRecordSelectCriteria GetSelectCriteria()
		{
			StudyDeleteRecordSelectCriteria criteria = new StudyDeleteRecordSelectCriteria();
			if (!String.IsNullOrEmpty(AccessionNumber))
			{
				string key = AccessionNumber.Replace("*", "%");
				key = key.Replace("?", "_");
				criteria.AccessionNumber.Like(key);
			}
			if (!String.IsNullOrEmpty(PatientId))
			{
				string key = PatientId.Replace("*", "%");
				key = key.Replace("?", "_");
				criteria.PatientId.Like(key);
			}
			if (!String.IsNullOrEmpty(PatientsName))
			{
				string key = PatientsName.Replace("*", "%");
				key = key.Replace("?", "_");
				criteria.PatientsName.Like(key);
			}
			if (!String.IsNullOrEmpty(StudyDescription))
			{
				string key = StudyDescription.Replace("*", "%");
				key = key.Replace("?", "_");
				criteria.StudyDescription.Like(key);
			}
			if (StudyDate != null)
				criteria.StudyDate.Like("%" + DateParser.ToDicomString(StudyDate.Value) + "%");

			return criteria;
		}

		public IEnumerable Select(int startRowIndex, int maxRows)
		{
			
			IStudyDeleteRecordEntityBroker broker = HttpContextData.Current.ReadContext.GetBroker<IStudyDeleteRecordEntityBroker>();
			StudyDeleteRecordSelectCriteria criteria = GetSelectCriteria();
			criteria.Timestamp.SortDesc(0);
			IList<StudyDeleteRecord> list = broker.Find(criteria, startRowIndex, maxRows);

			_studies = CollectionUtils.Map(
				list, (StudyDeleteRecord record) => DeletedStudyInfoAssembler.CreateDeletedStudyInfo(record));

			// Additional filter: DeletedBy
            if (String.IsNullOrEmpty(DeletedBy)==false)
            {
                _studies = CollectionUtils.Select(_studies, delegate(DeletedStudyInfo record)
                                       {
                                           if (String.IsNullOrEmpty(record.UserId) || String.IsNullOrEmpty(record.UserName))
                                               return false;

                                           // either the id or user matches
                                           return record.UserId.ToUpper().IndexOf(DeletedBy.ToUpper()) >= 0 ||
                                                  record.UserName.ToUpper().IndexOf(DeletedBy.ToUpper()) >= 0;
                                       });
            }

			return _studies;
		
		}

		public int SelectCount()
		{
			StudyDeleteRecordSelectCriteria criteria = GetSelectCriteria();

            IStudyDeleteRecordEntityBroker broker = HttpContextData.Current.ReadContext.GetBroker<IStudyDeleteRecordEntityBroker>();
		    return broker.Count(criteria);
		}
	}

	internal static class DeletedStudyInfoAssembler
	{
		public static DeletedStudyInfo CreateDeletedStudyInfo(StudyDeleteRecord record)
		{
			Filesystem fs = Filesystem.Load(record.FilesystemKey);

		    StudyDeleteExtendedInfo extendedInfo = XmlUtils.Deserialize<StudyDeleteExtendedInfo>(record.ExtendedInfo);
			DeletedStudyInfo info = new DeletedStudyInfo
			                        	{
			                        		DeleteStudyRecord = record.GetKey(),
			                        		RowKey = record.GetKey().Key,
			                        		StudyInstanceUid = record.StudyInstanceUid,
			                        		PatientsName = record.PatientsName,
			                        		AccessionNumber = record.AccessionNumber,
			                        		PatientId = record.PatientId,
			                        		StudyDate = record.StudyDate,
			                        		PartitionAE = record.ServerPartitionAE,
			                        		StudyDescription = record.StudyDescription,
			                        		BackupFolderPath = fs.GetAbsolutePath(record.BackupPath),
			                        		ReasonForDeletion = record.Reason,
			                        		DeleteTime = record.Timestamp,
			                        		UserName = extendedInfo.UserName,
			                        		UserId = extendedInfo.UserId
			                        	};
			if (record.ArchiveInfo!=null)
				info.Archives = XmlUtils.Deserialize<DeletedStudyArchiveInfoCollection>(record.ArchiveInfo);

            
			return info;
		}
	}
}