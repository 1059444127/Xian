﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

using ClearCanvas.Common;
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Validation;
using ClearCanvas.Ris.Application.Common;

namespace ClearCanvas.Ris.Client
{
	[ExtensionPoint]
    public class PatientEditorComponentViewExtensionPoint : ExtensionPoint<IApplicationComponentView>
    {
    }

    [AssociateView(typeof(PatientEditorComponentViewExtensionPoint))]
    public class PatientProfileDetailsEditorComponent : ApplicationComponent
    {
        private PatientProfileDetail _profile;
		private readonly List<EnumValueInfo> _sexChoices;

        private string _dateOfBirth;
		private readonly List<EnumValueInfo> _mrnAuthorityChoices;
		private readonly List<EnumValueInfo> _healthcardAuthorityChoices;

        public PatientProfileDetailsEditorComponent(List<EnumValueInfo> sexChoices, List<EnumValueInfo> mrnAuthorityChoices, List<EnumValueInfo> healthcardAuthorityChoices)
        {
            _sexChoices = sexChoices;
            _mrnAuthorityChoices = mrnAuthorityChoices;
            _healthcardAuthorityChoices = healthcardAuthorityChoices;
        }

        /// <summary>
        /// Gets or sets the subject (e.g PatientProfileDetail) that this editor operates on.  This property
        /// should never be used by the view.
        /// </summary>
        public PatientProfileDetail Subject
        {
            get { return _profile; }
            set
            {
                _profile = value;
                _dateOfBirth = _profile.DateOfBirth == null ? null : _profile.DateOfBirth.Value.ToString(this.DateOfBirthFormat);
            }
        }

        public override void Start()
        {
			this.Validation.Add(new ValidationRule("TimeOfDeath",
				delegate
				{
					// only need to validate the if Date of Birth and Time of Death are specified
					if (!_profile.DateOfBirth.HasValue || !_profile.TimeOfDeath.HasValue)
						return new ValidationResult(true, "");

					var ok = DateTime.Compare(_profile.TimeOfDeath.Value, _profile.DateOfBirth.Value) >= 0;
					return new ValidationResult(ok, SR.MessageDateOfDeathMustBeLaterThanOrEqualToDateOfBirth);
				}));

			// add validation rule to ensure the DateOfBirth is a valid DateTime
			this.Validation.Add(new ValidationRule("DateOfBirth",
				delegate
				{
					if (string.IsNullOrEmpty(this.DateOfBirth))
						return new ValidationResult(true, "");

					DateTime dt;
					return DateTime.TryParseExact(_dateOfBirth, this.DateOfBirthFormat, null, DateTimeStyles.None, out dt)
						? new ValidationResult(true, "")
						: new ValidationResult(false, SR.MessageInvalidDateFormat);
				}));

            base.Start();
        }

        #region Presentation Model

        [ValidateNotNull]
        public string MrnID
        {
            get { return _profile.Mrn.Id; }
            set
            {
                _profile.Mrn.Id = value;
                this.Modified = true;
            }
        }

        [ValidateNotNull]
        public EnumValueInfo MrnAuthority
        {
            get { return _profile.Mrn.AssigningAuthority; }
            set
            {
                _profile.Mrn.AssigningAuthority = value;
                this.Modified = true;
            }
        }

        [ValidateNotNull]
        public string FamilyName
        {
            get { return _profile.Name.FamilyName; }
            set { 
                _profile.Name.FamilyName = value;
                this.Modified = true;
            }
        }

        [ValidateNotNull]
        public string GivenName
        {
            get { return _profile.Name.GivenName; }
            set { 
                _profile.Name.GivenName = value;
                this.Modified = true;
            }
        }

        public string MiddleName
        {
            get { return _profile.Name.MiddleName; }
            set { 
                _profile.Name.MiddleName = value;
                this.Modified = true;
            }
        }

        [ValidateNotNull]
        public EnumValueInfo Sex
        {
            get { return _profile.Sex; }
            set
            {
                _profile.Sex = value;
                this.Modified = true;
            }
        }

        public IList SexChoices
        {
            get { return _sexChoices; }
        }

        public string DateOfBirthMask
        {
            get { return "0000-00-00"; }
        }

        public string DateOfBirthFormat
        {
            get { return "yyyyMMdd"; }
        }

        public string DateOfBirth
        {
            get { return _dateOfBirth; }
            set
            {
                _dateOfBirth = value;
                this.Modified = true;

                if (string.IsNullOrEmpty(_dateOfBirth) || _dateOfBirth == this.DateOfBirthMask)
                    _profile.DateOfBirth = null;
                else
                {
                    DateTime dt;
                    if (DateTime.TryParseExact(_dateOfBirth, this.DateOfBirthFormat, null, DateTimeStyles.None, out dt))
						_profile.DateOfBirth = dt;
                }
            }
        }

        public bool DeathIndicator
        {
            get { return _profile.DeathIndicator; }
            set
            { 
                _profile.DeathIndicator = value;
                this.Modified = true;
            }
        }

        public DateTime? TimeOfDeath
        {
            get { return _profile.TimeOfDeath; }
            set
            {
                _profile.TimeOfDeath = value;
                _profile.DeathIndicator = (_profile.TimeOfDeath == null ? false : true);
                this.Modified = true;
            }
        }

        public IList MrnAuthorityChoices
        {
            get { return _mrnAuthorityChoices;  }
        }

        public string HealthcardID
        {
            get { return _profile.Healthcard.Id; }
            set
            {
                _profile.Healthcard.Id = value;
                this.Modified = true;
            }
        }

        public string HealthcardMask
        {
            get { return TextFieldMasks.HealthcardNumberMask; }
        }

        public EnumValueInfo HealthcardAuthority
        {
            get { return _profile.Healthcard.AssigningAuthority; }
            set
            {
                _profile.Healthcard.AssigningAuthority = value;
                this.Modified = true;
            }
        }

        public IList HealthcardAuthorityChoices
        {
            get { return _healthcardAuthorityChoices;  }
        }

        public string HealthcardVersionCode
        {
            get { return _profile.Healthcard.VersionCode; }
            set
            {
                _profile.Healthcard.VersionCode = value;
                this.Modified = true;
            }
        }

        public string HealthcardVersionCodeMask
        {
            get { return TextFieldMasks.HealthcardVersionCodeMask; }
        }

        public DateTime? HealthcardExpiryDate
        {
            get { return _profile.Healthcard.ExpiryDate; }
            set
            {
                _profile.Healthcard.ExpiryDate = value;
                this.Modified = true;
            }
        }

        #endregion
    }
}