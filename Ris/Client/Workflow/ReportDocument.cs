#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using ClearCanvas.Common;
using ClearCanvas.Desktop;
using ClearCanvas.Enterprise.Common;
using ClearCanvas.Ris.Application.Common.ReportingWorkflow;
using ClearCanvas.Ris.Client.Formatting;

namespace ClearCanvas.Ris.Client.Workflow
{
	public class ReportDocument : Document
	{
		private readonly ReportingWorklistItemSummary _worklistItem;
		private readonly bool _shouldOpenImages;
		private readonly string _folderName;
		private readonly EntityRef _worklistRef;
		private readonly string _worklistClassName;
		private ReportingComponent _component;

		public ReportDocument(ReportingWorklistItemSummary worklistItem, bool shouldOpenImages, IReportingWorkflowItemToolContext context)
			: base(worklistItem.ProcedureStepRef, context.DesktopWindow)
		{
			_worklistItem = worklistItem;
			_folderName = context.SelectedFolder.Name;
			_shouldOpenImages = shouldOpenImages;

			if (context.SelectedFolder is ReportingWorkflowFolder)
			{
				_worklistRef = ((ReportingWorkflowFolder)context.SelectedFolder).WorklistRef;
				_worklistClassName = ((ReportingWorkflowFolder)context.SelectedFolder).WorklistClassName;
			}
			else
			{
				_worklistRef = null;
				_worklistClassName = null;
			}
		}

		public override string GetTitle()
		{
			return ReportDocument.GetTitle(_worklistItem);
		}

		public override bool SaveAndClose()
		{
			_component.SaveReport(true);
			return base.Close();
		}

		public override IApplicationComponent GetComponent()
		{
			_component = new ReportingComponent(_worklistItem, _folderName, _worklistRef, _worklistClassName, _shouldOpenImages);
			return _component;
		}

		/// <summary>
		/// Indicates if a user interaction cancelled the opening of the <see cref="ReportingComponent"/>
		/// </summary>
		/// <remarks>
		/// Should only be called after <see mref="Open()"/>
		/// </remarks>
		public bool UserCancelled
		{
			get
			{
				Platform.CheckForNullReference(_component, "_component");
				return _component.UserCancelled;
			}
		}

		public static string GetTitle(ReportingWorklistItemSummary item)
		{
			return string.Format("Report - {0} - {1}", PersonNameFormat.Format(item.PatientName), MrnFormat.Format(item.Mrn));
		}

		public static string StripTitle(string title)
		{
			return title.Replace("Report - ", "");
		}
	}
}
