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
	/// Subclass of the <see cref="TableColumn{TItem,TColumn}"/> class for a nullable DateTime column type.
	/// The value is formatted as Date only.
	/// </summary>
	/// <typeparam name="TItem">The type of item on which the table is based.</typeparam>
	public class DateTableColumn<TItem> : DateTimeTableColumn<TItem>
	{
        /// <summary>
        /// Constructs a read-only single-cellrow DateTime table column.
        /// </summary>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="valueGetter">A delegate that accepts an item and pulls the column value from the item.</param>
		public DateTableColumn(string columnName, GetColumnValueDelegate<TItem, DateTime?> valueGetter)
            : this(columnName, valueGetter, 1.0f)
        {
		}

        /// <summary>
        /// Constructs a read-only single-cellrow DateTime table column with specific width factor.
        /// </summary>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="valueGetter">A delegate that accepts an item and pulls the column value from the item.</param>
        /// <param name="widthFactor">A weighting factor that is applied to the width of the column.</param>
		public DateTableColumn(string columnName, GetColumnValueDelegate<TItem, DateTime?> valueGetter, float widthFactor)
            : base(columnName, valueGetter, widthFactor)
        {
			this.ValueFormatter = delegate(DateTime? value) { return Format.Date(value); };
		}

	}
}
