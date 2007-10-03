using System;
using System.Collections.Generic;
using System.Text;
using ClearCanvas.Common.Configuration;
using ClearCanvas.Common;
using System.Configuration;
using System.Reflection;
using System.Collections.Specialized;
using System.Xml;
using System.IO;
using System.ComponentModel;
using ClearCanvas.Common.Utilities;

namespace ClearCanvas.Desktop.Configuration
{
	/// <summary>
	/// The LocalConfigurationStore, although it implements <see cref="IConfigurationStore "/> does not serve 
	/// as a proper configuration store for the <see cref="StandardSettingsProvider"/> (notice that it is not
	/// an extension of <see cref="ConfigurationStoreExtensionPoint"/>.  Instead, this class is instantiated
	/// directly by the <see cref="SettingsManagementComponent"/> when there are no such extensions available,
	/// and the application is using the <see cref="LocalFileSettingsProvider"/> (or app/user .config) to 
	/// store settings locally.  This 'configuration store' is used solely to edit the default profile
	/// throught the settings management UI.
	/// </summary>
	internal class LocalConfigurationStore : IConfigurationStore
	{
		private static string _applicationSettingsGroup = "applicationSettings";
		private static string _userSettingsGroup = "userSettings";

		public LocalConfigurationStore()
		{
        }

		/// <summary>
		/// Determines how a particular property should be serialized based on its type.
		/// </summary>
		/// <param name="property">the property whose SerializeAs method is to be determined</param>
		/// <returns>a <see cref="SettingsSerializeAs"/> value</returns>
		private SettingsSerializeAs DetermineSerializeAs(PropertyInfo property)
		{
			object[] serializeAsAttributes = property.GetCustomAttributes(typeof(SettingsSerializeAsAttribute), false);
			if (serializeAsAttributes.Length > 0)
				return ((SettingsSerializeAsAttribute)serializeAsAttributes[0]).SerializeAs;

			TypeConverter converter = TypeDescriptor.GetConverter(property.PropertyType);
			Type stringType = typeof(string);
			if (converter.CanConvertTo(stringType) && converter.CanConvertFrom(stringType))
				return SettingsSerializeAs.String;

			return SettingsSerializeAs.Xml;
		}

		/// <summary>
		/// Constructs a new <see cref="ClientSettingsSection"/> containing all of the default values
		/// for the particular settings class.
		/// </summary>
		/// <param name="properties">the properties to be added to the new section</param>
		/// <returns>a <see cref="ClientSettingsSection"/> object</returns>
		private ClientSettingsSection ConstructDefaultSection(IEnumerable<PropertyInfo> properties)
		{
			ClientSettingsSection section = new ClientSettingsSection();
			section.SectionInformation.RequirePermission = false;

			XmlDocument document = new XmlDocument();

			foreach (PropertyInfo property in properties)
			{
				SettingElement element = new SettingElement();
				element.Name = property.Name;
				element.SerializeAs = DetermineSerializeAs(property);

				SettingValueElement newElement = new SettingValueElement();
				XmlNode valueXml = document.CreateElement("value");
				valueXml.InnerText = SettingsClassMetaDataReader.GetDefaultValue(property);

				newElement.ValueXml = valueXml;

				element.Value = newElement;
				section.Settings.Add(element);
			}

			return section;
		}

		/// <summary>
		/// Stores/adds all setting values to a particular <see cref="ConfigurationSection"/>, not just the ones 
		/// that are different (default values are also stored).  The reason for storing all of the values is, in the local
		/// file case, a value may or may not already be stored that needs to be changed back to the default (the newValues dictionary
		/// will not contain an entry for that value) and it is arguably more labour intensive to perform removal of the value
		/// than it is just to write out the entire section.
		/// </summary>
		/// <param name="sectionGroup">the parent section group (applicationSettings or userSettings)</param>
		/// <param name="sectionName">the name of the <see cref="ClientSettingsSection"/> to change/store</param>
		/// <param name="scopeProperties">the settings properties that correspond to the same scope as the 
		/// sectionGroup (e.g. applicationSettings or userSettings)</param>
		/// <param name="newValues">the 'new' values to store.  Only the values that are different from 
		/// the propery defaults are contained in this dictionary.  The implication being that existing values whose
		/// value is not in this dictionary must *be* the same as the default.</param>
		/// <param name="oldValues">for any values that get changed by this method, the previous values will be returned</param>
		/// <returns>whether or not any modifications were made, which will normally be true unless the values in the store
		/// are already the same.</returns>
		/// <exception cref="NotSupportedException">when the section corresponding to sectionName is not a <see cref="ClientSettingsSection"/></exception>
		private bool StoreSettings
			(
				ConfigurationSectionGroup sectionGroup,
				string sectionName,
				List<PropertyInfo> scopeProperties,
				IDictionary<string, string> newValues,
				IDictionary<string, string> oldValues
			)
		{
			bool newSection = false;
			ConfigurationSection section = sectionGroup.Sections[sectionName];
			if (section == null)
			{
				newSection = true;
				ClientSettingsSection newClientSection = ConstructDefaultSection(scopeProperties);
				sectionGroup.Sections.Add(sectionName, newClientSection);
				if (sectionGroup.Name == _userSettingsGroup)
					newClientSection.SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToLocalUser;

				section = sectionGroup.Sections[sectionName];
			}

			ClientSettingsSection clientSection = section as ClientSettingsSection;
			if (clientSection == null)
				throw new NotSupportedException(String.Format(SR.ExceptionSectionIsNotAClientSection, section.GetType().FullName));

			bool modified = false;

			foreach (PropertyInfo property in scopeProperties)
			{
				SettingElement element = clientSection.Settings.Get(property.Name);
				string requiredValue = SettingsClassMetaDataReader.GetDefaultValue(property);
				if (newValues.ContainsKey(property.Name))
					requiredValue = newValues[property.Name];

				if (element.Value.ValueXml.InnerText != requiredValue)
				{
					oldValues[property.Name] = element.Value.ValueXml.InnerText;

					element.Value.ValueXml.InnerText = requiredValue;
					modified = true;
				}
			}

			if (newSection && !modified)
				sectionGroup.Sections.Remove(sectionName);

			return modified;
		}

		/// <summary>
		/// Gets all the settings for a particular <see cref="ClientSettingsSection"/> that are different from the property defaults.
		/// </summary>
		/// <param name="sectionGroup">the parent section group (applicationSettings or userSettings)</param>
		/// <param name="sectionName">the name of the <see cref="ClientSettingsSection"/> whose values are to be retrieved</param>
		/// <param name="scopeProperties">the settings properties that correspond to the same scope as the 
		/// sectionGroup (e.g. applicationSettings or userSettings)</param>
		/// <param name="values">returns the property values that are different from the defaults</param>
		/// <exception cref="NotSupportedException"> when the section corresponding to sectionName is not a <see cref="ClientSettingsSection"/></exception>
		/// <exception cref="ArgumentException">if a property in scopeProperties is not found</exception>
		private void GetNonDefaultSettings
			(
				ConfigurationSectionGroup sectionGroup,
				string sectionName,
				List<PropertyInfo> scopeProperties,
				IDictionary<string, string> values
			)	
		{
			if (sectionGroup == null)
				return; //the values are the same as the defaults.

			ConfigurationSection section = sectionGroup.Sections[sectionName];
			if (section == null)
				return; //the values are the same as the defaults.

			ClientSettingsSection clientSection = section as ClientSettingsSection;
			if (clientSection == null)
				throw new NotSupportedException(String.Format(SR.ExceptionSectionIsNotAClientSection, section.GetType().FullName));

			foreach (PropertyInfo property in scopeProperties)
			{
				string defaultValueUntranslated = SettingsClassMetaDataReader.GetDefaultValue(property, false);
				string defaultValueTranslated = SettingsClassMetaDataReader.GetDefaultValue(property);

				SettingElement element = clientSection.Settings.Get(property.Name);
				if (element == null)
					throw new ArgumentException(String.Format(SR.ExceptionSettingsPropertyDoesNotExist, String.Format("{0}/{1}/{2}", sectionGroup, sectionName, property.Name)));

				string currentValue = element.Value.ValueXml.InnerText;
				//translated or untranslated, it's still the default.
				bool isDefaultValue = (currentValue == defaultValueTranslated || currentValue == defaultValueUntranslated);

				if (!isDefaultValue)
					values[property.Name] = currentValue;
			}
		}

		/// <summary>
		/// Gets the applicationSettings group <see cref="ConfigurationSectionGroup"/>.
		/// </summary>
		/// <param name="configuration">the local configuration object</param>
		/// <param name="create">a boolean indicating whether or not to create the applicationSettings group</param>
		/// <returns>the applicationSettings group</returns>
		private ConfigurationSectionGroup GetApplicationSettingsGroup(System.Configuration.Configuration configuration, bool create)
		{
			if (configuration.GetSectionGroup(_applicationSettingsGroup) == null && create)
				configuration.SectionGroups.Add(_applicationSettingsGroup, new ApplicationSettingsGroup());

			return configuration.GetSectionGroup(_applicationSettingsGroup);
		}

		/// <summary>
		/// Gets the userSettings group <see cref="ConfigurationSectionGroup"/>.
		/// </summary>
		/// <param name="configuration">the local configuration object</param>
		/// <param name="create">a boolean indicating whether or not to create the userSettings group</param>
		/// <returns>the userSettings group</returns>
		private ConfigurationSectionGroup GetUserSettingsGroup(System.Configuration.Configuration configuration, bool create)
		{
			if (configuration.GetSectionGroup(_userSettingsGroup) == null && create)
				configuration.SectionGroups.Add(_userSettingsGroup, new UserSettingsGroup());

			return configuration.GetSectionGroup(_userSettingsGroup);
		}

		/// <summary>
		/// Splits up the application/user scoped settings properties
		/// </summary>
		/// <param name="properties">the entire set of settings properties</param>
		/// <param name="applicationScopedProperties">upon returning, contains the application settings scoped properties</param>
		/// <param name="userScopedProperties">upon returning, contains the user settings scoped properties</param>
		private void SplitPropertiesByScope
			(
				IEnumerable<PropertyInfo> properties,
				out List<PropertyInfo> applicationScopedProperties,
				out List<PropertyInfo> userScopedProperties
			)
		{
			applicationScopedProperties = new List<PropertyInfo>();
			userScopedProperties = new List<PropertyInfo>();

			foreach (PropertyInfo property in properties)
			{
				if (SettingsClassMetaDataReader.IsUserScoped(property))
					userScopedProperties.Add(property);
				else if (SettingsClassMetaDataReader.IsAppScoped(property))
					applicationScopedProperties.Add(property);
			}
		}

		#region IConfigurationStore Members
		
		/// <summary>
		/// Loads the settings values (both application and user scoped) for a given settings class.  Only the default profile
		/// is supported (application settings + default user settings).
		/// </summary>
		/// <param name="settingsClass">the settings class for which to retrieve the defaults</param>
		/// <param name="user">must be null or ""</param>
		/// <param name="instanceKey">ignored</param>
		/// <param name="values">returns only those values that are different from the property defaults</param>
		/// <exception cref="NotSupportedException">will be thrown if the user is specified</exception>
        public Dictionary<string, string> LoadSettingsValues(SettingsGroupDescriptor group, string user, string instanceKey)
		{
			if (!String.IsNullOrEmpty(user))
				throw new NotSupportedException(SR.ExceptionOnlyDefaultProfileSupported);

            Type settingsClass = Type.GetType(group.AssemblyQualifiedTypeName);
			ICollection<PropertyInfo> properties = SettingsClassMetaDataReader.GetSettingsProperties(settingsClass);

			System.Configuration.Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

			List<PropertyInfo> applicationScopedProperties;
			List<PropertyInfo> userScopedProperties;
			SplitPropertiesByScope(properties, out applicationScopedProperties, out userScopedProperties);

            Dictionary<string, string> values = new Dictionary<string, string>();
			if (applicationScopedProperties.Count > 0)
				GetNonDefaultSettings(this.GetApplicationSettingsGroup(configuration, false), group.Name, applicationScopedProperties, values);
			if (userScopedProperties.Count > 0)
                GetNonDefaultSettings(this.GetUserSettingsGroup(configuration, false), group.Name, userScopedProperties, values);

            return values;
		}

		/// <summary>
		/// Stores the settings values (both application and user scoped) for a given settings class.  Only the default profile
		/// is supported (application settings + default user settings).
		/// </summary>
		/// <param name="settingsClass">the settings class for which to store the values</param>
		/// <param name="user">must be null or ""</param>
		/// <param name="instanceKey">ignored</param>
		/// <param name="values">contains the values to be stored</param>
		/// <exception cref="NotSupportedException">will be thrown if the user is specified</exception>
        public void SaveSettingsValues(SettingsGroupDescriptor group, string user, string instanceKey, Dictionary<string, string> values)
		{
			if (!String.IsNullOrEmpty(user))
				throw new NotSupportedException(SR.ExceptionOnlyDefaultProfileSupported);

            Type settingsClass = Type.GetType(group.AssemblyQualifiedTypeName);
            ICollection<PropertyInfo> properties = SettingsClassMetaDataReader.GetSettingsProperties(settingsClass);

			System.Configuration.Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

			List<PropertyInfo> applicationScopedProperties;
			List<PropertyInfo> userScopedProperties;
			SplitPropertiesByScope(properties, out applicationScopedProperties, out userScopedProperties);

			Dictionary<string, string> changedValues = new Dictionary<string, string>();

			bool modified = false;
			if (applicationScopedProperties.Count > 0)
                modified = StoreSettings(this.GetApplicationSettingsGroup(configuration, true), group.Name, applicationScopedProperties, values, changedValues);
			if (userScopedProperties.Count > 0)
                modified |= StoreSettings(this.GetUserSettingsGroup(configuration, true), group.Name, userScopedProperties, values, changedValues);

			if (modified)
			{
				configuration.Save(ConfigurationSaveMode.Minimal, true);
			}
		}

		/// <summary>
		/// Unsupported.  An exception will always be thrown.
		/// </summary>
		/// <param name="settingsClass"></param>
		/// <param name="user"></param>
		/// <param name="instanceKey"></param>
		/// <exception cref="NotSupportedException">always thrown</exception>
        public void RemoveUserSettings(SettingsGroupDescriptor group, string user, string instanceKey)
		{
			throw new NotSupportedException(SR.ExceptionRemoveUserSettingNotSupported);
		}

		/// <summary>
		/// Unsupported.  An exception will always be thrown.
		/// </summary>
		/// <param name="settingsClass"></param>
		/// <param name="user"></param>
		/// <param name="instanceKey"></param>
		/// <exception cref="NotSupportedException">always thrown</exception>
        public void UpgradeUserSettings(SettingsGroupDescriptor group, string user, string instanceKey)
		{
			throw new NotSupportedException(SR.ExceptionUpgradeNotSupported);
		}

        /// <summary>
        /// Returns settings groups installed on local machine
        /// </summary>
        /// <returns></returns>
        public IList<SettingsGroupDescriptor> ListSettingsGroups()
        {
            return SettingsGroupDescriptor.ListInstalledSettingsGroups();
        }

        /// <summary>
        /// Returns settings properties for specified group, assuming plugin containing group resides on local machine
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public IList<SettingsPropertyDescriptor> ListSettingsProperties(SettingsGroupDescriptor group)
        {
            return SettingsPropertyDescriptor.ListSettingsProperties(group);
        }

        #endregion
    }
}
