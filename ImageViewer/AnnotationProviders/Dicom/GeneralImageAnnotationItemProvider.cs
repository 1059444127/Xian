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
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Dicom;
using ClearCanvas.Dicom.Utilities;
using ClearCanvas.ImageViewer.Annotations;
using ClearCanvas.ImageViewer.Annotations.Dicom;
using ClearCanvas.ImageViewer.StudyManagement;
using ClearCanvas.Desktop;

namespace ClearCanvas.ImageViewer.AnnotationProviders.Dicom
{
	[ExtensionOf(typeof(AnnotationItemProviderExtensionPoint))]
	public class GeneralImageAnnotationItemProvider : AnnotationItemProvider
	{
		private readonly List<IAnnotationItem> _annotationItems;

		public GeneralImageAnnotationItemProvider()
			: base("AnnotationItemProviders.Dicom.GeneralImage", new AnnotationResourceResolver(typeof(GeneralImageAnnotationItemProvider).Assembly))
		{
			_annotationItems = new List<IAnnotationItem>();

			AnnotationResourceResolver resolver = new AnnotationResourceResolver(this);

			_annotationItems.Add
				(
					new DicomAnnotationItem<string>
					(
						"Dicom.GeneralImage.AcquisitionDate",
						resolver,
						delegate(Frame frame) { return frame.AcquisitionDate; },
						DicomDataFormatHelper.DateFormat
					)
				);


			_annotationItems.Add
				(
					new DicomAnnotationItem<string>
					(
						"Dicom.GeneralImage.AcquisitionTime",
						resolver,
						delegate(Frame frame) { return frame.AcquisitionTime; },
						delegate (string input)
							{
								if (String.IsNullOrEmpty(input))
									return String.Empty;

								DateTime time;
								if (!TimeParser.Parse(input, out time))
									return input;

								return time.ToString("HH:mm:ss.FFFFFF");
							}
					)
				);


			_annotationItems.Add
				(
					new DicomAnnotationItem<string>
					(
						"Dicom.GeneralImage.AcquisitionDateTime",
						resolver,
						delegate(Frame frame) { return frame.AcquisitionDateTime; },
						delegate(string input)
							{
								if (String.IsNullOrEmpty(input))
									return String.Empty;

								DateTime dateTime;
								if (!DateTimeParser.Parse(input, out dateTime))
									return input;

								return String.Format("{0} {1}", 
									dateTime.Date.ToString(Format.DateFormat),
									dateTime.ToString("HH:mm:ss.FFFFFF"));
							}
					)
				);

			_annotationItems.Add
				(
					new DicomAnnotationItem<string>
					(
						"Dicom.GeneralImage.AcquisitionNumber",
						resolver,
						delegate(Frame frame) { return frame.AcquisitionNumber.ToString(); },
						DicomDataFormatHelper.RawStringFormat
					)
				);

			_annotationItems.Add
				(
				new DicomAnnotationItem<string>
					(
						"Dicom.GeneralImage.ContentDate",
						resolver,
						FrameDataRetrieverFactory.GetStringRetriever(DicomTags.ContentDate),
						DicomDataFormatHelper.DateFormat
					)
				);

			_annotationItems.Add
				(
					new DicomAnnotationItem<string>
					(
						"Dicom.GeneralImage.ContentTime",
						resolver,
						FrameDataRetrieverFactory.GetStringRetriever(DicomTags.ContentTime),
						DicomDataFormatHelper.TimeFormat
					)
				);

			_annotationItems.Add
				(
					new DicomAnnotationItem<string>
					(
						"Dicom.GeneralImage.DerivationDescription",
						resolver,
						FrameDataRetrieverFactory.GetStringRetriever(DicomTags.DerivationDescription),
						DicomDataFormatHelper.RawStringFormat
					)
				);

			_annotationItems.Add
				(
					new DicomAnnotationItem<string>
					(
						"Dicom.GeneralImage.ImageComments",
						resolver,
						delegate(Frame frame) { return frame.ImageComments; },
						DicomDataFormatHelper.RawStringFormat
					)
				);

			_annotationItems.Add
				(
					new DicomAnnotationItem<string>
					(
						"Dicom.GeneralImage.ImagesInAcquisition",
						resolver,
						delegate(Frame frame) { return frame.ImagesInAcquisition.ToString(); },
						DicomDataFormatHelper.RawStringFormat
					)
				);

			_annotationItems.Add
				(
					new DicomAnnotationItem<string>
					(
						"Dicom.GeneralImage.ImageType",
						resolver,
						delegate(Frame frame) { return frame.ImageType; },
						DicomDataFormatHelper.RawStringFormat
					)
				);

			_annotationItems.Add(new InstanceNumberAnnotationItem());

			_annotationItems.Add
				(
					new DicomAnnotationItem<string>
					(
						"Dicom.GeneralImage.LossyImageCompression",
						resolver,
						delegate(Frame frame)
							{
								if (frame.LossyImageCompressionRatio.Length > 0)
								{
									var lossyRatios = StringUtilities.Combine(frame.LossyImageCompressionRatio, "/", "F1");
									if (!String.IsNullOrEmpty(lossyRatios))
										return String.Format("{0}({1})", SR.ValueLossy, lossyRatios);
								}

								if (!String.IsNullOrEmpty(frame.LossyImageCompression))
								{
									int lossyValue;
									if (Int32.TryParse(frame.LossyImageCompression, out lossyValue) && lossyValue != 0)
										return SR.ValueLossy;
								}

								return "";
							},
						DicomDataFormatHelper.RawStringFormat
					)
				);

			_annotationItems.Add
				(
					new DicomAnnotationItem<string>
					(
						"Dicom.GeneralImage.QualityControlImage",
						resolver,
						FrameDataRetrieverFactory.GetStringRetriever(DicomTags.QualityControlImage),
						DicomDataFormatHelper.BooleanFormatter
					)
				);

			_annotationItems.Add
				(
					new LateralityViewPositionAnnotationItem
					(
						"Dicom.GeneralImage.ViewPosition",
						false, true
					)
				);

			_annotationItems.Add
				(
					new LateralityViewPositionAnnotationItem
					(
						"Dicom.GeneralImage.ImageLaterality",
						true, false
					)
				);
			
			_annotationItems.Add
				(
					new LateralityViewPositionAnnotationItem
					(
						"Dicom.GeneralImage.Composite.LateralityViewPosition",
						true, true
					)
				);
		}

		public override IEnumerable<IAnnotationItem> GetAnnotationItems()
		{
			return _annotationItems;
		}
	}
}
