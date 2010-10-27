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
using ClearCanvas.Desktop.Tables;
using ClearCanvas.Enterprise.Common;
using ClearCanvas.Ris.Application.Common;
using ClearCanvas.Ris.Application.Common.BrowsePatientData;
using ClearCanvas.Ris.Client.Formatting;

namespace ClearCanvas.Ris.Client
{
	/// <summary>
	/// Defines an interface for providing custom pages to be displayed in the biography order history component.
	/// </summary>
	public interface IBiographyOrderHistoryPageProvider : IExtensionPageProvider<IBiographyOrderHistoryPage, IBiographyOrderHistoryContext>
	{
	}

	/// <summary>
	/// Defines an interface to a custom reporting page.
	/// </summary>
	public interface IBiographyOrderHistoryPage : IExtensionPage
	{
	}

	/// <summary>
	/// Defines an interface for providing a custom page with access to the order context.
	/// </summary>
	public interface IBiographyOrderHistoryContext
	{
		/// <summary>
		/// Gets the reporting worklist item.
		/// </summary>
		OrderListItem OrderListItem { get; }

		/// <summary>
		/// Occurs to indicate that the <see cref="OrderListItem"/> property has changed,
		/// meaning the entire order context is now focused on a different order.
		/// </summary>
		event EventHandler OrderListItemChanged;

		/// <summary>
		/// Gets the order detail associated with the report.
		/// </summary>
		OrderDetail Order { get; }
	}

	/// <summary>
	/// Defines an extension point for adding custom pages to the reporting component.
	/// </summary>
	[ExtensionPoint]
	public class BiographyOrderHistoryPageProviderExtensionPoint : ExtensionPoint<IBiographyOrderHistoryPageProvider>
	{
	}

	/// <summary>
	/// Extension point for views onto <see cref="BiographyOrderHistoryComponent"/>
	/// </summary>
	[ExtensionPoint]
	public class PatientOrderHistoryComponentViewExtensionPoint : ExtensionPoint<IApplicationComponentView>
	{
	}

	/// <summary>
	/// PatientOrderHistoryComponent class
	/// </summary>
	[AssociateView(typeof(PatientOrderHistoryComponentViewExtensionPoint))]
	public class BiographyOrderHistoryComponent : ApplicationComponent
	{
		private class BiographyOrderHistoryContext : IBiographyOrderHistoryContext
		{
			private readonly BiographyOrderHistoryComponent _component;

			public BiographyOrderHistoryContext(BiographyOrderHistoryComponent component)
			{
				_component = component;
			}

			#region IBiographyOrderHistoryContext Members

			public OrderListItem OrderListItem
			{
				get { return _component._selectedOrder; }
			}

			public event EventHandler OrderListItemChanged
			{
				add { _component._orderListItemChanged += value; }
				remove { _component._orderListItemChanged -= value; }
			}

			public OrderDetail Order
			{
				get { return _component._orderDetail; }
			}

			#endregion
		}

		private EntityRef _patientRef;
		private readonly EntityRef _initialSelectedOrderRef;
		private Timer _delaySelectionTimer;

		private readonly OrderListTable _orderList;
		private OrderListItem _selectedOrder;
		private OrderDetail _orderDetail;

		private ChildComponentHost _rightHandComponentContainerHost;
		private TabComponentContainer _rightHandComponentContainer;

		private BiographyOrderDetailViewComponent _orderDetailComponent;
		private VisitDetailViewComponent _visitDetailComponent;
		private AttachedDocumentPreviewComponent _orderDocumentComponent;
		private BiographyOrderReportsComponent _orderReportsComponent;
		private OrderAdditionalInfoComponent _orderAdditionalInfoComponent;

		private List<IBiographyOrderHistoryPage> _extensionPages;
		private event EventHandler _orderListItemChanged;

		/// <summary>
		/// Constructor
		/// </summary>
		public BiographyOrderHistoryComponent(EntityRef initialSelectedOrderRef)
		{
			_initialSelectedOrderRef = initialSelectedOrderRef;
			_orderList = new OrderListTable();
		}

		public override void Start()
		{
			_delaySelectionTimer = new Timer(state => SetSelectedOrder(state as EntityRef), _initialSelectedOrderRef, 250);

			_orderDetailComponent = new BiographyOrderDetailViewComponent();
			_visitDetailComponent = new BiographyVisitDetailViewComponent();
			_orderReportsComponent = new BiographyOrderReportsComponent();
			_orderDocumentComponent = new AttachedDocumentPreviewComponent(true, AttachedDocumentPreviewComponent.AttachmentMode.Order);
			_orderAdditionalInfoComponent = new OrderAdditionalInfoComponent(true);

			_rightHandComponentContainer = new TabComponentContainer();
			_rightHandComponentContainer.Pages.Add(new TabPage("Order Details", _orderDetailComponent));
			_rightHandComponentContainer.Pages.Add(new TabPage("Visit Details", _visitDetailComponent));
			_rightHandComponentContainer.Pages.Add(new TabPage("Additional Info", _orderAdditionalInfoComponent));
			_rightHandComponentContainer.Pages.Add(new TabPage("Reports", _orderReportsComponent));
			_rightHandComponentContainer.Pages.Add(new TabPage("Order Attachments", _orderDocumentComponent));

			// instantiate all extension pages
			_extensionPages = new List<IBiographyOrderHistoryPage>();
			foreach (IBiographyOrderHistoryPageProvider pageProvider in new BiographyOrderHistoryPageProviderExtensionPoint().CreateExtensions())
			{
				_extensionPages.AddRange(pageProvider.GetPages(new BiographyOrderHistoryContext(this)));
			}

			// add extension pages to container and set initial context
			// the container will start those components if the user goes to that page
			foreach (var page in _extensionPages)
			{
				_rightHandComponentContainer.Pages.Add(new TabPage(page.Path, page.GetComponent()));
			}

			_rightHandComponentContainerHost = new ChildComponentHost(this.Host, _rightHandComponentContainer);
			_rightHandComponentContainerHost.StartComponent();

			LoadOrders();

			base.Start();
		}

		public override void Stop()
		{
			if (_rightHandComponentContainerHost != null)
			{
				_rightHandComponentContainerHost.StopComponent();
				_rightHandComponentContainerHost = null;
			}

			if (_delaySelectionTimer != null)
			{
				_delaySelectionTimer.Stop();
				_delaySelectionTimer.Dispose();
				_delaySelectionTimer = null;
			}

			base.Stop();
		}

		public EntityRef PatientRef
		{
			get { return _patientRef; }
			set
			{
				_patientRef = value;

				if (this.IsStarted)
					LoadOrders();
			}
		}

		#region Presentation Model

		public ITable Orders
		{
			get { return _orderList; }
		}

		public ISelection SelectedOrder
		{
			get { return new Selection(_selectedOrder); }
			set
			{
				var newSelection = (OrderListItem)value.Item;
				if (_selectedOrder == newSelection)
					return;

				_selectedOrder = newSelection;
				OrderSelectionChanged();
			}
		}

		public ApplicationComponentHost RightHandComponentContainerHost
		{
			get { return _rightHandComponentContainerHost; }
		}

		public string BannerText
		{
			get
			{
				return _selectedOrder == null ? null :
					string.Format("{0} - {1}", AccessionFormat.Format(_selectedOrder.AccessionNumber),
					_selectedOrder.DiagnosticService.Name);
			}
		}

		#endregion

		private void OrderSelectionChanged()
		{
			if (_selectedOrder == null)
			{
				UpdatePages();
				NotifyPropertyChanged("SelectedOrder");
				NotifyAllPropertiesChanged();
				return;
			}

			LoadOrderDetail(_selectedOrder.OrderRef);
		}

		private void UpdatePages()
		{
			if (_selectedOrder == null)
			{
				_orderDetailComponent.Context = null;
				_visitDetailComponent.Context = null;
				_orderReportsComponent.Context = null;
				_orderDocumentComponent.OrderAttachments = new List<OrderAttachmentSummary>();
				_orderAdditionalInfoComponent.OrderExtendedProperties = new Dictionary<string, string>();
				_orderAdditionalInfoComponent.HealthcareContext = null;
			}
			else
			{
				_orderDetailComponent.Context = new OrderDetailViewComponent.OrderContext(_selectedOrder.OrderRef);
				_visitDetailComponent.Context = new VisitDetailViewComponent.VisitContext(_selectedOrder.VisitRef);
				_orderReportsComponent.Context = new BiographyOrderReportsComponent.ReportsContext(_selectedOrder.OrderRef, _orderDetail.PatientRef, _orderDetail.AccessionNumber);
				_orderDocumentComponent.OrderAttachments = _orderDetail == null ? new List<OrderAttachmentSummary>() : _orderDetail.Attachments;
				_orderAdditionalInfoComponent.OrderExtendedProperties = _orderDetail == null ? new Dictionary<string, string>() : _orderDetail.ExtendedProperties;
				_orderAdditionalInfoComponent.HealthcareContext = _selectedOrder;
			}

			EventsHelper.Fire(_orderListItemChanged, this, EventArgs.Empty);
		}

		private void LoadOrders()
		{
			Async.CancelPending(this);

			if (_patientRef == null)
				return;

			Async.Request(
				this,
				(IBrowsePatientDataService service) =>
				{
					var request = new GetDataRequest
					{
						ListOrdersRequest = new ListOrdersRequest(_patientRef, PatientOrdersQueryDetailLevel.Order)
					};

					return service.GetData(request);
				},
				response =>
				{
					_orderList.Items.Clear();
					_orderList.Items.AddRange(response.ListOrdersResponse.Orders);
					SetSelectedOrder(_initialSelectedOrderRef);

					// HACK:  There are chances that the TableView fire the initial selection after the list is retrieved, overwriting
					// the preferred selection with the first item in the list.  This hack force a selection after a time delay, reducing
					// the chance the first item is automatically selected by TableView.
					_delaySelectionTimer.Start();
				});
		}

		private void SetSelectedOrder(EntityRef orderRef)
		{
			var initialItem = orderRef == null
				? null
				: CollectionUtils.SelectFirst(_orderList.Items, item => item.OrderRef.Equals(orderRef, true));

			this.SelectedOrder = new Selection(initialItem);

			_delaySelectionTimer.Stop();
		}

		private void LoadOrderDetail(EntityRef orderRef)
		{
			if (orderRef == null)
				return;

			Async.Request(
				this,
				(IBrowsePatientDataService service) =>
				{
					var request = new GetDataRequest
					{
						GetOrderDetailRequest = new GetOrderDetailRequest(orderRef, true, true, false, false, true, false)
						{
							IncludeExtendedProperties = true
						}
					};

					return service.GetData(request);
				},
				response =>
				{
					_orderDetail = response.GetOrderDetailResponse.Order;

					UpdatePages();
					NotifyPropertyChanged("SelectedOrder");
					NotifyAllPropertiesChanged();
				},
				exception =>
				{
					ExceptionHandler.Report(exception, this.Host.DesktopWindow);
					NotifyPropertyChanged("SelectedOrder");
					NotifyAllPropertiesChanged();
				});
		}
	}
}
