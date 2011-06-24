﻿#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca

// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System.Collections.Generic;
using ClearCanvas.Common.Utilities;
using System;

namespace ClearCanvas.ImageViewer.Layout
{
	/// <summary>
	/// Provides a default implementation of <see cref="IHpProperty"/>.
	/// </summary>
	/// <remarks>
	/// This class can be used as-is or subclassed for advanced functionality.
	/// </remarks>
	/// <typeparam name="TProperty"></typeparam>
    public class HpProperty<TProperty> : IHpProperty
	{
        public delegate TProperty ValueGetter();
        public delegate void ValueSetter(TProperty value);

		private readonly ValueGetter _getter;
		private readonly ValueSetter _setter;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name"></param>
		/// <param name="description"></param>
		/// <param name="getter"></param>
		/// <param name="setter"></param>
        public HpProperty(string name, string description, ValueGetter getter, ValueSetter setter)
            : this(name, description, null, getter, setter)
		{
		}

        public HpProperty(string name, string description, List<TProperty> standardValues, ValueGetter getter, ValueSetter setter)
		{
            DisplayName = name;
            Description = description;
            if (standardValues != null)
		        StandardValues = standardValues.AsReadOnly();

            _getter = getter;
            _setter = setter;
        }

	    #region Implementation of IHpProperty

        public Type Type { get { return typeof(TProperty); } }

		/// <summary>
		/// Gets the display name of this property for display in the user-interface.
		/// </summary>
		public string DisplayName { get; private set; }

		/// <summary>
		/// Gets the description of this property for display in the user-interface.
		/// </summary>
		public string Description { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this property can be edited by a custom dialog box.
		/// </summary>
		public virtual bool HasEditor
		{
			get { return false; }
		}

		/// <summary>
		/// Called to invoke custom editing of this property, if <see cref="IHpProperty.HasEditor"/> returns true. 
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public virtual bool EditProperty(IHpPropertyEditContext context)
		{
			return false;
		}

	    public bool HasStandardValues
	    {
            get { return StandardValues != null && StandardValues.Count > 0; }    
	    }

        object[] IHpProperty.StandardValues
        {
            get
            {
                return CollectionUtils.Map<TProperty, object>(StandardValues, value => value).ToArray();
            }
        }

        public virtual bool CanSetValue
        {
            get { return true; }
        }

        object IHpProperty.Value
        {
            get { return Value; }
            set
            {
                if (!CanSetValue)
                    throw new InvalidOperationException();

                Value = (TProperty)value;
            }
        }
        
        #endregion

	    IList<TProperty> StandardValues { get; set; }

	    /// <summary>
		/// Gets or sets the value associated with this property.
		/// </summary>
		protected TProperty Value
		{
			get { return _getter(); }
			set { _setter(value); }
		}
	}
}
