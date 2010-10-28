#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using ClearCanvas.Common;
using ClearCanvas.Desktop.Actions;
using System.Text;

namespace ClearCanvas.Desktop.View.WinForms
{
    public class ActiveToolbarButton : ToolStripButton
    {
        private IClickAction _action;
        private EventHandler _actionEnabledChangedHandler;
        private EventHandler _actionCheckedChangedHandler;
		private EventHandler _actionVisibleChangedHandler;
		private EventHandler _actionAvailableChangedHandler;
		private EventHandler _actionLabelChangedHandler;
		private EventHandler _actionTooltipChangedHandler;
		private EventHandler _actionIconSetChangedHandler;

		private IconSize _iconSize;

		public ActiveToolbarButton(IClickAction action)
			: this(action, IconSize.Medium)
		{
		}

		public ActiveToolbarButton(IClickAction action, IconSize iconSize)
        {
            _action = action;

            _actionEnabledChangedHandler = new EventHandler(OnActionEnabledChanged);
            _actionCheckedChangedHandler = new EventHandler(OnActionCheckedChanged);
			_actionVisibleChangedHandler = new EventHandler(OnActionVisibleChanged);
			_actionAvailableChangedHandler = new EventHandler(OnActionAvailableChanged);
			_actionLabelChangedHandler = new EventHandler(OnActionLabelChanged);
			_actionTooltipChangedHandler = new EventHandler(OnActionTooltipChanged);
			_actionIconSetChangedHandler = new EventHandler(OnActionIconSetChanged);

            _action.EnabledChanged += _actionEnabledChangedHandler;
            _action.CheckedChanged += _actionCheckedChangedHandler;
			_action.VisibleChanged += _actionVisibleChangedHandler;
			_action.AvailableChanged += _actionAvailableChangedHandler;
			_action.LabelChanged += _actionLabelChangedHandler;
			_action.TooltipChanged += _actionTooltipChangedHandler;
			_action.IconSetChanged += _actionIconSetChangedHandler;

			_iconSize = iconSize;

            this.Text = _action.Label;
            this.Enabled = _action.Enabled;
			SetTooltipText();
			this.Checked = _action.Checked;

            UpdateVisibility();
            UpdateEnablement();
			UpdateIcon();

            this.Click += delegate(object sender, EventArgs e)
            {
                _action.Click();
            };
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

        private void OnActionCheckedChanged(object sender, EventArgs e)
        {
            this.Checked = _action.Checked;
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
                _action.CheckedChanged -= _actionCheckedChangedHandler;
				_action.VisibleChanged -= _actionVisibleChangedHandler;
				_action.AvailableChanged -= _actionAvailableChangedHandler;
				_action.LabelChanged -= _actionLabelChangedHandler;
				_action.TooltipChanged -= _actionTooltipChangedHandler;
				_action.IconSetChanged -= _actionIconSetChangedHandler;

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
			if (_action.IconSet != null && _action.ResourceResolver != null)
			{
				try
				{
					Image oldImage = this.Image;

					this.Image = _action.IconSet.CreateIcon(_iconSize, _action.ResourceResolver);
					if (oldImage != null)
						oldImage.Dispose();

					this.Invalidate();
				}
				catch (Exception e)
				{
					// the icon was either null or not found - log some helpful message
					Platform.Log(LogLevel.Error, e);
				}
			}
		}

		private void SetTooltipText()
		{
			ToolTipText = GetTooltipText(_action);
		}

		internal static string GetTooltipText(IClickAction action)
		{
			string actionTooltip = action.Tooltip;
			if (string.IsNullOrEmpty(actionTooltip))
				actionTooltip = (action.Label ?? string.Empty).Replace("&", "");

			if (action.KeyStroke == XKeys.None)
				return actionTooltip;

			XKeys keyCode = action.KeyStroke & XKeys.KeyCode;
			
			StringBuilder builder = new StringBuilder();
			builder.Append(actionTooltip);

			if (keyCode != XKeys.None)
			{
				if (builder.Length > 0)
					builder.AppendLine();
				builder.AppendFormat("{0}: ", SR.LabelKeyboardShortcut);
				builder.Append(XKeysConverter.Format(action.KeyStroke));
			}

			return builder.ToString();
		}
	}
}
