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

namespace ClearCanvas.ImageViewer.Explorer.Dicom.View.WinForms
{
    partial class SearchPanelComponentControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchPanelComponentControl));
			this._patientID = new ClearCanvas.Desktop.View.WinForms.TextField();
			this._accessionNumber = new ClearCanvas.Desktop.View.WinForms.TextField();
			this._patientsName = new ClearCanvas.Desktop.View.WinForms.TextField();
			this._studyDateFrom = new ClearCanvas.Desktop.View.WinForms.DateTimeField();
			this._studyDateTo = new ClearCanvas.Desktop.View.WinForms.DateTimeField();
			this._studyDescription = new ClearCanvas.Desktop.View.WinForms.TextField();
			this._searchButton = new System.Windows.Forms.Button();
			this._searchLastWeekButton = new System.Windows.Forms.Button();
			this._clearButton = new System.Windows.Forms.Button();
			this._searchTodayButton = new System.Windows.Forms.Button();
			this._titleBar = new Crownwood.DotNetMagic.Controls.TitleBar();
			this._modalityPicker = new ClearCanvas.ImageViewer.Explorer.Dicom.View.WinForms.ModalityPicker();
			this._referringPhysiciansName = new ClearCanvas.Desktop.View.WinForms.TextField();
			this.SuspendLayout();
			// 
			// _patientID
			// 
			this._patientID.LabelText = "Patient ID";
			resources.ApplyResources(this._patientID, "_patientID");
			this._patientID.Mask = "";
			this._patientID.Name = "_patientID";
			this._patientID.PasswordChar = '\0';
			this._patientID.ToolTip = null;
			this._patientID.Value = null;
			// 
			// _accessionNumber
			// 
			this._accessionNumber.LabelText = "Accession#";
			resources.ApplyResources(this._accessionNumber, "_accessionNumber");
			this._accessionNumber.Mask = "";
			this._accessionNumber.Name = "_accessionNumber";
			this._accessionNumber.PasswordChar = '\0';
			this._accessionNumber.ToolTip = null;
			this._accessionNumber.Value = null;
			// 
			// _patientsName
			// 
			this._patientsName.LabelText = "Name";
			resources.ApplyResources(this._patientsName, "_patientsName");
			this._patientsName.Mask = "";
			this._patientsName.Name = "_patientsName";
			this._patientsName.PasswordChar = '\0';
			this._patientsName.ToolTip = null;
			this._patientsName.Value = null;
			// 
			// _studyDateFrom
			// 
			this._studyDateFrom.LabelText = "Study Date (From)";
			resources.ApplyResources(this._studyDateFrom, "_studyDateFrom");
			this._studyDateFrom.Maximum = new System.DateTime(9998, 12, 31, 0, 0, 0, 0);
			this._studyDateFrom.Minimum = new System.DateTime(1753, 1, 1, 0, 0, 0, 0);
			this._studyDateFrom.Name = "_studyDateFrom";
			this._studyDateFrom.Nullable = true;
			this._studyDateFrom.Value = null;
			// 
			// _studyDateTo
			// 
			this._studyDateTo.LabelText = "Study Date (To)";
			resources.ApplyResources(this._studyDateTo, "_studyDateTo");
			this._studyDateTo.Maximum = new System.DateTime(9998, 12, 31, 0, 0, 0, 0);
			this._studyDateTo.Minimum = new System.DateTime(1753, 1, 1, 0, 0, 0, 0);
			this._studyDateTo.Name = "_studyDateTo";
			this._studyDateTo.Nullable = true;
			this._studyDateTo.Value = null;
			// 
			// _studyDescription
			// 
			this._studyDescription.LabelText = "Study Description";
			resources.ApplyResources(this._studyDescription, "_studyDescription");
			this._studyDescription.Mask = "";
			this._studyDescription.Name = "_studyDescription";
			this._studyDescription.PasswordChar = '\0';
			this._studyDescription.ToolTip = null;
			this._studyDescription.Value = null;
			// 
			// _searchButton
			// 
			resources.ApplyResources(this._searchButton, "_searchButton");
			this._searchButton.Name = "_searchButton";
			this._searchButton.UseVisualStyleBackColor = true;
			this._searchButton.Click += new System.EventHandler(this.OnSearchButtonClicked);
			// 
			// _searchLastWeekButton
			// 
			resources.ApplyResources(this._searchLastWeekButton, "_searchLastWeekButton");
			this._searchLastWeekButton.Name = "_searchLastWeekButton";
			this._searchLastWeekButton.UseVisualStyleBackColor = true;
			this._searchLastWeekButton.Click += new System.EventHandler(this.OnSearchLastWeekButtonClick);
			// 
			// _clearButton
			// 
			resources.ApplyResources(this._clearButton, "_clearButton");
			this._clearButton.Name = "_clearButton";
			this._clearButton.UseVisualStyleBackColor = true;
			this._clearButton.Click += new System.EventHandler(this.OnClearButonClicked);
			// 
			// _searchTodayButton
			// 
			resources.ApplyResources(this._searchTodayButton, "_searchTodayButton");
			this._searchTodayButton.Name = "_searchTodayButton";
			this._searchTodayButton.UseVisualStyleBackColor = true;
			this._searchTodayButton.Click += new System.EventHandler(this.OnSearchTodayButtonClicked);
			// 
			// _titleBar
			// 
			resources.ApplyResources(this._titleBar, "_titleBar");
			this._titleBar.GradientColoring = Crownwood.DotNetMagic.Controls.GradientColoring.LightBackToDarkBack;
			this._titleBar.MouseOverColor = System.Drawing.Color.Empty;
			this._titleBar.Name = "_titleBar";
			this._titleBar.Style = Crownwood.DotNetMagic.Common.VisualStyle.IDE2005;
			// 
			// _modalityPicker
			// 
			resources.ApplyResources(this._modalityPicker, "_modalityPicker");
			this._modalityPicker.LabelText = "Modality";
			this._modalityPicker.Name = "_modalityPicker";
			// 
			// _referringPhysiciansName
			// 
			this._referringPhysiciansName.LabelText = "Referring Physician";
			resources.ApplyResources(this._referringPhysiciansName, "_referringPhysiciansName");
			this._referringPhysiciansName.Mask = "";
			this._referringPhysiciansName.Name = "_referringPhysiciansName";
			this._referringPhysiciansName.PasswordChar = '\0';
			this._referringPhysiciansName.ToolTip = null;
			this._referringPhysiciansName.Value = null;
			// 
			// SearchPanelComponentControl
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Controls.Add(this._referringPhysiciansName);
			this.Controls.Add(this._titleBar);
			this.Controls.Add(this._patientID);
			this.Controls.Add(this._patientsName);
			this.Controls.Add(this._accessionNumber);
			this.Controls.Add(this._studyDateFrom);
			this.Controls.Add(this._studyDateTo);
			this.Controls.Add(this._studyDescription);
			this.Controls.Add(this._modalityPicker);
			this.Controls.Add(this._searchButton);
			this.Controls.Add(this._searchTodayButton);
			this.Controls.Add(this._searchLastWeekButton);
			this.Controls.Add(this._clearButton);
			this.Name = "SearchPanelComponentControl";
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

		private Crownwood.DotNetMagic.Controls.TitleBar _titleBar;
		private ClearCanvas.Desktop.View.WinForms.DateTimeField _studyDateTo;
		private System.Windows.Forms.Button _searchButton;
		private System.Windows.Forms.Button _searchLastWeekButton;
		private System.Windows.Forms.Button _clearButton;
		private System.Windows.Forms.Button _searchTodayButton;
		private ClearCanvas.Desktop.View.WinForms.DateTimeField _studyDateFrom;
		private ClearCanvas.Desktop.View.WinForms.TextField _patientID;
		private ClearCanvas.Desktop.View.WinForms.TextField _patientsName;
		private ClearCanvas.Desktop.View.WinForms.TextField _studyDescription;
		private ClearCanvas.Desktop.View.WinForms.TextField _accessionNumber;
		private ModalityPicker _modalityPicker;
		private ClearCanvas.Desktop.View.WinForms.TextField _referringPhysiciansName;
    }
}
