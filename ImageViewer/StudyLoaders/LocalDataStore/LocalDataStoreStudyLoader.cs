#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System.Collections.Generic;
using ClearCanvas.Common;
using ClearCanvas.ImageViewer.Services.Auditing;
using ClearCanvas.ImageViewer.StudyManagement;
using ClearCanvas.Dicom.DataStore;

namespace ClearCanvas.ImageViewer.StudyLoaders.LocalDataStore
{
    [ExtensionOf(typeof(StudyLoaderExtensionPoint))]
	public class LocalDataStoreStudyLoader : StudyLoader
    {
		private IEnumerator<ISopInstance> _sops;

        public LocalDataStoreStudyLoader() : base("DICOM_LOCAL")
        {
			var coreStrategy = new SimpleCorePrefetchingStrategy(frame => frame.ParentImageSop.DataSource is LocalDataStoreSopDataSource);
			PrefetchingStrategy = new WeightedWindowPrefetchingStrategy(coreStrategy, "DICOM_LOCAL", "Simple prefetcing strategy for local data store images.")
			{
				Enabled = PreLoadingSettings.Default.Enabled,
				RetrievalThreadConcurrency = PreLoadingSettings.Default.Concurrency,
				FrameLookAheadCount = PreLoadingSettings.Default.FrameLookAheadCount,
				SelectedImageBoxWeight = PreLoadingSettings.Default.SelectedImageBoxWeight,
				UnselectedImageBoxWeight = PreLoadingSettings.Default.UnselectedImageBoxWeight,
				DecompressionThreadConcurrency = 0
			};
		}

    	protected override int OnStart(StudyLoaderArgs studyLoaderArgs)
		{
    		_sops = null;

    		EventResult result = EventResult.Success;
			var loadedInstances = new AuditedInstances();
			try
			{
				using (IDataStoreReader reader = DataAccessLayer.GetIDataStoreReader())
				{
					IStudy study = reader.GetStudy(studyLoaderArgs.StudyInstanceUid);
					if (study == null)
					{
						result = EventResult.MajorFailure;
						loadedInstances.AddInstance(studyLoaderArgs.StudyInstanceUid);
						throw new NotFoundLoadStudyException(studyLoaderArgs.StudyInstanceUid);
					}
					loadedInstances.AddInstance(study.PatientId, study.PatientsName, study.StudyInstanceUid);

					_sops = study.GetSopInstances().GetEnumerator();
					return study.NumberOfStudyRelatedInstances;
				}
			}
			finally
			{
				AuditHelper.LogOpenStudies(new[] { AuditHelper.LocalAETitle }, loadedInstances, EventSource.CurrentUser, result);
			}
		}

		protected override SopDataSource LoadNextSopDataSource()
        {
			if (_sops == null)
				return null;

			if (!_sops.MoveNext())
			{
				_sops = null;
				return null;
			}

			return new LocalDataStoreSopDataSource(_sops.Current);
        }
    }
}