using System;
using System.Collections.Generic;
using System.Text;
using ClearCanvas.Enterprise.Common;
using System.Runtime.Serialization;

namespace ClearCanvas.Ris.Application.Common.ReportingWorkflow
{
    [DataContract]
    public class ReportSummary : DataContractBase
    {
        [DataMember]
        public EntityRef ReportRef;

        [DataMember]
        public EnumValueInfo ReportStatus;

        [DataMember]
        public List<ReportPartSummary> Parts;

        [DataMember]
        public PersonNameDetail Name;

        [DataMember]
        public MrnDetail Mrn;

        [DataMember]
        public DateTime? DateOfBirth;

        [DataMember]
        public string VisitNumberId;

        [DataMember]
        public string VisitNumberAssigningAuthority;

        [DataMember]
        public string AccessionNumber;

        [DataMember]
        public string DiagnosticServiceName;

        [DataMember]
        public string RequestedProcedureName;

        [DataMember]
        public string PerformedLocation;

        [DataMember]
        public DateTime? PerformedDate;

        public ReportPartSummary GetPart(int index)
        {
            if (this.Parts == null)
                return null;

            return this.Parts[index];
        }
    }
}
