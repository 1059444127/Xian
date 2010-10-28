#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;

namespace ClearCanvas.Desktop.Tables
{
    /// <summary>
    /// Defines the interface to a table, which provides a presentation model for viewing data in a tabular form.
    /// </summary>
    public interface ITable
    {
        /// <summary>
        /// Returns the <see cref="Type"/> of the items in this table.
        /// </summary>
        Type ItemType { get; }

        /// <summary>
        /// Gets the collection of items in the table.
        /// </summary>
        /// <remarks>
        /// <para>
		/// The returned collection is filtered if <see cref="Filter()"/> has been called.  To
		/// ensure all items are returned, use <see cref="RemoveFilter()"/> prior to using this property.
		/// </para>
		/// <para>
		/// CF: <see cref="ITable{TItem}.Items"/> which always returns the complete collection.
		/// </para>
		/// </remarks>
        IItemCollection Items { get; }

        /// <summary>
        /// Get the collection of columns.
        /// </summary>
        ITableColumnCollection Columns { get; }

        /// <summary>
        /// Sorts this table according to the cached sort parameters, if any exist.
        /// </summary>
        void Sort();

        /// <summary>
        /// Sorts this table according to the specified sort parameters.
        /// </summary>
        void Sort(TableSortParams sortParams);

		/// <summary>
		/// Raised before the table is sorted.
		/// </summary>
		event EventHandler BeforeSorted;
		
		/// <summary>
		/// Raised after the table is sorted.
        /// </summary>
        event EventHandler Sorted;

        /// <summary>
        /// Gets the cached sort parameters, or returns null if this table has not been sorted.
        /// </summary>
        TableSortParams SortParams { get; }

        /// <summary>
        /// Gets a value indicating if the table is filtered or not.
        /// </summary>
        bool IsFiltered { get; }

        /// <summary>
        /// Filters this table according to the cached filter parameters, if any exist.
        /// </summary>
        void Filter();

        /// <summary>
        /// Filters this table accordint ot the specified filter parameters.
        /// </summary>
        void Filter(TableFilterParams filterParams);

        /// <summary>
        /// Removes the applied filter, if one exists.
        /// </summary>
        void RemoveFilter();

		/// <summary>
		/// Gets the cached filter parameters, or returns null if this table has not been filtered.
		/// </summary>
		TableFilterParams FilterParams { get; }

		/// <summary>
        /// Gets the base column width for this table, in units that correspond roughly to the
        /// width of one character.
        /// </summary>
        float BaseColumnWidthChars { get; }

        /// <summary>
        /// Gets the number of cell rows in each row.
        /// </summary>
        int CellRowCount { get; }

        /// <summary>
        /// Gets the color for the background of a cell row.
        /// </summary>
        string GetItemBackgroundColor(object item);

        /// <summary>
		/// Gets color for the outline of a cell row.
        /// </summary>
        string GetItemOutlineColor(object item);
    }

    /// <summary>
    /// Defines an additional interface to a table, which provides generic methods for viewing its data.
    /// Used in conjunction with <see cref="ITable"/>.
    /// </summary>
    public interface ITable<TItem>
    {
        /// <summary>
        /// Gets the collection of items in the table.
        /// </summary>
		/// <remarks>The returned collection is never filtered.  CF: <see cref="ITable.Items"/> which may return a filtered list.</remarks>
        ItemCollection<TItem> Items { get; }

        /// <summary>
        /// Gets the collection of columns.
        /// </summary>
        TableColumnCollection<TItem> Columns { get; }
    }
}
