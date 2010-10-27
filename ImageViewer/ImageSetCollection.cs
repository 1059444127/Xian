#region License

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
using ClearCanvas.ImageViewer.Comparers;

namespace ClearCanvas.ImageViewer
{
	/// <summary>
	/// A collection of <see cref="IImageSet"/> objects.
	/// </summary>
	public class ImageSetCollection : ObservableList<IImageSet>
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="DisplaySetCollection"/>.
		/// </summary>
		internal ImageSetCollection()
		{
		}

		internal static IComparer<IImageSet> GetDefaultComparer()
		{
			return new StudyDateComparer();
		}

		/// <summary>
		/// The comparer that was last used to sort the collection, via <see cref="Sort"/>.
		/// </summary>
		/// <remarks>
		/// When an item is added to or replaced, this value is set to null.  When an item is
		/// simply removed, the sort order is maintained, so this value also will not change.
		/// </remarks>
		public IComparer<IImageSet> SortComparer { get; internal set; }

		/// <summary>
		/// Sorts the collection using <see cref="SortComparer"/>.
		/// </summary>
		/// <remarks>
		/// If <see cref="SortComparer"/> is null, it is first set to a default one.
		/// </remarks>
		public void Sort()
		{
			if (SortComparer == null)
				SortComparer = GetDefaultComparer();
			Sort(SortComparer);
		}

		/// <summary>
		/// Sorts the collection with the given comparer.
		/// </summary>
		public override void Sort(IComparer<IImageSet> sortComparer)
		{
			Platform.CheckForNullReference(sortComparer, "sortComparer");
			SortComparer = sortComparer;
			base.Sort(SortComparer);
		}

		protected override void OnItemAdded(ListEventArgs<IImageSet> e)
		{
			SortComparer = null; //we don't know the sort order anymore.
			base.OnItemAdded(e);
		}

		protected override void OnItemChanged(ListEventArgs<IImageSet> e)
		{
			SortComparer = null;//we don't know the sort order anymore.
			base.OnItemChanged(e);
		}
	}
}
