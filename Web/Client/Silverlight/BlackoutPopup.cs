#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace ClearCanvas.Web.Client.Silverlight
{
    public interface IBlackoutPopup : IPopup
    {
        event EventHandler ClickOutsideChild;
    }

    public class BlackoutPopup : FrameworkPopupProxy, IBlackoutPopup
    {
        private Popup _backgroundPopup;
        private Canvas _backgroundCanvas;

        private event EventHandler _clickOutsideContent;

        public BlackoutPopup()
        {
            _backgroundCanvas = new Canvas();
            _backgroundCanvas.Background = new SolidColorBrush(Colors.Transparent);
            _backgroundCanvas.MouseLeftButtonDown += OnCanvasButtonDown;
            _backgroundCanvas.MouseRightButtonDown += OnCanvasButtonDown;
            _backgroundCanvas.MouseLeftButtonUp += OnCanvasButtonUp;
            _backgroundCanvas.MouseRightButtonUp += OnCanvasButtonUp;

            _backgroundPopup = new Popup();
            _backgroundPopup.Opened += OnBackgroundPopupOpened;
            _backgroundPopup.Closed += OnBackgroundPopupClosed;
            _backgroundPopup.LayoutUpdated += OnBackgroundPopupLayoutUpdated;
            _backgroundPopup.Child = _backgroundCanvas;
        }

        private void OnBackgroundPopupLayoutUpdated(object sender, EventArgs e)
        {
            ResizeBackgroundCanvas();
        }

        public Brush Background
        {
            get { return _backgroundCanvas.Background; }
            set { _backgroundCanvas.Background = value; }
        }

        public override bool IsOpen
        {
            get { return base.IsOpen; }
            set
            {
                if (_backgroundPopup != null)
                    _backgroundPopup.IsOpen = value;
                else
                    return;

                base.IsOpen = value;
            }
        }

        public event EventHandler ClickOutsideChild
        {
            add { _clickOutsideContent += value; }
            remove { _clickOutsideContent -= value; }
        }

        private void ResizeBackgroundCanvas()
        {
            _backgroundCanvas.Width = Application.Current.Host.Content.ActualWidth;
            _backgroundCanvas.Height = Application.Current.Host.Content.ActualHeight;
        }

        private void OnRootVisualSizeChanged(object sender, EventArgs e)
        {
            ResizeBackgroundCanvas();
        }

        private void OnBackgroundPopupOpened(object sender, EventArgs e)
        {
            Application.Current.Host.Content.Resized += OnRootVisualSizeChanged;
        }

        private void OnBackgroundPopupClosed(object sender, EventArgs e)
        {
            Application.Current.Host.Content.Resized -= OnRootVisualSizeChanged;
        }

        private void OnCanvasButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void OnCanvasButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (_clickOutsideContent != null)
                _clickOutsideContent(sender, EventArgs.Empty);
        }
    }
}
