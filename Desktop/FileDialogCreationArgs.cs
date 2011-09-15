#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System.Collections.Generic;

namespace ClearCanvas.Desktop
{
	/// <summary>
	/// Describes a file dialog extension filter.
	/// </summary>
	public class FileExtensionFilter
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="description"></param>
		public FileExtensionFilter(string filter, string description)
		{
			Filter = filter;
			Description = description;
		}

		/// <summary>
		/// Gets or sets the value of the filter, which must be a wildcard expression (e.g. *.txt).
		/// </summary>
		public string Filter { get; set; }

		/// <summary>
		/// Gets or sets the value displayed for the filter, e.g. Text files (*.txt).
		/// </summary>
		public string Description { get; set; }
	}


	/// <summary>
	/// Holds parameters that initialize the display of a common file dialog.
	/// </summary>
	public class FileDialogCreationArgs
	{
		private readonly List<FileExtensionFilter> _filters;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="directory"></param>
		/// <param name="filename"></param>
		/// <param name="fileExtension"></param>
		/// <param name="filters"></param>
		public FileDialogCreationArgs(string filename, string directory, string fileExtension, IEnumerable<FileExtensionFilter> filters)
		{
			Directory = directory;
			FileName = filename;
			FileExtension = fileExtension;
			_filters = new List<FileExtensionFilter>(filters);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="filename"></param>
		public FileDialogCreationArgs(string filename)
			: this(filename, null, null, new FileExtensionFilter[] { })
		{

		}

		/// <summary>
		/// Constructor
		/// </summary>
		public FileDialogCreationArgs()
			: this(null, null, null, new FileExtensionFilter[] { })
		{
			
		}

		/// <summary>
		/// Gets or sets the default extension to append to the filename, if not specified by user.
		/// </summary>
		public string FileExtension { get; set; }

		/// <summary>
		/// Gets or sets the initial value of the file name.
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// Gets or sets the initial directory.
		/// </summary>
		public string Directory { get; set; }

		/// <summary>
		/// Gets or sets the title of the file dialog.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// For an Open File dialog, gets or sets a value indicating whether the dialog allows multiple files to be selected.
		/// </summary>
		/// <remarks>
		/// This property is ignored for a Save File dialog.
		/// </remarks>
		public bool MultiSelect { get; set; }

		/// <summary>
		/// Gets the list of file extension filters.
		/// </summary>
		public List<FileExtensionFilter> Filters
		{
			get { return _filters; }
		}
	}
}
