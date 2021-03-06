#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using ClearCanvas.Desktop.Tables;
using ClearCanvas.Ris.Application.Common;

namespace ClearCanvas.Ris.Client
{
	public class ProcedureTypeSummaryTable : Table<ProcedureTypeSummary>
	{
		private readonly int columnSortIndex = 0;

		public ProcedureTypeSummaryTable()
		{
			this.Columns.Add(new TableColumn<ProcedureTypeSummary, string>("ID",
				delegate(ProcedureTypeSummary rpt) { return rpt.Id; },
				0.5f));

			this.Columns.Add(new TableColumn<ProcedureTypeSummary, string>("Name",
				delegate(ProcedureTypeSummary rpt) { return rpt.Name; },
				0.5f));

			this.Sort(new TableSortParams(this.Columns[columnSortIndex], true));
		}
	}

}
