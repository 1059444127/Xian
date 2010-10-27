﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;

namespace ClearCanvas.Desktop.Actions
{
	/// <summary>
	/// Defines a generic mechanism for binding to an arbitrary observable property of an object.
	/// </summary>
	/// <remarks>
	/// An observable property is any property which has a corresponding change notification event.
	/// </remarks>
	/// <typeparam name="T">The type of the property.</typeparam>
	internal interface IObservablePropertyBinding<T>
	{
		/// <summary>
		/// The event that provides notification of when the <see cref="PropertyValue"/> has changed.
		/// </summary>
		event EventHandler PropertyChanged;

		/// <summary>
		/// Gets or sets the underlying property value.
		/// </summary>
		T PropertyValue { get; set; }
	}
}