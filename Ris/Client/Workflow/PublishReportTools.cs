using System;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Actions;
using ClearCanvas.Desktop.Tools;
using ClearCanvas.Ris.Application.Common.ReportingWorkflow;

namespace ClearCanvas.Ris.Client.Workflow
{
	[EnabledStateObserver("apply", "Enabled", "EnabledChanged")]
	public abstract class PublishReportTool<TPublishReportComponent> : Tool<IToolContext>
		where TPublishReportComponent : PublishReportComponent
	{
		private bool _enabled;
		private event EventHandler _enabledChanged;

		public override void Initialize()
		{
			base.Initialize();

			if (this.ContextBase is IReportingWorkflowItemToolContext)
			{
				((IReportingWorkflowItemToolContext)this.ContextBase).SelectionChanged += delegate
				{
					this.Enabled = DetermineEnablement();
				};
			}
		}

		public abstract TPublishReportComponent GetComponent(ReportingWorklistItem item);

		public void Apply()
		{
			if (this.ContextBase is IReportingWorkflowItemToolContext)
			{
				IReportingWorkflowItemToolContext context = (IReportingWorkflowItemToolContext)this.ContextBase;
				ReportingWorklistItem item = CollectionUtils.FirstElement(context.SelectedItems);

				TPublishReportComponent component = GetComponent(item);

				ApplicationComponent.LaunchAsDialog(
					context.DesktopWindow,
					component,
					SR.TitlePrintReport);
			}
		}

		private bool DetermineEnablement()
		{
			if (this.ContextBase is IReportingWorkflowItemToolContext)
			{
				IReportingWorkflowItemToolContext context = (IReportingWorkflowItemToolContext)this.ContextBase;
				if (context.SelectedItems == null || context.SelectedItems.Count != 1)
				{
					return false;
				}

				ReportingWorklistItem item = CollectionUtils.FirstElement(context.SelectedItems);
				if (item.ReportRef == null && item.ProcedureRef == null)
				{
					return false;
				}

				return true;
			}
			return false;
		}

		public bool Enabled
		{
			get
			{
				this.Enabled = DetermineEnablement();
				return _enabled;
			}
			set
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
	}

	[MenuAction("apply", "folderexplorer-items-contextmenu/Print//Fax Report", "Apply")]
	[ButtonAction("apply", "folderexplorer-items-toolbar/Print//Fax Report", "Apply")]
	[IconSet("apply", IconScheme.Colour, "Icons.PrintSmall.png", "Icons.PrintMedium.png", "Icons.PrintLarge.png")]
	[ExtensionOf(typeof(ReportingWorkflowItemToolExtensionPoint))]
	public class PrintReportTool : PublishReportTool<PublishReportComponent>
	{
		public override PublishReportComponent GetComponent(ReportingWorklistItem item)
		{
			return new PublishReportComponent(
					item.PatientProfileRef,
					item.OrderRef,
					item.ProcedureRef,
					item.ReportRef);
		}
	}
}
