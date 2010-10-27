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
using ClearCanvas.Enterprise.Common;
using ClearCanvas.Ris.Application.Common;

namespace ClearCanvas.Ris.Client.Workflow
{
	public enum WorklistItemCompletedResult
	{
		Completed,
		Skipped,
		Invalid
	}

	public interface IContinuousWorkflowComponentMode
	{
		bool ShouldUnclaim { get; }
		bool ShowStatusText { get; }
		bool CanContinue { get; }
	}

	public abstract class ContinuousWorkflowComponentMode : IContinuousWorkflowComponentMode
	{
		private readonly bool _shouldUnclaim;
		private readonly bool _showStatusText;
		private readonly bool _canContinue;

		protected ContinuousWorkflowComponentMode(bool shouldUnclaim, bool showStatusText, bool canContinue)
		{
			_shouldUnclaim = shouldUnclaim;
			_showStatusText = showStatusText;
			_canContinue = canContinue;
		}

		#region IContinuousWorkflowComponentMode Members

		public bool ShouldUnclaim
		{
			get { return _shouldUnclaim; }
		}

		public bool ShowStatusText
		{
			get { return _showStatusText; }
		}

		public bool CanContinue
		{
			get { return _canContinue; }
		}

		#endregion
	}

	public interface IWorklistItemManager<TWorklistItem>
	{
		/// <summary>
		/// The current <see cref="TWorklistItem"/>
		/// </summary>
		TWorklistItem WorklistItem { get; }

		/// <summary>
		/// Used to request the next <see cref="TWorklistItem"/> to be loaded.
		/// </summary>
		/// <param name="result">Indicates whether previous item was completed or skipped.</param>
		void ProceedToNextWorklistItem(WorklistItemCompletedResult result);

		/// <summary>
		/// Used to request the next <see cref="TWorklistItem"/> to be loaded.
		/// </summary>
		/// <param name="result">Indicates whether previous item was completed or skipped.</param>
		/// <param name="overrideDoNotPerformNextItem">Override the default behaviour.  Complete the current item and do not proceed to next item.</param>
		void ProceedToNextWorklistItem(WorklistItemCompletedResult result, bool overrideDoNotPerformNextItem);

		/// <summary>
		/// Specify a list of <see cref="TWorklistItem"/> that should be excluded from <see mref="ProceedToNextWorklistItem"/>
		/// </summary>
		/// <param name="worklistItems"></param>
		void IgnoreWorklistItems(List<TWorklistItem> worklistItems);

		/// <summary>
		/// Fired when the next worklist item is available.
		/// </summary>
		event EventHandler WorklistItemChanged;

		bool ShouldUnclaim { get; }

		/// <summary>
		/// A string indicating the name of the source folder system and counts of available, completed and skipped items.
		/// </summary>
		string StatusText { get; }

		bool StatusTextVisible { get; }

		/// <summary>
		/// Specifies if the next <see cref="TWorklistItem"/> should be reported
		/// </summary>
		bool ReportNextItem { get; set; }

		/// <summary>
		/// 
		/// </summary>
		bool ReportNextItemEnabled { get; }

		/// <summary>
		/// Specifies if a "Skip" button should be enabled based on mode and value of <see cref="ReportNextItem"/>
		/// </summary>
		bool CanSkipItem { get; }
	}

	public abstract class WorklistItemManager<TWorklistItem, TWorkflowService> : IWorklistItemManager<TWorklistItem>
		where TWorklistItem : WorklistItemSummaryBase
		where TWorkflowService : IWorklistService<TWorklistItem>
	{
		#region Private fields

		private TWorklistItem _worklistItem;
		private event EventHandler _worklistItemChanged;

		private IContinuousWorkflowComponentMode _componentMode;
		private readonly string _folderName;
		private readonly EntityRef _worklistRef;
		private readonly string _worklistClassName;
		private int _allAvailableItemsCount = 0;
		private int _completedItemsCount = 0;
		private int _skippedItemsCount = 0;
		private bool _isInitialItem = true;

		private readonly List<TWorklistItem> _visitedItems;
		private readonly Queue<TWorklistItem> _worklistCache;

		private bool _reportNextItem;

		private bool _isInitialized = false;

		#endregion

		protected abstract IContinuousWorkflowComponentMode GetMode<TWorklistITem>(TWorklistItem worklistItem);
		protected abstract string TaskName { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// Only one of worklistRef or worklistClassName should be specified.  worklistRef will take precedence if both are provided.
		/// </remarks>
		/// <param name="folderName">Folder system name, displayed in status text</param>
		/// <param name="worklistRef">An <see cref="EntityRef"/> for the folder from which additional worklist items should be loaded.</param>
		/// <param name="worklistClassName">A name for the folder class from which additional worklist items should be loaded.</param>
		protected WorklistItemManager(string folderName, EntityRef worklistRef, string worklistClassName)
		{
			_folderName = folderName;
			_worklistRef = worklistRef;
			_worklistClassName = worklistClassName;

			_visitedItems = new List<TWorklistItem>();
			_worklistCache = new Queue<TWorklistItem>();
		}

		public void Initialize(TWorklistItem worklistItem)
		{
			this.Initialize(worklistItem, GetMode<TWorklistItem>(worklistItem));
		}

		public void Initialize(TWorklistItem worklistItem, IContinuousWorkflowComponentMode mode)
		{
			_worklistItem = worklistItem;
			_componentMode = mode;
			_reportNextItem = WorklistItemManagerSettings.Default.ShouldProceedToNextItem
				&& this.ReportNextItemEnabled;

			_isInitialized = true;
		}

		public TWorklistItem WorklistItem
		{
			get
			{
				if (!_isInitialized)
					throw new Exception("Not initialized.");

				return _worklistItem;
			}
		}

		public void ProceedToNextWorklistItem(WorklistItemCompletedResult result)
		{
			ProceedToNextWorklistItem(result, false);
		}

		public void ProceedToNextWorklistItem(WorklistItemCompletedResult result, bool overrideDoNotPerformNextItem)
		{
			if (result == WorklistItemCompletedResult.Completed)
			{
				_completedItemsCount++;
				_visitedItems.Add(_worklistItem);
			}
			else if (result == WorklistItemCompletedResult.Skipped)
			{
				_skippedItemsCount++;
				_visitedItems.Add(_worklistItem);
			}

			_isInitialItem = false;

			if (_reportNextItem && _componentMode.CanContinue && overrideDoNotPerformNextItem == false)
			{
				if (_worklistCache.Count == 0)
				{
					RefreshWorklistItemCache();
				}

				_worklistItem = _worklistCache.Count > 0 ? _worklistCache.Dequeue() : null;
				_allAvailableItemsCount--;
			}
			else
			{
				_worklistItem = null;
			}

			EventsHelper.Fire(_worklistItemChanged, this, EventArgs.Empty);
		}

		public void IgnoreWorklistItems(List<TWorklistItem> interpretations)
		{
			_visitedItems.AddRange(interpretations);
			RefreshWorklistItemCache();
		}

		public event EventHandler WorklistItemChanged
		{
			add { _worklistItemChanged += value; }
			remove { _worklistItemChanged -= value; }
		}

		public bool ShouldUnclaim
		{
			get { return _componentMode.ShouldUnclaim; }
		}

		#region Presentation Model

		public string StatusText
		{
			get
			{
				string status = string.Format(SR.FormatContinuousWorkflowDescription, this.TaskName, _folderName);

				if (!_isInitialItem)
				{
					status = status + string.Format(SR.FormatReportingStatusText, _allAvailableItemsCount, _completedItemsCount, _skippedItemsCount);
				}

				return status;
			}
		}

		public bool StatusTextVisible
		{
			get { return _componentMode.ShowStatusText && this.HasValidWorklistContext; }
		}

		public bool ReportNextItem
		{
			get { return _reportNextItem; }
			set
			{
				_reportNextItem = value;
				WorklistItemManagerSettings.Default.ShouldProceedToNextItem = value;
				WorklistItemManagerSettings.Default.Save();
			}
		}

		public bool ReportNextItemEnabled
		{
			get { return _componentMode.CanContinue && this.HasValidWorklistContext; }
		}

		public bool CanSkipItem
		{
			get { return _reportNextItem && this.ReportNextItemEnabled; }
		}

		#endregion

		#region Private methods

		private bool HasValidWorklistContext
		{
			get { return _worklistRef != null || _worklistClassName != null; }
		}

		private void RefreshWorklistItemCache()
		{
			_worklistCache.Clear();

			Platform.GetService<TWorkflowService>(service =>
				{
					var workingFacilityRef = LoginSession.Current.WorkingFacility.FacilityRef;
					var request = _worklistRef != null
						? new QueryWorklistRequest(_worklistRef, true, true, DowntimeRecovery.InDowntimeRecoveryMode, workingFacilityRef)
						: new QueryWorklistRequest(_worklistClassName, true, true, DowntimeRecovery.InDowntimeRecoveryMode, workingFacilityRef);

					// Only cache the first 50 items instead of the whole worklist; querying for the whole worklist faults the connection when the
					// result set is ~1100 items or more.
					request.Page = new SearchResultPage(0, 50);

					var response = service.QueryWorklist(request);

					_allAvailableItemsCount = response.ItemCount;
					foreach (var item in response.WorklistItems)
					{
						if (WorklistItemWasPreviouslyVisited(item) == false)
						{
							_worklistCache.Enqueue(item);
						}
						else
						{
							// If any excluded items are still in the worklist, don't include them in the available items count
							_allAvailableItemsCount--;
						}
					}
				});
		}

		private bool WorklistItemWasPreviouslyVisited(TWorklistItem item)
		{
			return CollectionUtils.Contains(_visitedItems, skippedItem => skippedItem.AccessionNumber == item.AccessionNumber);
		}

		#endregion
	}
}