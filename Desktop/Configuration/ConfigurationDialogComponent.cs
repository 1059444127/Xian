#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using ClearCanvas.Common;
using ClearCanvas.Desktop.Validation;
using ClearCanvas.Common.Configuration;

namespace ClearCanvas.Desktop.Configuration
{
	[ExtensionPoint]
	public sealed class ConfigurationDialogComponentViewExtensionPoint : ExtensionPoint<IApplicationComponentView>
	{
	}

	[AssociateView(typeof(ConfigurationDialogComponentViewExtensionPoint))]
	public class ConfigurationDialogComponent : NavigatorComponentContainer
	{
		private class NavigatorPagePathComparer : IComparer<NavigatorPage>
		{
			#region IComparer<NavigatorPage> Members

			/// <summary>
			/// Compares two <see cref="NavigatorPage"/>s.
			/// </summary>
			public int Compare(NavigatorPage x, NavigatorPage y)
			{
				if (x == null)
				{
					if (y == null)
						return 0;
					else
						return -1;
				}

				if (y == null)
					return 1;

				return x.Path.LocalizedPath.CompareTo(y.Path.LocalizedPath);
			}

			#endregion
		}

		private readonly int _initialPageIndex;
		private readonly ConfigurationPageManager _configurationPageManager;
		private SettingsStoreWatcher _settingsStoreWatcher;
		private string _configurationWarning;

		internal ConfigurationDialogComponent(string initialPagePath)
			: base(ConfigurationDialogSettings.Default.ShowApplyButton)
		{
			// We want to validate all configuration pages
			this.ValidationStrategy = new StartedComponentsValidationStrategy();

			_configurationPageManager = new ConfigurationPageManager();

			List<NavigatorPage> pages = new List<NavigatorPage>();

			foreach (IConfigurationPage configurationPage in this.ConfigurationPages)
				pages.Add(new NavigatorPage(configurationPage.GetPath(), configurationPage.GetComponent()));

			pages.Sort(new NavigatorPagePathComparer());

			_initialPageIndex = 0;
			int i = 0;

			foreach (NavigatorPage page in pages)
			{
				//do the unresolved paths match?
				if (page.Path.ToString() == initialPagePath)
					_initialPageIndex = i;

				this.Pages.Add(page);
				++i;
			}

			if (Pages.Count == 0)
				throw new Exception(SR.MessageNoConfigurationPagesExist);
		}

		public string ConfigurationWarning
		{
			get { return _configurationWarning; }
		}

		public IEnumerable<IConfigurationPage> ConfigurationPages
		{
			get { return _configurationPageManager.Pages; }
		}

		public override void Start()
		{
			base.Start();
			MoveTo(_initialPageIndex);

			if (!SettingsStore.IsSupported)
				return;

			if (!SettingsStore.IsStoreOnline)
			{
				_configurationWarning = SR.LabelConfigurationWarningOffline;
				return;
			}

			_settingsStoreWatcher = new SettingsStoreWatcher();
			_settingsStoreWatcher.IsStoreOnlineChanged += SettingsStoreOnlineChanged;
		}

		private void SettingsStoreOnlineChanged(object sender, EventArgs e)
		{
			if (_settingsStoreWatcher == null || _settingsStoreWatcher.IsStoreOnline)
				return;

			_configurationWarning = SR.LabelConfigurationWarningOffline;
			NotifyPropertyChanged("ConfigurationWarning");

			_settingsStoreWatcher.IsStoreOnlineChanged -= SettingsStoreOnlineChanged;
			_settingsStoreWatcher.Dispose();
		}

		public override void Stop()
		{
			if (_settingsStoreWatcher != null)
			{
				_settingsStoreWatcher.IsStoreOnlineChanged -= SettingsStoreOnlineChanged;
				_settingsStoreWatcher.Dispose();
			}

			base.Stop();
		}

		public override void Accept()
		{
			if (this.HasValidationErrors)
			{
				this.ShowValidation(true);
				return;
			}

			Save();

			this.ExitCode = ApplicationComponentExitCode.Accepted;
			this.Host.Exit();
		}

		public override void Apply()
		{
			base.Apply();

			//The base method sets ApplyEnabled=false when there are no validation errors.
			if (!base.ApplyEnabled)
				Save();
		}

		private void Save()
		{
			try
			{
				foreach (IConfigurationPage configurationPage in this.ConfigurationPages)
				{
					if (configurationPage.GetComponent().Modified)
						configurationPage.SaveConfiguration();
				}
			}
			catch (Exception e)
			{
				ExceptionHandler.Report(e, SR.ExceptionFailedToSave, this.Host.DesktopWindow,
				                        () => this.Exit(ApplicationComponentExitCode.Error));
			}
		}
	}
}
