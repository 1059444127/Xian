#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using ClearCanvas.Common;
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Actions;
using ClearCanvas.Desktop.Tools;
using ClearCanvas.Ris.Application.Common;

namespace ClearCanvas.Ris.Client
{
	/// <summary>
	/// Allows a user to edit his own staff profile.
	/// </summary>
	[MenuAction("launch", "global-menus/MenuTools/Staff Profile", "Launch")]
	[ActionPermission("launch", ClearCanvas.Ris.Application.Common.AuthorityTokens.Workflow.StaffProfile.View)]
	[ActionPermission("launch", ClearCanvas.Ris.Application.Common.AuthorityTokens.Workflow.StaffProfile.Update)]
	[ExtensionOf(typeof(DesktopToolExtensionPoint), FeatureToken = FeatureTokens.RIS.Core)]
	public class StaffProfileEditorTool : Tool<IDesktopToolContext>
	{
		public void Launch()
		{
			if(LoginSession.Current == null)
			{
				this.Context.DesktopWindow.ShowMessageBox(
					"This feature will be available the next time you start the workstation.",
					MessageBoxActions.Ok);
				return;
			}

			try
			{
				if (LoginSession.Current.Staff == null)
				{
					this.Context.DesktopWindow.ShowMessageBox(
						string.Format("There is no staff profile associated with the user '{0}'", LoginSession.Current.UserName),
						MessageBoxActions.Ok);
					return;
				}

				var component = new StaffEditorComponent(LoginSession.Current.Staff.StaffRef);

				ApplicationComponent.LaunchAsDialog(
					this.Context.DesktopWindow,
					component,
					SR.TitleStaff);
			}
			catch (Exception e)
			{
				// could not launch component
				ExceptionHandler.Report(e, this.Context.DesktopWindow);
			}
		}
	}
}
