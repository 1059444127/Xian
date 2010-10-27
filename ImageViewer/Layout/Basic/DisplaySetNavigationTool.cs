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
using ClearCanvas.ImageViewer;
using ClearCanvas.ImageViewer.BaseTools;
using ClearCanvas.ImageViewer.StudyManagement;
using ClearCanvas.Desktop.Tools;

namespace ClearCanvas.ImageViewer.Layout.Basic
{
	[MenuAction("previous", "global-menus/MenuTools/MenuStandard/MenuPreviousDisplaySet", "PreviousDisplaySet")]
	[ButtonAction("previous", "global-toolbars/ToolbarStandard/ToolbarPreviousDisplaySet", "PreviousDisplaySet", KeyStroke = XKeys.E)]
	[Tooltip("previous", "TooltipPreviousDisplaySet")]
	[IconSet("previous", IconScheme.Colour, "Icons.PreviousDisplaySetToolSmall.png", "Icons.PreviousDisplaySetToolMedium.png", "Icons.PreviousDisplaySetToolLarge.png")]
	[GroupHint("previous", "Tools.Navigation.DisplaySets.Previous")]

	[MenuAction("next", "global-menus/MenuTools/MenuStandard/MenuNextDisplaySet", "NextDisplaySet")]
	[ButtonAction("next", "global-toolbars/ToolbarStandard/ToolbarNextDisplaySet", "NextDisplaySet", KeyStroke = XKeys.N)]
	[Tooltip("next", "TooltipNextDisplaySet")]
	[IconSet("next", IconScheme.Colour, "Icons.NextDisplaySetToolSmall.png", "Icons.NextDisplaySetToolMedium.png", "Icons.NextDisplaySetToolLarge.png")]
	[GroupHint("next", "Tools.Navigation.DisplaySets.Next")]

	[EnabledStateObserver("next", "Enabled", "EnabledChanged")]
	[EnabledStateObserver("previous", "Enabled", "EnabledChanged")]

	[ExtensionOf(typeof(ImageViewerToolExtensionPoint))]
	public class DisplaySetNavigationTool : Tool<IImageViewerToolContext>
	{
		// NOTE: this is purposely *not* derived from ImageViewerTool because that class sets Enabled differently than we want,
		// and we would have to override the methods and do nothing in order for it to work properly, which is a bit hacky.

		private bool _enabled = true;
		private event EventHandler _enabledChanged;

		public DisplaySetNavigationTool()
		{
		}

		public bool Enabled
		{
			get { return _enabled; }
			protected set
			{
				if (_enabled != value)
				{
					_enabled = value;
					EventsHelper.Fire(_enabledChanged, this, EventArgs.Empty);
				}
			}
		}

		public event EventHandler EnabledChanged
		{
			add { _enabledChanged += value; }
			remove { _enabledChanged -= value; }
		}

		private void OnStudyLoaded(object sender, StudyLoadedEventArgs e)
		{
			UpdateEnabled();
		}

		private void OnImageLoaded(object sender, ItemEventArgs<Sop> e)
		{
			UpdateEnabled();
		}

		private void OnDisplaySetSelected(object sender, DisplaySetSelectedEventArgs e)
		{
			UpdateEnabled();
		}

		private void OnImageBoxDrawing(object sender, ImageBoxDrawingEventArgs e)
		{
			UpdateEnabled();
		}

		private IDisplaySet GetSourceDisplaySet()
		{
			IImageBox imageBox = base.Context.Viewer.SelectedImageBox;
			if (imageBox == null)
				return null;

			IDisplaySet currentDisplaySet = imageBox.DisplaySet;

			if (currentDisplaySet == null || currentDisplaySet.ParentImageSet == null)
				return null;

			return CollectionUtils.SelectFirst(currentDisplaySet.ParentImageSet.DisplaySets,
									   delegate(IDisplaySet displaySet)
									   {
										   return displaySet.Uid == currentDisplaySet.Uid;
									   });
		}

		private void UpdateEnabled()
		{
			IImageBox imageBox = base.Context.Viewer.SelectedImageBox;
			if (imageBox == null || imageBox.DisplaySetLocked)
			{
				Enabled = false;
			}
			else
			{
				IDisplaySet sourceDisplaySet = GetSourceDisplaySet();
				Enabled = sourceDisplaySet != null && sourceDisplaySet.ParentImageSet.DisplaySets.Count > 1;
			}
		}

		public override void Initialize()
		{
			base.Initialize();

			UpdateEnabled();

			base.Context.Viewer.EventBroker.ImageLoaded += OnImageLoaded;
			base.Context.Viewer.EventBroker.StudyLoaded += OnStudyLoaded;
			base.Context.Viewer.EventBroker.ImageBoxDrawing += OnImageBoxDrawing;
			base.Context.Viewer.EventBroker.DisplaySetSelected += OnDisplaySetSelected;
		}

		protected override void Dispose(bool disposing)
		{
			base.Context.Viewer.EventBroker.ImageLoaded -= OnImageLoaded;
			base.Context.Viewer.EventBroker.StudyLoaded -= OnStudyLoaded;
			base.Context.Viewer.EventBroker.ImageBoxDrawing -= OnImageBoxDrawing;
			base.Context.Viewer.EventBroker.DisplaySetSelected -= OnDisplaySetSelected;

			base.Dispose(disposing);
		}

		public void NextDisplaySet()
		{
			AdvanceDisplaySet(+1);
		}

		public void PreviousDisplaySet()
		{
			AdvanceDisplaySet(-1);
		}

		public void AdvanceDisplaySet(int direction)
		{
			if (!Enabled)
				return;

			IDisplaySet sourceDisplaySet = GetSourceDisplaySet();
			if (sourceDisplaySet == null)
				return;

			IImageBox imageBox = base.Context.Viewer.SelectedImageBox;
			IImageSet parentImageSet = sourceDisplaySet.ParentImageSet;

			int sourceDisplaySetIndex = parentImageSet.DisplaySets.IndexOf(sourceDisplaySet);
			sourceDisplaySetIndex += direction;

			if (sourceDisplaySetIndex < 0)
				sourceDisplaySetIndex = parentImageSet.DisplaySets.Count - 1;
			else if (sourceDisplaySetIndex >= parentImageSet.DisplaySets.Count)
				sourceDisplaySetIndex = 0;

			MemorableUndoableCommand memorableCommand = new MemorableUndoableCommand(imageBox);
			memorableCommand.BeginState = imageBox.CreateMemento();

			imageBox.DisplaySet = parentImageSet.DisplaySets[sourceDisplaySetIndex].CreateFreshCopy();
			imageBox.Draw();

			memorableCommand.EndState = imageBox.CreateMemento();

			DrawableUndoableCommand historyCommand = new DrawableUndoableCommand(imageBox);
			historyCommand.Enqueue(memorableCommand);
			base.Context.Viewer.CommandHistory.AddCommand(historyCommand);
		}
	}
}
