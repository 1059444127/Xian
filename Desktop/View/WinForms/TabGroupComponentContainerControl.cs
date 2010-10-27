﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System.Windows.Forms;

namespace ClearCanvas.Desktop.View.WinForms
{
    public partial class TabGroupComponentContainerControl : UserControl
    {
        TabGroupComponentContainer _component;

        public TabGroupComponentContainerControl(TabGroupComponentContainer component)
        {
            InitializeComponent();
            _component = component;

            CreateTabGroups();
        }

        private void CreateTabGroups()
        {
            _tabbedGroupsControl.PageChanged += new Crownwood.DotNetMagic.Controls.TabbedGroups.PageChangeHandler(OnControlPageChanged);

            _tabbedGroupsControl.RootDirection = _component.LayoutDirection == LayoutDirection.Vertical ?
                Crownwood.DotNetMagic.Common.LayoutDirection.Vertical :
                Crownwood.DotNetMagic.Common.LayoutDirection.Horizontal;

            foreach (TabGroup tabGroup in _component.TabGroups)
            {
                Crownwood.DotNetMagic.Controls.TabGroupLeaf tgl = _tabbedGroupsControl.RootSequence.AddNewLeaf() as Crownwood.DotNetMagic.Controls.TabGroupLeaf;

                foreach (TabPage page in tabGroup.Component.Pages)
                {
                    Crownwood.DotNetMagic.Controls.TabPage tabPageUI = new Crownwood.DotNetMagic.Controls.TabPage(page.Name);
                    tabPageUI.Tag = page;
                    tgl.TabPages.Add(tabPageUI);
                }
            }

            // The weight can only be set after each leaf is created
            // Ask control to reposition children according to new spacing
            for (int i = 0; i < _component.TabGroups.Count; i++)
            {
                Crownwood.DotNetMagic.Controls.TabGroupLeaf tgl = _tabbedGroupsControl.RootSequence[i] as Crownwood.DotNetMagic.Controls.TabGroupLeaf;
                tgl.Space = (decimal)(_component.TabGroups[i].Weight * 100);
            }

            _tabbedGroupsControl.RootSequence.Reposition();
        }

        private void OnControlPageChanged(Crownwood.DotNetMagic.Controls.TabbedGroups tg, Crownwood.DotNetMagic.Controls.TabPage selectedPage)
        {
            if (selectedPage != null)
            {
                TabPage tabPage = selectedPage.Tag as TabPage;

                if (tabPage.Component.IsStarted == false)
                    tabPage.Component.Start();

                if (selectedPage.Control == null)
                {
                    TabGroup tabGroup = _component.GetTabGroup(tabPage);
                    selectedPage.Control = (Control)tabGroup.Component.GetPageView(tabPage).GuiElement;
                }
            }
        }

        private void OnTabControlCreated(Crownwood.DotNetMagic.Controls.TabbedGroups tg, Crownwood.DotNetMagic.Controls.TabControl tc)
        {
            // Place a thin border between edge of the tab control and inside contents
            tc.ControlTopOffset = 3;
            tc.ControlBottomOffset = 3;
            tc.ControlLeftOffset = 3;
            tc.ControlRightOffset = 3;
            tc.Style = Crownwood.DotNetMagic.Common.VisualStyle.IDE2005;
        }
    }
}
