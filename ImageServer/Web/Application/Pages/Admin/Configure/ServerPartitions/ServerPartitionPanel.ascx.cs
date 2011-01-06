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
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using ClearCanvas.ImageServer.Model;
using ClearCanvas.ImageServer.Model.EntityBrokers;
using ClearCanvas.ImageServer.Web.Application.Helpers;
using ClearCanvas.ImageServer.Web.Common.Data;
using ClearCanvas.ImageServer.Web.Common.WebControls.UI;

[assembly: WebResource("ClearCanvas.ImageServer.Web.Application.Pages.Admin.Configure.ServerPartitions.ServerPartitionPanel.js", "application/x-javascript")]

namespace ClearCanvas.ImageServer.Web.Application.Pages.Admin.Configure.ServerPartitions
{
    [ClientScriptResource(ComponentType = "ClearCanvas.ImageServer.Web.Application.Pages.Admin.Configure.ServerPartitions.ServerPartitionPanel", ResourcePath = "ClearCanvas.ImageServer.Web.Application.Pages.Admin.Configure.ServerPartitions.ServerPartitionPanel.js")]
    /// <summary>
    /// Server parition panel  used in <seealso cref="ServerPartitionPage"/> web page.
    /// </summary>
    public partial class ServerPartitionPanel : AJAXScriptControl
    {
        #region Private Members

        // list of partitions displayed in the list
        private IList<ServerPartition> _partitions = new List<ServerPartition>();
        // used for database interaction
        private ServerPartitionConfigController _theController;
        private Default _enclosingPage;

        #endregion Private Members

        #region Public Properties

        [ExtenderControlProperty]
        [ClientPropertyName("DeleteButtonClientID")]
        public string DeleteButtonClientID
        {
            get { return DeletePartitionButton.ClientID; }
        }

        [ExtenderControlProperty]
        [ClientPropertyName("EditButtonClientID")]
        public string EditButtonClientID
        {
            get { return EditPartitionButton.ClientID; }
        }

        [ExtenderControlProperty]
        [ClientPropertyName("ServerPartitionListClientID")]
        public string ServerPartitionListClientID
        {
            get { return ServerPartitionGridPanel.TheGrid.ClientID; }
        }

        // Sets/Gets the list of partitions displayed in the panel
        public IList<ServerPartition> Partitions
        {
            get { return _partitions; }
            set
            {
                _partitions = value;
                ServerPartitionGridPanel.Partitions = _partitions;
            }
        }

        // Sets/Gets the controller used to retrieve load partitions.
        public ServerPartitionConfigController Controller
        {
            get { return _theController; }
            set { _theController = value; }
        }

        public Default EnclosingPage
        {
            get { return _enclosingPage; }
            set { _enclosingPage = value; }
        }

        #endregion Public Properties

        #region Protected Methods

        /// <summary>
        /// Determines if filters are being specified.
        /// </summary>
        /// <returns></returns>
        protected bool HasFilters()
        {
            if (AETitleFilter.Text.Length > 0 || DescriptionFilter.Text.Length > 0 || StatusFilter.SelectedIndex > 0)
                return true;
            else
                return false;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            UpdateUI();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            GridPagerTop.InitializeGridPager(App_GlobalResources.SR.GridPagerPartitionSingleItem, App_GlobalResources.SR.GridPagerPartitionMultipleItems, ServerPartitionGridPanel.TheGrid, delegate { return Partitions.Count; }, ImageServerConstants.GridViewPagerPosition.Top);
            ServerPartitionGridPanel.Pager = GridPagerTop;
            GridPagerTop.Reset();

            StatusFilter.Items.Add(new ListItem(App_GlobalResources.SR.All));
            StatusFilter.Items.Add(new ListItem(App_GlobalResources.SR.Enabled));
            StatusFilter.Items.Add(new ListItem(App_GlobalResources.SR.Disabled));

        }

        public override void DataBind()
        {
            LoadData();
            base.DataBind();
        }


        protected void Clear()
        {
            AETitleFilter.Text = string.Empty;
            DescriptionFilter.Text = string.Empty;
            StatusFilter.SelectedIndex = 0;
            FolderFilter.Text = string.Empty;
        }

        protected void LoadData()
        {
            ServerPartitionSelectCriteria criteria = new ServerPartitionSelectCriteria();

            if (String.IsNullOrEmpty(AETitleFilter.Text) == false)
            {
                string key =SearchHelper.TrailingWildCard(AETitleFilter.Text);
                key = key.Replace("*", "%");
                criteria.AeTitle.Like(key);
            }

            if (String.IsNullOrEmpty(DescriptionFilter.Text) == false)
            {
                string key = SearchHelper.TrailingWildCard(DescriptionFilter.Text);
                key = key.Replace("*", "%");
                criteria.Description.Like(key);
            }

            if (String.IsNullOrEmpty(FolderFilter.Text) == false)
            {
                string key = SearchHelper.TrailingWildCard(FolderFilter.Text);
                key = key.Replace("*", "%");
                criteria.PartitionFolder.Like(key);
            }

            if (StatusFilter.SelectedIndex != 0)
            {
                if (StatusFilter.SelectedIndex == 1)
                    criteria.Enabled.EqualTo(true);
                else
                    criteria.Enabled.EqualTo(false);
            }

        	criteria.AeTitle.SortAsc(0);

            Partitions =
                _theController.GetPartitions(criteria);
            ServerPartitionGridPanel.RefreshCurrentPage();
        }

        protected void SearchButton_Click(object sender, ImageClickEventArgs e)
        {

        }

        protected void AddPartitionButton_Click(object sender, ImageClickEventArgs e)
        {
            EnclosingPage.AddPartition();
        }

        protected void EditPartitionButton_Click(object sender, ImageClickEventArgs e)
        {
            LoadData();
            ServerPartition selectedPartition =
                ServerPartitionGridPanel.SelectedPartition;
            
            if (selectedPartition != null)
            {
                EnclosingPage.EditPartition(selectedPartition);
            }
        }

        protected void DeletePartitionButton_Click(object sender, ImageClickEventArgs e)
        {
            ServerPartition selectedPartition =
                ServerPartitionGridPanel.SelectedPartition;

            if (selectedPartition != null)
            {
                EnclosingPage.DeletePartition(selectedPartition);
            }
        }

        protected void RefreshButton_Click(object sender, ImageClickEventArgs e)
        {
            // refresh the list
            Clear();
            LoadData();
            UpdateUI();
        }

        #endregion Protected Methods

        #region Public Methods

        public void UpdateUI()
        {
            LoadData();
            SearchUpdatePanel.Update();
            //ServerPartitionGridPanel.Refresh();
        }

        #endregion Public methods
       
    }
}