#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Desktop;
using ClearCanvas.ImageViewer.StudyManagement;

namespace ClearCanvas.ImageViewer
{
	public partial class ImageViewerComponent
	{
		internal class LocalSopLoader : IDisposable
		{
			private readonly IImageViewer _viewer;

			private int _total;
			private int _failed;

			public LocalSopLoader(IImageViewer viewer)
			{
				_viewer = viewer;
			}

			public int Total
			{
				get { return _total; }
			}

			public int Failed
			{
				get { return _failed; }
			}

			public void Load(string[] files, IDesktopWindow desktop, out bool cancelled)
			{
				Platform.CheckForNullReference(files, "files");

				_total = 0;
				_failed = 0;

				bool userCancelled = false;

				if (desktop != null)
				{
					BackgroundTask task = new BackgroundTask(
						delegate(IBackgroundTaskContext context)
						{
							for (int i = 0; i < files.Length; i++)
							{
								LoadSop(files[i]);

								int percentComplete = (int)(((float)(i + 1) / files.Length) * 100);
								string message = String.Format(SR.MessageFormatOpeningImages, i, files.Length);

								BackgroundTaskProgress progress = new BackgroundTaskProgress(percentComplete, message);
								context.ReportProgress(progress);

								if (context.CancelRequested)
								{
									userCancelled = true;
									break;
								}
							}

							context.Complete(null);

						}, true);

					ProgressDialog.Show(task, desktop, true, ProgressBarStyle.Blocks);
					cancelled = userCancelled;
				}
				else
				{
					foreach (string file in files)
						LoadSop(file);

					cancelled = false;
				}

				if (Failed > 0)
					throw new LoadSopsException(Total, Failed);
			}

			private void LoadSop(string file)
			{
				try
				{
					Sop sop = Sop.Create(file);
					try
					{
						_viewer.StudyTree.AddSop(sop);
					}
					catch (SopValidationException)
					{
						sop.Dispose();
						throw;
					}
				}
				catch (Exception e)
				{
					// Things that could go wrong in which an exception will be thrown:
					// 1) file is not a valid DICOM image
					// 2) file is a valid DICOM image, but its image parameters are invalid
					// 3) file is a valid DICOM image, but we can't handle this type of DICOM image

					_failed++;
					Platform.Log(LogLevel.Error, e);
				}

				_total++;
			}

			#region IDisposable Members

			public void Dispose()
			{
				GC.SuppressFinalize(this);
			}

			#endregion
		}
	}
}