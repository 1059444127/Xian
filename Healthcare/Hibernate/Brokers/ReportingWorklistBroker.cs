#region License

// Copyright (c) 2006-2007, ClearCanvas Inc.
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
using ClearCanvas.Enterprise.Common;
using ClearCanvas.Enterprise.Hibernate;
using ClearCanvas.Enterprise.Hibernate.Hql;
using ClearCanvas.Healthcare.Brokers;
using ClearCanvas.Healthcare.Workflow.Reporting;
using ClearCanvas.Workflow;
using ClearCanvas.Common.Utilities;

namespace ClearCanvas.Healthcare.Hibernate.Brokers
{
    [ExtensionOf(typeof(BrokerExtensionPoint))]
    public class ReportingWorklistBroker : Broker, IReportingWorklistBroker
    {
        private const string _hqlSelectCount = "select count(*)";
        private const string _hqlSelectWorklist =
            "select ps, pp, o.AccessionNumber, o.Priority, rpt.Name, ds.Name, ps.State, v.PatientClass";

        private const string _hqlFrom = " from {0} ps";
        private const string _hqlFromReportingStep = " from ReportingProcedureStep ps";

        private const string _hqlJoin =
            " join ps.RequestedProcedure rp" +
            " join rp.Type rpt" +
            " join rp.Order o" +
            " join o.DiagnosticService ds" +
            " join o.Visit v" +
            " join o.Patient p" +
            " join p.Profiles pp";

        private const string _hqlJoinReportPart = " join ps.ReportPart rpp";
        private const string _hqlJoinProtocol = " join ps.Protocol protocol";

        private const string _hqlWorklistSubQuery = 
            "rp.Type in" + 
            " (select distinct rpt from Worklist w" +
            " join w.RequestedProcedureTypeGroups rptg join rptg.RequestedProcedureTypes rpt where w = ?)";

        #region IReportingWorklistBroker Members

        public IList<WorklistItem> GetWorklist(Type fromType, ReportingWorklistItemSearchCriteria[] where, Worklist worklist)
        {
            string hqlFrom = string.Format(_hqlFrom, fromType.Name);
            string hqlJoin;
            if (fromType == typeof(ProtocolProcedureStep))
                hqlJoin = string.Concat(_hqlJoin, _hqlJoinProtocol);
            else
            {
                bool hasReportPartCriteria = CollectionUtils.Contains(where,
                    delegate(ReportingWorklistItemSearchCriteria c)
                        {
                            return c.ReportPart.SubCriteria.Count > 0;
                        });

                hqlJoin = hasReportPartCriteria ? string.Concat(_hqlJoin, _hqlJoinReportPart) : _hqlJoin;
            }

            HqlQuery query = new HqlQuery(string.Concat(_hqlSelectWorklist, hqlFrom, hqlJoin));
            ConstructWorklistCondition(query, where, worklist);

            return DoQuery(query);
        }

        public int GetWorklistCount(Type fromType, ReportingWorklistItemSearchCriteria[] where, Worklist worklist)
        {
            string hqlFrom = string.Format(_hqlFrom, fromType.Name);
            string hqlJoin;
            if (fromType == typeof(ProtocolProcedureStep))
                hqlJoin = string.Concat(_hqlJoin, _hqlJoinProtocol);
            else
            {
                bool hasReportPartCriteria = CollectionUtils.Contains(where,
                    delegate(ReportingWorklistItemSearchCriteria c)
                    {
                        return c.ReportPart.SubCriteria.Count > 0;
                    });

                hqlJoin = hasReportPartCriteria ? string.Concat(_hqlJoin, _hqlJoinReportPart) : _hqlJoin;
            }

            HqlQuery query = new HqlQuery(string.Concat(_hqlSelectCount, hqlFrom, hqlJoin));
            ConstructWorklistCondition(query, where, worklist);
            return DoQueryCount(query);
        }

        public IList<WorklistItem> Search(WorklistItemSearchCriteria[] where, SearchResultPage page, bool showActiveOnly)
        {
            HqlQuery query = new HqlQuery(string.Concat(_hqlSelectWorklist, _hqlFromReportingStep, _hqlJoin));
            query.Page = page;
            ConstructSearchCondition(query, where, showActiveOnly);
            return DoQuery(query);
        }

        public int SearchCount(WorklistItemSearchCriteria[] where, bool showActiveOnly)
        {
            HqlQuery query = new HqlQuery(string.Concat(_hqlSelectCount, _hqlFromReportingStep, _hqlJoin));
            ConstructSearchCondition(query, where, showActiveOnly);
            return DoQueryCount(query);
        }

        public IList<Report> GetPriorReport(Patient patient)
        {
            string hqlQuery = "select rep from Report rep" +
                              " join rep.Procedure rp" +
                              " join rp.Order o" +
                              " join o.Patient p";

            HqlQuery query = new HqlQuery(hqlQuery);

            PatientSearchCriteria patientCriteria = new PatientSearchCriteria(patient.GetRef());
            ReportSearchCriteria reportCriteria = new ReportSearchCriteria();
            reportCriteria.Status.In(new ReportStatus[] { ReportStatus.F, ReportStatus.C});

            query.Conditions.AddRange(HqlCondition.FromSearchCriteria("p", patientCriteria));
            query.Conditions.AddRange(HqlCondition.FromSearchCriteria("rep", reportCriteria));

            return ExecuteHql<Report>(query);
        }

        #endregion

        private static void ConstructWorklistCondition(HqlQuery query, IEnumerable<ReportingWorklistItemSearchCriteria> where, Worklist worklist)
        {
            HqlOr or = new HqlOr();
            foreach (ReportingWorklistItemSearchCriteria c in where)
            {
                HqlAnd and = new HqlAnd();

                and.Conditions.AddRange(HqlCondition.FromSearchCriteria("o", c.Order));
                and.Conditions.AddRange(HqlCondition.FromSearchCriteria("pp", c.PatientProfile));
                and.Conditions.AddRange(HqlCondition.FromSearchCriteria("ps", c.ReportingProcedureStep));
                and.Conditions.AddRange(HqlCondition.FromSearchCriteria("rpp", c.ReportPart));
                and.Conditions.AddRange(HqlCondition.FromSearchCriteria("protocol", c.Protocol));

                if (and.Conditions.Count > 0)
                    or.Conditions.Add(and);

                query.Sorts.AddRange(HqlSort.FromSearchCriteria("o", c.Order));
                query.Sorts.AddRange(HqlSort.FromSearchCriteria("pp", c.PatientProfile));
                query.Sorts.AddRange(HqlSort.FromSearchCriteria("ps", c.ReportingProcedureStep));
                query.Sorts.AddRange(HqlSort.FromSearchCriteria("rpp", c.ReportPart));
                query.Sorts.AddRange(HqlSort.FromSearchCriteria("protocol", c.Protocol));
            }

            if (or.Conditions.Count > 0)
                query.Conditions.Add(or);

            if (worklist != null)
            {
                query.Conditions.Add(new HqlCondition(_hqlWorklistSubQuery, worklist));
            }
        }

        private static void ConstructSearchCondition(HqlQuery query, IEnumerable<WorklistItemSearchCriteria> where, bool showActiveOnly)
        {
            if (showActiveOnly)
            {
                query.Conditions.Add(new HqlCondition("ps.State in (?, ?)", ActivityStatus.SC, ActivityStatus.IP));
            }
            else // Active Set of RPS union with inactive set of verification Step
            {
                query.Conditions.Add(new HqlCondition("(rps.State in (?, ?) or (rps.class = VerificationStep and rps.State in (?, ?)))",
                    ActivityStatus.SC, ActivityStatus.IP, ActivityStatus.SC, ActivityStatus.IP));
            }

            HqlOr or = new HqlOr();
            foreach (WorklistItemSearchCriteria c in where)
            {
                HqlAnd and = new HqlAnd();

                and.Conditions.AddRange(HqlCondition.FromSearchCriteria("o", c.Order));
                and.Conditions.AddRange(HqlCondition.FromSearchCriteria("pp", c.PatientProfile));

                if (and.Conditions.Count > 0)
                    or.Conditions.Add(and);

                query.Sorts.AddRange(HqlSort.FromSearchCriteria("o", c.Order));
                query.Sorts.AddRange(HqlSort.FromSearchCriteria("pp", c.PatientProfile));
            }

            if (or.Conditions.Count > 0)
                query.Conditions.Add(or);

        }

        private List<WorklistItem> DoQuery(HqlQuery query)
        {
            IList<object> list = ExecuteHql<object>(query);
            List<WorklistItem> results = new List<WorklistItem>();
            foreach (object[] tuple in list)
            {
                WorklistItem item = (WorklistItem)Activator.CreateInstance(typeof(WorklistItem), tuple);
                results.Add(item);
            }

            return results;
        }

        private int DoQueryCount(HqlQuery query)
        {
            return (int)ExecuteHqlUnique<long>(query);
        }

    }
}
