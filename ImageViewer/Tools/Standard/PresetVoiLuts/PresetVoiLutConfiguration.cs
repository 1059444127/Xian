#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using ClearCanvas.Common;
using ClearCanvas.ImageViewer.Tools.Standard.PresetVoiLuts.Operations;

namespace ClearCanvas.ImageViewer.Tools.Standard.PresetVoiLuts
{
	public sealed class PresetVoiLutConfiguration : IEnumerable<KeyValuePair<string, string>>
	{
		private readonly IPresetVoiLutOperationFactory _factory;
		private readonly Dictionary<string, string> _configurationValues;

		private PresetVoiLutConfiguration(IPresetVoiLutOperationFactory factory)
		{
			_factory = factory;
			_configurationValues = new Dictionary<string, string>();
		}

		public string FactoryName
		{
			get { return _factory.Name; }
		}

		public static PresetVoiLutConfiguration FromFactory(IPresetVoiLutOperationFactory factory)
		{
			Platform.CheckForNullReference(factory, "factory");
			Platform.CheckForEmptyString(factory.Name, "factory.Name");
			return new PresetVoiLutConfiguration(factory);
		}

		public string this[string key]
		{
			get
			{
				if (!_configurationValues.ContainsKey(key))
					return null;

				return _configurationValues[key];
			}
			set
			{
				if (String.IsNullOrEmpty(key))
					return;

				if (_configurationValues.ContainsKey(key) && String.IsNullOrEmpty(value))
					_configurationValues.Remove(key);

				if (!String.IsNullOrEmpty(value))
					_configurationValues[key] = value;
			}
		}

		public void Clear()
		{
			_configurationValues.Clear();
		}

		public void CopyTo(IDictionary<string, string> dictionary)
		{
			dictionary.Clear();
			foreach (KeyValuePair<string, string> pair in _configurationValues)
				dictionary[pair.Key] = pair.Value;
		}

		#region IEnumerable<KeyValuePair<string,string>> Members

		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			return _configurationValues.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _configurationValues.GetEnumerator();
		}

		#endregion
	}
}
