<%@ Page Language="C#" MasterPageFile="~/Admin/MasterPage.master" AutoEventWireup="true" 
    EnableEventValidation="false"
    CodeBehind="DevicePage.aspx.cs" Inherits="ImageServerWebApplication.Admin.Configuration.DevicePage" Title="ClearCanvas ImageServer" %>

<%@ Register Src="../../Common/ConfirmDialog.ascx" TagName="ConfirmDialog" TagPrefix="uc1" %>

<%@ Register Src="EditDeviceDialog.ascx" TagName="EditDeviceDialog" TagPrefix="uc3" %>

<%@ Register Src="AddDeviceDialog.ascx" TagName="AddDeviceDialog" TagPrefix="uc2" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table cellpadding="0" cellspacing="0" width="100%">
        <tr>
            <td class="WindowTitleBar">
            Device Management
            </td>
         </tr>
         <tr>
            <td>
                <asp:Panel runat="server" ID="PageContent"  CssClass="ContentWindow">
                    
                <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                    
                    <cc1:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0" 
                        CssClass="visoft__tab_xpie7">
                        <cc1:TabPanel ID="TabPanel1" runat="server" HeaderText="TabPanel1">
                        <HeaderTemplate>
                        </HeaderTemplate>

                        <ContentTemplate>

                            
                        </ContentTemplate>


                        </cc1:TabPanel>
                    </cc1:TabContainer><uc2:AddDeviceDialog ID="AddDeviceControl1" runat="server" />
                        <uc3:EditDeviceDialog ID="EditDeviceControl1" runat="server" />
                    <uc1:ConfirmDialog ID="ConfirmDialog1" runat="server" />

                
                    </ContentTemplate>
                </asp:UpdatePanel>
                </asp:Panel>
       
            </td>
         </tr>
         
    </table>
    

</asp:Content>
