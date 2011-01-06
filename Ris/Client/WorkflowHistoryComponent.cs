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
using System.Text;

using ClearCanvas.Common;
using ClearCanvas.Desktop;
using ClearCanvas.Ris.Application.Common;
using ClearCanvas.Desktop.Tables;
using ClearCanvas.Enterprise.Common;
using ClearCanvas.Ris.Application.Common.BrowsePatientData;

namespace ClearCanvas.Ris.Client
{
	/// <summary>
	/// Extension point for views onto <see cref="WorkflowHistoryComponent"/>.
	/// </summary>
	[ExtensionPoint]
	public sealed class WorkflowHistoryComponentViewExtensionPoint : ExtensionPoint<IApplicationComponentView>
	{
	}

	/// <summary>
	/// WorkflowHistoryComponent class.
	/// </summary>
	[AssociateView(typeof(WorkflowHistoryComponentViewExtensionPoint))]
	public class WorkflowHistoryComponent : ApplicationComponent
	{
		class ProcedureViewComponent : DHtmlComponent
		{
			private readonly WorkflowHistoryComponent _owner;

			public ProcedureViewComponent(WorkflowHistoryComponent owner)
			{
				_owner = owner;
			}

			public override void Start()
			{
				SetUrl(WebResourcesSettings.Default.WorkflowHistoryPageUrl);
				base.Start();
			}

			public void Refresh()
			{
				NotifyAllPropertiesChanged();
			}

			protected override DataContractBase GetHealthcareContext()
			{
				return _owner._selectedProcedure;
			}
		}

		private readonly EntityRef _orderRef;
		private Table<ProcedureDetail> _procedureTable;
		private ProcedureDetail _selectedProcedure;

		private ChildComponentHost _procedureViewComponentHost;

		/// <summary>
		/// Constructor.
		/// </summary>
		public WorkflowHistoryComponent(EntityRef orderRef)
		{
			_orderRef = orderRef;
		}

		/// <summary>
		/// Called by the host to initialize the application component.
		/// </summary>
		public override void Start()
		{
			_procedureTable = new Table<ProcedureDetail>();
			_procedureTable.Columns.Add(new TableColumn<ProcedureDetail, string>("Procedure",
				delegate(ProcedureDetail item) { return Formatting.ProcedureFormat.Format(item); }));

			_procedureViewComponentHost = new ChildComponentHost(this.Host, new ProcedureViewComponent(this));
			_procedureViewComponentHost.StartComponent();

			Platform.GetService<IBrowsePatientDataService>(
				delegate(IBrowsePatientDataService service)
				{
					GetDataRequest request = new GetDataRequest();
					request.GetOrderDetailRequest = new GetOrderDetailRequest(_orderRef, false, true, false, false, false, false);
					GetDataResponse response = service.GetData(request);
					_procedureTable.Items.AddRange(response.GetOrderDetailResponse.Order.Procedures);
				});

			base.Start();
		}

		/// <summary>
		/// Called by the host when the application component is being terminated.
		/// </summary>
		public override void Stop()
		{
			// TODO prepare the component to exit the live phase
			// This is a good place to do any clean up
			base.Stop();
		}

		#region Presentation Model

		public ITable ProcedureTable
		{
			get { return _procedureTable; }
		}

		public ISelection SelectedProcedure
		{
			get { return new Selection(_selectedProcedure); }
			set
			{
				if(!Equals(value.Item, _selectedProcedure))
				{
					_selectedProcedure = (ProcedureDetail) value.Item;
					NotifyPropertyChanged("SelectedProcedure");
					((ProcedureViewComponent)_procedureViewComponentHost.Component).Refresh();
				}
			}
		}

		public ApplicationComponentHost ProcedureViewComponentHost
		{
			get { return _procedureViewComponentHost; }
		}

		#endregion
	}
}
