﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using System.ServiceModel;
using ClearCanvas.Common;
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Actions;
using ClearCanvas.Enterprise.Common;
using ClearCanvas.Ris.Application.Common;
using ClearCanvas.Ris.Application.Common.ReportingWorkflow;

namespace ClearCanvas.Ris.Client.Workflow
{
	[MenuAction("apply", "folderexplorer-items-contextmenu/Send to Transcription", "Apply")]
	[IconSet("apply", IconScheme.Colour, "Icons.CompleteToolSmall.png", "Icons.CompleteToolMedium.png", "Icons.CompleteToolLarge.png")]
	[EnabledStateObserver("apply", "Enabled", "EnabledChanged")]
	[VisibleStateObserver("apply", "Visible", "VisibleChanged")]
	[ExtensionOf(typeof(ReportingWorkflowItemToolExtensionPoint))]
	public class CompleteInterpretationForTranscriptionTool : ReportingWorkflowItemTool
	{
		public CompleteInterpretationForTranscriptionTool()
			: base("CompleteInterpretationForTranscription")
		{
		}

		public override void Initialize()
		{
			this.Context.RegisterDropHandler(typeof(Folders.Reporting.InTranscriptionFolder), this);

			base.Initialize();
		}

		public bool Visible
		{
			get
			{
				return ReportingSettings.Default.EnableTranscriptionWorkflow;
			}
		}

		public event EventHandler VisibleChanged
		{
			add { }
			remove { }
		}

		protected override bool Execute(ReportingWorklistItemSummary item)
		{
			try
			{
				ExecuteHelper(item.ProcedureStepRef, null);
			}
			catch (FaultException<SupervisorValidationException>)
			{
				ExecuteHelper(item.ProcedureStepRef, GetSupervisorRef());
			}

			this.Context.InvalidateFolders(typeof(Folders.Reporting.InTranscriptionFolder));

			return true;
		}

		private void ExecuteHelper(EntityRef procedureStepRef, EntityRef supervisorRef)
		{
			Platform.GetService<IReportingWorkflowService>(
				delegate(IReportingWorkflowService service)
				{
					CompleteInterpretationForTranscriptionRequest request = new CompleteInterpretationForTranscriptionRequest(procedureStepRef);
					request.SupervisorRef = supervisorRef;
					service.CompleteInterpretationForTranscription(request);
				});
		}
	}

	[MenuAction("apply", "folderexplorer-items-contextmenu/Submit for Review", "Apply")]
	[EnabledStateObserver("apply", "Enabled", "EnabledChanged")]
	[IconSet("apply", IconScheme.Colour, "Icons.SubmitForReviewSmall.png", "Icons.SubmitForReviewMedium.png", "Icons.SubmitForReviewLarge.png")]
	[ActionPermission("apply", Application.Common.AuthorityTokens.Workflow.Report.Create, Application.Common.AuthorityTokens.Workflow.Report.SubmitForReview)]
	[ExtensionOf(typeof(ReportingWorkflowItemToolExtensionPoint))]
	public class CompleteInterpretationForVerificationTool : ReportingWorkflowItemTool
	{
		public CompleteInterpretationForVerificationTool()
			: base("CompleteInterpretationForVerification")
		{
		}

		public override void Initialize()
		{
			this.Context.RegisterDropHandler(typeof(Folders.Reporting.AwaitingReviewFolder), this);

			base.Initialize();
		}

		protected override bool Execute(ReportingWorklistItemSummary item)
		{
			try
			{
				ExecuteHelper(item.ProcedureStepRef, null);
			}
			catch (FaultException<SupervisorValidationException>)
			{
				ExecuteHelper(item.ProcedureStepRef, GetSupervisorRef());
			}

			this.Context.InvalidateFolders(typeof(Folders.Reporting.AwaitingReviewFolder));

			return true;
		}

		private void ExecuteHelper(EntityRef procedureStepRef, EntityRef supervisorRef)
		{
			Platform.GetService<IReportingWorkflowService>(
				delegate(IReportingWorkflowService service)
				{
					CompleteInterpretationForVerificationRequest request = new CompleteInterpretationForVerificationRequest(procedureStepRef);
					request.SupervisorRef = supervisorRef;
					service.CompleteInterpretationForVerification(request);
				});
		}
	}

	[MenuAction("apply", "folderexplorer-items-contextmenu/Discard Report", "Apply")]
	[MenuAction("apply", "folderexplorer-items-toolbar/Discard Report", "Apply")]
	[IconSet("apply", IconScheme.Colour, "Icons.CancelReportSmall.png", "Icons.CancelReportMedium.png", "Icons.CancelReportLarge.png")]
	[EnabledStateObserver("apply", "Enabled", "EnabledChanged")]
	[IconSetObserver("apply", "CurrentIconSet", "LabelChanged")]
	[LabelValueObserver("apply", "Label", "LabelChanged")]
	[TooltipValueObserver("apply", "Label", "LabelChanged")]
	[ExtensionOf(typeof(ReportingWorkflowItemToolExtensionPoint))]
	[ExtensionOf(typeof(RadiologistAdminWorkflowItemToolExtensionPoint))]
	public class DiscardReportTool : ReportingWorkflowItemTool
	{
		private IconSet _cancelAddendum = new IconSet(IconScheme.Colour, "Icons.CancelAddendumSmall.png", "Icons.CancelAddendumSmall.png", "Icons.CancelAddendumSmall.png");
		private IconSet _cancelReport = new IconSet(IconScheme.Colour, "Icons.CancelReportSmall.png", "Icons.CancelReportMedium.png", "Icons.CancelReportLarge.png");

		public DiscardReportTool()
			: base("CancelReportingStep")
		{
		}

		public string Label
		{
			get
			{
				ReportingWorklistItemSummary item = GetSelectedItem();
				return (item != null && item.IsAddendumStep) ? "Discard Addendum" : "Discard Report";
			}
		}

		public IconSet CurrentIconSet
		{
			get
			{
				ReportingWorklistItemSummary item = GetSelectedItem();
				return (item != null && item.IsAddendumStep) ? _cancelAddendum : _cancelReport;
			}
		}

		public event EventHandler LabelChanged
		{
			add { this.Context.SelectionChanged += value; }
			remove { this.Context.SelectionChanged -= value; }
		}

		protected override bool Execute(ReportingWorklistItemSummary item)
		{
			string msg = item.IsAddendumStep ? SR.MessageConfirmDiscardSelectedAddendum : SR.MessageConfirmDiscardSelectedReport;

			if (this.Context.DesktopWindow.ShowMessageBox(msg, MessageBoxActions.OkCancel)
				== DialogBoxAction.Cancel)
				return false;


			Platform.GetService<IReportingWorkflowService>(
				delegate(IReportingWorkflowService service)
				{
					service.CancelReportingStep(new CancelReportingStepRequest(item.ProcedureStepRef, null));
				});

			// no point in invalidating "to be reported" folder because its communal

			return true;
		}
	}

	[MenuAction("apply", "folderexplorer-items-contextmenu/Verify", "Apply")]
	[ButtonAction("apply", "folderexplorer-items-toolbar/Verify", "Apply")]
	[IconSet("apply", IconScheme.Colour, "Icons.VerifyReportSmall.png", "Icons.VerifyReportMedium.png", "Icons.VerifyReportLarge.png")]
	[EnabledStateObserver("apply", "Enabled", "EnabledChanged")]
	[ActionPermission("apply", ClearCanvas.Ris.Application.Common.AuthorityTokens.Workflow.Report.Create,
	   ClearCanvas.Ris.Application.Common.AuthorityTokens.Workflow.Report.Verify)]
	[ExtensionOf(typeof(ReportingWorkflowItemToolExtensionPoint))]
	public class VerifyTool : ReportingWorkflowItemTool
	{
		public VerifyTool()
			: base("Verify")
		{
		}

		public override void Initialize()
		{
			this.Context.RegisterDropHandler(typeof(Folders.Reporting.VerifiedFolder), this);

			base.Initialize();
		}

		public override bool Enabled
		{
			get
			{
				return this.Context.SelectedItems.Count == 1 &&
					(this.Context.GetOperationEnablement("CompleteInterpretationAndVerify") ||
					this.Context.GetOperationEnablement("CompleteVerification"));
			}
		}

		public override bool CanAcceptDrop(ICollection<ReportingWorklistItemSummary> items)
		{
			return this.Context.GetOperationEnablement("CompleteInterpretationAndVerify") ||
				this.Context.GetOperationEnablement("CompleteVerification");
		}

		protected override bool Execute(ReportingWorklistItemSummary item)
		{
			// show PD dialog if required
			return PreliminaryDiagnosis.ShowDialogOnVerifyIfRequired(item, this.Context.DesktopWindow,
				delegate
				{
					try
					{
						ExecuteHelper(item.ProcedureStepName, item.ProcedureStepRef, null);
					}
					catch (FaultException<SupervisorValidationException>)
					{
						ExecuteHelper(item.ProcedureStepName, item.ProcedureStepRef, GetSupervisorRef());
					}

					this.Context.InvalidateFolders(typeof(Folders.Reporting.VerifiedFolder));
				});
		}

		private void ExecuteHelper(string procedureStepName, EntityRef procedureStepRef, EntityRef supervisorRef)
		{
			if (procedureStepName == StepType.Interpretation || procedureStepName == StepType.TranscriptionReview)
			{
				Platform.GetService<IReportingWorkflowService>(
					delegate(IReportingWorkflowService service)
					{
						CompleteInterpretationAndVerifyRequest request = new CompleteInterpretationAndVerifyRequest(procedureStepRef);
						request.SupervisorRef = supervisorRef;
						service.CompleteInterpretationAndVerify(request);
					});
			}
			else if (procedureStepName == StepType.Verification)
			{
				Platform.GetService<IReportingWorkflowService>(
					delegate(IReportingWorkflowService service)
					{
						CompleteVerificationRequest request = new CompleteVerificationRequest(procedureStepRef);
						request.SupervisorRef = supervisorRef;
						service.CompleteVerification(request);
					});
			}
		}
	}

	[MenuAction("apply", "folderexplorer-items-contextmenu/Add Addendum", "Apply")]
	[ButtonAction("apply", "folderexplorer-items-toolbar/Add Addendum", "Apply")]
	[IconSet("apply", IconScheme.Colour, "Icons.AddAddendumToolSmall.png", "Icons.AddAddendumToolMedium.png", "Icons.AddAddendumToolLarge.png")]
	[EnabledStateObserver("apply", "Enabled", "EnabledChanged")]
	[ActionPermission("apply", ClearCanvas.Ris.Application.Common.AuthorityTokens.Workflow.Report.Create)]
	[ExtensionOf(typeof(ReportingWorkflowItemToolExtensionPoint))]
	public class AddendumTool : ReportingWorkflowItemTool
	{
		public AddendumTool()
			: base("CreateAddendum")
		{
		}

		protected override bool Execute(ReportingWorklistItemSummary item)
		{
			if (ActivateIfAlreadyOpen(item))
				return true;

			ReportingWorklistItemSummary interpretationWorklistItem = null;

			Platform.GetService<IReportingWorkflowService>(
				delegate(IReportingWorkflowService service)
				{
					CreateAddendumResponse response = service.CreateAddendum(new CreateAddendumRequest(item.ProcedureRef));
					interpretationWorklistItem = response.ReportingWorklistItem;
				});

			this.Context.InvalidateFolders(typeof(Folders.Reporting.DraftFolder));

			if (ActivateIfAlreadyOpen(interpretationWorklistItem))
				return true;

			OpenReportEditor(interpretationWorklistItem);

			return true;
		}
	}

	[MenuAction("apply", "folderexplorer-items-contextmenu/Publish", "Apply")]
	[EnabledStateObserver("apply", "Enabled", "EnabledChanged")]
	[ActionPermission("apply", ClearCanvas.Ris.Application.Common.AuthorityTokens.Development.TestPublishReport)]
	[ExtensionOf(typeof(ReportingWorkflowItemToolExtensionPoint))]
	public class PublishReportTool : ReportingWorkflowItemTool
	{
		public PublishReportTool()
			: base("PublishReport")
		{
		}

		protected override bool Execute(ReportingWorklistItemSummary item)
		{
			Platform.GetService<IReportingWorkflowService>(
				delegate(IReportingWorkflowService service)
				{
					service.PublishReport(new PublishReportRequest(item.ProcedureStepRef));
				});

			return true;
		}
	}
}

