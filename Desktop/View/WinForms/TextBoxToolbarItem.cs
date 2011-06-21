﻿#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca

// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using ClearCanvas.Desktop.Actions;

namespace ClearCanvas.Desktop.View.WinForms
{
	class TextBoxToolbarItem : ToolStripTextBox
	{
		private ITextBoxAction _action;
		private readonly EventHandler _actionEnabledChangedHandler;
		private readonly EventHandler _actionVisibleChangedHandler;
		private readonly EventHandler _actionAvailableChangedHandler;
		private readonly EventHandler _actionLabelChangedHandler;
		private readonly EventHandler _actionTooltipChangedHandler;
		private readonly EventHandler _actionIconSetChangedHandler;
		private readonly EventHandler _actionTextBoxValueChangedHandler;

		private IconSize _iconSize;

		public TextBoxToolbarItem(ITextBoxAction action)
			: this(action, IconSize.Medium)
		{
		}

		public TextBoxToolbarItem(ITextBoxAction action, IconSize iconSize)
		{
			_action = action;

			_actionEnabledChangedHandler = new EventHandler(OnActionEnabledChanged);
			_actionVisibleChangedHandler = new EventHandler(OnActionVisibleChanged);
			_actionAvailableChangedHandler = new EventHandler(OnActionAvailableChanged);
			_actionLabelChangedHandler = new EventHandler(OnActionLabelChanged);
			_actionTooltipChangedHandler = new EventHandler(OnActionTooltipChanged);
			_actionIconSetChangedHandler = new EventHandler(OnActionIconSetChanged);
			_actionTextBoxValueChangedHandler = new EventHandler(OnActionTextBoxValueChanged);

			_action.EnabledChanged += _actionEnabledChangedHandler;
			_action.VisibleChanged += _actionVisibleChangedHandler;
			_action.AvailableChanged += _actionAvailableChangedHandler;
			_action.LabelChanged += _actionLabelChangedHandler;
			_action.TooltipChanged += _actionTooltipChangedHandler;
			_action.IconSetChanged += _actionIconSetChangedHandler;
			_action.TextValueChanged += _actionTextBoxValueChangedHandler;

			_iconSize = iconSize;

			this.Text = _action.TextValue;
			this.Enabled = _action.Enabled;
			SetTooltipText();

			UpdateVisibility();
			UpdateEnablement();
			UpdateIcon();

		}

		public IconSize IconSize
		{
			get { return _iconSize; }
			set
			{
				if (_iconSize != value)
				{
					_iconSize = value;
					UpdateIcon();
				}
			}
		}

		protected override void OnTextChanged(EventArgs e)
		{
			_action.TextValue = this.Text;
			base.OnTextChanged(e);
		}

		private void OnActionEnabledChanged(object sender, EventArgs e)
		{
			UpdateEnablement();
		}

		private void OnActionVisibleChanged(object sender, EventArgs e)
		{
			UpdateVisibility();
		}

		private void OnActionAvailableChanged(object sender, EventArgs e)
		{
			UpdateEnablement();
			UpdateVisibility();
		}

		private void OnActionLabelChanged(object sender, EventArgs e)
		{
			this.Text = _action.Label;
		}

		private void OnActionTooltipChanged(object sender, EventArgs e)
		{
			SetTooltipText();
		}

		private void OnActionIconSetChanged(object sender, EventArgs e)
		{
			UpdateIcon();
		}

		private void OnActionTextBoxValueChanged(object sender, EventArgs e)
		{
			if(_action.TextValue != this.Text)
			{
				this.Text = _action.TextValue;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _action != null)
			{
				// VERY IMPORTANT: instances of this class will be created and discarded frequently
				// throughout the lifetime of the application
				// therefore is it extremely important that the event handlers are disconnected
				// from the underlying _action events
				// otherwise, this object will hang around for the entire lifetime of the _action object,
				// even though this object is no longer needed
				_action.EnabledChanged -= _actionEnabledChangedHandler;
				_action.VisibleChanged -= _actionVisibleChangedHandler;
				_action.AvailableChanged -= _actionAvailableChangedHandler;
				_action.LabelChanged -= _actionLabelChangedHandler;
				_action.TooltipChanged -= _actionTooltipChangedHandler;
				_action.IconSetChanged -= _actionIconSetChangedHandler;
				_action.TextValueChanged -= _actionTextBoxValueChangedHandler;

				_action = null;
			}
			base.Dispose(disposing);
		}

		private void UpdateVisibility()
		{
			this.Visible = _action.Available && _action.Visible && (_action.Permissible || DesktopViewSettings.Default.ShowNonPermissibleActions);
		}

		private void UpdateEnablement()
		{
			this.Enabled = _action.Available && _action.Enabled && (_action.Permissible || DesktopViewSettings.Default.EnableNonPermissibleActions);
		}

		private void UpdateIcon()
		{
			ActionViewUtils.SetIcon(this, _action, _iconSize);
		}

		private void SetTooltipText()
		{
			ActionViewUtils.SetTooltipText(this, _action);
		}
	}
}
