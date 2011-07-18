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
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Actions;
using ClearCanvas.ImageViewer.BaseTools;
using ClearCanvas.Desktop.Tools;
using System.Collections;
using ClearCanvas.Common.Utilities;

#pragma warning disable 0419,1574,1587,1591

namespace ClearCanvas.ImageViewer.Clipboard
{
	[ExtensionPoint]
	public sealed class ClipboardToolbarToolExtensionPoint : ExtensionPoint<ITool>
	{
	}

	[MenuAction("show", "global-menus/MenuView/MenuShowClipboard", "Show")]
	[DropDownButtonAction("show", "global-toolbars/ToolbarStandard/ToolbarShowClipboard", "Show", "ClipboardMenuModel")]
	[Tooltip("show", "TooltipShowClipboard")]
	[IconSet("show", IconScheme.Colour, "Icons.ShowClipboardToolSmall.png", "Icons.ShowClipboardToolMedium.png", "Icons.ShowClipboardToolLarge.png")]

	[ExtensionOf(typeof(ImageViewerToolExtensionPoint))]
	public class ShowClipboardTool : ImageViewerTool
	{
		private class ToolContextProxy : IImageViewerToolContext
		{
			private readonly IImageViewerToolContext _realContext;

			public ToolContextProxy(IImageViewerToolContext realContext)
			{ 
				_realContext = realContext;
			}

			#region IImageViewerToolContext Members

			public IImageViewer Viewer
			{
				get { return _realContext.Viewer; }
			}

			public IDesktopWindow DesktopWindow
			{
				get { return _realContext.DesktopWindow; }
			}

			#endregion
		}

		public const string ClipboardToolbarDropdownSite = "clipboard-toolbar-dropdown";
		private static IShelf _shelf;
		private ToolSet _toolSet;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <remarks>
		/// A no-args constructor is required by the framework.  Do not remove.
		/// </remarks>
		public ShowClipboardTool()
		{
		}

		public override void Initialize()
		{
			base.Initialize();

			object[] tools;

			try
			{

				tools = new ClipboardToolbarToolExtensionPoint().CreateExtensions();
			}
			catch(NotSupportedException)
			{
				tools = new object[0];
				Platform.Log(LogLevel.Debug, "No clipboard toolbar drop-down items found.");
			}
			catch (Exception e)
			{
				tools = new object[0]; 
				Platform.Log(LogLevel.Debug, "Failed to create clipboard toolbar drop-down items.", e);
			}

			_toolSet = new ToolSet(tools, new ToolContextProxy(Context));
		}

		protected override void Dispose(bool disposing)
		{
			_toolSet.Dispose();
			base.Dispose(disposing);
		}

		public ActionModelNode ClipboardMenuModel
		{
			get
			{
				return ActionModelRoot.CreateModel(typeof(ShowClipboardTool).FullName, ClipboardToolbarDropdownSite, _toolSet.Actions);
			}	
		}

		public void Show()
		{
			if (_shelf == null)
			{
				ClipboardComponent clipboardComponent = new ClipboardComponent();

				_shelf = ApplicationComponent.LaunchAsShelf(
					this.Context.DesktopWindow,
					clipboardComponent,
					SR.TitleClipboard,
					"Clipboard",
					ShelfDisplayHint.DockLeft | ShelfDisplayHint.DockAutoHide);

				_shelf.Closed += OnShelfClosed;
			}
			else
			{
				_shelf.Show();
			}
		}

		private static void OnShelfClosed(object sender, ClosedEventArgs e)
		{
			_shelf.Closed -= OnShelfClosed;
			_shelf = null;
		}
	}
}
