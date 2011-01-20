#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using ClearCanvas.Common;
using ClearCanvas.Enterprise.Core;

namespace ClearCanvas.Healthcare.Alerts
{
    [ExtensionPoint]
    public class PatientProfileAlertExtensionPoint : ExtensionPoint<IPatientProfileAlert>
    {
    }

    public interface IPatientProfileAlert : IAlert<PatientProfile>
    {
    }

    public abstract class PatientProfileAlertBase : AlertBase<PatientProfile>, IPatientProfileAlert
    {
    }
}
