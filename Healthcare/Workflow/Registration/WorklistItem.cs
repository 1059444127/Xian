using System;
using System.Collections.Generic;
using System.Text;

using Iesi.Collections;
using ClearCanvas.Common;
using ClearCanvas.Healthcare;
using ClearCanvas.Enterprise.Core;
using ClearCanvas.Workflow;
using ClearCanvas.Enterprise.Common;

namespace ClearCanvas.Healthcare.Workflow.Registration
{
    public class WorklistItemKey : IWorklistItemKey
    {
        private EntityRef _patientProfile;

        public WorklistItemKey(EntityRef patientProfile)
        {
            _patientProfile = patientProfile;
        }

        public EntityRef PatientProfile
        {
            get { return _patientProfile; }
            set { _patientProfile = value; }
        }
    }

    public class WorklistItem : WorklistItemBase
    {
        private CompositeIdentifier _mrn;
        private PersonName _patientName;
        private HealthcardNumber _healthcardNumber;
        private DateTime? _dateOfBirth;
        private Sex _sex;

        public WorklistItem(PatientProfile profile)
            : base(new WorklistItemKey(profile.GetRef()))
        {
            _mrn = profile.Mrn;
            _patientName = profile.Name;
            _healthcardNumber = profile.Healthcard;
            _dateOfBirth = profile.DateOfBirth;
            _sex = profile.Sex;
        }

        public WorklistItem(EntityRef profileRef,
            CompositeIdentifier mrn,
            PersonName patientName,
            HealthcardNumber healthcard,
            DateTime? dateOfBirth,
            Sex sex)
            : base(new WorklistItemKey(profileRef))
        {
            _mrn = mrn;
            _patientName = patientName;
            _healthcardNumber = healthcard;
            _dateOfBirth = dateOfBirth;
            _sex = sex;
        }

        #region Public Properties

        public EntityRef PatientProfile
        {
            get { return (this.Key as WorklistItemKey).PatientProfile; }
        }

        public CompositeIdentifier Mrn
        {
            get { return _mrn; }
        }

        public PersonName PatientName
        {
            get { return _patientName; }
        }

        public HealthcardNumber HealthcardNumber
        {
            get { return _healthcardNumber; }
        }

        public DateTime? DateOfBirth
        {
            get { return _dateOfBirth; }
        }

        public Sex Sex
        {
            get { return _sex; }
        }

        #endregion
    }
}
