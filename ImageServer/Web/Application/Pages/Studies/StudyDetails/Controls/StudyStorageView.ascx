<%-- License

Copyright (c) 2011, ClearCanvas Inc.
All rights reserved.
http://www.clearcanvas.ca

This software is licensed under the Open Software License v3.0.
For the complete license, see http://www.clearcanvas.ca/OSLv3.0
--%>


<%@ Import Namespace="ClearCanvas.ImageServer.Web.Application.App_GlobalResources" %>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StudyStorageView.ascx.cs" Inherits="ClearCanvas.ImageServer.Web.Application.Pages.Studies.StudyDetails.Controls.StudyStorageView" %>
<asp:Table runat="server" ID="ContainerTable" Height="100%" CellPadding="0" CellSpacing="0"
	Width="100%" style="border-left: solid 1px #3D98D1; border-right: solid 1px #3D98D1;">
	<asp:TableRow VerticalAlign="top">
		<asp:TableCell VerticalAlign="top">
<asp:DetailsView ID="StudyStorageViewControl" runat="server" AutoGenerateRows="False" GridLines="Horizontal" CellPadding="4" OnDataBound="StudyStorageView_DataBound"
     CssClass="GlobalGridView" Width="100%">
    <Fields>
        <asp:TemplateField HeaderText="<%$Resources: DetailedViewFieldLabels, StudyStorage_InsertTime %>">
            <HeaderStyle CssClass="StudyDetailsViewHeader" Wrap="false" />
            <ItemTemplate>
                <ccUI:DateTimeLabel ID="InsertTime" runat="server" Value='<%# Eval("InsertTime") %>' ></ccUI:DateTimeLabel>
            </ItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="<%$Resources: DetailedViewFieldLabels, StudyStorage_LastAccessed %>">
            <HeaderStyle CssClass="StudyDetailsViewHeader" Wrap="false" />
            <ItemTemplate>
                <ccUI:DateTimeLabel ID="LastAccessedTime" runat="server" Value='<%# Eval("LastAccessedTime") %>' ></ccUI:DateTimeLabel>
            </ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField DataField="WriteLock" HeaderText="<%$Resources: DetailedViewFieldLabels, StudyStorage_WriteLock %>">
            <HeaderStyle CssClass="StudyDetailsViewHeader" Wrap="false" />
        </asp:BoundField>
        <asp:BoundField DataField="ReadLock" HeaderText="<%$Resources: DetailedViewFieldLabels, StudyStorage_ReadLock %>">
            <HeaderStyle CssClass="StudyDetailsViewHeader" Wrap="false" />
        </asp:BoundField>
        <asp:TemplateField HeaderText="<%$Resources: DetailedViewFieldLabels, StudyStorage_Status %>">
            <HeaderStyle CssClass="StudyDetailsViewHeader" Wrap="false" />
            <ItemTemplate>
                <asp:Label ID="Status" runat="server" />
            </ItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="<%$Resources: DetailedViewFieldLabels, StudyStorage_QueueStatus %>">
            <HeaderStyle CssClass="StudyDetailsViewHeader" Wrap="false" />
            <ItemTemplate>
                <asp:Label ID="QueueState" runat="server" />
            </ItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="<%$Resources: DetailedViewFieldLabels, StudyStorage_TransferSyntax %>">
            <HeaderStyle CssClass="StudyDetailsViewHeader" Wrap="false" />
            <ItemTemplate>
                <asp:Label ID="TransferSyntaxUID" runat="server" />
            </ItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="<%$Resources: DetailedViewFieldLabels, StudyStorage_Tier %>">
            <HeaderStyle CssClass="StudyDetailsViewHeader" Wrap="false" />
            <ItemTemplate>
                <asp:Label ID="Tier" runat="server" />
            </ItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="<%$Resources: DetailedViewFieldLabels, StudyStorage_StudyFolder%>">
            <HeaderStyle CssClass="StudyDetailsViewHeader" Wrap="false" />
            <ItemTemplate>
                <asp:Label ID="StudyFolder" runat="server" />
            </ItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="<%$Resources: DetailedViewFieldLabels, StudyStorage_StudySize %>">
            <HeaderStyle CssClass="StudyDetailsViewHeader" Wrap="false" />
            <ItemTemplate>
                <asp:Label ID="StudySize" runat="server" /> 
            </ItemTemplate>
        </asp:TemplateField>
    </Fields>
    <RowStyle CssClass="GlobalGridViewRow"/>
    <AlternatingRowStyle CssClass="GlobalGridViewAlternatingRow" />
    <EmptyDataTemplate>
        <asp:Table ID="Table1" runat="server" Width="100%" CellPadding="0" CellSpacing="0" >
            <asp:TableRow>
                <asp:TableCell ColumnSpan="3" Height="50" HorizontalAlign="Center">
                    <asp:panel ID="Panel1" runat="server" CssClass="GlobalGridViewEmptyText"><%= SR.StudyDetails_NoStudyStorageForThisStudy %></asp:panel>
                </asp:TableCell>
            </asp:TableRow>
       </asp:Table>
    </EmptyDataTemplate>
</asp:DetailsView>
                    		</asp:TableCell>
	</asp:TableRow>
</asp:Table>