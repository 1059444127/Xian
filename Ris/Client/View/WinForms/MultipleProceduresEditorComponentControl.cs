#region License

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

namespace ClearCanvas.Ris.Client.View.WinForms
{
	/// <summary>
	/// Provides a Windows Forms user-interface for <see cref="MultipleProceduresEditorComponent"/>.
	/// </summary>
	public partial class MultipleProceduresEditorComponentControl : ApplicationComponentUserControl
	{
		private readonly MultipleProceduresEditorComponent _component;

		/// <summary>
		/// Constructor.
		/// </summary>
		public MultipleProceduresEditorComponentControl(MultipleProceduresEditorComponent component)
			:base(component)
		{
			_component = component;
			InitializeComponent();

			_enableScheduledDateTime.DataBindings.Add("Checked", _component, "IsScheduledDateTimeEditable", true, DataSourceUpdateMode.OnPropertyChanged);
			_enablePerformingFacility.DataBindings.Add("Checked", _component, "IsPerformingFacilityEditable", true, DataSourceUpdateMode.OnPropertyChanged);
			_enablePerformingDepartment.DataBindings.Add("Checked", _component, "IsPerformingDepartmentEditable", true, DataSourceUpdateMode.OnPropertyChanged);
			_enableLaterality.DataBindings.Add("Checked", _component, "IsLateralityEditable", true, DataSourceUpdateMode.OnPropertyChanged);
			_enableSchedulingCode.DataBindings.Add("Checked", _component, "IsSchedulingCodeEditable", true, DataSourceUpdateMode.OnPropertyChanged);
			_enablePortable.DataBindings.Add("Checked", _component, "IsPortableEditable", true, DataSourceUpdateMode.OnPropertyChanged);
			_enableCheckIn.DataBindings.Add("Checked", _component, "IsCheckedInEditable", true, DataSourceUpdateMode.OnPropertyChanged);

			_scheduledDate.DataBindings.Add("Value", _component, "ScheduledDate", true, DataSourceUpdateMode.OnPropertyChanged);
			_scheduledDate.DataBindings.Add("Enabled", _component, "IsScheduledDateTimeEditable");
			_scheduledTime.DataBindings.Add("Value", _component, "ScheduledTime", true, DataSourceUpdateMode.OnPropertyChanged);
			_scheduledTime.DataBindings.Add("Enabled", _component, "IsScheduledDateTimeEditable");

			_performingFacility.DataSource = _component.FacilityChoices;
			_performingFacility.DataBindings.Add("Value", _component, "SelectedFacility", true, DataSourceUpdateMode.OnPropertyChanged);
			_performingFacility.DataBindings.Add("Enabled", _component, "IsPerformingFacilityEditable");
			_performingFacility.Format += delegate(object sender, ListControlConvertEventArgs e)
			{
				e.Value = _component.FormatFacility(e.ListItem);
			};

			_performingDepartment.DataSource = _component.DepartmentChoices;
			_performingDepartment.DataBindings.Add("Value", _component, "SelectedDepartment", true, DataSourceUpdateMode.OnPropertyChanged);
			_performingDepartment.DataBindings.Add("Enabled", _component, "IsPerformingDepartmentEditable");
			_performingDepartment.Format += delegate(object sender, ListControlConvertEventArgs e)
			{
				e.Value = _component.FormatDepartment(e.ListItem);
			};

			_laterality.DataSource = _component.LateralityChoices;
			_laterality.DataBindings.Add("Value", _component, "SelectedLaterality", true, DataSourceUpdateMode.OnPropertyChanged);
			_laterality.DataBindings.Add("Enabled", _component, "IsLateralityEditable");

			_schedulingCode.DataSource = _component.SchedulingCodeChoices;
			_schedulingCode.DataBindings.Add("Value", _component, "SelectedSchedulingCode", true, DataSourceUpdateMode.OnPropertyChanged);
			_schedulingCode.DataBindings.Add("Enabled", _component, "IsSchedulingCodeEditable");
			_schedulingCode.Format += delegate(object sender, ListControlConvertEventArgs e)
			{
				e.Value = _component.FormatSchedulingCode(e.ListItem);
			};

			_portable.DataBindings.Add("Checked", _component, "PortableModality", true, DataSourceUpdateMode.OnPropertyChanged);
			_portable.DataBindings.Add("Enabled", _component, "IsPortableEditable", true, DataSourceUpdateMode.OnPropertyChanged);

			_checkedIn.DataBindings.Add("Checked", _component, "CheckedIn", true, DataSourceUpdateMode.OnPropertyChanged);
			_checkedIn.DataBindings.Add("Enabled", _component, "IsCheckedInEditable", true, DataSourceUpdateMode.OnPropertyChanged);

			_component.PropertyChanged += _component_PropertyChanged;
		}

		private void _component_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "DepartmentChoicesChanged")
			{
				_performingDepartment.DataSource = _component.DepartmentChoices;
			}
		}
		
		private void _acceptButton_Click(object sender, EventArgs e)
		{
			_component.Accept();
		}

		private void _cancelButton_Click(object sender, EventArgs e)
		{
			_component.Cancel();
		}
	}
}
