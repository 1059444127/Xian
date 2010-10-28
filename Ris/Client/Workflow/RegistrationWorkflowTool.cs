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
using ClearCanvas.Ris.Client.Formatting;
using ClearCanvas.Ris.Application.Common;
using ClearCanvas.Ris.Application.Common.RegistrationWorkflow;

namespace ClearCanvas.Ris.Client.Workflow
{
	public abstract class RegistrationWorkflowTool : WorkflowItemTool<RegistrationWorklistItemSummary, IRegistrationWorkflowItemToolContext>
	{
		protected RegistrationWorkflowTool(string operationName)
			: base(operationName)
		{
		}

		public override void Initialize()
		{
			base.Initialize();

			this.Context.RegisterWorkflowService(typeof(IRegistrationWorkflowService));
		}
	}


	[MenuAction("apply", "folderexplorer-items-contextmenu/Check-in", "Apply")]
	[ButtonAction("apply", "folderexplorer-items-toolbar/Check-in", "Apply")]
	[IconSet("apply", IconScheme.Colour, "Icons.CheckInToolSmall.png", "Icons.CheckInToolMedium.png", "Icons.CheckInToolLarge.png")]
	[EnabledStateObserver("apply", "Enabled", "EnabledChanged")]
	[ActionPermission("apply", ClearCanvas.Ris.Application.Common.AuthorityTokens.Workflow.Procedure.CheckIn)]
	[ExtensionOf(typeof(RegistrationWorkflowItemToolExtensionPoint))]
	public class CheckInProceduresTool : RegistrationWorkflowTool
	{
		public CheckInProceduresTool()
			: base("CheckInProcedure")
		{
		}

		public override void Initialize()
		{
			base.Initialize();

			this.Context.RegisterDropHandler(typeof(Folders.Registration.CheckedInFolder), this);
		}

		protected override bool Execute(RegistrationWorklistItemSummary item)
		{
			CheckInOrderComponent checkInComponent = new CheckInOrderComponent(item);
			ApplicationComponentExitCode exitCode = ApplicationComponent.LaunchAsDialog(
				this.Context.DesktopWindow,
				checkInComponent,
				String.Format("Checking in {0}", PersonNameFormat.Format(item.PatientName)));

			if (exitCode == ApplicationComponentExitCode.Accepted)
			{
				this.Context.InvalidateFolders(typeof(Folders.Registration.CheckedInFolder));
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}

