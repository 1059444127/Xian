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
using ClearCanvas.Desktop.Configuration;
using ClearCanvas.Common;
using ClearCanvas.ImageViewer.Common;

namespace ClearCanvas.ImageViewer.Layout.Basic
{
	[ExtensionOf(typeof(ConfigurationPageProviderExtensionPoint))]
	public class ConfigurationPageProvider : IConfigurationPageProvider
	{
		public ConfigurationPageProvider()
		{

		}

		public static string BasicLayoutConfigurationPath
		{
			get { return SR.TitleLayoutConfiguration; }
		}

		public static string DisplaySetCreationConfigurationPath
		{
			get { return String.Format("{0}/{1}", BasicLayoutConfigurationPath, SR.TitleDisplaySetCreationConfiguration); }
		}

		#region IConfigurationPageProvider Members

		public IEnumerable<IConfigurationPage> GetPages()
		{
			List<IConfigurationPage> listPages = new List<IConfigurationPage>();
			
			if (PermissionsHelper.IsInRole(AuthorityTokens.ViewerVisible))
			{
				listPages.Add(new ConfigurationPage<LayoutConfigurationComponent>(BasicLayoutConfigurationPath));
				listPages.Add(new ConfigurationPage<DisplaySetCreationConfigurationComponent>(DisplaySetCreationConfigurationPath));
			}

			return listPages.AsReadOnly();
		}

		#endregion
	}
}
