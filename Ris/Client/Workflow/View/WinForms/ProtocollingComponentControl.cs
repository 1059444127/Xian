﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.ComponentModel;
using System.Windows.Forms;
using ClearCanvas.Desktop.View.WinForms;

namespace ClearCanvas.Ris.Client.Workflow.View.WinForms
{
	/// <summary>
	/// Provides a Windows Forms user-interface for <see cref="ProtocollingComponent"/>
	/// </summary>
	public partial class ProtocollingComponentControl : ApplicationComponentUserControl
	{
		private readonly ProtocollingComponent _component;

		/// <summary>
		/// Constructor
		/// </summary>
		public ProtocollingComponentControl(ProtocollingComponent component)
			: base(component)
		{
			InitializeComponent();
			_component = component;

			_overviewLayoutPanel.RowStyles[0].Height = _component.BannerHeight;

			var orderSummary = (Control)_component.BannerComponentHost.ComponentView.GuiElement;
			orderSummary.Dock = DockStyle.Fill;
			_orderSummaryPanel.Controls.Add(orderSummary);

			var protocolEditor = (Control)_component.ProtocolEditorComponentHost.ComponentView.GuiElement;
			protocolEditor.Dock = DockStyle.Fill;
			_protocolEditorPanel.Controls.Add(protocolEditor);

			var orderNotes = (Control)_component.OrderNotesComponentHost.ComponentView.GuiElement;
			orderNotes.Dock = DockStyle.Fill;
			_orderNotesPanel.Controls.Add(orderNotes);

			var rightHandContent = (Control)_component.RightHandComponentContainerHost.ComponentView.GuiElement;
			rightHandContent.Dock = DockStyle.Fill;
			_rightHandPanel.Controls.Add(rightHandContent);

			_statusText.DataBindings.Add("Text", _component, "StatusText", true, DataSourceUpdateMode.OnPropertyChanged);
			_statusText.DataBindings.Add("Visible", _component, "ShowStatusText", true, DataSourceUpdateMode.OnPropertyChanged);

			_protocolledProcedures.DataBindings.Add("Text", _component, "ProceduresText", true, DataSourceUpdateMode.OnPropertyChanged);

			_protocolNextItem.DataBindings.Add("Checked", _component, "ProtocolNextItem", true, DataSourceUpdateMode.OnPropertyChanged);
			_protocolNextItem.DataBindings.Add("Enabled", _component, "ProtocolNextItemEnabled", true, DataSourceUpdateMode.OnPropertyChanged);

			_btnAccept.DataBindings.Add("Enabled", _component, "AcceptEnabled", true, DataSourceUpdateMode.OnPropertyChanged);
			_btnAccept.DataBindings.Add("Visible", _component, "AcceptVisible", true, DataSourceUpdateMode.OnPropertyChanged);

			_btnSubmitForApproval.DataBindings.Add("Enabled", _component, "SubmitForApprovalEnabled", true, DataSourceUpdateMode.OnPropertyChanged);
			_btnSubmitForApproval.Visible = _component.SubmitForApprovalVisible;

			_btnReject.DataBindings.Add("Enabled", _component, "RejectEnabled", true, DataSourceUpdateMode.OnPropertyChanged);
			_btnSave.DataBindings.Add("Enabled", _component, "SaveEnabled", true, DataSourceUpdateMode.OnPropertyChanged);
			_btnSkip.DataBindings.Add("Enabled", _component, "SkipEnabled", true, DataSourceUpdateMode.OnPropertyChanged);

			_component.PropertyChanged += _component_PropertyChanged;
		}

		private void _component_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "StatusText")
			{
				_statusText.Refresh();
			}
		}

		private void _btnAccept_Click(object sender, EventArgs e)
		{
			using (new CursorManager(this, Cursors.WaitCursor))
			{
				_component.Accept();
			}
		}

		private void _btnSubmitForApproval_Click(object sender, EventArgs e)
		{
			using (new CursorManager(this, Cursors.WaitCursor))
			{
				_component.SubmitForApproval();
			}
		}

		private void _btnReject_Click(object sender, EventArgs e)
		{
			using (new CursorManager(this, Cursors.WaitCursor))
			{
				_component.Reject();
			}
		}

		private void _btnSave_Click(object sender, EventArgs e)
		{
			using (new CursorManager(this, Cursors.WaitCursor))
			{
				_component.Save();
			}
		}

		private void _btnCancel_Click(object sender, EventArgs e)
		{
			using (new CursorManager(this, Cursors.WaitCursor))
			{
				_component.Cancel();
			}
		}

		private void _btnSkip_Click(object sender, EventArgs e)
		{
			using (new CursorManager(this, Cursors.WaitCursor))
			{
				_component.Skip();
			}
		}
	}
}
