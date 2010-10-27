﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using ClearCanvas.Enterprise.Core;
using ClearCanvas.ImageServer.Model;
using ClearCanvas.ImageServer.Web.Common.Data;

namespace ClearCanvas.ImageServer.Web.Application.Pages.Queues.WorkQueue.Edit
{
    /// <summary>
    /// Assembles an instance of  <see cref="StudyDetails"/> based on a <see cref="Study"/> object.
    /// </summary>
    public class StudyDetailsAssembler
    {
        /// <summary>
        /// Creates an instance of <see cref="StudyDetails"/> base on a <see cref="Study"/> object.
        /// </summary>
        /// <param name="study"></param>
        /// <returns></returns>
        public StudyDetails CreateStudyDetail(Study study)
        {
            var details = new StudyDetails();
            details.StudyInstanceUID = study.StudyInstanceUid;
            details.PatientName = study.PatientsName;
            details.AccessionNumber = study.AccessionNumber;
            details.PatientID = study.PatientId;
            details.StudyDescription = study.StudyDescription;
            details.StudyDate = study.StudyDate;
            details.StudyTime = study.StudyTime;

            var controller = new StudyController();
            using (IReadContext ctx = PersistentStoreRegistry.GetDefaultStore().OpenReadContext())
            {
                details.Modalities = controller.GetModalitiesInStudy(ctx, study);
            }

            if (study.StudyInstanceUid != null)
            {
            	StudyStorage storages = StudyStorage.Load(study.StudyStorageKey);
                if (storages != null)
                {
                    details.WriteLock = storages.WriteLock;
                	details.ReadLock = storages.ReadLock;
                    details.Status = storages.StudyStatusEnum.ToString();
                }
            }
			
            return details;
        }
    }
}