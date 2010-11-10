﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca

// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using ClearCanvas.Enterprise.Core;

namespace ClearCanvas.Ris.Shreds.Merge
{
	/// <summary>
	/// Abstract base implementation of <see cref="IMergeHandler"/>.
	/// </summary>
	/// <typeparam name="TTarget"></typeparam>
	public abstract class MergeHandlerBase<TTarget> : IMergeHandler
		where TTarget : Entity
	{
		protected delegate void Action<T1, T2>(T1 x, T2 y);
		protected delegate int MergeStep(TTarget target, int stage, IPersistenceContext context);
		protected delegate IList<TItem> BatchProvider<TItem, TItemCriteria>(TTarget practitioner, Action<TItemCriteria> priorityFilter, int batchSize, IPersistenceContext context);

		private readonly int _itemsPerTransaction;

		protected MergeHandlerBase(int itemsPerTransaction)
		{
			_itemsPerTransaction = itemsPerTransaction;
		}


		#region IMergeHandler members

		public bool Supports(Entity entity)
		{
			return entity.Is<TTarget>();
		}

		public int Merge(Entity entity, int stage, IPersistenceContext context)
		{
			var practitioner = (TTarget)entity;
			var steps = this.MergeSteps;

			if (stage < 0 || stage >= steps.Length)
				throw new InvalidOperationException("Invalid stage.");

			var step = steps[stage];
			return step(practitioner, stage, context);
		}

		#endregion

		#region Protected API
		
		/// <summary>
		/// Gets the set of merge steps to be performed.
		/// </summary>
		/// <remarks>
		/// Defines a set of migration steps to be executed. The first step in the list is always executed first.
		/// The execution of each step returns an integer indicating which step to execute next.
		/// </remarks>
		protected abstract MergeStep[] MergeSteps { get; }

		/// <summary>
		/// Helper method for defining a <see cref="MergeStep"/> to migrate a batch of items.
		/// </summary>
		/// <typeparam name="TItem"></typeparam>
		/// <typeparam name="TItemCriteria"></typeparam>
		/// <param name="target"></param>
		/// <param name="stage"></param>
		/// <param name="batchProvider"></param>
		/// <param name="priorityFilter"></param>
		/// <param name="processAction"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		protected int Migrate<TItem, TItemCriteria>(
			TTarget target,
			int stage,
			BatchProvider<TItem, TItemCriteria> batchProvider,
			Action<TItemCriteria> priorityFilter,
			Action<TTarget, TItem> processAction,
			IPersistenceContext context)
		{
			// get batch
			var batch = batchProvider(target, priorityFilter, _itemsPerTransaction, context);
			
			// process items
			foreach (var item in batch)
			{
				processAction(target, item);
			}

			// if any items were processed in this batch, there may be more items
			// so remain at the same stage
			// if no items were processed, advance to the next stage
			return (batch.Count > 0) ? stage : stage + 1;
		}

		#endregion
	}
}