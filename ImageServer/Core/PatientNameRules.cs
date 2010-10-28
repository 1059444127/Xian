#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Text;
using ClearCanvas.Common;
using ClearCanvas.Dicom;
using ClearCanvas.ImageServer.Common;
using ClearCanvas.ImageServer.Common.CommandProcessor;
using ClearCanvas.ImageServer.Common.Helpers;
using ClearCanvas.ImageServer.Common.Utilities;
using ClearCanvas.ImageServer.Core.Edit;
using ClearCanvas.ImageServer.Model;

namespace ClearCanvas.ImageServer.Core
{
    /// <summary>
    /// Helper class to update the patient's name 
    /// in the DICOM file based on the rules for patient name.
    /// </summary>
    public class PatientNameRules
    {
        readonly Study _theStudy;

        public PatientNameRules(Study study)
        {
            _theStudy = study;
        }

        #region IStudyPreProcessor Members

        /// <summary>
        /// Updates the Patient's Name tag in the specified <see cref="DicomFile"/>
        /// based on the specified <see cref="StudyStorageLocation"/>. Normalization
        /// may occur.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public UpdateItem Apply(DicomFile file)
        {
            Platform.CheckForNullReference(file, "file");
            
            string orginalPatientsNameInFile = file.DataSet[DicomTags.PatientsName].ToString();

			// Note: only apply the name rules if we can't update it to match the study
            if (!UpdateNameBasedOnTheStudy(file))
                UpdateNameBasedOnRules(file);

            string newPatientName = file.DataSet[DicomTags.PatientsName].ToString();
            UpdateItem change = null;

            if (!newPatientName.Equals(orginalPatientsNameInFile, StringComparison.InvariantCultureIgnoreCase))
            {
                change = new UpdateItem(DicomTags.PatientsName, orginalPatientsNameInFile, newPatientName);

                StringBuilder log = new StringBuilder();
                log.AppendLine(String.Format("AUTO-CORRECTION: SOP {0}", file.MediaStorageSopInstanceUid));
                log.AppendLine(String.Format("\tPatient's Name: {0} ==> {1}. ",
                                             change.OriginalValue, change.NewValue));
                Platform.Log(LogLevel.Info, log.ToString());
            }

            return change;
        }

        private bool UpdateNameBasedOnTheStudy(DicomFile file)
        {
            bool updated = false;
            string orginalPatientsNameInFile = file.DataSet[DicomTags.PatientsName].ToString();

            if (_theStudy==null)
            {
                return false;
            }

            StudyComparer comparer = new StudyComparer();
            ServerPartition partition = ServerPartitionMonitor.Instance.FindPartition(_theStudy.ServerPartitionKey);
            DifferenceCollection list = comparer.Compare(file, _theStudy, partition.GetComparisonOptions());

            if (list.Count == 1)
            {
                ComparisionDifference different = list[0];
                if (different.DicomTag.TagValue == DicomTags.PatientsName)
                {
                    if (DicomNameUtils.LookLikeSameNames(orginalPatientsNameInFile, _theStudy.PatientsName))
                    {
                        using (ServerCommandProcessor processor = new ServerCommandProcessor("Update Patient's Name"))
                        {
                            SetTagCommand command = new SetTagCommand(file, DicomTags.PatientsName, orginalPatientsNameInFile, _theStudy.PatientsName);
                            processor.AddCommand(command);

                            if (!processor.Execute())
                            {
                                throw new ApplicationException(String.Format("AUTO-CORRECTION Failed: Unable to correct the patient's name in the image. Reason: {0}",
                                                                             processor.FailureReason), processor.FailureException);
                            }

                            updated = true;
                        }
                    }
                }
            }
            return updated;
        }

        private static void UpdateNameBasedOnRules(DicomFile file)
        {
            string orginalPatientsNameInFile = file.DataSet[DicomTags.PatientsName].ToString();
            
            if (String.IsNullOrEmpty(orginalPatientsNameInFile))
                return;

            using (ServerCommandProcessor processor = new ServerCommandProcessor("Update Patient's Name"))
            {
                string normPatName = GetAcceptableName(orginalPatientsNameInFile);

                if (!orginalPatientsNameInFile.Equals(normPatName, StringComparison.InvariantCultureIgnoreCase))
                {
                    processor.AddCommand(new SetTagCommand(file, DicomTags.PatientsName, orginalPatientsNameInFile, normPatName));

                    if (!processor.Execute())
                    {
                        throw new ApplicationException(String.Format("AUTO-CORRECTION Failed: Unable to correct the patient's name in the image. Reason: {0}",
                                                                     processor.FailureReason), processor.FailureException);
                    }
                }
            }

            return;
        }

        /// <summary>
        /// Returns an acceptable name for the given name.
        /// </summary>
        /// <param name="originalName"></param>
        /// <returns></returns>
        static public string GetAcceptableName(string originalName)
        {
            return DicomNameUtils.Normalize(originalName, DicomNameUtils.NormalizeOptions.TrimSpaces | DicomNameUtils.NormalizeOptions.TrimEmptyEndingComponents);
        }

        #endregion
    }
}