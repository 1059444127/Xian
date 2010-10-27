﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Security.Permissions;
using ClearCanvas.ImageServer.Enterprise;
using ClearCanvas.ImageServer.Enterprise.Authentication;
using ClearCanvas.ImageServer.Web.Application.App_GlobalResources;
using ClearCanvas.ImageServer.Web.Application.Pages.Common;
using ClearCanvas.ImageServer.Web.Common.Data;

namespace ClearCanvas.ImageServer.Web.Application.Pages.Admin.Audit.DeletedStudies
{
    [PrincipalPermission(SecurityAction.Demand, Role = AuthorityTokens.Admin.StudyDeleteHistory.Search)]
    public partial class Default : BaseAdminPage
    {
        #region Protected Methods

        protected override void OnInit(EventArgs e)
        {
            SearchPanel.ViewDetailsClicked += SearchPanel_ViewDetailsClicked;
            SearchPanel.DeleteClicked += SearchPanel_DeleteClicked;
            DeleteConfirmMessageBox.Confirmed += DeleteConfirmMessageBox_Confirmed;
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SetPageTitle(Titles.DeletedStudiesPageTitle);

            DataBind();
        }

        #endregion

        #region Private Methods

        private void DeleteConfirmMessageBox_Confirmed(object data)
        {
            try
            {
                var record = data as ServerEntityKey;
                var controller = new DeletedStudyController();
                controller.Delete(record);
            }
            finally
            {
                SearchPanel.Refresh();
            }
        }

        private void SearchPanel_DeleteClicked(object sender, DeletedStudyDeleteClickedEventArgs e)
        {
            DeleteConfirmMessageBox.Data = e.SelectedItem.DeleteStudyRecord;
            DeleteConfirmMessageBox.Show();
        }

        private void SearchPanel_ViewDetailsClicked(object sender, DeletedStudyViewDetailsClickedEventArgs e)
        {
            var dialogViewModel = new DeletedStudyDetailsDialogViewModel {DeletedStudyRecord = e.DeletedStudyInfo};
            DetailsDialog.ViewModel = dialogViewModel;
            DetailsDialog.Show();
        }

        #endregion
    }
}