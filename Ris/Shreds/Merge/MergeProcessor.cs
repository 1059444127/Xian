﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca

// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using ClearCanvas.Common;
using ClearCanvas.Workflow;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Enterprise.Core;

namespace ClearCanvas.Ris.Shreds.Merge
{
	[ExtensionPoint]
	public class MergeHandlerExtensionPoint : ExtensionPoint<IMergeHandler>
	{
	}

	/// <summary>
	/// Processor responsible for processing WorkQueue items with type "Merge".
	/// </summary>
	class MergeProcessor : WorkQueueProcessor
	{
		private const string StageProperty = "Stage";
		private readonly TimeSpan _throttleInterval;

		internal MergeProcessor(MergeShredSettings settings)
			: base(settings.BatchSize, TimeSpan.FromSeconds(settings.EmptyQueueSleepTime))
		{
			_throttleInterval = TimeSpan.FromSeconds(settings.ThrottleInterval);
		}

		protected override string WorkQueueItemType
		{
			get { return MergeWorkQueueItem.Tag; }
		}

		protected override void ActOnItem(WorkQueueItem item)
		{
			var target = PersistenceScope.CurrentContext.Load(MergeWorkQueueItem.GetTargetRef(item), EntityLoadFlags.Proxy);

			var handler = CollectionUtils.SelectFirst<IMergeHandler>(
				new MergeHandlerExtensionPoint().CreateExtensions(),h => h.Supports(target));

			if(handler == null)
				throw new NotSupportedException(
					string.Format("No extension found that supports merging entities of class {0}.", target.GetClass().FullName));

			var stage = GetStage(item);

			stage = handler.Merge(target, stage, PersistenceScope.CurrentContext);

			// update the work item with the new stage value
			item.ExtendedProperties[StageProperty] = stage.ToString();
		}

		protected override bool ShouldReschedule(WorkQueueItem item, out DateTime retryTime)
		{
			var stage = GetStage(item);

			// a stage value of -1 signals that the merge operation is complete
			if (stage == -1)
			{
				retryTime = DateTime.MaxValue;
				return false;
			}

			// re-schedule
			retryTime = Platform.Time + _throttleInterval;
			return true;
		}

		protected override bool ShouldRetry(WorkQueueItem item, Exception error, out DateTime retryTime)
		{
			// Same action as ShouldReschedule, to restart from the last stage.
			return ShouldReschedule(item, out retryTime);
		}

		private static int GetStage(WorkQueueItem item)
		{
			// read the current stage from the work item
			return item.ExtendedProperties.ContainsKey(StageProperty)
					? int.Parse(item.ExtendedProperties[StageProperty])
					: 0;
		}

	}
}