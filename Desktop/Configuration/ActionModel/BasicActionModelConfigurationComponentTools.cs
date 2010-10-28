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
using ClearCanvas.Desktop.Actions;
using ClearCanvas.Desktop.Validation;

namespace ClearCanvas.Desktop.Configuration.ActionModel
{
	[ButtonAction("addGroup", "actionmodelconfig-toolbar/ToolbarAddGroup", "AddGroup")]
	[IconSet("addGroup", IconScheme.Colour, "Icons.AddActionModelGroupToolSmall.png", "Icons.AddActionModelGroupToolMedium.png", "Icons.AddActionModelGroupToolLarge.png")]
	[EnabledStateObserver("addGroup", "CanInsertGroup", "SelectedNodeChanged")]
	[ButtonAction("addSeparator", "actionmodelconfig-toolbar/ToolbarAddSeparator", "AddSeparator")]
	[IconSet("addSeparator", IconScheme.Colour, "Icons.AddActionModelSeparatorToolSmall.png", "Icons.AddActionModelSeparatorToolMedium.png", "Icons.AddActionModelSeparatorToolLarge.png")]
	[ButtonAction("removeNode", "actionmodelconfig-toolbar/ToolbarRemove", "RemoveNode", KeyStroke = XKeys.Delete)]
	[EnabledStateObserver("removeNode", "CanRemove", "SelectedNodeChanged")]
	[IconSet("removeNode", IconScheme.Colour, "Icons.DeleteToolSmall.png", "Icons.DeleteToolSmall.png", "Icons.DeleteToolSmall.png")]
	[ButtonAction("renameNode", "actionmodelconfig-toolbar/ToolbarRename", "RenameNode", KeyStroke = XKeys.F2)]
	[EnabledStateObserver("renameNode", "CanRename", "SelectedNodeChanged")]
	[IconSet("renameNode", IconScheme.Colour, "Icons.RenameToolSmall.png", "Icons.RenameToolSmall.png", "Icons.RenameToolSmall.png")]

	[ButtonAction("insertGroupBefore", "actionmodelconfig-contextmenu/MenuInsertGroupBefore", "InsertGroupBefore")]
	[IconSet("insertGroupBefore", IconScheme.Colour, "Icons.AddActionModelGroupToolSmall.png", "Icons.AddActionModelGroupToolMedium.png", "Icons.AddActionModelGroupToolLarge.png")]
	[EnabledStateObserver("insertGroupBefore", "CanInsertGroup", "SelectedNodeChanged")]
	[ButtonAction("insertGroupAfter", "actionmodelconfig-contextmenu/MenuInsertGroupAfter", "InsertGroupAfter")]
	[IconSet("insertGroupAfter", IconScheme.Colour, "Icons.AddActionModelGroupToolSmall.png", "Icons.AddActionModelGroupToolMedium.png", "Icons.AddActionModelGroupToolLarge.png")]
	[EnabledStateObserver("insertGroupAfter", "CanInsertGroup", "SelectedNodeChanged")]
	[ButtonAction("insertGroupAsChild", "actionmodelconfig-contextmenu/MenuInsertGroupAsChild", "InsertGroupAsChild")]
	[IconSet("insertGroupAsChild", IconScheme.Colour, "Icons.AddActionModelGroupToolSmall.png", "Icons.AddActionModelGroupToolMedium.png", "Icons.AddActionModelGroupToolLarge.png")]
	[EnabledStateObserver("insertGroupAsChild", "CanInsertGroup", "SelectedNodeChanged")]
	[VisibleStateObserver("insertGroupAsChild", "CanInsertChild", "SelectedNodeChanged")]

	[ButtonAction("insertSeparatorBefore", "actionmodelconfig-contextmenu/MenuInsertSeparatorBefore", "InsertSeparatorBefore")]
	[IconSet("insertSeparatorBefore", IconScheme.Colour, "Icons.AddActionModelSeparatorToolSmall.png", "Icons.AddActionModelSeparatorToolMedium.png", "Icons.AddActionModelSeparatorToolLarge.png")]
	[ButtonAction("insertSeparatorAfter", "actionmodelconfig-contextmenu/MenuInsertSeparatorAfter", "InsertSeparatorAfter")]
	[IconSet("insertSeparatorAfter", IconScheme.Colour, "Icons.AddActionModelSeparatorToolSmall.png", "Icons.AddActionModelSeparatorToolMedium.png", "Icons.AddActionModelSeparatorToolLarge.png")]
	[ButtonAction("insertSeparatorAsChild", "actionmodelconfig-contextmenu/MenuInsertSeparatorAsChild", "InsertSeparatorAsChild")]
	[IconSet("insertSeparatorAsChild", IconScheme.Colour, "Icons.AddActionModelSeparatorToolSmall.png", "Icons.AddActionModelSeparatorToolMedium.png", "Icons.AddActionModelSeparatorToolLarge.png")]
	[VisibleStateObserver("insertSeparatorAsChild", "CanInsertChild", "SelectedNodeChanged")]

	[ButtonAction("removeNodeCxt", "actionmodelconfig-contextmenu/MenuRemove", "RemoveNode", KeyStroke = XKeys.Delete)]
	[VisibleStateObserver("removeNodeCxt", "CanRemove", "SelectedNodeChanged")]
	[IconSet("removeNodeCxt", IconScheme.Colour, "Icons.DeleteToolSmall.png", "Icons.DeleteToolSmall.png", "Icons.DeleteToolSmall.png")]
	[ButtonAction("renameNodeCxt", "actionmodelconfig-contextmenu/MenuRename", "RenameNode", KeyStroke = XKeys.F2)]
	[VisibleStateObserver("renameNodeCxt", "CanRename", "SelectedNodeChanged")]
	[IconSet("renameNodeCxt", IconScheme.Colour, "Icons.RenameToolSmall.png", "Icons.RenameToolSmall.png", "Icons.RenameToolSmall.png")]
	[ExtensionOf(typeof (ActionModelConfigurationComponentToolExtensionPoint))]

	//TODO (CR Sept 2010): base class name - ActionModelConfigurationTool?
	public class BasicActionModelConfigurationComponentTools : ActionModelConfigurationComponentTool
	{
		public event EventHandler SelectedNodeChanged;

		public bool CanInsertGroup
		{
			get { return !base.Component.EnforceFlatActionModel; }
		}

		public bool CanRemove
		{
			get
			{
				if (base.SelectedNode == null)
					return false;
				if (base.SelectedNode is AbstractActionModelTreeLeafAction)
					return false;
				return true;
			}
		}

		public bool CanRename
		{
			get
			{
				if (base.SelectedNode == null)
					return false;
				if (base.SelectedNode is AbstractActionModelTreeLeafAction)
					return false;
				return true;
			}
		}

		public bool CanInsertChild
		{
			get
			{
				if (base.SelectedNode == null)
					return false;
				return base.SelectedNode is AbstractActionModelTreeBranch;
			}
		}

		protected override void OnSelectedNodeChanged()
		{
			base.OnSelectedNodeChanged();

			EventsHelper.Fire(this.SelectedNodeChanged, this, EventArgs.Empty);
		}

		private void InsertNode(AbstractActionModelTreeNode node)
		{
			AbstractActionModelTreeNode selectedNode = base.SelectedNode;
			if (selectedNode is AbstractActionModelTreeBranch && !selectedNode.IsExpanded && selectedNode.Parent != null)
			{
				this.InsertNode(node, selectedNode, DragDropPosition.After);
			}
			else if (selectedNode is AbstractActionModelTreeBranch)
			{
				this.InsertNode(node, selectedNode, DragDropPosition.Default);
			}
			else if (selectedNode is AbstractActionModelTreeLeaf && selectedNode.Parent != null)
			{
				this.InsertNode(node, selectedNode, DragDropPosition.After);
			}
			else
			{
				this.InsertNode(node, base.Component.AbstractActionModelTreeRoot, DragDropPosition.After);
			}
		}

		private void InsertNode(AbstractActionModelTreeNode node, AbstractActionModelTreeNode selectedNode, DragDropPosition position)
		{
			if (position == DragDropPosition.After && selectedNode.Parent != null)
			{
				selectedNode.Parent.Children.Insert(selectedNode.Parent.Children.IndexOf(selectedNode) + 1, node);
			}
			else if (position == DragDropPosition.Default && selectedNode is AbstractActionModelTreeBranch)
			{
				((AbstractActionModelTreeBranch) selectedNode).Children.Add(node);
				selectedNode.IsExpanded = true;
			}
			else if (position == DragDropPosition.Before && selectedNode.Parent != null)
			{
				selectedNode.Parent.Children.Insert(selectedNode.Parent.Children.IndexOf(selectedNode), node);
			}
			else
			{
				base.Component.AbstractActionModelTreeRoot.Children.Add(node);
				base.Component.AbstractActionModelTreeRoot.IsExpanded = true;
			}
		}

		public void AddGroup()
		{
			try
			{
				this.InsertNode(new AbstractActionModelTreeBranch(SR.LabelNewGroup));
			}
			catch (Exception ex)
			{
				ExceptionHandler.Report(ex, this.Context.DesktopWindow);
			}
		}

		public void AddSeparator()
		{
			try
			{
				this.InsertNode(new AbstractActionModelTreeLeafSeparator());
			}
			catch (Exception ex)
			{
				ExceptionHandler.Report(ex, this.Context.DesktopWindow);
			}
		}

		public void InsertGroupAfter()
		{
			try
			{
				this.InsertNode(new AbstractActionModelTreeBranch(SR.LabelNewGroup), this.SelectedNode, DragDropPosition.After);
			}
			catch (Exception ex)
			{
				ExceptionHandler.Report(ex, this.Context.DesktopWindow);
			}
		}

		public void InsertSeparatorAfter()
		{
			try
			{
				this.InsertNode(new AbstractActionModelTreeLeafSeparator(), this.SelectedNode, DragDropPosition.After);
			}
			catch (Exception ex)
			{
				ExceptionHandler.Report(ex, this.Context.DesktopWindow);
			}
		}

		public void InsertGroupBefore()
		{
			try
			{
				this.InsertNode(new AbstractActionModelTreeBranch(SR.LabelNewGroup), this.SelectedNode, DragDropPosition.Before);
			}
			catch (Exception ex)
			{
				ExceptionHandler.Report(ex, this.Context.DesktopWindow);
			}
		}

		public void InsertSeparatorBefore()
		{
			try
			{
				this.InsertNode(new AbstractActionModelTreeLeafSeparator(), this.SelectedNode, DragDropPosition.Before);
			}
			catch (Exception ex)
			{
				ExceptionHandler.Report(ex, this.Context.DesktopWindow);
			}
		}

		public void InsertGroupAsChild()
		{
			try
			{
				this.InsertNode(new AbstractActionModelTreeBranch(SR.LabelNewGroup), this.SelectedNode, DragDropPosition.Default);
			}
			catch (Exception ex)
			{
				ExceptionHandler.Report(ex, this.Context.DesktopWindow);
			}
		}

		public void InsertSeparatorAsChild()
		{
			try
			{
				this.InsertNode(new AbstractActionModelTreeLeafSeparator(), this.SelectedNode, DragDropPosition.Default);
			}
			catch (Exception ex)
			{
				ExceptionHandler.Report(ex, this.Context.DesktopWindow);
			}
		}

		public void RemoveNode()
		{
			try
			{
				AbstractActionModelTreeNode selectedNode = base.SelectedNode;
				if (this.CanRemove && selectedNode.Parent != null)
				{
					AbstractActionModelTreeBranch branch = selectedNode as AbstractActionModelTreeBranch;
					if (branch != null && !branch.HasNoActions)
					{
						base.Context.DesktopWindow.ShowMessageBox(SR.MessageNodeNotEmpty, MessageBoxActions.Ok);
						return;
					}

					selectedNode.Parent.Children.Remove(selectedNode);
				}
			}
			catch (Exception ex)
			{
				ExceptionHandler.Report(ex, this.Context.DesktopWindow);
			}
		}

		public void RenameNode()
		{
			try
			{
				if (this.CanRename)
				{
					AbstractActionModelTreeNode selectedNode = base.SelectedNode;
					RenameNodeComponent component = new RenameNodeComponent();
					component.Name = selectedNode.Label;

					ApplicationComponentExitCode result = ApplicationComponent.LaunchAsDialog(
						this.Context.DesktopWindow,
						new SimpleComponentContainer(component),
						SR.TitleRename);

					if (result == ApplicationComponentExitCode.Accepted)
					{
						selectedNode.Label = component.Name;
					}
				}
			}
			catch (Exception ex)
			{
				ExceptionHandler.Report(ex, this.Context.DesktopWindow);
			}
		}
	}

	[ExtensionPoint]
	public sealed class RenameNodeComponentViewExtensionPoint : ExtensionPoint<IApplicationComponentView> {}

	[AssociateView(typeof (RenameNodeComponentViewExtensionPoint))]
	public class RenameNodeComponent : ApplicationComponent
	{
		private string _name;

		[ValidateLength(1)]
		public string Name
		{
			get { return _name; }
			set
			{
				if (_name != value)
				{
					_name = value;
					this.NotifyPropertyChanged("Name");
				}
			}
		}
	}
}