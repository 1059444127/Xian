﻿using ClearCanvas.Enterprise.Hibernate.Hql;
using ClearCanvas.Workflow;
using System.Collections.Generic;

namespace ClearCanvas.Healthcare.Hibernate.Brokers.QueryBuilders
{

	public partial class QueryBuilderBase
	{
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// <para>
		/// The ability for subclasses to customize the queries relies on using an established set of HQL alias-variables
		/// to represent entities that are typically used in the query.  These are listed here:
		/// <list type="">
		/// <item>ps - ProcedureStep</item>
		/// <item>rp - Procedure</item>
		/// <item>pc - ProcedureCheckIn</item>
		/// <item>o - Order</item>
		/// <item>p - Patient</item>
		/// <item>v - Visit</item>
		/// <item>pp - PatientProfile</item>
		/// <item>ds - DiagnosticService</item>
		/// <item>rpt - ProcedureType</item>
		/// <item>pr - Protocol</item>
		/// <item>sst - Scheduled Performer Staff</item>
		/// <item>pst - Performer Staff</item>
		/// </list>
		/// Subclasses may define additional variables but must not attempt to override those defined by this class.
		/// </para>
		/// </remarks>
		public static class HqlConstants
		{
			/// <summary>
			/// This is just a dummy qualifier that is used when converting criteria to HQL - it has no meaning.
			/// </summary>
			public const string WorklistItemQualifier = "wi";

			public static readonly HqlSelect SelectProcedureStep = new HqlSelect("ps");
			public static readonly HqlSelect SelectProcedure = new HqlSelect("rp");
			public static readonly HqlSelect SelectProcedureCheckIn = new HqlSelect("pc");
			public static readonly HqlSelect SelectProtocol = new HqlSelect("pr");
			public static readonly HqlSelect SelectOrder = new HqlSelect("o");
			public static readonly HqlSelect SelectVisit = new HqlSelect("v");
			public static readonly HqlSelect SelectPatient = new HqlSelect("p");
			public static readonly HqlSelect SelectPatientProfile = new HqlSelect("pp");


			public static readonly HqlSelect SelectMrn = new HqlSelect("pp.Mrn");
			public static readonly HqlSelect SelectPatientName = new HqlSelect("pp.Name");
			public static readonly HqlSelect SelectAccessionNumber = new HqlSelect("o.AccessionNumber");
			public static readonly HqlSelect SelectPriority = new HqlSelect("o.Priority");
			public static readonly HqlSelect SelectPatientClass = new HqlSelect("v.PatientClass");
			public static readonly HqlSelect SelectDiagnosticServiceName = new HqlSelect("ds.Name");
			public static readonly HqlSelect SelectProcedureTypeName = new HqlSelect("rpt.Name");
			public static readonly HqlSelect SelectProcedurePortable = new HqlSelect("rp.Portable");
			public static readonly HqlSelect SelectProcedureLaterality = new HqlSelect("rp.Laterality");
			public static readonly HqlSelect SelectProcedureStepState = new HqlSelect("ps.State");
			
			
			
			public static readonly HqlSelect SelectReport = new HqlSelect("r");
			public static readonly HqlSelect SelectReportPart = new HqlSelect("rpp");
			public static readonly HqlSelect SelectReportPartIndex = new HqlSelect("rpp.Index");
			public static readonly HqlSelect SelectReportPartHasErrors = new HqlSelect("ps.HasErrors"); // valid only if ps is TranscriptionReviewStep

			public static readonly HqlSelect SelectOrderScheduledStartTime = new HqlSelect("o.ScheduledStartTime");
			public static readonly HqlSelect SelectOrderSchedulingRequestTime = new HqlSelect("o.SchedulingRequestTime");
			public static readonly HqlSelect SelectProcedureScheduledStartTime = new HqlSelect("rp.ScheduledStartTime");
			public static readonly HqlSelect SelectProcedureCheckInTime = new HqlSelect("pc.CheckInTime");
			public static readonly HqlSelect SelectProcedureCheckOutTime = new HqlSelect("pc.CheckOutTime");
			public static readonly HqlSelect SelectProcedureStartTime = new HqlSelect("rp.StartTime");
			public static readonly HqlSelect SelectProcedureEndTime = new HqlSelect("rp.EndTime");
			public static readonly HqlSelect SelectProcedureStepCreationTime = new HqlSelect("ps.CreationTime");
			public static readonly HqlSelect SelectProcedureStepScheduledStartTime = new HqlSelect("ps.Scheduling.StartTime");
			public static readonly HqlSelect SelectProcedureStepStartTime = new HqlSelect("ps.StartTime");
			public static readonly HqlSelect SelectProcedureStepEndTime = new HqlSelect("ps.EndTime");
			public static readonly HqlSelect SelectReportPartPreliminaryTime = new HqlSelect("rpp.PreliminaryTime");
			public static readonly HqlSelect SelectReportPartCompletedTime = new HqlSelect("rpp.CompletedTime");

			public static readonly HqlJoin JoinProcedure = new HqlJoin("ps.Procedure", "rp");
			public static readonly HqlJoin JoinProcedureType = new HqlJoin("rp.Type", "rpt");
			public static readonly HqlJoin JoinProcedureCheckIn = new HqlJoin("rp.ProcedureCheckIn", "pc");
			public static readonly HqlJoin JoinOrder = new HqlJoin("rp.Order", "o");
			public static readonly HqlJoin JoinProtocol = new HqlJoin("ps.Protocol", "pr");
			public static readonly HqlJoin JoinDiagnosticService = new HqlJoin("o.DiagnosticService", "ds");
			public static readonly HqlJoin JoinVisit = new HqlJoin("o.Visit", "v");
			public static readonly HqlJoin JoinPatient = new HqlJoin("o.Patient", "p");
			public static readonly HqlJoin JoinPatientProfile = new HqlJoin("p.Profiles", "pp");
			public static readonly HqlJoin JoinReportPart = new HqlJoin("ps.ReportPart", "rpp", HqlJoinMode.Left);
			public static readonly HqlJoin JoinReport = new HqlJoin("rpp.Report", "r", HqlJoinMode.Left);

			public static readonly HqlFrom FromWorklist = new HqlFrom("Worklist", "w");

			public static readonly HqlCondition ConditionActiveProcedureStep = new HqlCondition("(ps.State in (?, ?))", ActivityStatus.SC, ActivityStatus.IP);
			public static readonly HqlCondition ConditionConstrainPatientProfile = new HqlCondition("pp.Mrn.AssigningAuthority = rp.PerformingFacility.InformationAuthority");


			public static readonly HqlSelect[] DefaultCountProjection = { new HqlSelect("count(*)") };

			/// <summary>
			/// Provides mappings from <see cref="WorklistItemField"/> values to HQL expressions.
			/// </summary>
			public static readonly Dictionary<WorklistItemField, HqlSelect> MapWorklistItemFieldToHqlSelect = new Dictionary<WorklistItemField, HqlSelect>();

			/// <summary>
			/// Provides mappings from criteria "keys" to HQL expressions.
			/// </summary>
			public static readonly Dictionary<string, string> MapCriteriaKeyToHql = new Dictionary<string, string>();


			static HqlConstants()
			{
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.ProcedureStep, SelectProcedureStep);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.Procedure, SelectProcedure);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.ProcedureCheckIn, SelectProcedureCheckIn);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.Protocol, SelectProtocol);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.Order, SelectOrder);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.Visit, SelectVisit);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.Patient, SelectPatient);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.PatientProfile, SelectPatientProfile);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.Mrn, SelectMrn);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.PatientName, SelectPatientName);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.AccessionNumber, SelectAccessionNumber);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.Priority, SelectPriority);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.PatientClass, SelectPatientClass);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.DiagnosticServiceName, SelectDiagnosticServiceName);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.ProcedureTypeName, SelectProcedureTypeName);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.ProcedurePortable, SelectProcedurePortable);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.ProcedureLaterality, SelectProcedureLaterality);

				// we can't select ps.Name because it is not a persistent property,
				// therefore we map this field to SelectProcedureStep and fix it in the PreProcessTuple method
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.ProcedureStepName, SelectProcedureStep);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.ProcedureStepState, SelectProcedureStepState);

	
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.Report, SelectReport);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.ReportPart, SelectReportPart);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.ReportPartIndex, SelectReportPartIndex);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.ReportPartHasErrors, SelectReportPartHasErrors);

				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.OrderSchedulingRequestTime, SelectOrderSchedulingRequestTime);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.ProcedureScheduledStartTime, SelectProcedureScheduledStartTime);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.ProcedureCheckInTime, SelectProcedureCheckInTime);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.ProcedureCheckOutTime, SelectProcedureCheckOutTime);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.ProcedureStartTime, SelectProcedureStartTime);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.ProcedureEndTime, SelectProcedureEndTime);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.ProcedureStepCreationTime, SelectProcedureStepCreationTime);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.ProcedureStepScheduledStartTime, SelectProcedureStepScheduledStartTime);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.ProcedureStepStartTime, SelectProcedureStepStartTime);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.ProcedureStepEndTime, SelectProcedureStepEndTime);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.ReportPartPreliminaryTime, SelectReportPartPreliminaryTime);
				MapWorklistItemFieldToHqlSelect.Add(WorklistItemField.ReportPartCompletedTime, SelectReportPartCompletedTime);

				MapCriteriaKeyToHql.Add("Order", "o");
				MapCriteriaKeyToHql.Add("Visit", "v");
				MapCriteriaKeyToHql.Add("PatientProfile", "pp");
				MapCriteriaKeyToHql.Add("Procedure", "rp");
				MapCriteriaKeyToHql.Add("ProcedureStep", "ps");
				MapCriteriaKeyToHql.Add("ProcedureCheckIn", "pc");
				MapCriteriaKeyToHql.Add("Protocol", "pr");
				MapCriteriaKeyToHql.Add("ReportPart", "rpp");
				MapCriteriaKeyToHql.Add("Report", "r");
			}
		}
	}
}
