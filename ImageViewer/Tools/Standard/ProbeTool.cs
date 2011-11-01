#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Drawing;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Actions;
using ClearCanvas.Desktop.Tools;
using ClearCanvas.ImageViewer.BaseTools;
using ClearCanvas.ImageViewer.Graphics;
using ClearCanvas.ImageViewer.InputManagement;
using ClearCanvas.ImageViewer.StudyManagement;
using System.ComponentModel;

namespace ClearCanvas.ImageViewer.Tools.Standard
{
	[ExtensionPoint]
	public sealed class ProbeToolDropDownToolExtensionPoint : ExtensionPoint<ITool> { }

	[MenuAction("activate", "imageviewer-contextmenu/MenuProbe", "Select", Flags = ClickActionFlags.CheckAction, InitiallyAvailable = false)]
	[MenuAction("activate", "global-menus/MenuTools/MenuStandard/MenuProbe", "Select", Flags = ClickActionFlags.CheckAction)]
	[DropDownButtonAction("activate", "global-toolbars/ToolbarStandard/ToolbarProbe", "Select", "DropDownMenuModel", Flags = ClickActionFlags.CheckAction, KeyStroke = XKeys.B)]
	[TooltipValueObserver("activate", "Tooltip", "TooltipChanged")]
	[MouseButtonIconSet("activate", IconScheme.Colour, "Icons.ProbeToolSmall.png", "Icons.ProbeToolMedium.png", "Icons.ProbeToolLarge.png")]
	[CheckedStateObserver("activate", "Active", "ActivationChanged")]
    [GroupHint("activate", "Tools.Image.Inspection.Probe")]

	[MouseToolButton(XMouseButtons.Left, false)]

	#region Tool Settings Actions

	[MenuAction("showRawPix", "probetool-dropdown/MenuShowRawPixelValue", "ToggleShowRawPix")]
	[CheckedStateObserver("showRawPix", "ShowRawPix", "ShowRawPixChanged")]
	[GroupHint("showRawPix", "Tools.Image.Inspection.Probe.Modality.CT.ShowPixel")]

	[MenuAction("showVoiLut", "probetool-dropdown/MenuShowVoiPixelValue", "ToggleShowVoiLut")]
	[CheckedStateObserver("showVoiLut", "ShowVoiLut", "ShowVoiLutChanged")]
    [GroupHint("showVoiLut", "Tools.Image.Inspection.Probe.General.ShowVoiLut")]

	#endregion

	[ExtensionOf(typeof(ImageViewerToolExtensionPoint))]
	public class ProbeTool : MouseImageViewerTool
	{
		private Tile _selectedTile;
		private ImageGraphic _selectedImageGraphic;
		private Frame _selectedFrame;
		private ActionModelNode _actionModel;

		/// <summary>
		/// Default constructor.  A no-args constructor is required by the
		/// framework.  Do not remove.
		/// </summary>
		public ProbeTool()
			: base(SR.TooltipProbe)
		{
			this.CursorToken = new CursorToken("ProbeCursor.png", this.GetType().Assembly);
			Behaviour |= MouseButtonHandlerBehaviour.ConstrainToTile;
		}

		public override event EventHandler TooltipChanged
		{
			add { base.TooltipChanged += value; }
			remove { base.TooltipChanged -= value; }
		}

		public ActionModelNode DropDownMenuModel
		{
			get
			{
				if (_actionModel == null)
				{
					_actionModel = ActionModelRoot.CreateModel("ClearCanvas.ImageViewer.Tools.Standard", "probetool-dropdown", this.Actions);
				}

				return _actionModel;
			}
		}

		public override bool Start(IMouseInformation mouseInformation)
		{
			if (this.SelectedImageGraphicProvider == null)
				return false;

			_selectedTile = mouseInformation.Tile as Tile;
			_selectedTile.InformationBox = new InformationBox();
			_selectedImageGraphic = this.SelectedImageGraphicProvider.ImageGraphic;
			_selectedFrame = ((IImageSopProvider) SelectedPresentationImage).Frame;

			Probe(mouseInformation.Location);

			return true;
		}

		/// <summary>
		/// Called by the framework as the mouse moves while the assigned mouse button
		/// is pressed.
		/// </summary>
		/// <param name="e">Mouse event args</param>
		/// <returns>True if the event was handled, false otherwise</returns>
		public override bool Track(IMouseInformation mouseInformation)
		{
			if (_selectedTile == null || _selectedImageGraphic == null)
				return false;

			Probe(mouseInformation.Location);

			return true;
		}

		/// <summary>
		/// Called by the framework when the assigned mouse button is released.
		/// </summary>
		/// <param name="e">Mouse event args</param>
		/// <returns>True if the event was handled, false otherwise</returns>
		public override bool Stop(IMouseInformation mouseInformation)
		{
			Cancel();
			return false;
		}

		public override void Cancel()
		{
			if (_selectedTile == null || _selectedImageGraphic == null)
				return;

			_selectedImageGraphic = null;

			_selectedTile.InformationBox.Visible = false;
			_selectedTile.InformationBox = null;
			_selectedTile = null;
		}

		private void Probe(Point destinationPoint)
		{
			Point sourcePointRounded = Point.Truncate(_selectedImageGraphic.SpatialTransform.ConvertToSource(destinationPoint));

			ToolSettings settings = ToolSettings.Default;
			bool showPixelValue = settings.ShowRawPixelValue;
			bool showModalityValue = true;
			bool showVoiValue = settings.ShowVOIPixelValue;

			string probeString = String.Format(SR.FormatProbeInfo, SR.LabelLocation, string.Format(SR.FormatCoordinates, SR.LabelNotApplicable, SR.LabelNotApplicable));
			string pixelValueString = String.Format(SR.FormatProbeInfo, SR.LabelRawPixel, SR.LabelNotApplicable);
			string modalityLutString = String.Format(SR.FormatProbeInfo, SR.LabelModalityLut, SR.LabelNotApplicable);
			string voiLutString = String.Format(SR.FormatProbeInfo, SR.LabelVOILut, SR.LabelNotApplicable);

			try
			{
				if (_selectedImageGraphic.HitTest(destinationPoint))
				{
					probeString = String.Format(SR.FormatProbeInfo, SR.LabelLocation, string.Format(SR.FormatCoordinates, sourcePointRounded.X, sourcePointRounded.Y));

					if (_selectedImageGraphic is GrayscaleImageGraphic)
					{
						GrayscaleImageGraphic image = _selectedImageGraphic as GrayscaleImageGraphic;

						int pixelValue = 0;

						GetPixelValue(image, sourcePointRounded, ref pixelValue, ref pixelValueString);
						GetModalityLutValue(image, pixelValue, ref modalityLutString);
						GetVoiLutValue(image, pixelValue, ref voiLutString);
					}
					else if (_selectedImageGraphic is ColorImageGraphic)
					{
						showModalityValue = false;
						showVoiValue = false;

						ColorImageGraphic image = _selectedImageGraphic as ColorImageGraphic;
						Color color = image.PixelData.GetPixelAsColor(sourcePointRounded.X, sourcePointRounded.Y);
						string rgbFormatted = String.Format(SR.FormatRGB, color.R, color.G, color.B);
						pixelValueString = String.Format(SR.FormatProbeInfo, SR.LabelRawPixel, rgbFormatted);
					}
					else
					{
						showPixelValue = false;
						showModalityValue = false;
						showVoiValue = false;
					}
				}

				if (showPixelValue)
					probeString += Environment.NewLine + pixelValueString;
				if (showModalityValue)
					probeString += Environment.NewLine + modalityLutString;
				if (showVoiValue)
					probeString += Environment.NewLine + voiLutString;
			}
			catch (Exception e)
			{
				Platform.Log(LogLevel.Error, e);
				probeString = SR.MessageProbeToolError;
			}

			_selectedTile.InformationBox.Update(probeString, destinationPoint);
		}

		private void GetPixelValue(
			GrayscaleImageGraphic grayscaleImage,
			Point sourcePointRounded,
			ref int pixelValue,
			ref string pixelValueString)
		{
			pixelValue = grayscaleImage.PixelData.GetPixel(sourcePointRounded.X, sourcePointRounded.Y);
			pixelValueString = String.Format(SR.FormatProbeInfo, SR.LabelRawPixel, pixelValue);
		}

		private void GetModalityLutValue(
			GrayscaleImageGraphic grayscaleImage,
			int pixelValue,
			ref string modalityLutString)
		{
			if (grayscaleImage.ModalityLut != null)
			{
				var modalityLutValue = grayscaleImage.ModalityLut[pixelValue];
				
				var modalityLutValueDisplay = modalityLutValue.ToString(grayscaleImage.NormalizationLut != null ? @"G3" : @"F1");
				if (_selectedFrame != null)
				{
					var units = _selectedFrame.RescaleUnits;
					if (units != null && units != RescaleUnits.None)
						modalityLutValueDisplay = string.Format(SR.FormatValueUnits, modalityLutValueDisplay, units.Label);
				}

				modalityLutString = String.Format(SR.FormatProbeInfo, SR.LabelModalityLut, modalityLutValueDisplay);
			}
		}

		private void GetVoiLutValue(
			GrayscaleImageGraphic grayscaleImage,
			int pixelValue,
			ref string voiLutString)
		{
			if (grayscaleImage.VoiLut != null)
			{
				var voiLutValue = grayscaleImage.OutputLut[pixelValue];
				voiLutString = String.Format(SR.FormatProbeInfo, SR.LabelVOILut, voiLutValue);
			}
		}

		#region Probe Tool Settings

		private event EventHandler _showRawPixChanged;
		private event EventHandler _showVoiLutChanged;
		private ToolSettings _settings;

		public override void Initialize()
		{
			base.Initialize();

			_settings = ToolSettings.Default;
			_settings.PropertyChanged += OnPropertyChanged;
		}

		protected override void Dispose(bool disposing)
		{

			_settings.PropertyChanged -= OnPropertyChanged;
			_settings.Save();
			_settings = null;

			base.Dispose(disposing);
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "ShowRawPixelValue":
					EventsHelper.Fire(_showRawPixChanged, this, EventArgs.Empty);
					break;
				case "ShowVOIPixelValue":
					EventsHelper.Fire(_showVoiLutChanged, this, EventArgs.Empty);
					break;
			}
		}

		public event EventHandler ShowRawPixChanged
		{
			add { _showRawPixChanged += value; }
			remove { _showRawPixChanged -= value; }
		}

		public event EventHandler ShowVoiLutChanged
		{
			add { _showVoiLutChanged += value; }
			remove { _showVoiLutChanged -= value; }
		}

		public bool ShowRawPix
		{
			get
			{
				try
				{
					return _settings.ShowRawPixelValue;
				}
				catch
				{
					return false;
				}
			}
			set
			{
				_settings.ShowRawPixelValue = value;
			}
		}

		public bool ShowVoiLut
		{
			get
			{
				try
				{
					return _settings.ShowVOIPixelValue;
				}
				catch
				{
					return false;
				}
			}
			set
			{
				_settings.ShowVOIPixelValue = value;
			}
		}

		public void ToggleShowRawPix()
		{
			this.ShowRawPix = !this.ShowRawPix;
		}

		public void ToggleShowVoiLut()
		{
			this.ShowVoiLut = !this.ShowVoiLut;
		}

		#endregion
	}
}
