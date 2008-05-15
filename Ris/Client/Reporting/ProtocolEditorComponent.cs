#region License

// Copyright (c) 2006-2008, ClearCanvas Inc.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met:
//
//    * Redistributions of source code must retain the above copyright notice, 
//      this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, 
//      this list of conditions and the following disclaimer in the documentation 
//      and/or other materials provided with the distribution.
//    * Neither the name of ClearCanvas Inc. nor the names of its contributors 
//      may be used to endorse or promote products derived from this software without 
//      specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR 
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
// OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
// ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY 
// OF SUCH DAMAGE.

#endregion

using System;
using System.Collections.Generic;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Tables;
using ClearCanvas.Enterprise.Common;
using ClearCanvas.Ris.Application.Common;
using ClearCanvas.Ris.Application.Common.ProtocollingWorkflow;
using ClearCanvas.Ris.Application.Common.ReportingWorkflow;

namespace ClearCanvas.Ris.Client.Reporting
{
	public enum ClaimProtocolResult
	{
		AlreadyClaimed,
		Claimed,
		Failed
	}

	/// <summary>
	/// Extension point for views onto <see cref="ProtocolEditorComponent"/>
	/// </summary>
	[ExtensionPoint]
	public class ProtocolEditorComponentViewExtensionPoint : ExtensionPoint<IApplicationComponentView>
	{
	}

	/// <summary>
	/// ProtocolEditorComponent class
	/// </summary>
	[AssociateView(typeof(ProtocolEditorComponentViewExtensionPoint))]
	public class ProtocolEditorComponent : ApplicationComponent
	{
		internal interface IDefaultProtocolGroupSettingsProvider
		{
			string this[string procedureName] { get; set; }
			string GetSuggestedDefault();
		}

		private class DefaultProtocolGroupSettingsProvider : IDefaultProtocolGroupSettingsProvider
		{
			#region IDefaultProtocolGroupSettingsProvider Members

			public string this[string procedureName]
			{
				get
				{
					return ProtocolGroupSettings.Default.GetDefaultProtocolGroup(procedureName);
				}
				set
				{
					if (String.IsNullOrEmpty(value)) return;

					ProtocolGroupSettings.Default.SetDefaultProtocolGroup(value, procedureName);
				}
			}

			public string GetSuggestedDefault()
			{
				return ProtocolGroupSettings.Default.LastDefaultProtocolGroup;
			}

			#endregion
		}

		#region Private Fields

		private ReportingWorklistItem _worklistItem;
		private EntityRef _orderRef;

		private List<EnumValueInfo> _protocolUrgencyChoices;

		private readonly ProtocolEditorProcedurePlanSummaryTable _procedurePlanSummaryTable;
		private ProtocolEditorProcedurePlanSummaryTableItem _selectedProcodurePlanSummaryTableItem;
		private event EventHandler _selectedProcedurePlanSummaryTableItemChanged;

		private readonly ProtocolCodeTable _availableProtocolCodes;
		private readonly ProtocolCodeTable _selectedProtocolCodes;
		private ProtocolCodeDetail _selectedProtocolCodesSelection;

		private bool _acceptEnabled;
		private bool _rejectEnabled;
		private bool _suspendEnabled;
		private bool _saveEnabled;

		private List<ProtocolGroupSummary> _protocolGroupChoices;
		private ProtocolGroupSummary _protocolGroup;
		private string _defaultProtocolGroupName;
		private readonly IDefaultProtocolGroupSettingsProvider _defaultProtocolGroupProvider = new DefaultProtocolGroupSettingsProvider();

		private bool _protocolNextItem;
		private readonly ProtocollingComponentMode _componentMode;

		private event EventHandler _protocolAccepted;
		private event EventHandler _protocolRejected;
		private event EventHandler _protocolSuspended;
		private event EventHandler _protocolSaved;
		private event EventHandler _protocolSkipped;
		private event EventHandler _protocolCancelled;

		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		public ProtocolEditorComponent(ReportingWorklistItem worklistItem, ProtocollingComponentMode mode)
		{
			_worklistItem = worklistItem;
			_componentMode = mode;

			_protocolNextItem = _componentMode == ProtocollingComponentMode.Assign;

			_availableProtocolCodes = new ProtocolCodeTable();
			_selectedProtocolCodes = new ProtocolCodeTable();
			_selectedProtocolCodes.Items.ItemsChanged += SelectedProtocolCodesChanged;
			_protocolGroupChoices = new List<ProtocolGroupSummary>();
			_procedurePlanSummaryTable = new ProtocolEditorProcedurePlanSummaryTable();
		}


		public ReportingWorklistItem WorklistItem
		{
			get { return _worklistItem; }
			set
			{
				_worklistItem = value;
				StartWorklistItem();
			}
		}

		#region ApplicationComponent overrides

		public override void Start()
		{
			StartWorklistItem();
			base.Start();
		}

		private void StartWorklistItem()
		{
			// begin with validation turned off
			ShowValidation(false);

			Platform.GetService<IProtocollingWorkflowService>(
				delegate(IProtocollingWorkflowService service)
				{
					GetProtocolFormDataResponse response = service.GetProtocolFormData(new GetProtocolFormDataRequest());
					_protocolUrgencyChoices = response.ProtocolUrgencyChoices;

					ClaimProtocolResult claimResult = ClaimProtocolResult.AlreadyClaimed;

					// Only claim unassigned protocols
					if (_componentMode == ProtocollingComponentMode.Assign)
					{
						claimResult = ClaimOrderProtocol(_worklistItem.OrderRef, service);
					}

					if (claimResult != ClaimProtocolResult.Failed)
					{
						InitializeProcedurePlanSummary(service);
						InitializeActionEnablement(service);
					}
					else
					{
						EventsHelper.Fire(_protocolSkipped, this, EventArgs.Empty);
					}
				});
		}


		private ClaimProtocolResult ClaimOrderProtocol(EntityRef orderRef, IProtocollingWorkflowService service)
		{
			try
			{
				StartOrderProtocolResponse response = service.StartOrderProtocol(new StartOrderProtocolRequest(orderRef));
				return response.ProtocolClaimed ? ClaimProtocolResult.Claimed : ClaimProtocolResult.AlreadyClaimed;
			}
			catch (Exception)
			{
				return ClaimProtocolResult.Failed;
			}
		}

		#endregion

		#region Presentation Model

		public string Urgency
		{
			get
			{
				if (_selectedProcodurePlanSummaryTableItem != null &&
					_selectedProcodurePlanSummaryTableItem.ProtocolDetail.Urgency != null)
				{
					return _selectedProcodurePlanSummaryTableItem.ProtocolDetail.Urgency.Value;
				}
				else
				{
					return null;
				}
			}
			set
			{
				_selectedProcodurePlanSummaryTableItem.ProtocolDetail.Urgency = EnumValueUtils.MapDisplayValue(_protocolUrgencyChoices, value);
				this.Modified = true;
			}
		}

		public List<string> UrgencyChoices
		{
			get { return EnumValueUtils.GetDisplayValues(_protocolUrgencyChoices); }
		}

		public IList<string> ProtocolGroupChoices
		{
			get
			{
				return CollectionUtils.Map<ProtocolGroupSummary, string>(
					_protocolGroupChoices,
					delegate(ProtocolGroupSummary summary) { return AppendDefaultText(summary.Name); });
			}
		}

		public string ProtocolGroup
		{
			get { return _protocolGroup == null ? "" : AppendDefaultText(_protocolGroup.Name); }
			set
			{
				_protocolGroup = (value == null)
									? null
									: CollectionUtils.SelectFirst<ProtocolGroupSummary>(
										_protocolGroupChoices,
										delegate(ProtocolGroupSummary summary) { return summary.Name == RemoveDefaultText(value); });

				ProtocolGroupSelectionChanged();
			}
		}

		private string AppendDefaultText(string value)
		{
			if (value == _defaultProtocolGroupName)
				return value + " (Default)";
			else
				return value;
		}

		private string RemoveDefaultText(string value)
		{
			if (value.EndsWith(" (Default)"))
				value.Replace(" (Default)", "");

			return value;
		}

		public bool SetDefaultProtocolGroupEnabled
		{
			get
			{
				return _defaultProtocolGroupName != _protocolGroup.Name;
			}
		}

		public void SetDefaultProtocolGroup()
		{
			_defaultProtocolGroupProvider[_selectedProcodurePlanSummaryTableItem.ProcedureDetail.Type.Name] = _defaultProtocolGroupName = _protocolGroup.Name;

			NotifyPropertyChanged("ProtocolGroupChoices");
			NotifyPropertyChanged("ProtocolGroup");
			NotifyPropertyChanged("SetDefaultProtocolGroupEnabled");
		}

		public ITable AvailableProtocolCodesTable
		{
			get { return _availableProtocolCodes; }
		}

		public ITable SelectedProtocolCodesTable
		{
			get { return _selectedProtocolCodes; }
		}

		// this isn't actually used by the component, but we need it for validation
		// so that we can show an error icon next to the "selected items" table
		public ISelection SelectedProtocolCodesSelection
		{
			get { return new Selection(_selectedProtocolCodesSelection); }
			set
			{
				_selectedProtocolCodesSelection = (ProtocolCodeDetail)value.Item;
			}
		}

		public ITable ProcedurePlanSummaryTable
		{
			get { return _procedurePlanSummaryTable; }
		}

		public ISelection SelectedProcedure
		{
			get { return new Selection(_selectedProcodurePlanSummaryTableItem); }
			set
			{
				ProtocolEditorProcedurePlanSummaryTableItem item = (ProtocolEditorProcedurePlanSummaryTableItem)value.Item;
				ProcedureSelectionChanged(item);
			}
		}

		public event EventHandler SelectionChanged
		{
			add { _selectedProcedurePlanSummaryTableItemChanged += value; }
			remove { _selectedProcedurePlanSummaryTableItemChanged -= value; }
		}

		/// <summary>
		/// Specifies to containing <see cref="ProtocollingComponentDocument"/> if the next <see cref="ReportingWorklistItem"/> should be protocolled
		/// </summary>
		public bool ProtocolNextItem
		{
			get { return _protocolNextItem; }
			set { _protocolNextItem = value; }
		}

		public bool ProtocolNextItemEnabled
		{
			get { return _componentMode == ProtocollingComponentMode.Assign; }
		}

		#region Accept

		public void Accept()
		{
			// don't allow accept if there are validation errors
			if (HasValidationErrors)
			{
				ShowValidation(true);
				return;
			}

			try
			{
				Platform.GetService<IProtocollingWorkflowService>(
					delegate(IProtocollingWorkflowService service)
					{
						SaveProtocols(service);
						service.AcceptOrderProtocol(new AcceptOrderProtocolRequest(_orderRef));
					});

				EventsHelper.Fire(_protocolAccepted, this, EventArgs.Empty);
			}
			catch (Exception e)
			{
				ExceptionHandler.Report(e, this.Host.DesktopWindow);
			}
		}

		public bool AcceptEnabled
		{
			get { return _acceptEnabled; }
		}

		public event EventHandler ProtocolAccepted
		{
			add { _protocolAccepted += value; }
			remove { _protocolAccepted -= value; }
		}

		#endregion

		#region Reject

		public void Reject()
		{
			try
			{
				EnumValueInfo reason = GetRejectOrSuspendReason("Reject Reason");

				if (reason == null)
					return;

				Platform.GetService<IProtocollingWorkflowService>(
					delegate(IProtocollingWorkflowService service)
					{
						SaveProtocols(service);
						service.RejectOrderProtocol(new RejectOrderProtocolRequest(_orderRef, reason));
					});

				EventsHelper.Fire(_protocolRejected, this, EventArgs.Empty);
			}
			catch (Exception e)
			{
				ExceptionHandler.Report(e, this.Host.DesktopWindow);
			}
		}

		public bool RejectEnabled
		{
			get { return _rejectEnabled; }
		}

		public event EventHandler ProtocolRejected
		{
			add { _protocolRejected += value; }
			remove { _protocolRejected -= value; }
		}

		#endregion

		#region Suspend

		public void Suspend()
		{
			try
			{
				EnumValueInfo reason = GetRejectOrSuspendReason("Suspend Reason");

				if (reason == null)
					return;

				Platform.GetService<IProtocollingWorkflowService>(
					delegate(IProtocollingWorkflowService service)
					{
						SaveProtocols(service);
						service.SuspendOrderProtocol(new SuspendOrderProtocolRequest(_orderRef, reason));
					});

				EventsHelper.Fire(_protocolSuspended, this, EventArgs.Empty);
			}
			catch (Exception e)
			{
				ExceptionHandler.Report(e, this.Host.DesktopWindow);
			}
		}

		public bool SuspendEnabled
		{
			get { return _suspendEnabled; }
		}

		public event EventHandler ProtocolSuspended
		{
			add { _protocolSuspended += value; }
			remove { _protocolSuspended -= value; }
		}

		#endregion

		#region Save

		public void Save()
		{
			try
			{
				Platform.GetService<IProtocollingWorkflowService>(
					delegate(IProtocollingWorkflowService service)
					{
						SaveProtocols(service);
					});

				EventsHelper.Fire(_protocolSaved, this, EventArgs.Empty);
			}
			catch (Exception e)
			{
				ExceptionHandler.Report(e, this.Host.DesktopWindow);
			}
		}

		public bool SaveEnabled
		{
			get { return _saveEnabled; }
		}

		public event EventHandler ProtocolSaved
		{
			add { _protocolSaved += value; }
			remove { _protocolSaved -= value; }
		}

		#endregion

		#region Skip

		public void Skip()
		{
			try
			{
				// Unclaim any newly started protocols
				if (_componentMode == ProtocollingComponentMode.Assign)
				{
					Platform.GetService<IProtocollingWorkflowService>(
						delegate(IProtocollingWorkflowService service)
						{
							service.DiscardOrderProtocol(new DiscardOrderProtocolRequest(_orderRef));
						});
				}

				EventsHelper.Fire(_protocolSkipped, this, EventArgs.Empty);
			}
			catch (Exception e)
			{
				ExceptionHandler.Report(e, this.Host.DesktopWindow);
			}
		}

		public bool SkipEnabled
		{
			get { return _protocolNextItem && this.ProtocolNextItemEnabled; }
		}

		public event EventHandler ProtocolSkipped
		{
			add { _protocolSkipped += value; }
			remove { _protocolSkipped -= value; }
		}

		#endregion

		#region Close

		public void Close()
		{
			try
			{
				// Unclaim any newly started protocols
				if (_componentMode == ProtocollingComponentMode.Assign)
				{
					Platform.GetService<IProtocollingWorkflowService>(
						delegate(IProtocollingWorkflowService service)
						{
							service.DiscardOrderProtocol(new DiscardOrderProtocolRequest(_orderRef));
						});
				}

				EventsHelper.Fire(_protocolCancelled, this, EventArgs.Empty);
			}
			catch (Exception e)
			{
				ExceptionHandler.Report(e, this.Host.DesktopWindow);
			}
		}

		public event EventHandler ProtocolCancelled
		{
			add { _protocolCancelled += value; }
			remove { _protocolCancelled -= value; }
		}

		#endregion

		#endregion

		#region Private Methods

		private EnumValueInfo GetRejectOrSuspendReason(string title)
		{
			ProtocolReasonComponent component = new ProtocolReasonComponent();

			ApplicationComponentExitCode exitCode = LaunchAsDialog(this.Host.DesktopWindow, component, title);

			if (exitCode == ApplicationComponentExitCode.Accepted)
			{
				return component.Reason;
			}

			return null;
		}

		private void InitializeProcedurePlanSummary(IProtocollingWorkflowService service)
		{
			GetProcedurePlanForProtocollingWorklistItemRequest procedurePlanRequest = new GetProcedurePlanForProtocollingWorklistItemRequest(_worklistItem.ProcedureStepRef);
			GetProcedurePlanForProtocollingWorklistItemResponse procedurePlanResponse = service.GetProcedurePlanForProtocollingWorklistItem(procedurePlanRequest);

			if (procedurePlanResponse.ProcedurePlan != null)
			{
				_orderRef = procedurePlanResponse.ProcedurePlan.OrderRef;

				_procedurePlanSummaryTable.Items.Clear();

				foreach (ProcedureDetail rp in procedurePlanResponse.ProcedurePlan.Procedures)
				{
					GetProcedureProtocolRequest protocolRequest = new GetProcedureProtocolRequest(rp.ProcedureRef);
					GetProcedureProtocolResponse protocolResponse = service.GetProcedureProtocol(protocolRequest);

					if (protocolResponse.ProtocolRef != null)
					{
						_procedurePlanSummaryTable.Items.Add(new ProtocolEditorProcedurePlanSummaryTableItem(rp, protocolResponse.ProtocolRef, protocolResponse.ProtocolDetail));
					}
				}
				_procedurePlanSummaryTable.Sort();
			}
		}

		private void InitializeActionEnablement(IProtocollingWorkflowService service)
		{
			GetProtocolOperationEnablementResponse response = service.GetProtocolOperationEnablement(new GetProtocolOperationEnablementRequest(_worklistItem.ProcedureStepRef));

			_acceptEnabled = response.AcceptEnabled;
			_suspendEnabled = response.SuspendEnabled;
			_rejectEnabled = response.RejectEnabled;
			_saveEnabled = response.SaveEnabled;
		}

		private void SelectedProtocolCodesChanged(object sender, ItemChangedEventArgs e)
		{
			ProtocolCodeDetail detail = (ProtocolCodeDetail)e.Item;
			switch (e.ChangeType)
			{
				case ItemChangeType.ItemAdded:
					_selectedProcodurePlanSummaryTableItem.ProtocolDetail.Codes.Add(detail);
					break;
				case ItemChangeType.ItemRemoved:
					_selectedProcodurePlanSummaryTableItem.ProtocolDetail.Codes.Remove(detail);
					break;
				default:
					return;
			}

			this.Modified = true;
		}

		private void ProcedureSelectionChanged(ProtocolEditorProcedurePlanSummaryTableItem item)
		{
			// Same selection, do nothing
			if (item == _selectedProcodurePlanSummaryTableItem)
			{
				return;
			}

			ResetDocument();

			// Ensure something is selected
			if (item != null)
			{
				//Refresh protocol
				try
				{
					Platform.GetService<IProtocollingWorkflowService>(
						delegate(IProtocollingWorkflowService service)
						{
							// Load available protocol groups
							ListProtocolGroupsForProcedureRequest request = new ListProtocolGroupsForProcedureRequest(item.ProcedureDetail.ProcedureRef);
							ListProtocolGroupsForProcedureResponse response = service.ListProtocolGroupsForProcedure(request);

							_protocolGroupChoices = response.ProtocolGroups;
							//_protocolGroup = response.InitialProtocolGroup;
							_protocolGroup = GetInitialProtocolGroup(item);

							RefreshAvailableProtocolCodes(item.ProtocolDetail.Codes, service);

							// fill out selected item codes
							_selectedProtocolCodes.Items.Clear();
							_selectedProtocolCodes.Items.AddRange(item.ProtocolDetail.Codes);
						});
				}
				catch (Exception e)
				{
					ExceptionHandler.Report(e, this.Host.DesktopWindow);
				}
			}

			_selectedProcodurePlanSummaryTableItem = item;
			EventsHelper.Fire(_selectedProcedurePlanSummaryTableItemChanged, this, EventArgs.Empty);

			NotifyPropertyChanged("ProtocolGroupChoices");
			//NotifyPropertyChanged("Urgency");
		}

		private ProtocolGroupSummary GetInitialProtocolGroup(ProtocolEditorProcedurePlanSummaryTableItem item)
		{
			ProtocolGroupSummary defaultProtocolGroup = null;

			// Use the default if one exists for this procedure.
			// Otherwise, get a suggested initial group and set the default to the suggested value
			_defaultProtocolGroupName = _defaultProtocolGroupProvider[item.ProcedureDetail.Type.Name]
									?? (_defaultProtocolGroupProvider[item.ProcedureDetail.Type.Name] = GetSuggestedDefault(item.ProcedureDetail.Type.Name));

			if (string.IsNullOrEmpty(_defaultProtocolGroupName) == false)
			{
				defaultProtocolGroup = CollectionUtils.SelectFirst<ProtocolGroupSummary>(
					_protocolGroupChoices,
					delegate(ProtocolGroupSummary summary) { return summary.Name == _defaultProtocolGroupName; });
			}

			defaultProtocolGroup = defaultProtocolGroup ?? CollectionUtils.FirstElement(_protocolGroupChoices);

			return defaultProtocolGroup;
		}

		private string GetSuggestedDefault(string procedureName)
		{
			if (CollectionUtils.Contains(
				_protocolGroupChoices,
				delegate(ProtocolGroupSummary pgs) { return pgs.Name == _defaultProtocolGroupProvider.GetSuggestedDefault(); }))
			{
				return _defaultProtocolGroupProvider.GetSuggestedDefault();
			}
			else
			{
				return null;
			}
		}

		private void ProtocolGroupSelectionChanged()
		{
			try
			{
				Platform.GetService<IProtocollingWorkflowService>(
					delegate(IProtocollingWorkflowService service)
					{
						RefreshAvailableProtocolCodes(_selectedProcodurePlanSummaryTableItem.ProtocolDetail.Codes, service);
					});
			}
			catch (Exception e)
			{
				ExceptionHandler.Report(e, this.Host.DesktopWindow);
			}
		}

		// Refresh the list of available protocol codes when the list of protocol groups is initially loaded 
		// and whenever the protocol group selection changes
		private void RefreshAvailableProtocolCodes(IEnumerable<ProtocolCodeDetail> existingSelectedCodes, IProtocollingWorkflowService service)
		{
			_availableProtocolCodes.Items.Clear();

			if (_protocolGroup != null)
			{
				GetProtocolGroupDetailRequest protocolCodesDetailRequest = new GetProtocolGroupDetailRequest(_protocolGroup);
				GetProtocolGroupDetailResponse protocolCodesDetailResponse = service.GetProtocolGroupDetail(protocolCodesDetailRequest);

				_availableProtocolCodes.Items.AddRange(protocolCodesDetailResponse.ProtocolGroup.Codes);

				// Make existing code selections unavailable
				foreach (ProtocolCodeDetail code in existingSelectedCodes)
				{
					_availableProtocolCodes.Items.Remove(code);
				}
			}
		}

		private void ResetDocument()
		{
			_protocolGroup = null;
			_protocolGroupChoices = new List<ProtocolGroupSummary>();
			_availableProtocolCodes.Items.Clear();
			_selectedProtocolCodes.Items.Clear();
		}

		private void SaveProtocols(IProtocollingWorkflowService service)
		{
			foreach (ProtocolEditorProcedurePlanSummaryTableItem item in _procedurePlanSummaryTable.Items)
			{
				service.SaveProtocol(new SaveProtocolRequest(item.ProtocolRef, item.ProtocolDetail));
			}
			this.Modified = false;
		}

		#endregion
	}
}
