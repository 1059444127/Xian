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

namespace ClearCanvas.Common.Utilities
{
    /// <summary>
    /// When placed on a field/property of a class derived from <see cref="CommandLine"/>, instructs
    /// the base class to attempt to set the field/property according to the parsed command line arguments.
    /// </summary>
    /// <remarks>
    /// If the field/property is of type string, int, or enum, it is treated as a named parameter, unless
    /// the <see cref="Position"/> property of the attribute is set, in which case it is treated as a positional
    /// parameter.  If the field/property is of type boolean, it is treated as a switch.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field|AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class CommandLineParameterAttribute : Attribute
    {
        private readonly int _position = -1;
        private bool _required;
        private readonly string _key;
        private readonly string _keyShortForm;
        private readonly string _usage;
        private readonly string _displayName;

        /// <summary>
        /// Constructor for declaring a positional parameter.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="displayName"></param>
        public CommandLineParameterAttribute(int position, string displayName)
        {
            _position = position;
            _displayName = displayName;
        }

        /// <summary>
        /// Constructor for declaring a named parameter or boolean switch.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="usage"></param>
        public CommandLineParameterAttribute(string key, string usage)
        {
            _key = key;
            _usage = usage;
        }

        /// <summary>
        /// Constructor for declaring a named parameter or boolean switch.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keyShortForm"></param>
        /// <param name="usage"></param>
        public CommandLineParameterAttribute(string key, string keyShortForm, string usage)
        {
            _key = key;
            _keyShortForm = keyShortForm;
            _usage = usage;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this parameter is a required parameter.
        /// </summary>
        public bool Required
        {
            get { return _required; }
            set { _required = value; }
        }

        /// <summary>
        /// Gets the position of a positional parameter.
        /// </summary>
        internal int Position
        {
            get { return _position; }
        }

        /// <summary>
        /// Gets the display name for a positional parameter.
        /// </summary>
        internal string DisplayName
        {
            get { return _displayName; }
        }

        /// <summary>
        /// Gets the key (parameter name) for a named parameter.
        /// </summary>
        internal string Key
        {
            get { return _key; }
        }

        /// <summary>
        /// Gets the key short-form for a named parameter.
        /// </summary>
        internal string KeyShortForm
        {
            get { return _keyShortForm; }
        }

        /// <summary>
        /// Gets a message describing the usage of this parameter.
        /// </summary>
        internal string Usage
        {
            get { return _usage; }
        }

    }
}
