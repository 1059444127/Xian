#region License

// Copyright (c) 2006-2008, ClearCanvas Inc.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met:
//
//    * Redistributions of source code must retain the above copyright notice, 
//      this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, 
//      this list of conditions and the following disclaimer in the documentation 
//      and/or other materials provided with the distribution.
//    * Neither the name of ClearCanvas Inc. nor the names of its contributors 
//      may be used to endorse or promote products derived from this software without 
//      specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR 
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
// OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
// ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY 
// OF SUCH DAMAGE.

#endregion

using System;
using System.Collections.Generic;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Actions;
using ClearCanvas.Desktop.Tools;
using ClearCanvas.Enterprise.Common;
using ClearCanvas.Ris.Application.Common.Admin.UserAdmin;

namespace ClearCanvas.Ris.Client.Admin
{
    [MenuAction("launch", "global-menus/Admin/Users", "Launch")]
    [ActionPermission("launch", ClearCanvas.Ris.Application.Common.AuthorityTokens.Admin.Security.User)]
    [ExtensionOf(typeof(DesktopToolExtensionPoint))]
    public class UserSummaryTool : Tool<IDesktopToolContext>
    {
        private IWorkspace _workspace;

        public void Launch()
        {
            if (_workspace == null)
            {
                try
                {
                    UserSummaryComponent component = new UserSummaryComponent();

                    _workspace = ApplicationComponent.LaunchAsWorkspace(
                        this.Context.DesktopWindow,
                        component,
                        SR.TitleUser);
                    _workspace.Closed += delegate { _workspace = null; };

                }
                catch (Exception e)
                {
                    // could not launch component
                    ExceptionHandler.Report(e, this.Context.DesktopWindow);
                }
            }
            else
            {
                _workspace.Activate();
            }
        }
    }

    /// <summary>
    /// UserSummaryComponent class
    /// </summary>
    public class UserSummaryComponent : SummaryComponentBase<UserSummary, UserTable>
    {
        private Action _resetPasswordAction;

        /// <summary>
        /// Constructor
        /// </summary>
        public UserSummaryComponent()
        {
        }

        #region Presentation Model

        public void ResetUserPassword()
        {
            try
            {
            	UserSummary selectedUser = CollectionUtils.FirstElement(this.SelectedItems);
				if (selectedUser == null) return;

                // confirm this action
				if (this.Host.ShowMessageBox(string.Format("Reset password for user {0}?", selectedUser.UserName), MessageBoxActions.OkCancel) == DialogBoxAction.Cancel)
                    return;

                Platform.GetService<IUserAdminService>(
                    delegate(IUserAdminService service)
                    {
						ResetUserPasswordResponse response = service.ResetUserPassword(new ResetUserPasswordRequest(selectedUser.UserName));
                        this.Table.Items.Replace(delegate(UserSummary u) { return u.UserName == response.UserSummary.UserName; },
                           response.UserSummary);
                    });
            }
            catch (Exception e)
            {
                // failed
                ExceptionHandler.Report(e, this.Host.DesktopWindow);
            }
        }

        #endregion

		/// <summary>
		/// Override this method to perform custom initialization of the action model,
		/// such as adding permissions or adding custom actions.
		/// </summary>
		/// <param name="model"></param>
		protected override void InitializeActionModel(CrudActionModel model)
		{
			base.InitializeActionModel(model);

			_resetPasswordAction = model.AddAction("resetPassword", SR.TitleResetPassword, "Icons.ResetToolSmall.png",
				SR.TitleResetPassword, ResetUserPassword, ClearCanvas.Ris.Application.Common.AuthorityTokens.Admin.Security.User);

			model.Add.SetPermissibility(ClearCanvas.Ris.Application.Common.AuthorityTokens.Admin.Security.User);
			model.Edit.SetPermissibility(ClearCanvas.Ris.Application.Common.AuthorityTokens.Admin.Security.User);
			model.Delete.SetPermissibility(ClearCanvas.Ris.Application.Common.AuthorityTokens.Admin.Security.User);
		}

		protected override bool SupportsDelete
		{
			get { return true; }
		}

		/// <summary>
		/// Gets the list of items to show in the table, according to the specifed first and max items.
		/// </summary>
		/// <param name="firstItem"></param>
		/// <param name="maxItems"></param>
		/// <returns></returns>
		protected override IList<UserSummary> ListItems(int firstItem, int maxItems)
		{
			ListUsersResponse listResponse = null;
			Platform.GetService<IUserAdminService>(
				delegate(IUserAdminService service)
				{
					listResponse = service.ListUsers(new ListUsersRequest(new SearchResultPage(firstItem, maxItems)));
				});

			return listResponse.Users;
		}

		/// <summary>
		/// Called to handle the "add" action.
		/// </summary>
		/// <param name="addedItems"></param>
		/// <returns>True if items were added, false otherwise.</returns>
		protected override bool AddItems(out IList<UserSummary> addedItems)
		{
			addedItems = new List<UserSummary>();
			UserEditorComponent editor = new UserEditorComponent();
			ApplicationComponentExitCode exitCode = ApplicationComponent.LaunchAsDialog(
				this.Host.DesktopWindow, editor, SR.TitleAddUser);
			if (exitCode == ApplicationComponentExitCode.Accepted)
			{
				addedItems.Add(editor.UserSummary);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Called to handle the "edit" action.
		/// </summary>
		/// <param name="items">A list of items to edit.</param>
		/// <param name="editedItems">The list of items that were edited.</param>
		/// <returns>True if items were edited, false otherwise.</returns>
		protected override bool EditItems(IList<UserSummary> items, out IList<UserSummary> editedItems)
		{
			editedItems = new List<UserSummary>();
			UserSummary item = CollectionUtils.FirstElement(items);

			UserEditorComponent editor = new UserEditorComponent(item.UserName);
			ApplicationComponentExitCode exitCode = ApplicationComponent.LaunchAsDialog(
				this.Host.DesktopWindow, editor, SR.TitleUpdateUser + " - " + item.UserName);
			if (exitCode == ApplicationComponentExitCode.Accepted)
			{
				editedItems.Add(editor.UserSummary);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Called to handle the "delete" action, if supported.
		/// </summary>
		/// <param name="items"></param>
		/// <param name="deletedItems">The list of items that were deleted.</param>
		/// <param name="failureMessage">The message if there any errors that occurs during deletion.</param>
		/// <returns>True if items were deleted, false otherwise.</returns>
		protected override bool DeleteItems(IList<UserSummary> items, out IList<UserSummary> deletedItems, out string failureMessage)
		{
			failureMessage = null;
			deletedItems = new List<UserSummary>();

			foreach (UserSummary item in items)
			{
				try
				{
					Platform.GetService<IUserAdminService>(
						delegate(IUserAdminService service)
						{
							service.DeleteUser(new DeleteUserRequest(item.UserName));
						});

					deletedItems.Add(item);
				}
				catch (Exception e)
				{
					failureMessage = e.Message;
				}
			}

			return deletedItems.Count > 0;
		}

		/// <summary>
		/// Compares two items to see if they represent the same item.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		protected override bool IsSameItem(UserSummary x, UserSummary y)
		{
			return x.UserName.Equals(y.UserName);
		}

    	/// <summary>
    	/// Called when the user changes the selected items in the table.
    	/// </summary>
    	protected override void OnSelectedItemsChanged()
		{
			base.OnSelectedItemsChanged();

			_resetPasswordAction.Enabled = this.SelectedItems.Count == 1;
		}
	}
}
