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
using ClearCanvas.Desktop.Actions;
using ClearCanvas.Desktop.Tools;
using ClearCanvas.ImageViewer.BaseTools;
using ClearCanvas.ImageViewer.Layout.Basic;

namespace ClearCanvas.ImageViewer.Web.Layout
{
     
    [MenuAction("show", "global-menus/MenuTools/MenuStandard/MenuLayoutManager", "Show")]
    [DropDownAction("show", "global-toolbars/ToolbarStandard/ToolbarLayoutManager", "LayoutDropDownMenuModel")]
    [IconSet("show", IconScheme.Colour, "Icons.LayoutToolSmall.png", "Icons.LayoutToolMedium.png", "Icons.LayoutToolLarge.png")]
	[Tooltip("show", "Layout Manager")]
	[GroupHint("show", "Application.Workspace.Layout.Basic")]
	[EnabledStateObserver("show", "Enabled", "EnabledChanged")]

	/// <summary>
	/// This tool is a WebViewer specific DropDownAction tool for changing the layout.  The tool used by the 
	/// ImageViewer is a DropDownButtonAction, which wasn't needed in this case.
	/// </summary>
	[ExtensionOf(typeof (ImageViewerToolExtensionPoint))]
	public class WebLayoutTool : Tool<IImageViewerToolContext>
	{
		private ActionModelRoot _actionModel;
		private bool _enabled;

        public bool Enabled
		{
			get { return _enabled; }
			set
			{
				if (value == _enabled)
					return;

				_enabled = value;
				EventsHelper.Fire(EnabledChanged, this, EventArgs.Empty);
			}
		}

        public ActionModelNode Menu { get; set; }

		public event EventHandler EnabledChanged;

		/// <summary>
		/// Gets the action model for the layout drop down menu.
		/// </summary>
		public ActionModelNode LayoutDropDownMenuModel
		{
			get
			{
				if (_actionModel == null)
				{
					ActionModelRoot root = new ActionModelRoot();
                    ResourceResolver resolver = new ResourceResolver(GetType().Assembly);

					ActionPath pathBoxes = new ActionPath("root/ToolbarLayoutBoxesChooser", resolver);
					LayoutChangerAction actionBoxes = new LayoutChangerAction("chooseBoxLayout",
					                                                          4,
					                                                          8,
					                                                          SetImageBoxLayout, pathBoxes, resolver);
					root.InsertAction(actionBoxes);

					ActionPath pathTiles = new ActionPath("root/ToolbarLayoutTilesChooser", resolver);
					LayoutChangerAction actionTiles = new LayoutChangerAction("chooseTileLayout",
					                                                          4,
					                                                          4,
					                                                          SetTileLayout, pathTiles, resolver);
					root.InsertAction(actionTiles);

					_actionModel = root;
				}

				return _actionModel;
			}
		}

		/// <summary>
		/// Sets the layout of the current imageviewer to the specified number of imageboxes.
		/// </summary>
		/// <param name="rows">The number of rows to show.</param>
		/// <param name="columns">The number of columns to show.</param>
		public void SetImageBoxLayout(int rows, int columns)
		{
			LayoutComponent.SetImageBoxLayout(Context.Viewer, rows, columns);
		}

		/// <summary>
		/// Sets the layout of the current imageviewer to the specified number of tiles.
		/// </summary>
		/// <param name="rows">The number of rows to show.</param>
		/// <param name="columns">The number of columns to show.</param>
		public void SetTileLayout(int rows, int columns)
		{
			LayoutComponent.SetTileLayout(Context.Viewer, rows, columns);
		}

		public override void Initialize()
		{
			base.Initialize();
			Context.Viewer.PhysicalWorkspace.LockedChanged += OnLockedChanged;
			Enabled = !Context.Viewer.PhysicalWorkspace.Locked;
		}

		protected override void Dispose(bool disposing)
		{
			Context.Viewer.PhysicalWorkspace.LockedChanged -= OnLockedChanged;
			base.Dispose(disposing);
		}
		
		private void OnLockedChanged(object sender, EventArgs e)
		{
			Enabled = !Context.Viewer.PhysicalWorkspace.Locked;
		}		

        public void Show()
        { }
	}
}
