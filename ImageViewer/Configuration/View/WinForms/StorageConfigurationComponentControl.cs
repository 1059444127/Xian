#region License

// Copyright (c) 2011, ClearCanvas Inc.
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
using System.Drawing;

namespace ClearCanvas.ImageViewer.Configuration.View.WinForms
{
    /// <summary>
    /// Provides a Windows Forms user-interface for <see cref="StorageConfigurationComponent"/>
    /// </summary>
    public partial class StorageConfigurationComponentControl : ApplicationComponentUserControl
    {
        private StorageConfigurationComponent _component;

        /// <summary>
        /// Constructor
        /// </summary>
        public StorageConfigurationComponentControl(StorageConfigurationComponent component)
            : base(component)
        {
            InitializeComponent();

            _component = component;

            _fileStoreDirectory.DataBindings.Add("Text", _component, "FileStoreDirectory", true, DataSourceUpdateMode.OnPropertyChanged);

            Binding maxDiskUsageBinding = new Binding("Value", _component, "MaximumUsedSpacePercent", true, DataSourceUpdateMode.OnPropertyChanged);
			maxDiskUsageBinding.Parse += ParseDiskUsageBinding;
			maxDiskUsageBinding.Format += FormatDiskUsageBinding;

            _maxDiskSpaceDisplay.DataBindings.Add("Text", _component, "MaximumUsedSpaceDisplay", true, DataSourceUpdateMode.OnPropertyChanged);
            _upDownMaxDiskSpace.DataBindings.Add("Value", _component, "MaximumUsedSpacePercent", true, DataSourceUpdateMode.OnPropertyChanged);
			_maxDiskSpace.DataBindings.Add(maxDiskUsageBinding);

            Binding usedSpaceMeterBinding = new Binding("FillState", _component, "IsMaximumUsedSpaceExceeded", true, DataSourceUpdateMode.OnPropertyChanged);
            usedSpaceMeterBinding.Parse += ParseMeterFillState;
            usedSpaceMeterBinding.Format += FormatMeterFillState;

            _usedSpaceMeter.DataBindings.Add(usedSpaceMeterBinding);
            _usedSpaceMeter.DataBindings.Add("Value", _component, "UsedSpacePercent", true, DataSourceUpdateMode.OnPropertyChanged);

            _usedDiskSpace.DataBindings.Add("Text", _component, "UsedSpacePercentDisplay", true, DataSourceUpdateMode.OnPropertyChanged);
            _usedDiskSpaceDisplay.DataBindings.Add("Text", _component, "UsedSpaceBytesDisplay", true, DataSourceUpdateMode.OnPropertyChanged);

            _diskSpaceWarningIcon.DataBindings.Add("Visible", _component, "IsMaximumUsedSpaceExceeded", true, DataSourceUpdateMode.OnPropertyChanged);
            _diskSpaceWarningMessage.DataBindings.Add("Visible", _component, "IsMaximumUsedSpaceExceeded", true, DataSourceUpdateMode.OnPropertyChanged);
            _diskSpaceWarningMessage.DataBindings.Add("Text", _component, "MaximumUsedSpaceExceededMessage", true, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void FormatMeterFillState(object sender, ConvertEventArgs e)
        {
            bool isMaximumUsedSpaceExceeded = (bool)e.Value;
            e.Value = isMaximumUsedSpaceExceeded ? MeterFillState.Error : MeterFillState.Normal;
        }

        private void ParseMeterFillState(object sender, ConvertEventArgs e)
        {
            var meterState = (MeterFillState)e.Value;
            e.Value = meterState == MeterFillState.Error;
        }

        private void FormatDiskUsageBinding(object sender, ConvertEventArgs e)
		{
            double value = (double)e.Value;
			e.Value = (int)(value * 1000F);
		}

		private void ParseDiskUsageBinding(object sender, ConvertEventArgs e)
		{
			int value = (int)e.Value;
			e.Value = value / 1000.0;
		}

        private void _changeFileStore_Click(object sender, EventArgs e)
        {
            _component.ChangeFileStore();
        }
    }
}
