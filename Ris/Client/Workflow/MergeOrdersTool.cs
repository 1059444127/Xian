#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System.Collections.Generic;
using ClearCanvas.Common;
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Actions;
using ClearCanvas.Ris.Application.Common;
using ClearCanvas.Ris.Application.Common.ModalityWorkflow;
using ClearCanvas.Ris.Application.Common.RegistrationWorkflow;
using ClearCanvas.Ris.Application.Common.RegistrationWorkflow.OrderEntry;

namespace ClearCanvas.Ris.Client.Workflow
{
	[ButtonAction("apply", "folderexplorer-items-toolbar/Merge Orders", "Apply")]
	[MenuAction("apply", "folderexplorer-items-contextmenu/Merge Orders", "Apply")]
	[Tooltip("apply", "Merge Orders")]
	[IconSet("apply", IconScheme.Colour, "MergeOrdersSmall.png", "MergeOrdersMedium.png", "MergeOrdersLarge.png")]
	[EnabledStateObserver("apply", "Enabled", "EnabledChanged")]
	[ActionPermission("apply", Application.Common.AuthorityTokens.Workflow.Order.Merge)]
	public abstract class MergeOrdersToolBase<TItem, TContext> : WorkflowItemTool<TItem, TContext>
		where TItem : WorklistItemSummaryBase
		where TContext : IWorkflowItemToolContext<TItem>
	{
		protected MergeOrdersToolBase()
			: base("MergeOrder")
		{
		}

		public override void Initialize()
		{
			base.Initialize();

			this.Context.RegisterWorkflowService(typeof(IOrderEntryService));
		}

		protected abstract void InvalidateFolders();

		public override bool Enabled
		{
			get
			{
				if (this.Context.SelectedItems.Count != 2)
					return false;

				var list = new List<TItem>(this.Context.SelectedItems);

				// Obvious cases where merging should not be allowed.
				// Cannot merge the same order.
				if (list[0].AccessionNumber == list[1].AccessionNumber)
					return false;

				// Cannot merge orders from different patient
				if (!list[0].PatientRef.Equals(list[1].PatientRef, true))
					return false;

				// Return true, let the server decide how to inform user of more complicated error.
				return true;
			}
		}

		protected bool ExecuteCore(WorklistItemSummaryBase item)
		{
			var list = new List<TItem>(this.Context.SelectedItems);
			var component = new MergeOrdersComponent(list[0].OrderRef, list[1].OrderRef);

			string failureReason;
			if (!component.ValidateMergeRequest(out failureReason))
			{
				this.Context.DesktopWindow.ShowMessageBox(failureReason, MessageBoxActions.Ok);
				return false;
			}

			if (ApplicationComponentExitCode.Accepted != ApplicationComponent.LaunchAsDialog(this.Context.DesktopWindow, component, SR.TitleMergeOrders))
				return false;

			InvalidateFolders();

			return true;
		}
	}

	[ExtensionOf(typeof(RegistrationWorkflowItemToolExtensionPoint))]
	public class RegistrationMergeOrdersTool : MergeOrdersToolBase<RegistrationWorklistItemSummary, IRegistrationWorkflowItemToolContext>
	{
		protected override bool Execute(RegistrationWorklistItemSummary item)
		{
			return ExecuteCore(item);
		}

		protected override void InvalidateFolders()
		{
			DocumentManager.InvalidateFolder(typeof(Folders.Registration.ScheduledFolder));
			DocumentManager.InvalidateFolder(typeof(Folders.Registration.CancelledFolder));
		}
	}

	[ExtensionOf(typeof(BookingWorkflowItemToolExtensionPoint))]
	public class BookingMergeOrdersTool : MergeOrdersToolBase<RegistrationWorklistItemSummary, IRegistrationWorkflowItemToolContext>
	{
		protected override bool Execute(RegistrationWorklistItemSummary item)
		{
			return ExecuteCore(item);
		}

		protected override void InvalidateFolders()
		{
			DocumentManager.InvalidateFolder(typeof(Folders.Registration.ToBeScheduledFolder));
			DocumentManager.InvalidateFolder(typeof(Folders.Registration.PendingProtocolFolder));
		}
	}

	[ExtensionOf(typeof(PerformingWorkflowItemToolExtensionPoint))]
	public class PerformingMergeOrdersTool : MergeOrdersToolBase<ModalityWorklistItemSummary, IPerformingWorkflowItemToolContext>
	{
		protected override bool Execute(ModalityWorklistItemSummary item)
		{
			return ExecuteCore(item);
		}

		protected override void InvalidateFolders()
		{
			DocumentManager.InvalidateFolder(typeof(Folders.Performing.ScheduledFolder));
			DocumentManager.InvalidateFolder(typeof(Folders.Performing.CancelledFolder));
		}
	}
}
