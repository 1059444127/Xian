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

using System;
using System.Collections.Generic;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Validation;
using ClearCanvas.Ris.Application.Common;
using ClearCanvas.Ris.Application.Common.RegistrationWorkflow;

namespace ClearCanvas.Ris.Client
{
	/// <summary>
	/// Extension point for views onto <see cref="MultipleProceduresEditorComponent"/>.
	/// </summary>
	[ExtensionPoint]
	public sealed class MultipleProceduresEditorComponentViewExtensionPoint : ExtensionPoint<IApplicationComponentView>
	{
	}

	/// <summary>
	/// MultipleProceduresEditorComponent class.
	/// </summary>
	[AssociateView(typeof(MultipleProceduresEditorComponentViewExtensionPoint))]
	public class MultipleProceduresEditorComponent : ProcedureEditorComponentBase
	{
		private readonly List<ProcedureRequisition> _requisitions;

		private bool _isScheduledTimeEditable;
		private bool _isPerformingFacilityEditable;
		private bool _isPerformingDepartmentEditable;
		private bool _isLateralityEditable;
		private bool _isSchedulingCodeEditable;
		private bool _isPortableEditable;
		private bool _isCheckedInEditable;

		public MultipleProceduresEditorComponent(List<ProcedureRequisition> requisitions,
			List<FacilitySummary> facilityChoices,
			List<DepartmentSummary> departmentChoices,
			List<EnumValueInfo> lateralityChoices,
			List<EnumValueInfo> schedulingCodeChoices)
			: base(facilityChoices, departmentChoices, lateralityChoices, schedulingCodeChoices)
		{
			Platform.CheckForNullReference(requisitions, "requisitions");

			_requisitions = requisitions;
		}

		public override void Start()
		{
			// This validation shows the icon beside the scheduled time if it's being edited
			this.Validation.Add(new ValidationRule("ScheduledTime",
				delegate
				{
					return this.IsScheduledDateTimeEditable ? ValidateCheckInTime() : new ValidationResult(true, "");
				}));

			// This validation shows the icon beside the checkedIn if it's being edited
			this.Validation.Add(new ValidationRule("CheckedIn",
				delegate
				{
					return this.IsCheckedInEditable ? ValidateCheckInTime() : new ValidationResult(true, "");
				}));

			base.Start();
		}

		protected override void LoadFromRequisition()
		{
			this.ScheduledTime = GetCommonValue(_requisitions, r => r.ScheduledTime);
			this.SelectedFacility = GetCommonValue(_requisitions, r => r.PerformingFacility);
			this.SelectedDepartment = GetCommonValue(_requisitions, r => r.PerformingDepartment);
			this.SelectedLaterality = GetCommonValue(_requisitions, r => r.Laterality);
			this.SelectedSchedulingCode = GetCommonValue(_requisitions, r => r.SchedulingCode);
			this.PortableModality = GetCommonValue(_requisitions, r => r.PortableModality);
			this.CheckedIn = GetCommonValue(_requisitions, r => r.CheckedIn);
		}

		protected override void UpdateRequisition()
		{
			foreach (var requisition in _requisitions)
			{
				if (_isScheduledTimeEditable)
					requisition.ScheduledTime = this.ScheduledTime;

				if (_isPerformingFacilityEditable)
					requisition.PerformingFacility = this.SelectedFacility;

				if (_isPerformingDepartmentEditable)
					requisition.PerformingDepartment = this.SelectedDepartment;

				if (_isLateralityEditable)
					requisition.Laterality = this.SelectedLaterality;

				if (_isSchedulingCodeEditable)
					requisition.SchedulingCode = this.SelectedSchedulingCode;

				if (_isPortableEditable)
					requisition.PortableModality = this.PortableModality;

				if (_isCheckedInEditable)
					requisition.CheckedIn = this.CheckedIn;
			}
		}

		#region Presentation Models

		public override bool IsScheduledDateTimeEditable
		{
			get { return _isScheduledTimeEditable; }
			set { _isScheduledTimeEditable = value; }
		}

		public override bool IsPerformingFacilityEditable
		{
			get { return _isPerformingFacilityEditable; }
			set { _isPerformingFacilityEditable = value; }
		}

		public override bool IsPerformingDepartmentEditable
		{
			get { return _isPerformingDepartmentEditable; }
			set { _isPerformingDepartmentEditable = value; }
		}

		public override bool IsLateralityEditable
		{
			get { return _isLateralityEditable; }
			set { _isLateralityEditable = value; }
		}

		public override bool IsSchedulingCodeEditable
		{
			get { return _isSchedulingCodeEditable; }
			set { _isSchedulingCodeEditable = value; }
		}

		public override bool IsPortableEditable
		{
			get { return _isPortableEditable; }
			set { _isPortableEditable = value; }
		}

		public override bool IsCheckedInEditable
		{
			get { return _isCheckedInEditable; }
			set { _isCheckedInEditable = value; }
		}

		#endregion

		/// <summary>
		/// Find a common property value for a list of requisitions.
		/// </summary>
		private static TValue GetCommonValue<TValue>(
			List<ProcedureRequisition> requisitions,
			Converter<ProcedureRequisition, TValue> propertyGetter)
		{
			var mappedValues = CollectionUtils.Map(requisitions, propertyGetter);
			var uniqueValues = CollectionUtils.Unique(mappedValues);
			return uniqueValues.Count != 1 ? default(TValue) : uniqueValues[0];
		}

		private ValidationResult ValidateCheckInTime()
		{
			var checkInTime = Platform.Time;
			foreach (var r in _requisitions)
			{
				// Use the edited property if the property is being edited
				var checkedIn = this.IsCheckedInEditable ? this.CheckedIn : r.CheckedIn;
				var scheduledTime = this.IsScheduledDateTimeEditable ? this.ScheduledTime : r.ScheduledTime;

				if (!checkedIn)
					continue;

				string alertMessage;
				if (CheckInSettings.ValidateResult.Success ==
					CheckInSettings.Validate(scheduledTime, checkInTime, out alertMessage))
					continue;

				// Validation failed.
				if (this.IsCheckedInEditable && this.IsScheduledDateTimeEditable)
				{
					// If user is modifying both checkIn and scheduledDateTime, give them a more detail alert message.
					return new ValidationResult(false, alertMessage);
				}

				// Otherwise, they must edit each procedure individually.
				return new ValidationResult(false, SR.MessageAlertMultipleProceduresCheckInValidation);
			}

			return new ValidationResult(true, string.Empty);
		}
	}
}
