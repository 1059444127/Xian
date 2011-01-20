#region License

// Copyright (c) 2011, ClearCanvas Inc.
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
using ClearCanvas.ImageServer.Model;
using ClearCanvas.ImageServer.Web.Application.App_GlobalResources;
using ClearCanvas.ImageServer.Web.Application.Controls;
using ClearCanvas.ImageServer.Web.Application.Pages.Common;
using ClearCanvas.ImageServer.Web.Common.Data;

namespace ClearCanvas.ImageServer.Web.Application.Pages.Admin.Configure.ServerRules
{
    [PrincipalPermission(SecurityAction.Demand, Role = AuthorityTokens.Admin.Configuration.ServerRules)]
    public partial class Default : BasePage
    {
        private readonly ServerRuleController _controller = new ServerRuleController();

        #region Protected Methods

        protected override void OnInit(EventArgs e)
        {
            ServerPartitionTabs.SetupLoadPartitionTabs(delegate(ServerPartition partition)
                                                           {
                                                               var panel =
                                                                   LoadControl("ServerRulePanel.ascx") as
                                                                   ServerRulePanel;
                                                               if (panel != null)
                                                               {
                                                                   panel.ServerPartition = partition;
                                                                   panel.ID = "ServerRulePanel_" + partition.AeTitle;

                                                                   panel.EnclosingPage = this;
                                                               }
                                                               return panel;
                                                           });

            ConfirmDialog.Confirmed += delegate(object data)
                                           {
                                               // delete the device and reload the affected partition.
                                               var key = data as ServerEntityKey;

                                               ServerRule rule = ServerRule.Load(key);

                                               _controller.DeleteServerRule(rule);

                                               ServerPartitionTabs.Update(rule.ServerPartitionKey);
                                           };


            AddEditServerRuleControl.OKClicked += delegate(ServerRule rule)
                                                      {
                                                          if (AddEditServerRuleControl.EditMode)
                                                          {
                                                              // Commit the change into database
                                                              if (_controller.UpdateServerRule(rule))
                                                              {
                                                              }
                                                              else
                                                              {
                                                                  // TODO: alert user
                                                              }
                                                          }
                                                          else
                                                          {
                                                              // Create new device in the database
                                                              ServerRule newRule = _controller.AddServerRule(rule);
                                                              if (newRule != null)
                                                              {
                                                              }
                                                              else
                                                              {
                                                                  //TODO: alert user
                                                              }
                                                          }

                                                          ServerPartitionTabs.Update(rule.ServerPartitionKey);
                                                      };


            SetPageTitle(Titles.ServerRulesPageTitle);

            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                ServerPartitionTabs.Update(0);
            }
        }

        #endregion Protected Methods

        #region Public Methods

        /// <summary>
        /// Displays a popup dialog box for users to edit a rule
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="partition"></param>
        public void OnEditRule(ServerRule rule, ServerPartition partition)
        {
            AddEditServerRuleControl.EditMode = true;
            AddEditServerRuleControl.ServerRule = rule;
            AddEditServerRuleControl.Partition = partition;
            AddEditServerRuleControl.Show();
        }

        /// <summary>
        /// Displays a popup dialog box for users to delete a rule
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="partition"></param>
        public void OnDeleteRule(ServerRule rule, ServerPartition partition)
        {
            ConfirmDialog.Message = string.Format("Are you sure you want to remove rule \"{0}\" from partition {1}?",
                                                  rule.RuleName, partition.AeTitle);
            ConfirmDialog.MessageType = MessageBox.MessageTypeEnum.YESNO;
            ConfirmDialog.Data = rule.GetKey();
            ConfirmDialog.Show();
        }

        /// <summary>
        /// Displays a popup dialog box for users to add a new rule
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="partition"></param>
        public void OnAddRule(ServerRule rule, ServerPartition partition)
        {
            AddEditServerRuleControl.EditMode = false;
            AddEditServerRuleControl.ServerRule = null;
            AddEditServerRuleControl.Partition = partition;
            AddEditServerRuleControl.Show();
        }

        #endregion Public Methods
    }
}