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
using System.Text;
using ClearCanvas.Enterprise.Common;
using System.Runtime.Serialization;

namespace ClearCanvas.Ris.Application.Common.Admin.StaffGroupAdmin
{
    [DataContract]
	public class ListStaffGroupsRequest : ListRequestBase
    {
        public ListStaffGroupsRequest()
        {
        }

        public ListStaffGroupsRequest(SearchResultPage page)
            :base(page)
        {
        }

		/// <summary>
		/// If true, only elective staff groups will be returned.
		/// </summary>
		[DataMember]
		public bool ElectiveGroupsOnly;
	}
}
