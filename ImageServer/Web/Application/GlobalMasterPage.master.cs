#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClearCanvas.Common;
using ClearCanvas.ImageServer.Common;
using ClearCanvas.ImageServer.Web.Application.Controls;
using ClearCanvas.ImageServer.Web.Application.Pages.Common;
using ClearCanvas.ImageServer.Web.Common.Exceptions;
using ClearCanvas.ImageServer.Web.Common.Security;

namespace ClearCanvas.ImageServer.Web.Application
{
    public partial class GlobalMasterPage : MasterPage, IMasterProperties
    {
        private bool _displayUserInfo = true;

        public bool DisplayUserInformationPanel
        {
            get { return _displayUserInfo; }
            set { _displayUserInfo = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
                return;

            if (ConfigurationManager.AppSettings.GetValues("CachePages")[0].Equals("false"))
            {
                Response.CacheControl = "no-cache";
                Response.AddHeader("Pragma", "no-cache");
                Response.Expires = -1;
            }

            AddIE6PngBugFixCSS();

            CustomIdentity id = SessionManager.Current.User.Identity as CustomIdentity;

            if (DisplayUserInformationPanel)
            {
                if (id != null)
                {
                    Username.Text = id.DisplayName;
                }
                else
                {
                    Username.Text = "unknown";
                }

                try
                {
                    AlertIndicator alertControl = (AlertIndicator)LoadControl("~/Controls/AlertIndicator.ascx");
                    AlertIndicatorPlaceHolder.Controls.Add(alertControl);
                }
                catch (Exception)
                {
                    //No permissions for Alerts, control won't be displayed
                    //hide table cell that contains the control.
                    AlertIndicatorCell.Visible = false;
                }
            }
            else
            {
                UserInformationCell.Width = Unit.Percentage(0);
                MenuCell.Width = Unit.Percentage(100);
            }
        }

        private void AddIE6PngBugFixCSS()
        {
            IE6PNGBugFixCSS.InnerHtml = @"
            input, img
            {
                background-image: expression
                (
                        this.src.toLowerCase().indexOf('.png')>-1?
                        (
                            this.runtimeStyle.backgroundImage = ""none"",
                            this.runtimeStyle.filter = ""progid:DXImageTransform.Microsoft.AlphaImageLoader(src='"" + this.src + ""', sizingMethod='image')"",
                            this.src = """ + Page.ResolveClientUrl("~/App_Themes/Default/Images/blank.gif") + @"""
                        )
                        
                );
            }
        ";
        }

        protected void Logout_Click(Object sender, EventArgs e)
        {
            Platform.Log(LogLevel.Info, "{0} has logged out.", SessionManager.Current.User.Identity.Name);
            SessionManager.SignOut();
            Response.Redirect(SessionManager.LoginUrl, false);
        }

        protected void GlobalScriptManager_AsyncPostBackError(object sender, AsyncPostBackErrorEventArgs e)
        {
            GlobalScriptManager.AsyncPostBackErrorMessage = ExceptionHandler.ThrowAJAXException(e.Exception);
        }
    }
}