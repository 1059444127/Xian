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
using ClearCanvas.ImageViewer.Graphics;
using ClearCanvas.ImageViewer.Imaging;

namespace ClearCanvas.ImageViewer.Tools.Standard
{
	[MenuAction("showHide", "imageviewer-contextmenu/MenuShowHideColorBar", "ShowHide", InitiallyAvailable = false)]
	[MenuAction("showHide", "global-menus/MenuTools/MenuStandard/MenuShowHideColorBar", "ShowHide")]
	[EnabledStateObserver("showHide", "Enabled", "EnabledChanged")]
	[Tooltip("showHide", "TooltipShowHideColorBar")]
	[GroupHint("showHide", "Tools.Image.Overlays.ColourBar.ShowHide")]
	[IconSet("showHide", IconScheme.Colour, "Icons.ColorBarToolSmall.png", "Icons.ColorBarToolMedium.png", "Icons.ColorBarToolLarge.png")]
	//
	[MenuAction("toggle", "overlays-dropdown/ToolbarColorBar", "ShowHide")]
	[EnabledStateObserver("toggle", "Enabled", "EnabledChanged")]
	[CheckedStateObserver("toggle", "Checked", "CheckedChanged")]
	[Tooltip("toggle", "TooltipColorBar")]
	[GroupHint("toggle", "Tools.Image.Overlays.ColourBar.ShowHide")]
	[IconSet("toggle", IconScheme.Colour, "Icons.ColorBarToolSmall.png", "Icons.ColorBarToolMedium.png", "Icons.ColorBarToolLarge.png")]
	//
	[ExtensionOf(typeof (ImageViewerToolExtensionPoint))]
	public class ColorBarTool : OverlayToolBase
	{
		private bool _enabled;

		public event EventHandler EnabledChanged;

		public ColorBarTool() 
			: base(false)
		{
		}

		public bool Enabled
		{
			get { return _enabled; }
			private set
			{
				if (_enabled != value)
				{
					_enabled = value;
					EventsHelper.Fire(EnabledChanged, this, EventArgs.Empty);
				}
			}
		}

		protected override void UpdateVisibility(IPresentationImage image, bool visible)
		{
			ColorBarCompositeGraphic graphic = GetCompositeScaleGraphic(image, visible);
			if (graphic != null)
				graphic.Visible = visible;
		}

		//TODO (CR Sept 2010): GetCompositeColorBarGraphic?
		private static ColorBarCompositeGraphic GetCompositeScaleGraphic(IPresentationImage image, bool createIfNull)
		{
			if (image is IColorMapProvider && image is IApplicationGraphicsProvider)
			{
				GraphicCollection applicationGraphics = ((IApplicationGraphicsProvider) image).ApplicationGraphics;
				ColorBarCompositeGraphic graphic = (ColorBarCompositeGraphic) CollectionUtils.SelectFirst(applicationGraphics, g => g is ColorBarCompositeGraphic);

				if (graphic == null && createIfNull)
					applicationGraphics.Add(graphic = new ColorBarCompositeGraphic());

				return graphic;
			}

			return null;
		}

		public override void Initialize()
		{
			base.Initialize();

			base.Context.Viewer.EventBroker.PresentationImageSelected += OnPresentationImageSelected;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				base.Context.Viewer.EventBroker.PresentationImageSelected -= OnPresentationImageSelected;
			}

			base.Dispose(disposing);
		}

		private void OnPresentationImageSelected(object sender, PresentationImageSelectedEventArgs e)
		{
			this.Enabled = e.SelectedPresentationImage is IColorMapProvider;
		}

		[Cloneable(false)]
		private class ColorBarCompositeGraphic : CompositeGraphic
		{
			[CloneIgnore]
			private ColorBarGraphic _colorBarGraphic;

			public ColorBarCompositeGraphic()
			{
				base.Graphics.Add(_colorBarGraphic = new ColorBarGraphic());
			}

			/// <summary>
			/// Cloning constructor.
			/// </summary>
			/// <param name="source">The source object from which to clone.</param>
			/// <param name="context">The cloning context object.</param>
			protected ColorBarCompositeGraphic(ColorBarCompositeGraphic source, ICloningContext context)
			{
				context.CloneFields(source, this);
			}

			[OnCloneComplete]
			private void OnCloneComplete()
			{
				_colorBarGraphic = (ColorBarGraphic) CollectionUtils.SelectFirst(base.Graphics, g => g is ColorBarGraphic);
			}

			public override void OnDrawing()
			{
				// ensure the color bar uses the same color map as the underlying image
				if (base.ParentPresentationImage is IColorMapProvider)
					_colorBarGraphic.ColorMapManager.SetMemento(((IColorMapProvider) base.ParentPresentationImage).ColorMapManager.CreateMemento());
				if (base.ParentPresentationImage != null)
				{
					_colorBarGraphic.CoordinateSystem = CoordinateSystem.Destination;
					try
					{
						_colorBarGraphic.Length = (int) (base.ParentPresentationImage.ClientRectangle.Height*0.3f);
						_colorBarGraphic.Location = new PointF(25, (base.ParentPresentationImage.ClientRectangle.Height - _colorBarGraphic.Length)/2f);
					}
					finally
					{
						_colorBarGraphic.ResetCoordinateSystem();
					}
				}

				base.OnDrawing();
			}
		}
	}
}