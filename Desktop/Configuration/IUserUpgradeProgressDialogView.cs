﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using ClearCanvas.Common;
using ClearCanvas.Common.Configuration;
using ClearCanvas.Common.Utilities;

namespace ClearCanvas.Desktop.Configuration
{
	[ExtensionPoint]
	public class UserUpgradeProgressDialogViewExtensionPoint : ExtensionPoint<IUserUpgradeProgressDialogView>
	{}

	internal class UserUpgradeProgressDialog
	{
		private IUserUpgradeProgressDialogView _dialogView;

		private UserUpgradeProgressDialog()
		{
		}

		private void RunUpgrade()
		{
			var strategy = UserUpgradeStrategy.Create();
			if (strategy == null)
				return;

			if (strategy.TotalSteps < 5 || null == (_dialogView = CreateDialog()))
			{
				strategy.Run();
				return;
			}

			var task = new BackgroundTask(
				delegate(IBackgroundTaskContext context)
					{
						strategy.ProgressChanged += (sender, e) => context.ReportProgress(new BackgroundTaskProgress(strategy.CurrentStep - 1, strategy.TotalSteps, String.Empty));
						strategy.Run();
						context.Complete();
					}, false, strategy);

			task.ProgressUpdated += (sender, e) => _dialogView.SetProgressPercent(e.Progress.Percent);
			task.Terminated += (sender, e) => _dialogView.Close(strategy.FailedCount > 0 ? SR.MessageUserUpgradeFailures : null);
			task.Run();

			_dialogView.RunModal(SR.TitleUpdatingPreferences, SR.MessageUpdatingPreferences);
			task.Dispose();
		}

		private static IUserUpgradeProgressDialogView CreateDialog()
		{
			try
			{
				return (IUserUpgradeProgressDialogView)new UserUpgradeProgressDialogViewExtensionPoint().CreateExtension();
			}
			catch (NotSupportedException)
			{
			}
			catch(Exception e)
			{
				Platform.Log(LogLevel.Debug, e, "Failed to create user upgrade progress dialog view.");
			}

			return null;
		}

		public static void RunUpgradeAndShowProgress()
		{
			new UserUpgradeProgressDialog().RunUpgrade();
		}
	}

	public interface IUserUpgradeProgressDialogView : IView
	{
		void RunModal(string title, string startupMessage);
		void SetMessage(string message);
		void SetProgressPercent(int progressPercent);
		void Close(string failureMessage);
	}
}