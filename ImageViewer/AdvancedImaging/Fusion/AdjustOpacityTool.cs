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
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Actions;
using ClearCanvas.ImageViewer.BaseTools;
using ClearCanvas.ImageViewer.InputManagement;

namespace ClearCanvas.ImageViewer.AdvancedImaging.Fusion
{
	[MenuAction("activate", "imageviewer-contextmenu/MenuAdjustOpacity", "Select", Flags = ClickActionFlags.CheckAction)]
	[MenuAction("activate", "global-menus/MenuTools/MenuStandard/MenuAdjustOpacity", "Select", Flags = ClickActionFlags.CheckAction)]
	[ButtonAction("activate", "global-toolbars/ToolbarStandard/ToolbarAdjustOpacity", "Select", Flags = ClickActionFlags.CheckAction)]
	[CheckedStateObserver("activate", "Active", "ActivationChanged")]
	[TooltipValueObserver("activate", "Tooltip", "TooltipChanged")]
	[GroupHint("activate", "Tools.Image.Manipulation.Layers.Opacity")]
	[MouseButtonIconSet("activate", IconScheme.Colour, "Icons.AdjustOpacityToolSmall.png", "Icons.AdjustOpacityToolMedium.png", "Icons.AdjustOpacityToolLarge.png")]
	[MouseToolButton(XMouseButtons.Right, false)]
	[ExtensionOf(typeof (ImageViewerToolExtensionPoint))]
	public class AdjustOpacityTool : MouseImageViewerTool
	{
		private readonly LayerOpacityOperation _operation;
		private MemorableUndoableCommand _memorableCommand;
		private ImageOperationApplicator _applicator;

		public AdjustOpacityTool()
			: base(SR.TooltipAdjustOpacity)
		{
			this.CursorToken = new CursorToken("Icons.AdjustOpacityToolSmall.png", this.GetType().Assembly);
			_operation = new LayerOpacityOperation(Apply);
		}

		private float CurrentSensitivity
		{
			get { return 0.001f; }
		}

		private ILayerOpacityProvider SelectedLayerOpacityProvider
		{
			get { return this.SelectedPresentationImage as ILayerOpacityProvider; }
		}

		public override event EventHandler TooltipChanged
		{
			add { base.TooltipChanged += value; }
			remove { base.TooltipChanged -= value; }
		}

		private ILayerOpacityManager GetSelectedLayerOpacityManager()
		{
			return _operation.GetOriginator(this.SelectedPresentationImage) as ILayerOpacityManager;
		}

		private bool CanAdjustAlpha()
		{
			ILayerOpacityManager manager = GetSelectedLayerOpacityManager();
			return manager != null && manager.Enabled;
		}

		protected override void OnPresentationImageSelected(object sender, PresentationImageSelectedEventArgs e)
		{
			base.OnPresentationImageSelected(sender, e);

			this.Enabled = (e.SelectedPresentationImage is ILayerOpacityProvider);
		}

		private void CaptureBeginState()
		{
			if (!CanAdjustAlpha())
				return;

			ILayerOpacityManager originator = GetSelectedLayerOpacityManager();
			_applicator = new ImageOperationApplicator(this.SelectedPresentationImage, _operation);
			_memorableCommand = new MemorableUndoableCommand(originator);
			_memorableCommand.BeginState = originator.CreateMemento();
		}

		private void CaptureEndState()
		{
			if (!CanAdjustAlpha() || _memorableCommand == null)
				return;

			if (this.SelectedLayerOpacityProvider != null)
			{
				_memorableCommand.EndState = GetSelectedLayerOpacityManager().CreateMemento();
				UndoableCommand applicatorCommand = _applicator.ApplyToLinkedImages();
				DrawableUndoableCommand historyCommand = new DrawableUndoableCommand(this.SelectedPresentationImage);

				if (!_memorableCommand.EndState.Equals(_memorableCommand.BeginState))
					historyCommand.Enqueue(_memorableCommand);
				if (applicatorCommand != null)
					historyCommand.Enqueue(applicatorCommand);

				if (historyCommand.Count > 0)
				{
					historyCommand.Name = SR.CommandAdjustOpacity;
					this.Context.Viewer.CommandHistory.AddCommand(historyCommand);
				}
			}
		}

		private void IncrementOpacity(float opacityIncrement)
		{
			if (!CanAdjustAlpha())
				return;

			ILayerOpacityManager manager = this.SelectedLayerOpacityProvider.LayerOpacityManager;

			manager.Opacity = Restrict(manager.Opacity + opacityIncrement, 0, 1);

			this.SelectedLayerOpacityProvider.Draw();
		}

		private void IncrementOpacityWithUndo(float opacityIncrement)
		{
			this.CaptureBeginState();
			this.IncrementOpacity(opacityIncrement);
			this.CaptureEndState();
		}

		private void Apply(IPresentationImage image)
		{
			ILayerOpacityManager manager = this.SelectedLayerOpacityProvider.LayerOpacityManager;

			ILayerOpacityProvider provider = ((ILayerOpacityProvider)image);

			ILayerOpacityManager lut = provider.LayerOpacityManager;
			lut.Opacity = Restrict(manager.Opacity, 0, 1);
		}

		public override bool Start(IMouseInformation mouseInformation)
		{
			if (this.SelectedLayerOpacityProvider == null)
				return false;

			base.Start(mouseInformation);

			CaptureBeginState();

			return true;
		}

		public override bool Track(IMouseInformation mouseInformation)
		{
			if (this.SelectedLayerOpacityProvider == null)
				return false;

			base.Track(mouseInformation);

			double sensitivity = this.CurrentSensitivity;
			double magnitude = Math.Sqrt(DeltaX*DeltaX + DeltaY*DeltaY);
			double angle = Math.Atan2(DeltaY, DeltaX);
			double sign = angle >= -Math.PI/4 && angle < 3*Math.PI/4 ? 1 : -1;
			IncrementOpacity((float) (sign*magnitude*sensitivity));

			return true;
		}

		public override bool Stop(IMouseInformation mouseInformation)
		{
			if (this.SelectedLayerOpacityProvider == null)
				return false;

			base.Stop(mouseInformation);

			this.CaptureEndState();

			return false;
		}

		public override void Cancel()
		{
			if (this.SelectedLayerOpacityProvider == null)
				return;

			this.CaptureEndState();
		}

		private static float Restrict(float value, float min, float max)
		{
			return Math.Max(Math.Min(value, max), min);
		}
	}
}