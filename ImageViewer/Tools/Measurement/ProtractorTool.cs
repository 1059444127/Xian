#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using ClearCanvas.Common;
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Actions;
using ClearCanvas.ImageViewer.BaseTools;
using ClearCanvas.ImageViewer.Graphics;
using ClearCanvas.ImageViewer.InteractiveGraphics;
using ClearCanvas.ImageViewer.RoiGraphics;

namespace ClearCanvas.ImageViewer.Tools.Measurement
{
	[MenuAction("activate", "global-menus/MenuTools/MenuMeasurement/MenuProtractor", "Select", Flags = ClickActionFlags.CheckAction)]
	[ButtonAction("activate", "global-toolbars/ToolbarMeasurement/ToolbarProtractor", "Select", Flags = ClickActionFlags.CheckAction)]
	[CheckedStateObserver("activate", "Active", "ActivationChanged")]
	[TooltipValueObserver("activate", "Tooltip", "TooltipChanged")]
	[MouseButtonIconSet("activate", IconScheme.Colour, "Icons.ProtractorToolSmall.png", "Icons.ProtractorToolMedium.png", "Icons.ProtractorToolLarge.png")]
	[GroupHint("activate", "Tools.Image.Measurement.Angle")]
	[MouseToolButton(XMouseButtons.Left, false)]
	[ExtensionOf(typeof (ImageViewerToolExtensionPoint))]
	public class ProtractorTool : MeasurementTool
	{
		public ProtractorTool()
			: base(SR.TooltipProtractor) {}

		protected override string CreationCommandName
		{
			get { return SR.CommandCreateProtractor; }
		}

		protected override string RoiNameFormat
		{
			get { return SR.FormatProtractorName; }
		}

		protected override InteractiveGraphicBuilder CreateGraphicBuilder(IGraphic graphic)
		{
			return new InteractivePolylineGraphicBuilder(3, (IPointsGraphic) graphic);
		}

		protected override IGraphic CreateGraphic()
		{
			return new VerticesControlGraphic(new MoveControlGraphic(new ProtractorGraphic()));
		}

		protected override IAnnotationCalloutLocationStrategy CreateCalloutLocationStrategy()
		{
			return new ProtractorRoiCalloutLocationStrategy();
		}
	}
}