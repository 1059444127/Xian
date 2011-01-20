#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using ClearCanvas.Common.Utilities;
using ClearCanvas.Dicom.Iod;
using ClearCanvas.ImageViewer.Annotations;
using ClearCanvas.ImageViewer.Graphics;
using ClearCanvas.ImageViewer.Imaging;
using ClearCanvas.ImageViewer.StudyManagement;

namespace ClearCanvas.ImageViewer.AdvancedImaging.Fusion
{
	[Cloneable]
	public class FusionPresentationImage : BasicPresentationImage, IImageSopProvider, IColorMapProvider, ILayerOpacityProvider, IModalityLutProvider
	{
		private const string _fusionOverlayLayerName = "Fusion";

		[CloneIgnore]
		private IFrameReference _baseFrameReference;

		[CloneIgnore]
		private IFusionOverlayFrameDataReference _overlayFrameDataReference;

		[CloneIgnore]
		private FusionOverlayCompositeGraphic _fusionOverlayComposite;

		[CloneIgnore]
		private CompositeGraphic _fusionOverlayLayer;

		public FusionPresentationImage(Frame baseFrame, FusionOverlayFrameData overlayData)
			: this(baseFrame.CreateTransientReference(), overlayData.CreateTransientReference()) {}

		public FusionPresentationImage(IFrameReference baseFrame, IFusionOverlayFrameDataReference overlayFrameData)
			: base(Create(baseFrame),
			       baseFrame.Frame.NormalizedPixelSpacing.Column,
			       baseFrame.Frame.NormalizedPixelSpacing.Row,
			       baseFrame.Frame.PixelAspectRatio.Column,
			       baseFrame.Frame.PixelAspectRatio.Row)
		{
			_baseFrameReference = baseFrame;
			_overlayFrameDataReference = overlayFrameData;

			Initialize();
		}

		/// <summary>
		/// Cloning constructor.
		/// </summary>
		/// <param name="source">The source object from which to clone.</param>
		/// <param name="context">The cloning context object.</param>
		protected FusionPresentationImage(FusionPresentationImage source, ICloningContext context) : base(source, context)
		{
			context.CloneFields(source, this);

			_baseFrameReference = source._baseFrameReference.Clone();
			_overlayFrameDataReference = source._overlayFrameDataReference.Clone();
		}

		[OnCloneComplete]
		private void OnCloneComplete()
		{
			_fusionOverlayLayer = (CompositeGraphic) CollectionUtils.SelectFirst(base.CompositeImageGraphic.Graphics,
			                                                                     g => g is CompositeGraphic && g.Name == _fusionOverlayLayerName);

			if (_fusionOverlayLayer != null)
			{
				_fusionOverlayComposite = (FusionOverlayCompositeGraphic) CollectionUtils.SelectFirst(_fusionOverlayLayer.Graphics, g => g is FusionOverlayCompositeGraphic);
			}

			Initialize();
		}

		private void Initialize()
		{
			if (_fusionOverlayLayer == null)
			{
				_fusionOverlayLayer = new CompositeGraphic {Name = _fusionOverlayLayerName};

				// insert the fusion graphics layer right after the base image graphic (both contain domain-level graphics)
				base.CompositeImageGraphic.Graphics.Insert(base.CompositeImageGraphic.Graphics.IndexOf(this.ImageGraphic) + 1, _fusionOverlayLayer);
			}

			if (_fusionOverlayComposite == null)
			{
				_fusionOverlayComposite = new FusionOverlayCompositeGraphic(_overlayFrameDataReference.FusionOverlayFrameData);
				_fusionOverlayLayer.Graphics.Add(_fusionOverlayComposite);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_fusionOverlayLayer = null;
				_fusionOverlayComposite = null;

				if (_baseFrameReference != null)
				{
					_baseFrameReference.Dispose();
					_baseFrameReference = null;
				}

				if (_overlayFrameDataReference != null)
				{
					_overlayFrameDataReference.Dispose();
					_overlayFrameDataReference = null;
				}
			}

			base.Dispose(disposing);
		}

		public FusionOverlayFrameData OverlayFrameData
		{
			get { return _overlayFrameDataReference.FusionOverlayFrameData; }
		}

		/// <summary>
		/// Gets the <see cref="GrayscaleImageGraphic"/> associated with this <see cref="GrayscalePresentationImage"/>.
		/// </summary>
		public new GrayscaleImageGraphic ImageGraphic
		{
			get { return (GrayscaleImageGraphic) base.ImageGraphic; }
		}

		public IVoiLutManager BaseVoiLutManager
		{
			get { return ImageGraphic.VoiLutManager; }
		}

		public IVoiLutManager OverlayVoiLutManager
		{
			get { return _fusionOverlayComposite.VoiLutManager; }
		}

		public override IPresentationImage CreateFreshCopy()
		{
			return new FusionPresentationImage(this.Frame, _fusionOverlayComposite.OverlayFrameData) {PresentationState = this.PresentationState};
		}

		/// <summary>
		/// Creates the <see cref="IAnnotationLayout"/> for this image.
		/// </summary>
		/// <returns></returns>
		protected override IAnnotationLayout CreateAnnotationLayout()
		{
			return AnnotationLayoutFactory.CreateLayout("AdvancedImaging.Fusion.PET-CT");
		}

		public override string ToString()
		{
			return Frame.ParentImageSop.InstanceNumber.ToString();
		}

		#region VOI LUT Synchronization Support

		private object _lastBaseVoiLutManagerMemento;
		private object _lastOverlayVoiLutManagerMemento;

		internal bool SetBaseVoiLutManagerMemento(object memento)
		{
			if (!Equals(memento, _lastBaseVoiLutManagerMemento))
			{
				ImageGraphic.VoiLutManager.SetMemento(_lastBaseVoiLutManagerMemento = memento);
				return true;
			}
			return false;
		}

		internal bool SetOverlayVoiLutManagerMemento(object memento)
		{
			if (!Equals(memento, _lastOverlayVoiLutManagerMemento))
			{
				_fusionOverlayComposite.VoiLutManager.SetMemento(_lastOverlayVoiLutManagerMemento = memento);
				return true;
			}
			return false;
		}

		#endregion

		#region IImageSopProvider Members

		/// <summary>
		/// Gets this presentation image's associated <see cref="ImageSop"/>.
		/// </summary>
		/// <remarks>
		/// Use <see cref="ImageSop"/> to access DICOM tags.
		/// </remarks>
		public ImageSop ImageSop
		{
			get { return Frame.ParentImageSop; }
		}

		/// <summary>
		/// Gets this presentation image's associated <see cref="Frame"/>.
		/// </summary>
		public Frame Frame
		{
			get { return _baseFrameReference.Frame; }
		}

		#endregion

		#region ISopProvider Members

		Sop ISopProvider.Sop
		{
			get { return ImageSop; }
		}

		#endregion

		#region IColorMapProvider Members

		public IColorMapManager ColorMapManager
		{
			get { return _fusionOverlayComposite.ColorMapManager; }
		}

		#endregion

		#region ILayerOpacityProvider Members

		public ILayerOpacityManager LayerOpacityManager
		{
			get { return _fusionOverlayComposite.LayerOpacityManager; }
		}

		#endregion

		#region IModalityLutProvider Members

		public IComposableLut ModalityLut
		{
			get { return ImageGraphic.ModalityLut; }
		}

		#endregion

		#region Private Helpers

		//TODO (CR Sept 2010): Name - Create implies it will create an instance of FusionPresentationImage.
		private static GrayscaleImageGraphic Create(IImageSopProvider frameReference)
		{
			return new GrayscaleImageGraphic(
				frameReference.Frame.Rows,
				frameReference.Frame.Columns,
				frameReference.Frame.BitsAllocated,
				frameReference.Frame.BitsStored,
				frameReference.Frame.HighBit,
				frameReference.Frame.PixelRepresentation != 0 ? true : false,
				frameReference.Frame.PhotometricInterpretation == PhotometricInterpretation.Monochrome1 ? true : false,
				frameReference.Frame.RescaleSlope,
				frameReference.Frame.RescaleIntercept,
				frameReference.Frame.GetNormalizedPixelData);
		}

		#endregion
	}
}