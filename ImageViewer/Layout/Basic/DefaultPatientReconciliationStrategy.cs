#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Xml;
using ClearCanvas.Common.Utilities;
using ClearCanvas.ImageViewer.StudyManagement;
using ClearCanvas.Dicom.Iod;
using ClearCanvas.Common;

namespace ClearCanvas.ImageViewer.Layout.Basic
{
	[Cloneable(true)]
	public class PatientInformation : IPatientData
	{
		public PatientInformation()
		{
		}
		internal PatientInformation(IPatientData patientData)
		{
			PatientId = patientData.PatientId;
			PatientsName = patientData.PatientsName;
			PatientsBirthDate = patientData.PatientsBirthDate;
			PatientsBirthTime = patientData.PatientsBirthTime;
			PatientsSex = patientData.PatientsSex;
		}

		#region IPatientData Members

		public string PatientId { get; set; }
		public string PatientsName { get; private set; }
		public string PatientsBirthDate { get; private set; }
		public string PatientsBirthTime { get; private set; }
		public string PatientsSex { get; private set; }

		#endregion

		public PatientInformation Clone()
		{
			return CloneBuilder.Clone(this) as PatientInformation;
		}
	}

	//TODO: at some point in the future, expand to a full blown auto reconciler that just wraps the Ris' reconciliation service.

	internal interface IPatientReconciliationStrategy
	{
		//NOTE: I dislike doing this everywhere - need centralized study management.
		void SetStudyTree(StudyTree studyTree);

		IPatientData ReconcileSearchCriteria(IPatientData patient);
		IPatientData ReconcilePatientInformation(IPatientData patient);
	}

	public class DefaultPatientReconciliationStrategy : IPatientReconciliationStrategy
	{
		#region PatientInformation class

		#endregion

		private readonly XmlActionsApplicator _applicator;

		public DefaultPatientReconciliationStrategy()
		{
			_applicator = new XmlActionsApplicator(DefaultActions.GetStandardActions());
		}

		private StudyTree StudyTree { get; set; }

		void IPatientReconciliationStrategy.SetStudyTree(StudyTree studyTree)
		{
			StudyTree = studyTree;		
		}

		public IPatientData ReconcileSearchCriteria(IPatientData patientInfo)
		{
			var patientInformation = new PatientInformation{ PatientId = patientInfo.PatientId };
			return Reconcile(patientInformation, DefaultPatientReconciliationSettings.Default.SearchReconciliationRulesXml, "search-reconciliation-rules");
		}

		public IPatientData ReconcilePatientInformation(IPatientData patientInfo)
		{
			Platform.CheckMemberIsSet(StudyTree, "StudyTree");

			var testPatientInformation = new PatientInformation{ PatientId = patientInfo.PatientId };
			testPatientInformation = Reconcile(testPatientInformation, DefaultPatientReconciliationSettings.Default.PatientReconciliationRulesXml, "patient-reconciliation-rules");

			foreach (var patient in StudyTree.Patients)
			{
				var reconciledPatientInfo = new PatientInformation { PatientId = patient.PatientId };
				reconciledPatientInfo = Reconcile(reconciledPatientInfo, DefaultPatientReconciliationSettings.Default.PatientReconciliationRulesXml, "patient-reconciliation-rules");

				if (reconciledPatientInfo.PatientId == testPatientInformation.PatientId)
					return new PatientInformation(patient) { PatientId = reconciledPatientInfo.PatientId };
			}

			return null;
		}

		private PatientInformation Reconcile(PatientInformation patient, XmlDocument rulesDocument, string rulesElementName)
		{
			PatientInformation returnPatient = patient.Clone();
			if (String.IsNullOrEmpty(patient.PatientId))
				return returnPatient;

			returnPatient.PatientId = returnPatient.PatientId.Trim();

			XmlElement rulesNode = rulesDocument.SelectSingleNode("//" + rulesElementName) as XmlElement;
			if (rulesNode != null)
			{
				foreach (XmlNode ruleNode in rulesNode.SelectNodes("rule"))
				{
					XmlElement ruleElement = ruleNode as XmlElement;
					if (ruleElement != null)
					{
						if (_applicator.Apply(ruleElement, returnPatient))
							break;
					}
				}
			}

			return returnPatient;
		}
	}
}