using System;
using System.Collections.Generic;
using System.Text;
using ClearCanvas.Common;
using ClearCanvas.Healthcare;
using ClearCanvas.Enterprise;
using ClearCanvas.Healthcare.Brokers;

namespace ClearCanvas.Ris.Services
{
    [ExtensionOf(typeof(PatientReconciliationStrategyExtensionPoint))]
    public class DefaultPatientReconciliationStrategy : IPatientReconciliationStrategy
    {
        #region IPatientReconciliationStrategy Members

        public IList<PatientProfileMatch> FindReconciliationMatches(PatientProfile patient, IPatientProfileBroker broker)
        {
            /* User needs to resort to manual linking of patient records from multiple HIS when automatic MPI fails. 
             * 
             * Allow user to to select 2 or more patient records from different hospitals and merge into an MPI.  
             * 
             * Display High-Probability match/search results from muliple HIS of patients with various MRNs when these 4 fields 
             * are matched/identical: surname, given name, DOB, healthcard #.  
             * 
             * Display Moderate-Probability match/search results from multiple HIS of patients with various MRNs when fields: surname, 
             * given name, DOB, gender are matched/identical.  
             * 
             * Display Moderate-Probability match/search results from multiple HIS of patients with various MRNs when 
             * field: healthcard # is matched/identical.
            */

            IList<PatientProfileMatch> matches = new List<PatientProfileMatch>();

            PatientProfileSearchCriteria high = new PatientProfileSearchCriteria();
            high.Name.FamilyName.EqualTo(patient.Name.FamilyName);
            high.Name.GivenName.EqualTo(patient.Name.GivenName);
            high.DateOfBirth.EqualTo(patient.DateOfBirth);
            high.Healthcard.Id.EqualTo(patient.Healthcard.Id);

            IList<PatientProfileMatch> highMatches = PatientProfileMatch.CreateList(patient, broker.Find(high), PatientProfileMatch.ScoreValue.High);

            PatientProfileSearchCriteria moderateViaName = new PatientProfileSearchCriteria();
            moderateViaName.Name.FamilyName.EqualTo(patient.Name.FamilyName);
            moderateViaName.Name.GivenName.EqualTo(patient.Name.GivenName);
            moderateViaName.DateOfBirth.EqualTo(patient.DateOfBirth);
            moderateViaName.Sex.EqualTo(patient.Sex);

            IList<PatientProfileMatch> moderateMatchesViaName = PatientProfileMatch.CreateList(patient, broker.Find(moderateViaName), PatientProfileMatch.ScoreValue.Moderate);

            PatientProfileSearchCriteria moderateViaHealthcard = new PatientProfileSearchCriteria();
            moderateViaHealthcard.Healthcard.Id.EqualTo(patient.Healthcard.Id);

            IList<PatientProfileMatch> moderateMatchesViaHealthcard = PatientProfileMatch.CreateList(patient, broker.Find(moderateViaHealthcard), PatientProfileMatch.ScoreValue.Moderate);

            matches = PatientProfileMatch.Join(highMatches, moderateMatchesViaName);
            matches = PatientProfileMatch.Join(matches, moderateMatchesViaHealthcard);

            RemoveConflicts(patient.Patient, matches);

            return matches;
        }

        #endregion

        private void RemoveConflicts(Patient patient, IList<PatientProfileMatch> matches)
        {
            IList<PatientProfileMatch> conflicts = new List<PatientProfileMatch>();
            foreach (PatientProfile existingReconciledProfile in patient.Profiles)
            {
                IdentifyConflictsForSiteFromProposedMatches(existingReconciledProfile, matches, conflicts);
            }
            foreach (PatientProfileMatch conflict in conflicts)
            {
                matches.Remove(conflict);
            }
        }

        private static void IdentifyConflictsForSiteFromProposedMatches(PatientProfile existingReconciledProfile, IList<PatientProfileMatch> matches, IList<PatientProfileMatch> conflicts)
        {
            String existingMRN = existingReconciledProfile.MRN.AssigningAuthority;
            foreach (PatientProfileMatch proposedMatch in matches)
            {
                if (proposedMatch.PatientProfile.MRN.AssigningAuthority == existingMRN)
                {
                    conflicts.Add(proposedMatch);
                    RemoveAllProfilesRelatedToConflict(proposedMatch, matches, conflicts);
                }
            }
        }

        private static void RemoveAllProfilesRelatedToConflict(PatientProfileMatch proposedMatch, IList<PatientProfileMatch> matches, IList<PatientProfileMatch> conflicts)
        {
            foreach (PatientProfileMatch otherProfilesInConflictingPatient in matches)
            {
                if (otherProfilesInConflictingPatient.PatientProfile.Patient == proposedMatch.PatientProfile.Patient)
                {
                    conflicts.Add(otherProfilesInConflictingPatient);
                }
            }
        }

    }
}
