﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Enterprise.Core;
using ClearCanvas.Healthcare;
using ClearCanvas.Ris.Application.Common;
using ClearCanvas.Ris.Application.Common.Admin.ProcedureTypeGroupAdmin;
using ClearCanvas.Enterprise.Core.Modelling;

namespace ClearCanvas.Ris.Application.Services
{
    internal class ProcedureTypeGroupAssembler
    {
        public ProcedureTypeGroupSummary GetProcedureTypeGroupSummary(ProcedureTypeGroup rptGroup, IPersistenceContext context)
        {
            EnumValueInfo category = GetCategoryEnumValueInfo(rptGroup.GetType());
            return new ProcedureTypeGroupSummary(rptGroup.GetRef(), rptGroup.Name, rptGroup.Description, category);
        }

        public ProcedureTypeGroupDetail GetProcedureTypeGroupDetail(ProcedureTypeGroup rptGroup, IPersistenceContext context)
        {
            ProcedureTypeGroupDetail detail = new ProcedureTypeGroupDetail();

            detail.Name = rptGroup.Name;
            detail.Description = rptGroup.Description;
            detail.Category = GetCategoryEnumValueInfo(rptGroup.GetType());

            ProcedureTypeAssembler assembler = new ProcedureTypeAssembler();
            detail.ProcedureTypes = CollectionUtils.Map<ProcedureType, ProcedureTypeSummary, List<ProcedureTypeSummary>>(
                rptGroup.ProcedureTypes,
                delegate (ProcedureType rpt)
                    {
                        return assembler.CreateSummary(rpt);
                    });

            return detail;
        }

        public EnumValueInfo GetCategoryEnumValueInfo(Type groupClass)
        {
            // this is a bit hokey but avoids having to modify the client code that is expecting an EnumValueInfo
            return new EnumValueInfo(groupClass.AssemblyQualifiedName, TerminologyTranslator.Translate(groupClass));
        }

        public void UpdateProcedureTypeGroup(ProcedureTypeGroup group, ProcedureTypeGroupDetail detail, IPersistenceContext context)
        {
            group.Name = detail.Name;
            group.Description = detail.Description;
            
            group.ProcedureTypes.Clear();
            detail.ProcedureTypes.ForEach(
                delegate(ProcedureTypeSummary summary)
                    {
                        group.ProcedureTypes.Add(context.Load<ProcedureType>(summary.ProcedureTypeRef));
                    });
        }
    }
}