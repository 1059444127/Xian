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
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Enterprise.Core;
using NHibernate.Mapping;
using NHibernate.Cfg;

namespace ClearCanvas.Enterprise.Hibernate.Ddl
{
	/// <summary>
	/// Utility class for reading Enumeration data embedded in plugins.
	/// </summary>
	class EnumMetadataReader
	{
		private readonly Dictionary<Type, Type> _mapClassToEnum;

		public EnumMetadataReader()
		{
			_mapClassToEnum = new Dictionary<Type, Type>();
			foreach (PluginInfo plugin in Platform.PluginManager.Plugins)
			{
				foreach (Type type in plugin.Assembly.GetTypes())
				{
					if (type.IsEnum)
					{
						EnumValueClassAttribute attr = CollectionUtils.FirstElement<EnumValueClassAttribute>(
							type.GetCustomAttributes(typeof(EnumValueClassAttribute), false));
						if (attr != null)
							_mapClassToEnum.Add(attr.EnumValueClass, type);
					}
				}
			}
		}

		/// <summary>
		/// Gets all enumerations mapped by the specified NHibernate configuration.
		/// </summary>
		/// <param name="config"></param>
		/// <returns></returns>
		public List<EnumerationInfo> GetEnums(Configuration config)
		{
			return GetEnums(config, delegate { return true; });
		}

		/// <summary>
		/// Gets hard enumerations mapped by the specified NHibernate configuration.
		/// </summary>
		/// <param name="config"></param>
		/// <returns></returns>
		public List<EnumerationInfo> GetHardEnums(Configuration config)
		{
			return GetEnums(config, delegate(Type t) { return IsHardEnum(t); });
		}

		/// <summary>
		/// Gets soft enumerations mapped by the specified NHibernate configuration.
		/// </summary>
		/// <param name="config"></param>
		/// <returns></returns>
		public List<EnumerationInfo> GetSoftEnums(Configuration config)
		{
			return GetEnums(config, delegate(Type t) { return !IsHardEnum(t); });
		}

		/// <summary>
		/// Gets all enumerations mapped by the specified NHibernate configuration that match the filter.
		/// </summary>
		/// <param name="config"></param>
		/// <returns></returns>
		/// <param name="filter"></param>
		public List<EnumerationInfo> GetEnums(Configuration config, Predicate<Type> filter)
		{
			// get all enum class mappings
			List<PersistentClass> persistentEnumClasses = CollectionUtils.Select<PersistentClass>(
				config.ClassMappings,
				delegate(PersistentClass c) { return typeof(EnumValue).IsAssignableFrom(c.MappedClass); });

			// process those that match the filter
			return CollectionUtils.Map<PersistentClass, EnumerationInfo>(
				persistentEnumClasses.FindAll(delegate(PersistentClass pclass) { return filter(pclass.MappedClass); }),
				delegate(PersistentClass pclass)
				{
					return IsHardEnum(pclass.MappedClass) ? 
						ReadHardEnum(pclass.MappedClass, pclass.Table) : ReadSoftEnum(pclass.MappedClass, pclass.Table);
				});
		}

		private bool IsHardEnum(Type enumValueClass)
		{
			return _mapClassToEnum.ContainsKey(enumValueClass);
		}

		private EnumerationInfo ReadHardEnum(Type enumValueClass, Table table)
		{
			Type enumType = _mapClassToEnum[enumValueClass];

			int displayOrder = 1;

			// note that we process the enum constants in order of the underlying value assigned
			// so that the initial displayOrder reflects the natural ordering
			// (see msdn docs for Enum.GetValues for details)
			return new EnumerationInfo(enumValueClass.FullName, table.Name, true, 
				CollectionUtils.Map<object, EnumerationMemberInfo>(Enum.GetValues(enumType),
					delegate(object value)
					{
						string code = Enum.GetName(enumType, value);
						FieldInfo fi = enumType.GetField(code);

						EnumValueAttribute attr = AttributeUtils.GetAttribute<EnumValueAttribute>(fi);
						return new EnumerationMemberInfo(
							code,
							attr != null ? attr.Value : null,
							attr != null ? attr.Description : null,
							displayOrder++,
							false);
					}));
		}

		private EnumerationInfo ReadSoftEnum(Type enumValueClass, Table table)
		{
			// look for an embedded resource that matches the enum class
			string res = string.Format("{0}.enum.xml", enumValueClass.FullName);
			IResourceResolver resolver = new ResourceResolver(enumValueClass.Assembly);
			try
			{
				using (Stream xmlStream = resolver.OpenResource(res))
				{
					XmlDocument xmlDoc = new XmlDocument();
					xmlDoc.Load(xmlStream);
					int displayOrder = 1;

					return new EnumerationInfo(enumValueClass.FullName, table.Name, false,
						CollectionUtils.Map<XmlElement, EnumerationMemberInfo>(xmlDoc.GetElementsByTagName("enum-value"),
							delegate(XmlElement enumValueElement)
							{
								XmlElement valueNode = CollectionUtils.FirstElement<XmlElement>(enumValueElement.GetElementsByTagName("value"));
								XmlElement descNode = CollectionUtils.FirstElement<XmlElement>(enumValueElement.GetElementsByTagName("description"));

								return new EnumerationMemberInfo(
									enumValueElement.GetAttribute("code"),
									valueNode != null ? valueNode.InnerText : null,
									descNode != null ? descNode.InnerText : null,
									displayOrder++,
									false);
							}));
				}
			}
			catch (Exception)
			{
				// no embedded resource found - no values defined
				return new EnumerationInfo(enumValueClass.FullName, table.Name, false, new List<EnumerationMemberInfo>());
			}
		}
	}
}
