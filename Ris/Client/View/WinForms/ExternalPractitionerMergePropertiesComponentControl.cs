#region License

// Copyright (c) 2006-2008, ClearCanvas Inc.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met:
//
//    * Redistributions of source code must retain the above copyright notice, 
//      this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, 
//      this list of conditions and the following disclaimer in the documentation 
//      and/or other materials provided with the distribution.
//    * Neither the name of ClearCanvas Inc. nor the names of its contributors 
//      may be used to endorse or promote products derived from this software without 
//      specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR 
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
// OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
// ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY 
// OF SUCH DAMAGE.

#endregion

using System.Windows.Forms;
using ClearCanvas.Desktop.View.WinForms;

namespace ClearCanvas.Ris.Client.View.WinForms
{
	/// <summary>
	/// Provides a Windows Forms user-interface for <see cref="ExternalPractitionerMergePropertiesComponent"/>.
	/// </summary>
	public partial class ExternalPractitionerMergePropertiesComponentControl : ApplicationComponentUserControl
	{
		private readonly ExternalPractitionerMergePropertiesComponent _component;

		public ExternalPractitionerMergePropertiesComponentControl(ExternalPractitionerMergePropertiesComponent component)
			:base(component)
		{
			_component = component;
			InitializeComponent();

			_instruction.DataBindings.Add("Text", _component, "Instruction", true, DataSourceUpdateMode.OnPropertyChanged);

			_name.DataSource = _component.NameChoices;
			_name.DataBindings.Add("Value", _component, "Name", true, DataSourceUpdateMode.OnPropertyChanged);
			_name.DataBindings.Add("Enabled", _component, "NameEnabled", true, DataSourceUpdateMode.OnPropertyChanged);
			_name.Format += delegate(object sender, ListControlConvertEventArgs e) { e.Value = _component.FormatName(e.ListItem); };

			_licenseNumber.DataSource = _component.LicenseNumberChoices;
			_licenseNumber.DataBindings.Add("Value", _component, "LicenseNumber", true, DataSourceUpdateMode.OnPropertyChanged);
			_licenseNumber.DataBindings.Add("Enabled", _component, "LicenseNumberEnabled", true, DataSourceUpdateMode.OnPropertyChanged);

			_billingNumber.DataSource = _component.BillingNumberChoices;
			_billingNumber.DataBindings.Add("Value", _component, "BillingNumber", true, DataSourceUpdateMode.OnPropertyChanged);
			_billingNumber.DataBindings.Add("Enabled", _component, "BillingNumberEnabled", true, DataSourceUpdateMode.OnPropertyChanged);

			_extendedProperties.Items.AddRange(_component.ExtendedPropertyChoices);

			_component.AllPropertiesChanged += OnAllPropertiesChanged;
			_component.SaveRequested += OnSaveRequested;
		}

		private void OnSaveRequested(object sender, System.EventArgs e)
		{
			_component.ExtendedProperties = _extendedProperties.CurrentValues;
		}

		private void OnAllPropertiesChanged(object sender, System.EventArgs e)
		{
			_name.DataSource = _component.NameChoices;
			_licenseNumber.DataSource = _component.LicenseNumberChoices;
			_billingNumber.DataSource = _component.BillingNumberChoices;

			_extendedProperties.Items.Clear();
			_extendedProperties.Items.AddRange(_component.ExtendedPropertyChoices);
		}
	}
}