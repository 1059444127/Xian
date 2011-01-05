﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
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

using System.ComponentModel;
using System.Windows.Forms;

using ClearCanvas.Desktop.View.WinForms;

namespace ClearCanvas.Ris.Client.View.WinForms
{
	/// <summary>
	/// Provides a Windows Forms user-interface for <see cref="ExternalPractitionerContactPointDetailsEditorComponent"/>
	/// </summary>
	public partial class ExternalPractitionerContactPointDetailsEditorComponentControl : ApplicationComponentUserControl
	{
		private readonly ExternalPractitionerContactPointDetailsEditorComponent _component;

		/// <summary>
		/// Constructor
		/// </summary>
		public ExternalPractitionerContactPointDetailsEditorComponentControl(ExternalPractitionerContactPointDetailsEditorComponent component)
			: base(component)
		{
			InitializeComponent();

			_component = component;
			_component.PropertyChanged += _component_PropertyChanged;

			if (_component.HasWarning)
			{
				_warning.Text = _component.WarningMessage;
				_warning.Visible = true;
			}

			_name.DataBindings.Add("Value", _component, "ContactPointName", true, DataSourceUpdateMode.OnPropertyChanged);
			_description.DataBindings.Add("Value", _component, "ContactPointDescription", true, DataSourceUpdateMode.OnPropertyChanged);
			_isDefaultContactPoint.DataBindings.Add("Checked", _component, "IsDefaultContactPoint", true, DataSourceUpdateMode.OnPropertyChanged);
			_resultCommunicationMode.DataBindings.Add("Value", _component, "SelectedResultCommunicationMode", true, DataSourceUpdateMode.OnPropertyChanged);
			_resultCommunicationMode.DataSource = _component.ResultCommunicationModeChoices;
			_informationAuthority.DataBindings.Add("Value", _component, "SelectedInformationAuthority", true, DataSourceUpdateMode.OnPropertyChanged);
			_informationAuthority.DataSource = _component.InformationAuthorityChoices;
		}

		private void _component_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "IsDefaultContactPoint")
			{
				if (_isDefaultContactPoint.Checked != _component.IsDefaultContactPoint)
				{
					_isDefaultContactPoint.Checked = _component.IsDefaultContactPoint;
				}
			}
		}
	}
}
