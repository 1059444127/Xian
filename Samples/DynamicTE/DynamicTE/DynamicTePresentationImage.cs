﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using ClearCanvas.Common;
using ClearCanvas.ImageViewer.Graphics;
using ClearCanvas.ImageViewer.StudyManagement;

namespace ClearCanvas.ImageViewer.Tools.ImageProcessing.DynamicTe
{
	public class DynamicTePresentationImage 
		: DicomGrayscalePresentationImage, 
		IDynamicTeProvider
	{
		#region Private fields

		private Frame _frame;
		private DynamicTe _dynamicTe;
		private ColorImageGraphic _probabilityOverlay;

		#endregion

		public DynamicTePresentationImage(
			Frame frame, 
			byte[] protonDensityMap,
			byte[] t2Map,
			byte[] probabilityMap)
			: base(frame)
		{
			Platform.CheckForNullReference(frame, "imageSop");

			_frame = frame;

			// TODO (Norman): DicomFilteredAnnotationLayoutProvider was made internal.  Either need to derive
			// this class from DicomGrayscalePresentationImage or create a layout provider.
			//this.AnnotationLayoutProvider = new DicomFilteredAnnotationLayoutProvider(this);

			AddProbabilityOverlay();
			_dynamicTe = new DynamicTe(
				this.ImageGraphic as GrayscaleImageGraphic, 
				protonDensityMap, 
				t2Map,
				_probabilityOverlay,
				probabilityMap);
		}

		public DynamicTe DynamicTe
		{
			get { return _dynamicTe; }
		}

		public bool ProbabilityOverlayVisible
		{
			get { return _probabilityOverlay.Visible; }
			set { _probabilityOverlay.Visible = value; }
		}

		public override IPresentationImage CreateFreshCopy()
		{
 			 return new DynamicTePresentationImage(
				 _frame, 
				 this.DynamicTe.ProtonDensityMap, 
				 this.DynamicTe.T2Map,
				 this.DynamicTe.ProbabilityMap);
		}


		private void AddProbabilityOverlay()
		{
			_probabilityOverlay = new ColorImageGraphic(_frame.Rows, _frame.Columns);
			this.OverlayGraphics.Add(_probabilityOverlay);
		}
	}
}
