#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ClearCanvas.Enterprise.Common;

namespace ClearCanvas.Ris.Application.Common.ModalityWorkflow
{
    [DataContract]
    public class DiscontinueModalityProcedureStepsRequest : DataContractBase
    {
        public DiscontinueModalityProcedureStepsRequest(List<EntityRef> modalityProcedureSteps)
        {
            ModalityProcedureSteps = modalityProcedureSteps;
        }

        [DataMember]
        public List<EntityRef> ModalityProcedureSteps;

		/// <summary>
		/// Optional. Specifies the discontinued time.  If null, the current time is assumed.
		/// </summary>
		[DataMember]
		public DateTime? DiscontinuedTime;
	}
}