#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System.ComponentModel;
using System.Drawing;
using ClearCanvas.Dicom.Utilities.StudyBuilder;

namespace ClearCanvas.ImageViewer.Utilities.StudyComposer
{
	/// <summary>
	/// An interface for items in the <see cref="StudyComposerComponent"/> tree.
	/// </summary>
	public interface IStudyComposerItem
	{
		/// <summary>
		/// Indicates that a property on the node has changed, and that any views should refresh its display of the item.
		/// </summary>
		event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Gets or sets the name label of this item.
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Gets a short, multi-line description of the item that contains ancillary information.
		/// </summary>
		string Description { get; }

		/// <summary>
		/// Gets the <see cref="StudyBuilderNode"/> node that is encapsulated by this <see cref="StudyComposerItemBase{T}"/>.
		/// </summary>
		StudyBuilderNode Node { get; }

		/// <summary>
		/// Gets an <see cref="Image"/> icon that can be used to represent the item in thumbnail views.
		/// </summary>
		Image Icon { get; }

		/// <summary>
		/// Regenerates the icon for a specific icon size.
		/// </summary>
		/// <param name="iconSize">The <see cref="Size"/> of the icon to generate.</param>
		void UpdateIcon(Size iconSize);

		/// <summary>
		/// Regenerates the icon for the default icon size (64x64).
		/// </summary>
		void UpdateIcon();

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>A new object that is a copy of this instance.</returns>
		IStudyComposerItem Clone();
	}
}