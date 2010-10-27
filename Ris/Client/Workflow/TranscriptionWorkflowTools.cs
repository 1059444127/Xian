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
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Actions;
using ClearCanvas.Ris.Application.Common;
using ClearCanvas.Ris.Application.Common.ReportingWorkflow;
using ClearCanvas.Ris.Application.Common.TranscriptionWorkflow;

namespace ClearCanvas.Ris.Client.Workflow
{
	[MenuAction("apply", "folderexplorer-items-contextmenu/Complete", "Apply")]
	[ButtonAction("apply", "folderexplorer-items-toolbar/Complete", "Apply")]
	[IconSet("apply", IconScheme.Colour, "Icons.VerifyReportSmall.png", "Icons.VerifyReportMedium.png", "Icons.VerifyReportLarge.png")]
	[EnabledStateObserver("apply", "Enabled", "EnabledChanged")]
	[ActionPermission("apply", ClearCanvas.Ris.Application.Common.AuthorityTokens.Workflow.Transcription.Create)]
	[ExtensionOf(typeof(TranscriptionWorkflowItemToolExtensionPoint))]
	public class CompleteTranscriptionTool : TranscriptionWorkflowItemTool
	{
		public CompleteTranscriptionTool()
			: base("CompleteTranscription")
		{
		}

		public override void Initialize()
		{
			this.Context.RegisterDropHandler(typeof(Folders.Transcription.CompletedFolder), this);

			base.Initialize();
		}

		protected override bool Execute(ReportingWorklistItemSummary item)
		{
			Platform.GetService<ITranscriptionWorkflowService>(
				delegate(ITranscriptionWorkflowService service)
				{
					service.CompleteTranscription(new CompleteTranscriptionRequest(item.ProcedureStepRef));
				});

			this.Context.InvalidateFolders(typeof(Folders.Transcription.CompletedFolder));

			return true;
		}
	}

	[MenuAction("apply", "folderexplorer-items-contextmenu/Reject", "Apply")]
	[ButtonAction("apply", "folderexplorer-items-toolbar/Reject", "Apply")]
	[IconSet("apply", IconScheme.Colour, "Icons.RejectProtocolSmall.png", "Icons.RejectProtocolMedium.png", "Icons.RejectProtocolLarge.png")]
	[EnabledStateObserver("apply", "Enabled", "EnabledChanged")]
	[ActionPermission("apply", ClearCanvas.Ris.Application.Common.AuthorityTokens.Workflow.Transcription.Create)]
	[ExtensionOf(typeof(TranscriptionWorkflowItemToolExtensionPoint))]
	public class RejectTranscriptionTool : TranscriptionWorkflowItemTool
	{
		public RejectTranscriptionTool()
			: base("RejectTranscription")
		{
		}

		protected override bool Execute(ReportingWorklistItemSummary item)
		{
			TranscriptionRejectReasonComponent component = new TranscriptionRejectReasonComponent();
			if (this.Context.DesktopWindow.ShowDialogBox(component, "Reason") == DialogBoxAction.Ok)
			{
				Platform.GetService<ITranscriptionWorkflowService>(
					delegate(ITranscriptionWorkflowService service)
					{
						service.RejectTranscription(new RejectTranscriptionRequest(
							item.ProcedureStepRef, 
							component.Reason,
							CreateAdditionalCommentsNote(component.OtherReason)));
					});

				this.Context.InvalidateFolders(typeof (Folders.Transcription.CompletedFolder));
			}
			return true;
		}

		private static OrderNoteDetail CreateAdditionalCommentsNote(string additionalComments)
		{
			if (!string.IsNullOrEmpty(additionalComments))
				return new OrderNoteDetail(OrderNoteCategory.General.Key, additionalComments, null, false, null, null);
			else
				return null;
		}
	}

	[MenuAction("apply", "folderexplorer-items-contextmenu/Submit for Review", "Apply")]
	[ButtonAction("apply", "folderexplorer-items-toolbar/Submit for Review", "Apply")]
	[IconSet("apply", IconScheme.Colour, "Icons.VerifyReportSmall.png", "Icons.VerifyReportMedium.png", "Icons.VerifyReportLarge.png")]
	[EnabledStateObserver("apply", "Enabled", "EnabledChanged")]
	[ActionPermission("apply", ClearCanvas.Ris.Application.Common.AuthorityTokens.Workflow.Transcription.SubmitForReview)]
	[ExtensionOf(typeof(TranscriptionWorkflowItemToolExtensionPoint))]
	public class SubmitTranscriptionForReviewTool : TranscriptionWorkflowItemTool
	{
		public SubmitTranscriptionForReviewTool()
			: base("SubmitTranscriptionForReview")
		{
		}

		public override void Initialize()
		{
			this.Context.RegisterDropHandler(typeof(Folders.Transcription.AwaitingReviewFolder), this);

			base.Initialize();
		}

		protected override bool Execute(ReportingWorklistItemSummary item)
		{
			Platform.GetService<ITranscriptionWorkflowService>(
				delegate(ITranscriptionWorkflowService service)
				{
					service.SubmitTranscriptionForReview(new SubmitTranscriptionForReviewRequest(item.ProcedureStepRef));
				});

			this.Context.InvalidateFolders(typeof(Folders.Transcription.AwaitingReviewFolder));

			return true;
		}
	}

	[MenuAction("apply", "folderexplorer-items-contextmenu/Open Report", "Apply")]
	[ButtonAction("apply", "folderexplorer-items-toolbar/Open Report", "Apply")]
	[IconSet("apply", IconScheme.Colour, "Icons.EditReportToolSmall.png", "Icons.EditReportToolMedium.png", "Icons.EditReportToolLarge.png")]
	[EnabledStateObserver("apply", "Enabled", "EnabledChanged")]
	[ActionPermission("apply", ClearCanvas.Ris.Application.Common.AuthorityTokens.Workflow.Transcription.Create)]
	[ExtensionOf(typeof(TranscriptionWorkflowItemToolExtensionPoint))]
	public class EditTranscriptionTool : TranscriptionWorkflowItemTool
	{
		public EditTranscriptionTool()
			: base("EditTranscription")
		{
		}

		public override void Initialize()
		{
			base.Initialize();

			this.Context.RegisterDropHandler(typeof(Folders.Transcription.DraftFolder), this);
			this.Context.RegisterDoubleClickHandler(
				(IClickAction)CollectionUtils.SelectFirst(this.Actions,
					delegate(IAction a) { return a is IClickAction && a.ActionID.EndsWith("apply"); }));
		}

		public override bool Enabled
		{
			get
			{
				ReportingWorklistItemSummary item = GetSelectedItem();

				if (this.Context.SelectedItems.Count != 1)
					return false;

				return
					this.Context.GetOperationEnablement("StartTranscription") ||

					// there is no specific workflow operation for editing a previously created draft,
					// so we enable the tool if it looks like a draft and SaveReport is enabled
					(this.Context.GetOperationEnablement("SaveTranscription") && item != null && item.ActivityStatus.Code == StepState.InProgress);
			}
		}

		public override bool CanAcceptDrop(ICollection<ReportingWorklistItemSummary> items)
		{
			// this tool is only registered as a drop handler for the Drafts folder
			// and the only operation that would make sense in this context is StartInterpretation
			return this.Context.GetOperationEnablement("StartTranscription");
		}

		protected override bool Execute(ReportingWorklistItemSummary item)
		{
			// check if the document is already open
			if (ActivateIfAlreadyOpen(item))
				return true;

			// open the report editor
			OpenTranscriptionEditor(item);

			return true;
		}
	}
}
