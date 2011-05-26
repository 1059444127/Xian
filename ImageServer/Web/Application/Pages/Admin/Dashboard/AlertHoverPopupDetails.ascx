<%--  License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

--%>

<%@ Import namespace="ClearCanvas.ImageServer.Core.Validation"%>
<%@ Import namespace="ClearCanvas.ImageServer.Web.Common.Utilities"%>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AlertHoverPopupDetails.ascx.cs" Inherits="ClearCanvas.ImageServer.Web.Application.Pages.Admin.Dashboard.AlertHoverPopupDetails" %>
<%@ Import Namespace="Resources"%>

<asp:Panel runat="server" ID="DetailsIndicator" CssClass="MoreInfo">[<%=Labels.AlertHoverPopupDetails_MoreInfo %>]</asp:Panel>
<asp:Panel runat="server" ID="DetailsPanel" CssClass="AlertHoverPopupDetails" style="display:none">
    <asp:PlaceHolder runat="server" ID="DetailsPlaceHolder">
    </asp:PlaceHolder>    
</asp:Panel>				            


<aspAjax:DropShadowExtender runat="server" ID="Shadow" TargetControlID="DetailsPanel" Opacity="0.4" TrackPosition="true">
</aspAjax:DropShadowExtender>

<aspAjax:HoverMenuExtender  runat="server" ID="Details" 
        PopupControlID="DetailsPanel" TargetControlID="DetailsIndicator" PopupPosition="bottom">
</aspAjax:HoverMenuExtender>
