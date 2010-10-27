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

namespace ClearCanvas.Enterprise.Support
{
    public class PrimitiveTypeInfoExchange : IInfoExchange
    {
        #region IConversion Members

        public object GetInfoFromObject(object pobj, IPersistenceContext pctx)
        {
            return pobj;
        }

        public object GetObjectFromInfo(object info, IPersistenceContext pctx)
        {
            return info;
        }

        #endregion
    }
}
