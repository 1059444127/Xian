#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using ClearCanvas.Enterprise.Core;
using ClearCanvas.Healthcare;
using ClearCanvas.Ris.Application.Common;

namespace ClearCanvas.Ris.Application.Services
{
    public class ProtocolProcedureStepAssembler
    {
        public ProtocolProcedureStepDetail CreateProtocolProcedureStepDetail(ProtocolProcedureStep step, IPersistenceContext context)
        {
            ProtocolProcedureStepDetail detail = new ProtocolProcedureStepDetail();

            detail.ProtocolProcedureStepRef = step.GetRef();
            detail.Status = EnumUtils.GetEnumValueInfo(step.State, context);
            detail.ProtocolRef = step.Protocol.GetRef();

            return detail;
        }
    }
}
