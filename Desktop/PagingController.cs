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
using ClearCanvas.Common.Utilities;

namespace ClearCanvas.Desktop
{

	///<summary>
	/// Default implementation of <see cref="IPagingController{TItem}"/>.
	///</summary>
	///<typeparam name="TItem"></typeparam>
	public class PagingController<TItem> : IPagingController<TItem>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="firstRow"></param>
		/// <param name="maxRows"></param>
		/// <param name="resultHandler"></param>
		/// <returns></returns>
		public delegate void PageQueryDelegate(int firstRow, int maxRows, Action<IList<TItem>> resultHandler);

		private const int DefaultPageSize = 50;

		private readonly int _pageSize;
		private int _currentPageNumber;
		private bool _hasNext;
		private readonly PageQueryDelegate _queryDelegate;

		private event EventHandler<PageChangedEventArgs<TItem>> _pageChanged;

		private bool _busy;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="pageSize"></param>
		/// <param name="queryDelegate"></param>
		public PagingController(int pageSize, PageQueryDelegate queryDelegate)
		{
			_pageSize = pageSize;
			_queryDelegate = queryDelegate;
			_currentPageNumber = 0;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="queryDelegate"></param>
		public PagingController(PageQueryDelegate queryDelegate)
			: this(DefaultPageSize, queryDelegate)
		{
		}

		#region IPagingController<T> Members

		/// <summary>
		/// Gets or sets the number of items per page.
		/// </summary>
		public int PageSize
		{
			get { return _pageSize; }
		}

		/// <summary>
		/// Gets a value indicating whether there is a next page.
		/// </summary>
		/// <returns></returns>
		public bool HasNext
		{
			get { return _hasNext; }
		}

		/// <summary>
		/// Gets a value indicating whether there is a previous page.
		/// </summary>
		/// <returns></returns>
		public bool HasPrevious
		{
			get { return _currentPageNumber > 0; }
		}

		/// <summary>
		/// Gets the next page of items.
		/// </summary>
		/// <returns></returns>
		public void GetNext()
		{
			DoQuery(NextPageNumber, delegate { _currentPageNumber++; });
		}

		/// <summary>
		/// Gets the previous page of items.
		/// </summary>
		/// <returns></returns>
		public void GetPrevious()
		{
			DoQuery(PrevPageNumber, delegate { _currentPageNumber--; });
		}

		/// <summary>
		/// Resets this instance to the first page of items.
		/// </summary>
		/// <returns></returns>
		public void GetFirst()
		{
			DoQuery(FirstPageNumber, delegate { _currentPageNumber = 0; });
		}

		/// <summary>
		/// Occurs when the current page changes (by calling any of <see cref="GetFirst"/>, <see cref="GetNext"/> or <see cref="GetPrevious"/>.
		/// </summary>
		public event EventHandler<PageChangedEventArgs<TItem>> PageChanged
		{
			add { _pageChanged += value; }
			remove { _pageChanged -= value; }
		}

		#endregion

		private void DoQuery(int firstRow, Action<object> updateCurrentPageCallback)
		{
			if (_busy)
				return;

			try
			{
				_busy = true;

				// request 1 more item than we actually need, so that we know
				// if there is a next page or not
				_queryDelegate(firstRow, _pageSize + 1,
					delegate(IList<TItem> results)
					{
						// ensure that the _busy state is reset no matter what!!
						try
						{
							OnQueryCompleted(results, updateCurrentPageCallback);
						}
						finally
						{
							_busy = false;
						}
					});
			}
			catch (Exception)
			{
				_busy = false;
				throw;
			}
		}

		private void OnQueryCompleted(IList<TItem> results, Action<object> updateCurrentPageCallback)
		{
			// determine if we have a next page and set _hasNext appropriately
			if (results.Count == _pageSize + 1)
			{
				_hasNext = true;
				results.RemoveAt(_pageSize);
			}
			else
			{
				_hasNext = false;
			}

			// update our current page prior to firing the public event
			updateCurrentPageCallback(null);

			// fire the public event
			EventsHelper.Fire(_pageChanged, this, new PageChangedEventArgs<TItem>(results));
		}

		private int NextPageNumber
		{
			get { return HasNext ? (_currentPageNumber + 1) * _pageSize : 0; }
		}

		private int PrevPageNumber
		{
			get { return HasPrevious ? (_currentPageNumber - 1) * _pageSize : 0; }
		}

		private static int FirstPageNumber
		{
			get { return 0; }
		}
	}
}