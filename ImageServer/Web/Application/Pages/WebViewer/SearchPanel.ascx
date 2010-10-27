<%@ Control Language="C#" AutoEventWireup="true" Codebehind="SearchPanel.ascx.cs" Inherits="ClearCanvas.ImageServer.Web.Application.Pages.WebViewer.SearchPanel" %>

<%@ Register Src="StudyListGridView.ascx" TagName="StudyListGridView" TagPrefix="localAsp" %>
<%@ Register Src="GridPager.ascx" TagName="GridPager" TagPrefix="localAsp" %>

<asp:UpdatePanel ID="SearchUpdatePanel" runat="server" UpdateMode="conditional">
    <ContentTemplate>
            <asp:Table runat="server">
                <asp:TableRow>
                    <asp:TableCell>
                        <table width="100%" cellpadding="0" cellspacing="0" class="WebViewerToolbarButtonPanel">
                            <tr><td>
                            <asp:UpdatePanel ID="ToolBarUpdatePanel" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <asp:Panel ID="ToolbarButtons" runat="server" CssClass="ToolbarButtons" style="margin-top: 4px;">
                                        <ccUI:ToolbarButton ID="ViewImageButton" runat="server" SkinID="ViewImagesButton" />
                                    </asp:Panel>
                             </ContentTemplate>
                          </asp:UpdatePanel>                  
                        </td></tr>
                        <tr><td>

                         <asp:Panel ID="Panel2" runat="server">
                            <table width="100%" cellpadding="0" cellspacing="0">
                                 <tr><td><localAsp:GridPager ID="GridPagerTop" runat="server" /></td></tr>                        
                                <tr><td style="background-color: #222222; border-bottom: solid 2px #999999;">
                                <localAsp:StudyListGridView id="StudyListGridView" runat="server" Height="500px"></localAsp:StudyListGridView></td></tr>
                            </table>                        
                        </asp:Panel>
                        </td>
                        </tr>
                        </table>
                    </asp:TableCell>
                </asp:TableRow>
            </asp:Table>
    </ContentTemplate>
</asp:UpdatePanel>
