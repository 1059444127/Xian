#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Reflection;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Validation;

namespace ClearCanvas.Desktop.Validation
{
	/// <summary>
	/// Abstract base class for validation attributes.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
	public abstract class ValidationAttribute : Attribute
	{
		/// <summary>
		/// Delegate used to get the value of a property from an <see cref="IApplicationComponent"/>.
		/// </summary>
		public delegate object PropertyGetter(IApplicationComponent component);

		private string _message;

		/// <summary>
		/// Constructor.
		/// </summary>
		protected ValidationAttribute()
		{
		}

		/// <summary>
		/// Gets or sets the custom message to be displayed when a validation error occurs.
		/// </summary>
		public string Message
		{
			get { return _message; }
			set { _message = value; }
		}

		/// <summary>
		/// Gets the custom message localized according to the specified resource resolver.
		/// </summary>
		protected string GetLocalizedCustomMessage(IResourceResolver resourceResolver)
		{
			return string.IsNullOrEmpty(_message) ? null : resourceResolver.LocalizeString(_message);
		}

		/// <summary>
		/// Validates that the specified property is assignable to one of the specified types.
		/// </summary>
		protected void CheckPropertyIsType(PropertyInfo property, params Type[] types)
		{
			if (!CollectionUtils.Contains<Type>(types, delegate(Type t) { return t.IsAssignableFrom(property.PropertyType); }))
				throw new ValidationAttributeException(
					string.Format(SR.FormatAttributeCannotBeAppliedToPropertyType, this.GetType().Name, property.PropertyType.FullName));
		}

		/// <summary>
		/// Factory method to create a delegate that invokes the getter of the specified property.
		/// </summary>
		protected PropertyGetter CreatePropertyGetter(PropertyInfo property)
		{
			//JR> in theory we should be able to bind a delegate to the property's GetMethod,
			//which would perform better than reflection for repeated invocations,
			//but for some reason it fails
			//MethodInfo propertyGetter = property.GetGetMethod();
			//return (PropertyGetter)Delegate.CreateDelegate(typeof(PropertyGetter), null, propertyGetter);

			// oh well, too bad for performance - invoke via reflection
			return new PropertyGetter(
				delegate(IApplicationComponent component)
				{
					return property.GetGetMethod().Invoke(component, null);
				});
		}

		/// <summary>
		/// Factory method to create an <see cref="IValidationRule"/> based on this attribute.
		/// </summary>
		public IValidationRule CreateRule(PropertyInfo property, IResourceResolver resourceResolver)
		{
			PropertyGetter getter = CreatePropertyGetter(property);
			string customMessage = GetLocalizedCustomMessage(resourceResolver);
			return CreateRule(property, getter, customMessage);
		}

		/// <summary>
		/// Factory method to create an <see cref="IValidationRule"/> based on this attribute.
		/// </summary>
		/// <param name="property">The property on which the attribute is applied.</param>
		/// <param name="getter">A delegate that, when invoked, returns the current value of the property.</param>
		/// <param name="customMessage">A custom message to be displayed, or null if none was supplied.</param>
		/// <returns></returns>
		protected abstract IValidationRule CreateRule(PropertyInfo property, PropertyGetter getter, string customMessage);
	}
}