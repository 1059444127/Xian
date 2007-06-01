using System;
using System.Collections.Generic;
using System.Text;
using ClearCanvas.Ris.Application.Common.PatientReconciliation;
using ClearCanvas.Healthcare;
using ClearCanvas.Healthcare.Brokers;
using ClearCanvas.Healthcare.PatientReconciliation;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Enterprise.Core;
using ClearCanvas.Enterprise.Common;
using ClearCanvas.Common;
using System.Security.Permissions;
using ClearCanvas.Ris.Application.Common;
using ClearCanvas.Healthcare.Workflow;
using ClearCanvas.Healthcare.Workflow.Registration;

namespace ClearCanvas.Ris.Application.Services.PatientReconciliation
{
    [ServiceImplementsContract(typeof(IPatientReconciliationService))]
    [ExtensionOf(typeof(ApplicationServiceExtensionPoint))]
    public class PatientReconciliationService : ApplicationServiceBase, IPatientReconciliationService
    {
        #region IPatientReconciliationService Members

        [ReadOperation]
        public ListPatientReconciliationMatchesResponse ListPatientReconciliationMatches(ListPatientReconciliationMatchesRequest request)
        {
            IPatientProfileBroker broker = PersistenceContext.GetBroker<IPatientProfileBroker>();
            PatientProfile targetProfile = broker.Load(request.PatientProfileRef);

            IPatientReconciliationStrategy strategy = (IPatientReconciliationStrategy)(new PatientReconciliationStrategyExtensionPoint()).CreateExtension();
            IList<PatientProfileMatch> matches = strategy.FindReconciliationMatches(targetProfile, PersistenceContext);

            PatientProfileAssembler profileAssembler = new PatientProfileAssembler();
            ListPatientReconciliationMatchesResponse response = new ListPatientReconciliationMatchesResponse();
            response.ReconciledProfiles = CollectionUtils.Map<PatientProfile, PatientProfileSummary, List<PatientProfileSummary>>(
                targetProfile.Patient.Profiles,
                delegate(PatientProfile profile)
                {
                    return profileAssembler.CreatePatientProfileSummary(profile, PersistenceContext);
                });

            ReconciliationCandidateAssembler rcAssembler = new ReconciliationCandidateAssembler();
            response.MatchCandidates = CollectionUtils.Map<PatientProfileMatch, ReconciliationCandidate, List<ReconciliationCandidate>>(
                matches,
                delegate(PatientProfileMatch match)
                {
                    return rcAssembler.CreateReconciliationCandidate(match, PersistenceContext);
                });

            return response;
        }

        [ReadOperation]
        public LoadPatientProfileDiffResponse LoadPatientProfileDiff(LoadPatientProfileDiffRequest request)
        {
            IPatientProfileBroker broker = PersistenceContext.GetBroker<IPatientProfileBroker>();

            // load profiles to compare
            PatientProfile leftProfile = broker.Load(request.LeftProfileRef);
            PatientProfile rightProfile = broker.Load(request.RightProfileRef);

            // ask model to compute discrepancies
            IList<DiscrepancyTestResult> results = PatientProfileDiscrepancyTest.GetDiscrepancies(leftProfile, rightProfile, PatientProfileDiscrepancy.All);

            // build response
            PatientProfileDiffAssembler assembler = new PatientProfileDiffAssembler();
            PatientProfileDiff diff = assembler.CreatePatientProfileDiff(leftProfile, rightProfile, results);
            return new LoadPatientProfileDiffResponse(diff);
        }

        [ReadOperation]
        public ListProfilesForPatientsResponse ListProfilesForPatients(ListProfilesForPatientsRequest request)
        {
            PatientProfileAssembler assembler = new PatientProfileAssembler();
            List<PatientProfileSummary> summaries = new List<PatientProfileSummary>();
            foreach(EntityRef patientRef in request.PatientRefs)
            {
                Patient patient = (Patient)PersistenceContext.Load(patientRef);
                foreach (PatientProfile profile in patient.Profiles)
                {
                    summaries.Add(assembler.CreatePatientProfileSummary(profile, PersistenceContext));
                }
            }

            return new ListProfilesForPatientsResponse(summaries);
        }

        [UpdateOperation]
        [PrincipalPermission(SecurityAction.Demand, Role = ClearCanvas.Ris.Application.Common.AuthorityTokens.ReconcilePatients)]
        public ReconcilePatientsResponse ReconcilePatients(ReconcilePatientsRequest request)
        {
            List<Patient> patients = CollectionUtils.Map<EntityRef, Patient, List<Patient>>(
                request.PatientRefs,
                delegate(EntityRef patientRef)
                {
                    return (Patient)PersistenceContext.Load(patientRef, EntityLoadFlags.CheckVersion);
                });

            if (patients.Count < 2)
                throw new RequestValidationException(SR.ExceptionReconciliationRequiresAtLeast2Patients);

            Operations.ReconcilePatient op = new Operations.ReconcilePatient();
            op.Execute(patients, this.PersistenceContext);

            return new ReconcilePatientsResponse();
        }

        #endregion
    }
}
