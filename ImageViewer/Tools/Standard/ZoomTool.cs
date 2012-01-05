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
using ClearCanvas.Common.Utilities;
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Actions;
using ClearCanvas.ImageViewer.InputManagement;
using ClearCanvas.ImageViewer.BaseTools;
using ClearCanvas.ImageViewer.Graphics;
using ClearCanvas.ImageViewer.Tools.Standard.Configuration;

namespace ClearCanvas.ImageViewer.Tools.Standard
{
	[MenuAction("activate", "imageviewer-contextmenu/MenuZoom", "Select", Flags = ClickActionFlags.CheckAction)]
	[MenuAction("activate", "global-menus/MenuTools/MenuStandard/MenuZoom", "Select", Flags = ClickActionFlags.CheckAction)]
	[DropDownButtonAction("activate", "global-toolbars/ToolbarStandard/ToolbarZoom", "Select", "ZoomDropDownMenuModel", Flags = ClickActionFlags.CheckAction, KeyStroke = XKeys.Z)]
	[CheckedStateObserver("activate", "Active", "ActivationChanged")]
	[TooltipValueObserver("activate", "Tooltip", "TooltipChanged")]
	[MouseButtonIconSet("activate", "Icons.ZoomToolSmall.png", "Icons.ZoomToolMedium.png", "Icons.ZoomToolLarge.png")]
	[GroupHint("activate", "Tools.Image.Manipulation.Zoom")]

	[KeyboardAction("zoomin", "imageviewer-keyboard/ToolsStandardZoom/ZoomIn", "ZoomIn", KeyStroke = XKeys.OemPeriod)]
	[KeyboardAction("zoomout", "imageviewer-keyboard/ToolsStandardZoom/ZoomOut", "ZoomOut", KeyStroke = XKeys.OemComma)]

	[MouseWheelHandler(ModifierFlags.Control)]
	[MouseToolButton(XMouseButtons.Right, false)]

	[ExtensionOf(typeof(ImageViewerToolExtensionPoint))]
	public class ZoomTool : MouseImageViewerTool
	{
		internal static readonly float DefaultMinimumZoom = 0.25F;
		internal static readonly float DefaultMaximumZoom = 64F;

		private readonly ImageSpatialTransformImageOperation _operation; 
		private MemorableUndoableCommand _memorableCommand;
		private ImageOperationApplicator _applicator;
		private ToolModalityBehaviorHelper _toolBehavior;

		public ZoomTool()
			: base(SR.TooltipZoom)
		{
			this.CursorToken = new CursorToken("Icons.ZoomToolSmall.png", this.GetType().Assembly);
			_operation = new ImageSpatialTransformImageOperation(Apply);
		}

		public override void Initialize()
		{
			base.Initialize();

			_toolBehavior = new ToolModalityBehaviorHelper(ImageViewer);
		}

		public override event EventHandler TooltipChanged
		{
			add { base.TooltipChanged += value; }
			remove { base.TooltipChanged -= value; }
		}

		public ActionModelNode ZoomDropDownMenuModel
		{
			get
			{
				SimpleActionModel actionModel = new SimpleActionModel(new ApplicationThemeResourceResolver(this.GetType().Assembly));

				actionModel.AddAction("fit", SR.LabelZoomFit, null, SR.LabelZoomFit, delegate { SetScale(0); });
				AddFixedZoomAction(actionModel, 1);
				AddFixedZoomAction(actionModel, 2);
				AddFixedZoomAction(actionModel, 4);
				AddFixedZoomAction(actionModel, 8);

				return actionModel;
			}	
		}

		private ImageSpatialTransform GetSelectedImageTransform()
		{
			return _operation.GetOriginator(this.SelectedPresentationImage) as ImageSpatialTransform;
		}

		private bool CanZoom()
		{
			return GetSelectedImageTransform() != null;
		}

		private void AddFixedZoomAction(SimpleActionModel actionModel, int scale)
		{
			string label = String.Format(SR.FormatLabelZoomFixed, scale);
			actionModel.AddAction("fixedzoom" + label, label, null, label, delegate { SetScale(scale); });
		}

		private void CaptureBeginState()
		{
			if (!CanZoom())
				return;

			ImageSpatialTransform originator = GetSelectedImageTransform();
			_applicator = new ImageOperationApplicator(this.SelectedPresentationImage, _operation);
			_memorableCommand = new MemorableUndoableCommand(originator);
			_memorableCommand.BeginState = originator.CreateMemento();
		}

		private void CaptureEndState()
		{
			if (!CanZoom() || _memorableCommand == null)
				return;

			_memorableCommand.EndState = GetSelectedImageTransform().CreateMemento();
			UndoableCommand applicatorCommand = _toolBehavior.Behavior.SelectedImageZoomTool ? null : _applicator.ApplyToLinkedImages();
			DrawableUndoableCommand historyCommand = new DrawableUndoableCommand(this.SelectedPresentationImage);

			if (!_memorableCommand.EndState.Equals(_memorableCommand.BeginState))
				historyCommand.Enqueue(_memorableCommand);
			if (applicatorCommand != null)
				historyCommand.Enqueue(applicatorCommand);

			if (historyCommand.Count > 0)
			{
				historyCommand.Name = SR.CommandZoom;
				this.Context.Viewer.CommandHistory.AddCommand(historyCommand);
			}

			_memorableCommand = null;
		}
		
		private void ZoomIn()
		{
			if (this.SelectedPresentationImage == null)
				return;

			CaptureBeginState();

			float increment = 0.1F * this.SelectedSpatialTransformProvider.SpatialTransform.Scale;
			IncrementScale(increment);

			CaptureEndState();
		}

		private void ZoomOut()
		{
			if (this.SelectedPresentationImage == null)
				return;

			CaptureBeginState();

			float increment = -0.1F * this.SelectedSpatialTransformProvider.SpatialTransform.Scale;
			IncrementScale(increment);

			CaptureEndState();
		}

		private void SetScale(float scale)
		{
			if (this.SelectedPresentationImage == null)
				return;

			CaptureBeginState();

			if (!CanZoom())
				return;

			IImageSpatialTransform transform = GetSelectedImageTransform();
			if (scale <= 0)
			{
				transform.ScaleToFit = true;
			}
			else
			{
				transform.ScaleToFit = false;
				transform.Scale = scale;
			}

			CaptureEndState();

			this.SelectedSpatialTransformProvider.Draw();
		}
		
		private void IncrementScale(float scaleIncrement)
		{
			if (!CanZoom())
				return;

			IImageSpatialTransform transform = GetSelectedImageTransform();

			float currentScale = transform.Scale;

			// Use the 'to fit' scale value to calculate a minimum scale value.
			transform.ScaleToFit = true;
			float minimum = transform.Scale;
			
			// in the case of ridiculously small client rectangles, don't allow the scale to get any smaller than it is.
			if (base.SelectedPresentationImage.ClientRectangle.Width > 32 && 
				base.SelectedPresentationImage.ClientRectangle.Height > 32)
				minimum /= 2;

			// Set the minimum scale to 1/2 the size of the 'to fit' scale value, or the default, whichever is smaller.
			minimum = Math.Min(minimum, DefaultMinimumZoom);
			// When the scale is already smaller than the 'preferred' minimum, just don't let it get any smaller.
			minimum = Math.Min(minimum, currentScale);

			//make sure to reset the scale to what it was before we calculated the minimum.
			transform.ScaleToFit = false;
			transform.Scale = currentScale;

			float newScale = currentScale + scaleIncrement;
			if (newScale < minimum)
				newScale = minimum;
			else if (newScale > DefaultMaximumZoom)
				newScale = DefaultMaximumZoom;

			transform.Scale = newScale;

			this.SelectedSpatialTransformProvider.Draw();
		}

		public override bool Start(IMouseInformation mouseInformation)
		{
			if (this.SelectedPresentationImage == null)
				return false;

			base.Start(mouseInformation);

			CaptureBeginState();

			return true;
		}

		public override bool Track(IMouseInformation mouseInformation)
		{
			base.Track(mouseInformation);

			float increment = -base.DeltaY*0.025f;
			increment *= ToolSettings.Default.InvertedZoomToolOperation ? -1f : 1f;
			IncrementScale(increment);

			return true;
		}

		public override bool Stop(IMouseInformation mouseInformation)
		{
			if (this.SelectedPresentationImage == null)
				return false;

			base.Stop(mouseInformation);

			CaptureEndState();
			
			return false;
		}

		public override void Cancel()
		{
			if (this.SelectedPresentationImage == null)
				return;

			this.CaptureEndState();
		}

		public override void StartWheel()
		{
			if (this.SelectedPresentationImage == null)
				return;

			CaptureBeginState();
		}

		public override void StopWheel()
		{
			if (this.SelectedPresentationImage == null)
				return;

			CaptureEndState();
		}

		protected override void WheelBack()
		{
			if (this.SelectedPresentationImage == null)
				return;

			float increment = -0.1F * this.SelectedSpatialTransformProvider.SpatialTransform.Scale;
			increment *= ToolSettings.Default.InvertedZoomToolOperation ? -1f : 1f;
			IncrementScale(increment);
		}

		protected override void WheelForward()
		{
			if (this.SelectedPresentationImage == null)
				return;

			float increment = 0.1F * this.SelectedSpatialTransformProvider.SpatialTransform.Scale;
			increment *= ToolSettings.Default.InvertedZoomToolOperation ? -1f : 1f;
			IncrementScale(increment);
		}

		public void Apply(IPresentationImage image)
		{
			IImageSpatialTransform transform = (IImageSpatialTransform)_operation.GetOriginator(image);
			IImageSpatialTransform referenceTransform = (IImageSpatialTransform)this.SelectedSpatialTransformProvider.SpatialTransform;

			transform.Scale = referenceTransform.Scale;
			transform.ScaleToFit = referenceTransform.ScaleToFit;
		}
	}
}
