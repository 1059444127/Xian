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
		public class CannotDeleteException : Exception
		{
			public CannotDeleteException(Exception innerException)
				: base("", innerException)
			{
			}
		}

		public class TargetAlreadyDeletedException : Exception
		{
		}

		private const string StageProperty = "Stage";
		private readonly TimeSpan _throttleInterval;

		internal MergeProcessor(MergeShredSettings settings)
			: base(settings.BatchSize, TimeSpan.FromSeconds(settings.EmptyQueueSleepTime))
		{
			_throttleInterval = TimeSpan.FromSeconds(settings.ThrottleInterval);
		}

		public override string Name
		{
			get { return SR.MergeShredName; }
		}

		protected override string WorkQueueItemType
		{
			get { return MergeWorkQueueItem.Tag; }
		}

		protected override void ActOnItem(WorkQueueItem item)
		{
			// We need to know if the target still exist.  Use the default entity flag, rather than proxy.
			var target = PersistenceScope.CurrentContext.Load(MergeWorkQueueItem.GetTargetRef(item), EntityLoadFlags.None);
			if (target == null)
				throw new TargetAlreadyDeletedException();  // target has already been deleted somewhere else.  Nothing to act on.

			var handler = CollectionUtils.SelectFirst<IMergeHandler>(
				new MergeHandlerExtensionPoint().CreateExtensions(),h => h.SupportsTarget(target));

			if(handler == null)
				throw new NotSupportedException(
					string.Format("No extension found that supports merging entities of class {0}.", target.GetClass().FullName));

			var stage = GetStage(item);

			Platform.Log(LogLevel.Info, "Starting merge step on target {0} (stage {1})...", target.GetRef(), stage);

			var nextStage = handler.Merge(target, stage, PersistenceScope.CurrentContext);

			Platform.Log(LogLevel.Info, "Completed merge step on target {0} (stage {1}, next stage is {2}).", target.GetRef(), stage, nextStage);

			// update the work item with the new stage value
			item.ExtendedProperties[StageProperty] = nextStage.ToString();
		}

		protected override bool ShouldReschedule(WorkQueueItem item, Exception error, out DateTime retryTime)
		{
			if (error is TargetAlreadyDeletedException)
			{
				retryTime = DateTime.MinValue;
				return false;
			}

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

		protected override void OnItemFailed(WorkQueueItem item, Exception error)
		{
			if (error is CannotDeleteException)
			{
				RestartItemAtStageZero(item);
			}

			base.OnItemFailed(item, error);
		}

		private void RestartItemAtStageZero(WorkQueueItem item)
		{
			var target = PersistenceScope.CurrentContext.Load(MergeWorkQueueItem.GetTargetRef(item), EntityLoadFlags.Proxy);
			var stage = GetStage(item);
			var nextStage = 0;

			Platform.Log(LogLevel.Info, "Failed to complete merge step on target {0} (stage {1}).  Restarting at stage 0.", target.GetRef(), stage, nextStage);

			// update the work item with the new stage value
			item.ExtendedProperties[StageProperty] = nextStage.ToString();
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
