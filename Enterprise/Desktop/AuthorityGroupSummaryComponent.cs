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
using System.ServiceModel;
using ClearCanvas.Common;
using ClearCanvas.Common.Authorization;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Actions;
using ClearCanvas.Desktop.Tools;
using ClearCanvas.Enterprise.Common;
using ClearCanvas.Enterprise.Common.Admin.AuthorityGroupAdmin;

namespace ClearCanvas.Enterprise.Desktop
{
    [MenuAction("launch", "global-menus/MenuAdmin/MenuAuthorityGroups", "Launch")]
    [ActionPermission("launch", AuthorityTokens.Admin.Security.AuthorityGroup)]

    [ExtensionOf(typeof(DesktopToolExtensionPoint))]
    public class AuthorityGroupSummaryTool : Tool<IDesktopToolContext>
    {
        private IWorkspace _workspace;

        public void Launch()
        {
            if (_workspace == null)
            {
                try
                {
					if (Application.SessionStatus != SessionStatus.Online)
					{
						Context.DesktopWindow.ShowMessageBox(SR.MessageServerOffline, MessageBoxActions.Ok);
						return;
					}

                	AuthorityGroupSummaryComponent component = new AuthorityGroupSummaryComponent();

                    _workspace = ApplicationComponent.LaunchAsWorkspace(
                        Context.DesktopWindow,
                        component,
                        SR.TitleAuthorityGroup);
                    _workspace.Closed += delegate { _workspace = null; };
                }
                catch (Exception e)
                {
                    ExceptionHandler.Report(e, Context.DesktopWindow);
                }
            }
            else
            {
                _workspace.Activate();
            }
        }
    }

    /// <summary>
    /// Extension point for views onto <see cref="AuthorityGroupSummaryComponent"/>
    /// </summary>
    [ExtensionPoint]
    public class AuthorityGroupSummaryComponentViewExtensionPoint : ExtensionPoint<IApplicationComponentView>
    {
    }

    /// <summary>
    /// AuthorityGroupSummaryComponent class
    /// </summary>
    public class AuthorityGroupSummaryComponent : SummaryComponentBase<AuthorityGroupSummary, AuthorityGroupTable>
    {
		/// <summary>
		/// Override this method to perform custom initialization of the action model,
		/// such as adding permissions or adding custom actions.
		/// </summary>
		/// <param name="model"></param>
		protected override void InitializeActionModel(AdminActionModel model)
		{
			base.InitializeActionModel(model);

			model.AddAction("duplicate", SR.TitleDuplicate, "Icons.DuplicateSmall.png", SR.TooltipDuplicateAuthorityGroup,
								DuplicateSelectedItem,
								AuthorityTokens.Admin.Security.AuthorityGroup);

			model.AddAction("import", SR.TitleImport, "Icons.ImportAuthorityTokensSmall.png", SR.TooltipImportAuthorityGroup,
								Import,
								AuthorityTokens.Admin.Security.AuthorityGroup);


			model.Add.SetPermissibility(AuthorityTokens.Admin.Security.AuthorityGroup);
			model.Edit.SetPermissibility(AuthorityTokens.Admin.Security.AuthorityGroup);
			model.Delete.SetPermissibility(AuthorityTokens.Admin.Security.AuthorityGroup);
		}

		#region Presentation Model

		public void Import()
		{
			try
			{
				DialogBoxAction action = Host.ShowMessageBox("Import authority tokens and groups defined in locally installed plugins?",
								 MessageBoxActions.OkCancel);
				if (action == DialogBoxAction.Ok)
				{
					AuthorityTokenDefinition[] tokens = AuthorityGroupSetup.GetAuthorityTokens();
					AuthorityGroupDefinition[] groups = AuthorityGroupSetup.GetDefaultAuthorityGroups();

					Platform.GetService(
						delegate(IAuthorityGroupAdminService service)
						{
							// first import the tokens, since the default groups will likely depend on these tokens
							service.ImportAuthorityTokens(
								new ImportAuthorityTokensRequest(
									CollectionUtils.Map(tokens,
									                    (AuthorityTokenDefinition t) => new AuthorityTokenSummary(t.Token, t.Description))));

							// then import the default groups
							service.ImportAuthorityGroups(
								new ImportAuthorityGroupsRequest(
									CollectionUtils.Map(groups,
									                    (AuthorityGroupDefinition g) => 
                                                                new AuthorityGroupDetail(null, g.Name, CollectionUtils.Map(g.Tokens, (string t) => new AuthorityTokenSummary(t,null))))));
						});

				}

			}
			catch (Exception e)
			{
				ExceptionHandler.Report(e, Host.DesktopWindow);
			}
		}

		public void DuplicateSelectedItem()
		{
			try
			{
				AuthorityGroupSummary item = CollectionUtils.FirstElement(SelectedItems);
				if(item == null) return;

				AuthorityGroupEditorComponent editor = new AuthorityGroupEditorComponent(item, true);
				ApplicationComponentExitCode exitCode = LaunchAsDialog(
					Host.DesktopWindow, editor, SR.TitleUpdateAuthorityGroup);
				if (exitCode == ApplicationComponentExitCode.Accepted)
				{
					Table.Items.Add(editor.AuthorityGroupSummary);
					SummarySelection = new Selection(editor.AuthorityGroupSummary);
				}
			}
			catch (Exception e)
			{
				ExceptionHandler.Report(e, Host.DesktopWindow);
			}
		}



		#endregion

		protected override bool SupportsDelete
		{
			get { return true; }
		}

		/// <summary>
		/// Gets the list of items to show in the table, according to the specifed first and max items.
		/// </summary>
		/// <returns></returns>
		protected override IList<AuthorityGroupSummary> ListItems(int firstRow, int maxRows)
		{
			ListAuthorityGroupsRequest request = new ListAuthorityGroupsRequest
			                                         {
			                                             Page = { FirstRow = firstRow, MaxRows = maxRows }
			                                         };

		    ListAuthorityGroupsResponse listResponse = null;
			Platform.GetService(
				delegate(IAuthorityGroupAdminService service)
				{
					listResponse = service.ListAuthorityGroups(request);
				});

			return listResponse.AuthorityGroups;
		}

		/// <summary>
		/// Called to handle the "add" action.
		/// </summary>
		/// <param name="addedItems"></param>
		/// <returns>True if items were added, false otherwise.</returns>
		protected override bool AddItems(out IList<AuthorityGroupSummary> addedItems)
		{
			addedItems = new List<AuthorityGroupSummary>();
			AuthorityGroupEditorComponent editor = new AuthorityGroupEditorComponent();
			ApplicationComponentExitCode exitCode = LaunchAsDialog(
				Host.DesktopWindow, editor, SR.TitleAddAuthorityGroup);
			if (exitCode == ApplicationComponentExitCode.Accepted)
			{
				addedItems.Add(editor.AuthorityGroupSummary);
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
		protected override bool EditItems(IList<AuthorityGroupSummary> items, out IList<AuthorityGroupSummary> editedItems)
		{
			editedItems = new List<AuthorityGroupSummary>();
			AuthorityGroupSummary item = CollectionUtils.FirstElement(items);

			AuthorityGroupEditorComponent editor = new AuthorityGroupEditorComponent(item, false);
			ApplicationComponentExitCode exitCode = LaunchAsDialog(
				Host.DesktopWindow, editor, SR.TitleUpdateAuthorityGroup + " - " + item.Name);
			if (exitCode == ApplicationComponentExitCode.Accepted)
			{
				editedItems.Add(editor.AuthorityGroupSummary);
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
		protected override bool DeleteItems(IList<AuthorityGroupSummary> items, out IList<AuthorityGroupSummary> deletedItems, out string failureMessage)
		{
			failureMessage = null;
			deletedItems = new List<AuthorityGroupSummary>();

			foreach (AuthorityGroupSummary item in items)
			{
			    try
				{
                    if (DoDeleteAuthorityGroup(item))
					    deletedItems.Add(item);
                    else 
                        break;
                }
				catch (Exception e)
				{
					failureMessage = e.Message;
				}
			}

			return deletedItems.Count > 0;
		}

        /// <summary>
        /// Deletes the specified user group and prompt user for confirmation if the group is not empty
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        private bool DoDeleteAuthorityGroup(AuthorityGroupSummary group)
        {
            try
            {
                DeleteGroupHelper(group, true);
                return true;
            }
            catch (AuthorityGroupIsNotEmptyException ex)
            {
                string message = ex.UserCount > 1
                                         ? string.Format(SR.ExceptionAuthorityGroupIsNotEmpty_MultitpleUsers, ex.UserCount, ex.GroupName)
                                         : string.Format(SR.ExceptionAuthorityGroupIsNotEmpty_OneUser, ex.GroupName);

                DialogBoxAction action = Host.ShowMessageBox(message, MessageBoxActions.YesNo);

                switch (action)
                {
                    case DialogBoxAction.No:
                        return false;  // not deleted

                    case DialogBoxAction.Yes:
                        
                        DeleteGroupHelper(group, false); // note: exceptions will be handled by caller
                        return true;
                }
            }

            return false;
        }

        private static void DeleteGroupHelper(AuthorityGroupSummary group, bool checkIfEmpty)
        {
            try
            {
                Platform.GetService<IAuthorityGroupAdminService>(
                    service => service.DeleteAuthorityGroup(new DeleteAuthorityGroupRequest(group.AuthorityGroupRef) { DeleteOnlyWhenEmpty = checkIfEmpty }));
            }
            catch (FaultException<RequestValidationException> ex)
            {
                throw ex.Detail;
            }
            catch (FaultException<ConcurrentModificationException> ex)
            {
                throw ex.Detail;
            }
            catch (FaultException<AuthorityGroupIsNotEmptyException> ex)
            {
                throw ex.Detail;
            }
        }

		/// <summary>
		/// Compares two items to see if they represent the same item.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		protected override bool IsSameItem(AuthorityGroupSummary x, AuthorityGroupSummary y)
		{
            return x.AuthorityGroupRef.Equals(y.AuthorityGroupRef, true);
		}
	}
}
