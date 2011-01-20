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

namespace ClearCanvas.Healthcare
{
    /// <summary>
    /// Exception thrown when an error occurs with <see cref="Note"/> acknowledgement.
    /// </summary>
    public class NoteAcknowledgementException : Exception
    {
        internal NoteAcknowledgementException(string message)
            :base(message)
        {
        }
    }
}
