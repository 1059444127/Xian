#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using ClearCanvas.Common.Utilities;

namespace ClearCanvas.ImageViewer.StudyManagement
{
	/// <summary>
	/// A collection of <see cref="Patient"/> objects.
	/// </summary>
	public class PatientCollection : ObservableList<Patient>
	{
		/// <summary>
		/// Initializes a new instance of <see cref="PatientCollection"/>.
		/// </summary>
		public PatientCollection()
		{

		}

		internal Patient this[string patientId]
		{
			get
			{
				return CollectionUtils.SelectFirst(this, delegate(Patient patient) { return patient.PatientId == patientId; });
			}
		}
	}
}
