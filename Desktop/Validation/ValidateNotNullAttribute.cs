#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System.Reflection;

namespace ClearCanvas.Desktop.Validation
{
	/// <summary>
	/// Validates that a property is not null.
	/// </summary>
	public class ValidateNotNullAttribute : ValidationAttribute
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public ValidateNotNullAttribute()
		{
		}

		/// <summary>
		/// Factory method to create an <see cref="IValidationRule"/> based on this attribute.
		/// </summary>
		/// <param name="property">The property on which the attribute is applied.</param>
		/// <param name="getter">A delegate that, when invoked, returns the current value of the property.</param>
		/// <param name="customMessage">A custom message to be displayed, or null if none was supplied.</param>
		/// <returns></returns>
		protected override IValidationRule CreateRule(PropertyInfo property, PropertyGetter getter, string customMessage)
		{
			string message = customMessage ?? SR.MessageValueRequired;
			return new ValidationRule(property.Name,
			   delegate(IApplicationComponent component)
			   {
				   object value = getter(component);
				   return new ValidationResult((value is string) ? !string.IsNullOrEmpty(value as string) : value != null, message);
			   });
		}
	}
}
