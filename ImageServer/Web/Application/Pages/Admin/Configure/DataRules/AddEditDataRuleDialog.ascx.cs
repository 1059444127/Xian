﻿#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Enterprise.Common.Admin.AuthorityGroupAdmin;
using ClearCanvas.ImageServer.Enterprise;
using ClearCanvas.ImageServer.Model;
using ClearCanvas.ImageServer.Rules;
using ClearCanvas.ImageServer.Web.Application.Controls;
using ClearCanvas.ImageServer.Web.Common.Data;
using ClearCanvas.ImageServer.Web.Common.WebControls.Validators;
using ClearCanvas.Web.Enterprise.Admin;
using Resources;

namespace ClearCanvas.ImageServer.Web.Application.Pages.Admin.Configure.DataRules
{
    public partial class AddEditDataRuleDialog : UserControl
    {
        #region private variables

        // The server partitions that the new device can be associated with
        // This list will be determined by the user level permission.

        private bool _editMode;
        private ServerPartition _partition;
        private ServerRule _rule;

        #endregion

        #region public members

        /// <summary>
        /// Sets the list of partitions users allowed to pick.
        /// </summary>
        public ServerPartition Partition
        {
            set
            {
                _partition = value;
                ViewState["_ServerPartition"] = value;
            }

            get { return _partition; }
        }

        /// <summary>
        /// Sets or gets the value which indicates whether the dialog is in edit mode.
        /// </summary>
        public bool EditMode
        {
            get { return _editMode; }
            set
            {
                _editMode = value;
                ViewState["_EditMode"] = value;
            }
        }

        /// <summary>
        /// Sets/Gets the current editing device.
        /// </summary>
        public ServerRule ServerRule
        {
            set
            {
                _rule = value;
                // put into viewstate to retrieve later
                if (_rule != null)
                    ViewState["_EdittedRule"] = _rule.GetKey();
            }
            get { return _rule; }
        }

        #endregion // public members

        #region Events

        #region Delegates

        /// <summary>
        /// Defines the event handler for <seealso cref="OKClicked"/>.
        /// </summary>
        /// <param name="rule">The device being added.</param>
        public delegate void OnOKClickedEventHandler(ServerRule rule);

        #endregion

        /// <summary>
        /// Occurs when users click on "OK".
        /// </summary>
        public event OnOKClickedEventHandler OKClicked;

        #endregion Events

        #region Protected Methods

        private static Dictionary<ServerRuleTypeEnum, IList<ServerRuleApplyTimeEnum>> LoadRuleTypes(object[] extensions)
        {

            IList<ISampleRule> list = new List<ISampleRule>();
            foreach (ISampleRule rule in extensions)
            {
                if (rule.Type.Equals(ServerRuleTypeEnum.DataAccess))
                {
                    list.Add(rule);
                }
            }

            var ruleTypeList = new Dictionary<ServerRuleTypeEnum, IList<ServerRuleApplyTimeEnum>>();
            foreach (ISampleRule extension in list)
            {
                if (!ruleTypeList.ContainsKey(extension.Type))
                    ruleTypeList.Add(extension.Type, extension.ApplyTimeList);
            }

            return ruleTypeList;
        }

        private static string GetJavascriptForSampleRule(ServerRuleTypeEnum typeEnum, object[] extensions)
        {
            string sampleList = string.Empty;

            foreach (ISampleRule extension in extensions)
            {
                sampleList +=
                    String.Format(
                        @"        myEle = document.createElement('option') ;
                    myEle.value = '{0}';
                    myEle.text = '{1}' ;
                    if(navigator.appName == 'Microsoft Internet Explorer') sampleList.add(myEle);
                    else sampleList.add(myEle, null);",
                        extension.Name, extension.Description);
            }
         
            return
                String.Format(
                    @"if (val == '{0}')
                {{
                    myEle = document.createElement('option') ;
                    myEle.value = '';
                    myEle.text = '' ;
                    if(navigator.appName == 'Microsoft Internet Explorer') sampleList.add(myEle);
                    else sampleList.add(myEle, null);
                    {1}
                }}",
                    typeEnum.Lookup, sampleList);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var ep = new SampleRuleExtensionPoint();
            var extensions = ep.CreateExtensions();
        
            Dictionary<ServerRuleTypeEnum, IList<ServerRuleApplyTimeEnum>> ruleTypeList = LoadRuleTypes(extensions);


            SampleRuleDropDownList.Attributes.Add("onchange", "webServiceScript(this, this.SelectedIndex);");

            string javascript =
                @"<script type='text/javascript'>
            function ValidationServerRuleParams()
            {
                control = document.getElementById('" +
                RuleXmlTextBox.ClientID +
                @"');
                params = new Array();
                params.serverRule=escape(CodeMirrorEditor.getCode());
				params.ruleType = '" + ServerRuleTypeEnum.DataAccess.Lookup + @"';
                return params;
            }

            function selectRuleType(oList, selectedIndex)
            {                         
                var val = oList.value; 
                var sampleList = document.getElementById('" +
                SampleRuleDropDownList.ClientID +
                @"');
                
                for (var q=sampleList.options.length; q>=0; q--) sampleList.options[q]=null;
				";

            bool first = true;
            foreach (ServerRuleTypeEnum type in ruleTypeList.Keys)
            {
                if (!first)
                {
                    javascript += "else ";
                }
                else
                    first = false;

                javascript += GetJavascriptForSampleRule(type, extensions);
            }

            javascript +=
                @"}

            // This function calls the Web Service method.  
            function webServiceScript(oList)
            {
                var type = oList.value;
             
                ClearCanvas.ImageServer.Web.Application.Pages.Admin.Configure.DataRules.DataRuleSamples.GetXml(type,
                    OnSucess, OnError);
            }
            function OnError(result)
            {
                alert('Error: ' + result.get_message());
            }

            // This is the callback function that
            // processes the Web Service return value.
            function OnSucess(result)
            {
                var oList = document.getElementById('" +
                SampleRuleDropDownList.ClientID +
                @"');
                var sValue = oList.options[oList.selectedIndex].value;
             
                RsltElem = document.getElementById('" +
                RuleXmlTextBox.ClientID +
                @"');

                //Set the value on the TextArea and then set the value in the Editor.
                //CodeMirror doesn't monitor changes to the textarea.
                RsltElem.value = result;
                CodeMirrorEditor.setCode(RsltElem.value);
            }
           
            function pageLoad(){
                $find('" +
                ModalDialog.PopupExtenderID +
                @"').add_shown(HighlightXML);
            }

            function HighlightXML() {
                CodeMirrorEditor = CodeMirror.fromTextArea('" +
                RuleXmlTextBox.ClientID +
                @"', {parserfile: 'parsexml.js',path: '../../../../Scripts/CodeMirror/js/', stylesheet: '../../../../Scripts/CodeMirror/css/xmlcolors.css'});
            }

	        function UpdateRuleXML() {
                RsltElem = document.getElementById('" +
                RuleXmlTextBox.ClientID +
                @"');	            

                RsltElem.value = CodeMirrorEditor.getCode();    
	        }
  
            var CodeMirrorEditor = null;
            </script>";

            Page.ClientScript.RegisterClientScriptBlock(GetType(), ClientID, javascript);

            Page.ClientScript.RegisterClientScriptInclude(GetType(), "CodeMirrorLibrary",
                                                          "../../../../Scripts/CodeMirror/js/codemirror.js");

            EditDataRuleValidationSummary.HeaderText = ErrorMessages.EditServerRuleValidationError;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack == false)
            {

                using (AuthorityManagement service = new AuthorityManagement())
                {
                    IList<AuthorityGroupSummary> tokens = service.ListDataAccessAuthorityGroups();
                    IList<ListItem> items = CollectionUtils.Map<AuthorityGroupSummary, ListItem>(
                                            tokens,
                                            delegate(AuthorityGroupSummary group)
                                            {
                                                ListItem item =  new ListItem(group.Name, group.AuthorityGroupRef.ToString(false,false));
                                                item.Attributes["title"] = group.Description;
                                                return item;
                                            });

                    AuthorityGroupCheckBoxList.Items.AddRange(CollectionUtils.ToArray(items));
                }
            }
            else
            {
                if (ViewState["_EditMode"] != null)
                    _editMode = (bool)ViewState["_EditMode"];

                if (ViewState["_ServerPartition"] != null)
                    _partition = (ServerPartition)ViewState["_ServerPartition"];

                if (ViewState["_EdittedRule"] != null)
                {
                    var ruleKey = ViewState["_EdittedRule"] as ServerEntityKey;
                    _rule = ServerRule.Load(ruleKey);
                }
            }
        }

        protected void OKButton_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                SaveData();
                if (OKClicked != null)
                {
                    OKClicked(ServerRule);
                }

                Close();
            }
            else
            {
                Show();
            }
        }

        protected void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion Protected Methods

        #region Private Methods

        private void SaveData()
        {
            if (_rule == null)
            {
                _rule = new ServerRule
                            {
                                ServerRuleApplyTimeEnum = ServerRuleApplyTimeEnum.StudyProcessed
                            };
            }
            

            if (RuleXmlTextBox.Text.Length > 0)
            {
                StudyDataAccessController controller = new StudyDataAccessController();

                _rule.RuleXml = new XmlDocument();

                StringBuilder sb = new StringBuilder();
                sb.Append("<rule>");
                sb.Append(RuleXmlTextBox.Text);
                sb.Append("<action>");
                foreach (ListItem item in AuthorityGroupCheckBoxList.Items)
                {
                    if (item.Selected)
                    {
                        sb.AppendFormat("<grant-access authorityGroupOid=\"{0}\"/>", item.Value);
                        // Add if it doesn't exist to the DataAccessGroup table
                        controller.AddDataAccessIfNotExists(item.Value);
                    }
                }
                sb.Append("</action>");
                sb.Append("</rule>");

                _rule.RuleXml.Load(new StringReader(sb.ToString()));
            }

            _rule.RuleName = RuleNameTextBox.Text;

            _rule.ServerRuleTypeEnum = ServerRuleTypeEnum.DataAccess;

            _rule.Enabled = EnabledCheckBox.Checked;
            _rule.DefaultRule = DefaultCheckBox.Checked;
            _rule.ServerPartitionKey = Partition.GetKey();
            _rule.ExemptRule = ExemptRuleCheckBox.Checked;
        }

        #endregion Private Methods

        #region Public methods

        /// <summary>
        /// Displays the add/edit device dialog box.
        /// </summary>
        public void Show()
        {
            //If the validation failed, keep everything as is, and 
            //make sure the dialog stays visible.
            if (!Page.IsValid)
            {
                ModalDialog.Show();
                return;
            }


            var ep = new SampleRuleExtensionPoint();
            object[] extensions = ep.CreateExtensions();

            if (EditMode)
            {
                ModalDialog.Title = SR.DialogEditDataRuleTitle;
                OKButton.Visible = false;
                UpdateButton.Visible = true;

                DefaultCheckBox.Checked = _rule.DefaultRule;
                EnabledCheckBox.Checked = _rule.Enabled;
                ExemptRuleCheckBox.Checked = _rule.ExemptRule;

                //if (_rule.DefaultRule)
                //	DefaultCheckBox.Enabled = false;

                RuleNameTextBox.Text = _rule.RuleName;

                SampleRuleDropDownList.Visible = false;
                SelectSampleRuleLabel.Visible = false;

                // Fill in the drop down menus


                // Fill in the Rule XML
                var sw = new StringWriter();

                var xmlSettings = new XmlWriterSettings
                                      {
                                          Encoding = Encoding.UTF8,
                                          ConformanceLevel = ConformanceLevel.Fragment,
                                          Indent = true,
                                          NewLineOnAttributes = false,
                                          CheckCharacters = true,
                                          IndentChars = "  "
                                      };

                XmlWriter tw = XmlWriter.Create(sw, xmlSettings);

                XmlNode node2 = _rule.RuleXml.SelectSingleNode("/rule/condition");
                
                node2.WriteTo(tw);

                tw.Close();

                RuleXmlTextBox.Text = sw.ToString();

                DataRuleValidator.RuleTypeControl = ServerRuleTypeEnum.DataAccess.Lookup;

                AuthorityGroupCheckBoxList.ClearSelection();

                foreach (XmlNode node in _rule.RuleXml.SelectNodes("/rule/action/grant-access"))
                {
                    string oid = node.Attributes["authorityGroupOid"].Value;
                    ListItem item = AuthorityGroupCheckBoxList.Items.FindByValue(oid);
                    if (item != null) item.Selected = true;
                }
            }
            else
            {
                ModalDialog.Title = SR.DialogAddDataRuleTitle;
                OKButton.Visible = false;
                UpdateButton.Visible = true;

                DefaultCheckBox.Checked = false;
                EnabledCheckBox.Checked = true;
                ExemptRuleCheckBox.Checked = false;

                RuleNameTextBox.Text = string.Empty;
                RuleXmlTextBox.Text = string.Empty;

                SampleRuleDropDownList.Visible = true;
                SelectSampleRuleLabel.Visible = true;

                // Do the drop down lists
                SampleRuleDropDownList.Items.Clear();
                SampleRuleDropDownList.Items.Add(new ListItem(string.Empty, string.Empty));
                foreach (ISampleRule extension in extensions)
                {
                    if (extension.Type.Equals(ServerRuleTypeEnum.DataAccess))
                    {
                        SampleRuleDropDownList.Items.Add(new ListItem(extension.Description, extension.Name));
                    }
                }

                DataRuleValidator.RuleTypeControl = ServerRuleTypeEnum.DataAccess.Lookup;
            }

            ModalDialog.Show();
            return;
        }

        public void Close()
        {
            ModalDialog.Hide();
        }

        #endregion
    }
}