#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;

namespace ClearCanvas.ImageServer.Enterprise.Authentication
{
    /// <summary>
    /// Represents an exception thrown when a <see cref="SessionInfo"/> cannot be 
    /// validated.
    /// </summary>
    public class SessionValidationException : Exception
    {
        public SessionValidationException()
        {
        }

        public SessionValidationException(Exception baseException)
            : base(baseException.Message, baseException)
        {
            
        }

    }
}