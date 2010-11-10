﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met:
//
//    * Redistributions of source code must retain the above copyright notice, 
//      this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, 
//      this list of conditions and the following disclaimer in the documentation 
//      and/or other materials provided with the distribution.
//    * Neither the name of ClearCanvas Inc. nor the names of its contributors 
//      may be used to endorse or promote products derived from this software without 
//      specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR 
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
// OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
// ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY 
// OF SUCH DAMAGE.

#endregion

using ClearCanvas.Common;
using ClearCanvas.Common.Specifications;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Enterprise.Core.Modelling;

namespace ClearCanvas.Healthcare
{


	/// <summary>
	/// ExternalPractitioner entity
	/// </summary>
	[Validation(HighLevelRulesProviderMethod="GetValidationRules")]
	public partial class ExternalPractitioner : ClearCanvas.Enterprise.Core.Entity
	{
		/// <summary>
		/// Returns the default contact point, or null if no default contact point exists.
		/// </summary>
		public virtual ExternalPractitionerContactPoint DefaultContactPoint
		{
			get
			{
				return CollectionUtils.SelectFirst(_contactPoints,
					delegate(ExternalPractitionerContactPoint cp) { return cp.IsDefaultContactPoint; });
			}
		}

		/// <summary>
		/// This method is called from the constructor.  Use this method to implement any custom
		/// object initialization.
		/// </summary>
		private void CustomInitialize()
		{
		}

		/// <summary>
		/// Mark the entity as being edited.  The edit time is recorded and the entity is now unverified.
		/// </summary>
		public virtual void MarkEdited()
		{
			_lastEditedTime = Platform.Time;
			_isVerified = false;
		}

		/// <summary>
		/// Mark the entity as being verified.  The verify time is recorded.
		/// </summary>
		public virtual void MarkVerified()
		{
			_lastVerifiedTime = Platform.Time;
			_isVerified = true;
		}

		public virtual ExternalPractitioner GetUltimateMergeDestination()
		{
			var dest = this;
			while (dest.MergedInto != null)
				dest = dest.MergedInto;

			return dest;
		}

		private static IValidationRuleSet GetValidationRules()
		{
			// ensure that not both the procedure type and procedure type groups filters are being applied
			var exactlyOneDefaultContactPointRule = new ValidationRule<ExternalPractitioner>(
				delegate(ExternalPractitioner externalPractitioner)
				{
					// The rule is not applicable to deactivated external practitioner
					if (externalPractitioner.Deactivated)
						return new TestResult(true, "");

					var activeDefaultContactPoints = CollectionUtils.Select(
						externalPractitioner.ContactPoints,
						contactPoint => contactPoint.IsDefaultContactPoint && !contactPoint.Deactivated);
					var success = activeDefaultContactPoints.Count == 1;

					return new TestResult(success, SR.MessageValidateExternalPractitionerRequiresExactlyOneDefaultContactPoint);
				});

			return new ValidationRuleSet(new[] { exactlyOneDefaultContactPointRule });
		}

	}
}