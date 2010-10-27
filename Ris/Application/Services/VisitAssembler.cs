﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System.Collections.Generic;
using ClearCanvas.Enterprise.Core;
using ClearCanvas.Healthcare;
using ClearCanvas.Ris.Application.Common;

namespace ClearCanvas.Ris.Application.Services
{
	public class VisitAssembler
	{
		public VisitSummary CreateVisitSummary(Visit visit, IPersistenceContext context)
		{
			var summary = new VisitSummary
				{
					VisitRef = visit.GetRef(),
					PatientRef = visit.Patient.GetRef(),
					VisitNumber = CreateVisitNumberDetail(visit.VisitNumber),
					AdmissionType = EnumUtils.GetEnumValueInfo(visit.AdmissionType),
					PatientClass = EnumUtils.GetEnumValueInfo(visit.PatientClass),
					PatientType = EnumUtils.GetEnumValueInfo(visit.PatientType),
					Status = EnumUtils.GetEnumValueInfo(visit.Status, context),
					AdmitTime = visit.AdmitTime,
					DischargeTime = visit.DischargeTime
				};

			var facilityAssembler = new FacilityAssembler();
			summary.Facility = visit.Facility == null ? null : facilityAssembler.CreateFacilitySummary(visit.Facility);

			var locationAssembler = new LocationAssembler();
			summary.CurrentLocation = visit.CurrentLocation == null ? null : locationAssembler.CreateLocationSummary(visit.CurrentLocation);

			return summary;
		}

		public VisitDetail CreateVisitDetail(Visit visit, IPersistenceContext context)
		{
			var detail = new VisitDetail
				{
					VisitRef = visit.GetRef(),
					PatientRef = visit.Patient.GetRef(),
					VisitNumber = CreateVisitNumberDetail(visit.VisitNumber),
					AdmissionType = EnumUtils.GetEnumValueInfo(visit.AdmissionType),
					PatientClass = EnumUtils.GetEnumValueInfo(visit.PatientClass),
					PatientType = EnumUtils.GetEnumValueInfo(visit.PatientType),
					Status = EnumUtils.GetEnumValueInfo(visit.Status, context),
					AdmitTime = visit.AdmitTime,
					DischargeTime = visit.DischargeTime,
					DischargeDisposition = visit.DischargeDisposition,
					Facility = new FacilityAssembler().CreateFacilitySummary(visit.Facility),
					CurrentLocation = visit.CurrentLocation == null ? null : new LocationAssembler().CreateLocationSummary(visit.CurrentLocation),
					Locations = new List<VisitLocationDetail>(),
					PreadmitNumber = visit.PreadmitNumber,
					VipIndicator = visit.VipIndicator,
					ExtendedProperties = ExtendedPropertyUtils.Copy(visit.ExtendedProperties)
				};

			foreach (var vl in visit.Locations)
			{
				detail.Locations.Add(CreateVisitLocationDetail(vl, context));
			}

			detail.Practitioners = new List<VisitPractitionerDetail>();
			foreach (var vp in visit.Practitioners)
			{
				detail.Practitioners.Add(CreateVisitPractitionerDetail(vp, context));
			}

			detail.AmbulatoryStatuses = new List<EnumValueInfo>();
			foreach (var ambulatoryStatus in visit.AmbulatoryStatuses)
			{
				detail.AmbulatoryStatuses.Add(EnumUtils.GetEnumValueInfo(ambulatoryStatus));
			}

			return detail;
		}

		public CompositeIdentifierDetail CreateVisitNumberDetail(VisitNumber vn)
		{
			return new CompositeIdentifierDetail(vn.Id, EnumUtils.GetEnumValueInfo(vn.AssigningAuthority));
		}

		public void UpdateVisit(Visit visit, VisitDetail detail, IPersistenceContext context)
		{
			visit.Patient = context.Load<Patient>(detail.PatientRef, EntityLoadFlags.Proxy);
			visit.VisitNumber.Id = detail.VisitNumber.Id;
			visit.VisitNumber.AssigningAuthority = EnumUtils.GetEnumValue<InformationAuthorityEnum>(detail.VisitNumber.AssigningAuthority, context);

			visit.AdmissionType = EnumUtils.GetEnumValue<AdmissionTypeEnum>(detail.AdmissionType, context);
			visit.PatientClass = EnumUtils.GetEnumValue<PatientClassEnum>(detail.PatientClass, context);
			visit.PatientType = EnumUtils.GetEnumValue<PatientTypeEnum>(detail.PatientType, context);
			visit.Status = EnumUtils.GetEnumValue<VisitStatus>(detail.Status);

			visit.AdmitTime = detail.AdmitTime;
			visit.DischargeTime = detail.DischargeTime;
			visit.DischargeDisposition = detail.DischargeDisposition;
			visit.VipIndicator = detail.VipIndicator;

			visit.Facility = detail.Facility == null ? null :
				context.Load<Facility>(detail.Facility.FacilityRef, EntityLoadFlags.Proxy);
			visit.CurrentLocation = detail.CurrentLocation == null ? null :
				context.Load<Location>(detail.CurrentLocation.LocationRef, EntityLoadFlags.Proxy);

			visit.Locations.Clear();
			foreach (var vlDetail in detail.Locations)
			{
				visit.Locations.Add(new VisitLocation(
					context.Load<Location>(vlDetail.Location.LocationRef, EntityLoadFlags.Proxy),
					EnumUtils.GetEnumValue<VisitLocationRole>(vlDetail.Role),
					vlDetail.StartTime,
					vlDetail.EndTime));
			}

			visit.Practitioners.Clear();
			foreach (var vpDetail in detail.Practitioners)
			{
				visit.Practitioners.Add(new VisitPractitioner(
					context.Load<ExternalPractitioner>(vpDetail.Practitioner.PractitionerRef, EntityLoadFlags.Proxy),
					EnumUtils.GetEnumValue<VisitPractitionerRole>(vpDetail.Role),
					vpDetail.StartTime,
					vpDetail.EndTime));
			}

			visit.AmbulatoryStatuses.Clear();
			foreach (var ambulatoryStatus in detail.AmbulatoryStatuses)
			{
				visit.AmbulatoryStatuses.Add(EnumUtils.GetEnumValue<AmbulatoryStatusEnum>(ambulatoryStatus, context));   
			}

			ExtendedPropertyUtils.Update(visit.ExtendedProperties, detail.ExtendedProperties);
		}

		private static VisitLocationDetail CreateVisitLocationDetail(VisitLocation vl, IPersistenceContext context)
		{
			var detail = new VisitLocationDetail
				{
					Location = new LocationAssembler().CreateLocationSummary(vl.Location),
					Role = EnumUtils.GetEnumValueInfo(vl.Role, context),
					StartTime = vl.StartTime,
					EndTime = vl.EndTime
				};

			return detail;
		}

		private static VisitPractitionerDetail CreateVisitPractitionerDetail(VisitPractitioner vp, IPersistenceContext context)
		{
			var detail = new VisitPractitionerDetail
				{
					Practitioner = new ExternalPractitionerAssembler().CreateExternalPractitionerSummary(vp.Practitioner, context),
					Role = EnumUtils.GetEnumValueInfo(vp.Role, context),
					StartTime = vp.StartTime,
					EndTime = vp.EndTime
				};

			return detail;
		}
	}
}
