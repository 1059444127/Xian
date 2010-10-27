﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections;
using System.Text;

using ClearCanvas.Common;
using ClearCanvas.Enterprise.Core;

namespace ClearCanvas.Healthcare {

    /// <summary>
    /// NoteSeverity enumeration
    /// </summary>
    [EnumValueClass(typeof(NoteSeverityEnum))]
    public enum NoteSeverity
	{
        /// <summary>
        /// Low
        /// </summary>
        [EnumValue("Low")]
        L,

        /// <summary>
        /// Normal
        /// </summary>
        [EnumValue("Normal")]
        N,

        /// <summary>
        /// High
        /// </summary>
        [EnumValue("High")]
        H
    }
}