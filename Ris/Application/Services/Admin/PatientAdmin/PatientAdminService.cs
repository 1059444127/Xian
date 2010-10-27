﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System.Collections.Generic;
using System.Security.Permissions;
using System.Threading;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Enterprise.Core;
using ClearCanvas.Healthcare;
using ClearCanvas.Healthcare.Brokers;
using ClearCanvas.Ris.Application.Common;
using ClearCanvas.Ris.Application.Common.Admin.PatientAdmin;

namespace ClearCanvas.Ris.Application.Services.Admin.PatientAdmin
{
	[ExtensionOf(typeof(ApplicationServiceExtensionPoint))]
	[ServiceImplementsContract(typeof(IPatientAdminService))]
	public class PatientAdminService : ApplicationServiceBase, IPatientAdminService
	{
		#region IPatientAdminService Members

		[ReadOperation]
		public LoadPatientProfileEditorFormDataResponse LoadPatientProfileEditorFormData(LoadPatientProfileEditorFormDataRequest request)
		{
			// ignore request
			// Sort the category from High to Low, then sort by name
			var categoryAssembler = new PatientNoteCategoryAssembler();
			IList<PatientNoteCategory> sortedCategoryList = CollectionUtils.Sort(
				PersistenceContext.GetBroker<IPatientNoteCategoryBroker>().FindAll(false),
				(x, y) => string.Compare(x.Name, y.Name));

			var response = new LoadPatientProfileEditorFormDataResponse
				{
					AddressTypeChoices = EnumUtils.GetEnumValueList<AddressTypeEnum>(PersistenceContext),
					ContactPersonRelationshipChoices = EnumUtils.GetEnumValueList<ContactPersonRelationshipEnum>(PersistenceContext),
					ContactPersonTypeChoices = EnumUtils.GetEnumValueList<ContactPersonTypeEnum>(PersistenceContext),
					HealthcardAssigningAuthorityChoices = EnumUtils.GetEnumValueList<InsuranceAuthorityEnum>(PersistenceContext),
					MrnAssigningAuthorityChoices = EnumUtils.GetEnumValueList<InformationAuthorityEnum>(PersistenceContext),
					PrimaryLanguageChoices = EnumUtils.GetEnumValueList<SpokenLanguageEnum>(PersistenceContext),
					ReligionChoices = EnumUtils.GetEnumValueList<ReligionEnum>(PersistenceContext),
					SexChoices = EnumUtils.GetEnumValueList<SexEnum>(PersistenceContext),
					PhoneTypeChoices = (new SimplifiedPhoneTypeAssembler()).GetPatientPhoneTypeChoices(),
					NoteCategoryChoices = CollectionUtils.Map<PatientNoteCategory, PatientNoteCategorySummary, List<PatientNoteCategorySummary>>(sortedCategoryList,
						category => categoryAssembler.CreateNoteCategorySummary(category, this.PersistenceContext)),

					// Allergies related choices
					AllergenTypeChoices = EnumUtils.GetEnumValueList<AllergyAllergenTypeEnum>(PersistenceContext),
					AllergySeverityChoices = EnumUtils.GetEnumValueList<AllergySeverityEnum>(PersistenceContext),
					AllergySensitivityTypeChoices = EnumUtils.GetEnumValueList<AllergySensitivityTypeEnum>(PersistenceContext),
					PersonRelationshipTypeChoices = EnumUtils.GetEnumValueList<PersonRelationshipTypeEnum>(PersistenceContext)
				};

			return response;
		}


		[ReadOperation]
		public LoadPatientProfileForEditResponse LoadPatientProfileForEdit(LoadPatientProfileForEditRequest request)
		{
			var broker = PersistenceContext.GetBroker<IPatientProfileBroker>();

			var profile = broker.Load(request.PatientProfileRef);
			var assembler = new PatientProfileAssembler();
			return new LoadPatientProfileForEditResponse(profile.Patient.GetRef(), profile.GetRef(), assembler.CreatePatientProfileDetail(profile, PersistenceContext));
		}

		[UpdateOperation]
		[PrincipalPermission(SecurityAction.Demand, Role = AuthorityTokens.Workflow.Patient.Update)]
		[PrincipalPermission(SecurityAction.Demand, Role = AuthorityTokens.Workflow.PatientProfile.Update)]
		public UpdatePatientProfileResponse UpdatePatientProfile(UpdatePatientProfileRequest request)
		{
			var profile = PersistenceContext.Load<PatientProfile>(request.PatientProfileRef, EntityLoadFlags.CheckVersion);

			var updatePatient = Thread.CurrentPrincipal.IsInRole(AuthorityTokens.Workflow.Patient.Update);
			var updateProfile = Thread.CurrentPrincipal.IsInRole(AuthorityTokens.Workflow.PatientProfile.Update);

			UpdateHelper(profile, request.PatientDetail, updatePatient, updateProfile);

			this.PersistenceContext.SynchState();

			var assembler = new PatientProfileAssembler();
			return new UpdatePatientProfileResponse(assembler.CreatePatientProfileSummary(profile, PersistenceContext));
		}

		[UpdateOperation]
		[PrincipalPermission(SecurityAction.Demand, Role = AuthorityTokens.Workflow.Patient.Create)]
		public AddPatientResponse AddPatient(AddPatientRequest request)
		{
			var profile = new PatientProfile();
			var patient = new Patient();
			patient.AddProfile(profile);

			UpdateHelper(profile, request.PatientDetail, true, true);

			PersistenceContext.Lock(patient, DirtyState.New);
			PersistenceContext.SynchState();

			var assembler = new PatientProfileAssembler();
			return new AddPatientResponse(assembler.CreatePatientProfileSummary(profile, PersistenceContext));
		}

		#endregion

		private void UpdateHelper(PatientProfile profile, PatientProfileDetail detail, bool updatePatient, bool updateProfile)
		{
			if (updatePatient)
			{
				var patient = profile.Patient;

				var noteAssembler = new PatientNoteAssembler();
				noteAssembler.Synchronize(patient, detail.Notes, CurrentUserStaff, PersistenceContext);

				var attachmentAssembler = new PatientAttachmentAssembler();
				attachmentAssembler.Synchronize(patient.Attachments, detail.Attachments, this.CurrentUserStaff, PersistenceContext);

				var allergyAssembler = new PatientAllergyAssembler();
				allergyAssembler.Synchronize(patient.Allergies, detail.Allergies, PersistenceContext);
			}

			if (updateProfile)
			{
				var assembler = new PatientProfileAssembler();
				assembler.UpdatePatientProfile(profile, detail, PersistenceContext);
			}
		}
	}
}
