﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;

namespace ClearCanvas.ImageServer.Web.Application.Pages.WebViewer
{
    public partial class JQuery : System.Web.UI.UserControl
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            Page.ClientScript.RegisterClientScriptInclude(typeof(JQuery), "jQuery", ResolveUrl("~/Pages/WebViewer/jquery-1.4.2.min.js"));

            //Default Libraries
//            Page.ClientScript.RegisterClientScriptInclude(typeof(JQuery), "ClearCanvas", ResolveUrl("~/Scripts/ClearCanvas.js"));
//            Page.ClientScript.RegisterClientScriptInclude(typeof(JQuery), "DropShadow", ResolveUrl("~/Scripts/jquery/jquery.dropshadow.js")); 
		}
    }
}