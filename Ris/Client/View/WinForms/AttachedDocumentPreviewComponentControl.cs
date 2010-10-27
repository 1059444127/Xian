﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System.Windows.Forms;
using ClearCanvas.Desktop.View.WinForms;

namespace ClearCanvas.Ris.Client.View.WinForms
{
	/// <summary>
	/// Provides a Windows Forms user-interface for <see cref="AttachedDocumentPreviewComponent"/>
	/// </summary>
	public partial class AttachedDocumentPreviewComponentControl : ApplicationComponentUserControl
	{
		private readonly AttachedDocumentPreviewComponent _component;

		public AttachedDocumentPreviewComponentControl(AttachedDocumentPreviewComponent component)
			: base(component)
		{
			InitializeComponent();
			_component = component;

			_attachments.ShowToolbar = !_component.Readonly;

			_attachments.Table = _component.AttachmentTable;

			if (!_component.Readonly)
			{
				_attachments.MenuModel = _component.AttachmentActionModel;
				_attachments.ToolbarModel = _component.AttachmentActionModel;

			}
			_attachments.DataBindings.Add("Selection", _component, "Selection", true, DataSourceUpdateMode.OnPropertyChanged);

			var preview = (Control)_component.PreviewHost.ComponentView.GuiElement;
			preview.Dock = DockStyle.Fill;
			_splitContainer.Panel2.Controls.Add(preview);
		}

		private void AttachedDocumentPreviewComponentControl_Load(object sender, System.EventArgs e)
		{
			_component.OnControlLoad();
		}

	}
}
