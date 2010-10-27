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
using ClearCanvas.Common.Shreds;
using ClearCanvas.Enterprise.Core;
using ClearCanvas.Workflow.Brokers;

namespace ClearCanvas.Workflow
{
	/// <summary>
	/// A specialization of <see cref="QueueProcessor{TItem}"/> that operates on items of type <see cref="WorkQueueItem"/>.
	/// </summary>
	public abstract class WorkQueueProcessor : EntityQueueProcessor<WorkQueueItem>
	{
		protected WorkQueueProcessor(int batchSize, TimeSpan sleepTime)
			:base(batchSize, sleepTime)
		{
		}

		/// <summary>
		/// Gets the next batch of items from the queue.
		/// </summary>
		/// <remarks>
		/// Subclasses should not need to override this method.
		/// </remarks>
		/// <param name="batchSize"></param>
		/// <returns></returns>
		protected override IList<WorkQueueItem> GetNextEntityBatch(int batchSize)
		{
			return PersistenceScope.CurrentContext.GetBroker<IWorkQueueItemBroker>().GetPendingItems(WorkQueueItemType, batchSize);
		}

		/// <summary>
		/// Called when <see cref="QueueProcessor{TItem}.ProcessItem"/> succeeds.
		/// </summary>
		/// <remarks>
		/// Subclasses should not need to override this method.
		/// </remarks>
		/// <param name="item"></param>
		protected override void OnItemSucceeded(WorkQueueItem item)
		{
			item.Complete();
		}

		/// <summary>
		/// Called when <see cref="QueueProcessor{TItem}.ProcessItem"/> throws an exception.
		/// </summary>
		/// <remarks>
		/// Subclasses should not need to override this method.
		/// </remarks>
		/// <param name="item"></param>
		/// <param name="error"></param>
		protected override void OnItemFailed(WorkQueueItem item, Exception error)
		{
			// mark item as failed
			item.Fail(error.Message);

			// optionally reschedule the item
			DateTime retryTime;
			if (ShouldRetry(item, error, out retryTime))
				item.Reschedule(retryTime);
		}

		/// <summary>
		/// Gets the type of work queue item that this processor operates on.
		/// </summary>
		protected abstract string WorkQueueItemType { get; }

		/// <summary>
		/// Called after a work item fails, to determine whether it should be re-tried.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="error"></param>
		/// <param name="retryTime"></param>
		/// <returns></returns>
		protected abstract bool ShouldRetry(WorkQueueItem item, Exception error, out DateTime retryTime);

	}
}
