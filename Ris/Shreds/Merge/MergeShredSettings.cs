#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca

// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Configuration;
using ClearCanvas.Common.Configuration;

namespace ClearCanvas.Ris.Shreds.Merge
{

	// TODO add a description of the purpose of the settings group here
	[SettingsGroupDescription("")]
	[SettingsProvider(typeof(ClearCanvas.Common.Configuration.StandardSettingsProvider))]
	internal sealed partial class MergeShredSettings
	{
		///<summary>
		/// Public constructor.  Server-side settings classes should be instantiated via constructor rather
		/// than using the <see cref="MergeShredSettings.Default"/> property to avoid creating a static instance.
		///</summary>
		public MergeShredSettings()
		{
			// Note: server-side settings classes do not register in the <see cref="ApplicationSettingsRegistry"/>
		}
	}
}
