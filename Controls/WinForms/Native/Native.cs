﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

namespace ClearCanvas.Controls.WinForms
{
	// ReSharper disable InconsistentNaming
	internal static partial class Native
	{
		public const int FILE_ATTRIBUTE_NORMAL = 0x80;

		/// <summary>
		/// The maximum file system path length (260).
		/// </summary>
		/// <remarks>
		/// The maximum component length (i.e. for any given filename or directory name) is 256.
		/// The maximum path length (i.e. the concatenation of all components in the path) is 260.
		/// </remarks>
		public const int MAX_PATH = 260;
	}

	// ReSharper restore InconsistentNaming
}