#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

namespace ClearCanvas.Desktop
{
	public interface IGalleryItem<T> : IGalleryItem where T : class
	{
        new T Image { get; }    
	}

    /// <summary>
	/// An item for display in a gallery-style view.
	/// </summary>
	public interface IGalleryItem
	{
		/// <summary>
		/// The image/icon to display.
		/// </summary>
		object Image { get; }

		/// <summary>
		/// The name of the object.
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// A brief description of the object.
		/// </summary>
		string Description { get; }

		/// <summary>
		/// The actual object that is being visually represented in the gallery.
		/// </summary>
		object Item { get; }
	}
}
