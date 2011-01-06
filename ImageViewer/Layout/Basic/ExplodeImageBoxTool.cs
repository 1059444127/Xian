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
using ClearCanvas.Common.Utilities;
using ClearCanvas.ImageViewer.BaseTools;
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Actions;
using ClearCanvas.ImageViewer.Common;
using ClearCanvas.ImageViewer.InputManagement;

namespace ClearCanvas.ImageViewer.Layout.Basic
{
	[ButtonAction("explodeImageBox", "global-toolbars/ToolbarStandard/ToolbarExplodeImageBox", "ToggleExplode", Flags = ClickActionFlags.CheckAction, KeyStroke = XKeys.X)]
	[MenuAction("explodeImageBox", "global-menus/MenuTools/MenuStandard/MenuExplodeImageBox", "ToggleExplode", Flags = ClickActionFlags.CheckAction)]
	[CheckedStateObserver("explodeImageBox", "Checked", "CheckedChanged")]
	[EnabledStateObserver("explodeImageBox", "Enabled", "EnabledChanged")]
	[Tooltip("explodeImageBox", "TooltipExplodeImageBox")]
	[MouseButtonIconSet("explodeImageBox", IconScheme.Colour, "Icons.ExplodeImageBoxToolSmall.png", "Icons.ExplodeImageBoxMedium.png", "Icons.ExplodeImageBoxLarge.png")]
	[GroupHint("explodeImageBox", "Tools.Layout.ImageBox.Explode")]

	[DefaultMouseToolButton(XMouseButtons.Left)]
	[ExtensionOf(typeof(ImageViewerToolExtensionPoint))]
	public class ExplodeImageBoxTool : MouseImageViewerTool
	{
		[ThreadStatic]
		private static Dictionary<IImageViewer, ExplodeImageBoxTool> _tools;

		private ListObserver<IImageBox> _imageBoxesObserver;
		private object _unexplodeMemento;
		private IImageBox _oldImageBox;

		private static Dictionary<IImageViewer, ExplodeImageBoxTool> Tools
		{
			get
			{
				if (_tools == null)
					_tools = new Dictionary<IImageViewer, ExplodeImageBoxTool>();
				return _tools;
			}	
		}

		public ExplodeImageBoxTool()
		{
			//this tool is activated on a double-click
			base.Behaviour &= ~MouseButtonHandlerBehaviour.CancelStartOnDoubleClick;
		}

		public bool Checked
		{
			get { return _unexplodeMemento != null; }
			set
			{
				if (value == Checked)
					return;

				ToggleExplode();
			}
		}

		public event EventHandler CheckedChanged;

		public override void Initialize()
		{
			base.Initialize();
			Tools[ImageViewer] = this;

			_imageBoxesObserver = new ListObserver<IImageBox>(ImageViewer.PhysicalWorkspace.ImageBoxes, OnImageBoxesChanged);

			UpdateEnabled();
		}

		protected override void Dispose(bool disposing)
		{
			Tools.Remove(ImageViewer);

			_imageBoxesObserver.Dispose();
			
			base.Dispose(disposing);
		}

		protected override void OnTileSelected(object sender, TileSelectedEventArgs e)
		{
			UpdateEnabled();
		}

		protected override void OnPresentationImageSelected(object sender, PresentationImageSelectedEventArgs e)
		{
			UpdateEnabled();
		}

		private void OnImageBoxesChanged()
		{
			CancelExplodeMode();
			UpdateEnabled();
		}

		private void UpdateEnabled()
		{
			IPhysicalWorkspace workspace = base.ImageViewer.PhysicalWorkspace;
			if (Checked)
			{
				base.Enabled = true;
			}
			else
			{
				base.Enabled = !workspace.Locked && workspace.ImageBoxes.Count > 1 && workspace.SelectedImageBox != null &&
				               workspace.SelectedImageBox.SelectedTile != null &&
				               workspace.SelectedImageBox.SelectedTile.PresentationImage != null;
			}
		}

		private void CancelExplodeMode()
		{
			_unexplodeMemento = null;
			_oldImageBox = null;
			OnCheckedChanged();
		}

		private void OnCheckedChanged()
		{
			EventsHelper.Fire(CheckedChanged, this, EventArgs.Empty);
		}

		public override bool Start(IMouseInformation mouseInformation)
		{
			//this is a double-click tool.
			if (mouseInformation.ClickCount < 2)
				return false;

			if (!Enabled)
				return false;

			if (Checked)
			{
				UnexplodeImageBox();
				return true;
			}
			else
			{
				return false;
			}	
		}

		private static bool CanExplodeImageBox(IImageBox imageBox)
		{
			if (imageBox == null)
				return false;

			if (imageBox.ParentPhysicalWorkspace == null)
				return false;

			if (imageBox.DisplaySet == null || imageBox.SelectedTile == null || imageBox.SelectedTile.PresentationImage == null)
				return false;

			return true;
		}

		private bool CanUnexplodeImageBox(IImageBox imageBox)
		{
			if (imageBox == null)
				return false;

			IPhysicalWorkspace workspace = imageBox.ParentPhysicalWorkspace;
			if (workspace == null)
				return false;

			if (imageBox.DisplaySet == null)
			{
				CancelExplodeMode();
				return false;
			}

			return true;
		}

		private void ExplodeImageBox()
		{
			IImageBox imageBox = ImageViewer.SelectedImageBox;
			if (!CanExplodeImageBox(imageBox))
				return;

			IPhysicalWorkspace workspace = imageBox.ParentPhysicalWorkspace;
			MemorableUndoableCommand memorableCommand = new MemorableUndoableCommand(workspace);
			memorableCommand.BeginState = workspace.CreateMemento();

			_imageBoxesObserver.SuppressChangedEvent = true;

			//set this here so checked will be correct.
			_unexplodeMemento = memorableCommand.BeginState;
			_oldImageBox = imageBox;
			IDisplaySet displaySet = _oldImageBox.DisplaySet;
			IPresentationImage selectedImage = _oldImageBox.SelectedTile.PresentationImage;

			object imageBoxMemento = _oldImageBox.CreateMemento();
			workspace.SetImageBoxGrid(1, 1);
			IImageBox newImageBox = workspace.ImageBoxes[0];
			newImageBox.SetMemento(imageBoxMemento);

			//TODO (architecture): this wouldn't be necessary if we had a SetImageBoxGrid(imageBox[,]).
			//This stuff with mementos is actually a hacky workaround.

			bool locked = newImageBox.DisplaySetLocked;
			newImageBox.DisplaySetLocked = false;
			newImageBox.DisplaySet = displaySet;
			newImageBox.TopLeftPresentationImage = selectedImage;
			newImageBox.DisplaySetLocked = locked;

			workspace.SelectDefaultImageBox();
			
			_imageBoxesObserver.SuppressChangedEvent = false;

			workspace.Draw();

			memorableCommand.EndState = workspace.CreateMemento();
			DrawableUndoableCommand historyCommand = new DrawableUndoableCommand(workspace);
			historyCommand.Name = SR.CommandSurveyExplode;
			historyCommand.Enqueue(memorableCommand);
			base.ImageViewer.CommandHistory.AddCommand(historyCommand);

			OnCheckedChanged();
			UpdateEnabled();
		}

		private void UnexplodeImageBox()
		{
			IImageBox imageBox = ImageViewer.SelectedImageBox;
			if (!CanUnexplodeImageBox(imageBox))
				return;

			object imageBoxMemento = imageBox.CreateMemento();

			IPhysicalWorkspace workspace = imageBox.ParentPhysicalWorkspace;
			MemorableUndoableCommand memorableCommand = new MemorableUndoableCommand(workspace);
			memorableCommand.BeginState = workspace.CreateMemento();

			IImageBox oldImageBox = _oldImageBox;

			workspace.SetMemento(_unexplodeMemento);

			foreach (IImageBox box in workspace.ImageBoxes)
			{
				//Keep the state of the image box the same.
				if (box == oldImageBox)
				{
					box.SetMemento(imageBoxMemento);
					break;
				}
				
			}

			workspace.Draw();

			memorableCommand.EndState = workspace.CreateMemento();

			DrawableUndoableCommand historyCommand = new DrawableUndoableCommand(workspace);
			historyCommand.Name = SR.CommandSurveyExplode;
			historyCommand.Enqueue(memorableCommand);
			base.ImageViewer.CommandHistory.AddCommand(historyCommand);

			CancelExplodeMode();
			OnCheckedChanged();
			UpdateEnabled();
		}

		public void ToggleExplode()
		{
			if (!Enabled)
				return;

			if (Checked)
				UnexplodeImageBox();
			else
				ExplodeImageBox();
		}

		internal static bool IsExploded(IImageViewer viewer)
		{
			if (Tools.ContainsKey(viewer))
				return Tools[viewer].Checked;

			return false;
		}
	}
}
