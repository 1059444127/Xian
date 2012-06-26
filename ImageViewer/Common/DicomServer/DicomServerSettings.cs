﻿#region License

// Copyright (c) 2012, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Xml.Serialization;
using ClearCanvas.Common;
using ClearCanvas.Common.Configuration;
using ClearCanvas.ImageViewer.Common.Configuration;

namespace ClearCanvas.ImageViewer.Common.DicomServer
{
    // TODO (CR Jun 2012): Leaving internal, at least for now, since the DicomServerConfiguration data contract
    // and some of the internal get/set code actually provides a useful abstraction, and there's really no immediate need to change it.

    [SettingsGroupDescription("Configuration for the local DICOM server.")]
	[SettingsProvider(typeof (SystemConfigurationSettingsProvider))]
	internal sealed partial class DicomServerSettings : IMigrateSettings
	{
	    public sealed class Proxy
		{
			private readonly DicomServerSettings _settings;

			public Proxy(DicomServerSettings settings)
			{
				_settings = settings;
			}

			private object this[string propertyName]
			{
				get { return _settings[propertyName]; }
				set { _settings.SetSharedPropertyValue(propertyName, value); }
			}

			[DefaultValue("localhost")]
			public string HostName
			{
				get { return (string) this["HostName"]; }
				set { this["HostName"] = value; }
			}

			[DefaultValue("CLEARCANVAS")]
			public string AETitle
			{
				get { return (string) this["AETitle"]; }
				set { this["AETitle"] = value; }
			}

			[DefaultValue("104")]
			public int Port
			{
				get { return (int) this["Port"]; }
				set { this["Port"] = value; }
			}

            [DefaultValue(false)]
            public bool QueryResponsesInUtf8
            {
                get { return (bool)this["QueryResponsesInUtf8"]; }
                set { this["QueryResponsesInUtf8"] = value; }
            }
            
            [DefaultValue(true)]
			public bool AllowUnknownCaller
			{
				get { return (bool) this["AllowUnknownCaller"]; }
				set { this["AllowUnknownCaller"] = value; }
			}

			public ImageSopClassConfigurationElementCollection ImageStorageSopClasses
			{
				get { return (ImageSopClassConfigurationElementCollection) this["ImageStorageSopClasses"]; }
				set { this["ImageStorageSopClasses"] = value; }
			}

			public NonImageSopClassConfigurationElementCollection NonImageStorageSopClasses
			{
				get { return (NonImageSopClassConfigurationElementCollection) this["NonImageStorageSopClasses"]; }
				set { this["NonImageStorageSopClasses"] = value; }
			}

			public TransferSyntaxConfigurationElementCollection StorageTransferSyntaxes
			{
				get { return (TransferSyntaxConfigurationElementCollection) this["StorageTransferSyntaxes"]; }
				set { this["StorageTransferSyntaxes"] = value; }
			}

			public void Save()
			{
				_settings.Save();
			}
		}

        public Proxy GetProxy()
        {
            return new Proxy(this);
        }

        public DicomServerConfiguration GetBasicConfiguration()
        {
            return new DicomServerConfiguration {AETitle = AETitle, HostName = HostName, Port = Port};
        }

        public DicomServerExtendedConfiguration GetExtendedConfiguration()
        {
            return new DicomServerExtendedConfiguration
                       {
                           AllowUnknownCaller = AllowUnknownCaller,
                           QueryResponsesInUtf8 = QueryResponsesInUtf8,
                           ImageStorageSopClassUids = ImageStorageSopClasses.Select(e => e.Uid).ToList(),
                           NonImageStorageSopClassUids = NonImageStorageSopClasses.Select(e => e.Uid).ToList(),
                           StorageTransferSyntaxUids = StorageTransferSyntaxes.Select(e => e.Uid).ToList()
                       };
        }

        public DicomServerConfiguration UpdateBasicConfiguration(DicomServerConfiguration newConfiguration)
        {
            Platform.CheckForNullReference(newConfiguration, "newConfiguration");
            Platform.CheckForEmptyString(newConfiguration.AETitle, "AETitle");
            Platform.CheckArgumentRange(newConfiguration.Port, 1, 65535, "Port");

            //Trim the strings before saving.
            newConfiguration.AETitle = newConfiguration.AETitle.Trim();
            if (!String.IsNullOrEmpty(newConfiguration.HostName))
                newConfiguration.HostName = newConfiguration.HostName.Trim();

            var settings = new DicomServerSettings();
            var proxy = settings.GetProxy();
            proxy.AETitle = newConfiguration.AETitle;
            proxy.HostName = newConfiguration.HostName;
            proxy.Port = newConfiguration.Port;
            proxy.Save();

            // TODO (Marmot): While it doesn't do any harm to do this here, the listener should also poll periodically for configuration changes, just in case.
            // TODO (CR Jun 2012): Also, now that we have settings classes, this suddenly becomes more important, especially if this class gets removed,
            // or is no longer used.
            try
            {
                DicomServer.RestartListener();
            }
            catch (EndpointNotFoundException)
            {
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Warn, e, "Failed to restart the DICOM Server listener.");
                throw;
            }
            
            return settings.GetBasicConfiguration();
        }

        public DicomServerExtendedConfiguration UpdateExtendedConfiguration(DicomServerExtendedConfiguration newConfiguration)
        {
            var settings = new DicomServerSettings();
            var proxy = settings.GetProxy();
            proxy.AllowUnknownCaller = newConfiguration.AllowUnknownCaller;
            proxy.QueryResponsesInUtf8 = newConfiguration.QueryResponsesInUtf8;
            // TODO (CR Jun 2012): For now, the storage SOP Classes and Transfer Syntaxes are ignored.
            
            return settings.GetExtendedConfiguration();
        }

	    #region IMigrateSettings Members

		public void MigrateSettingsProperty(SettingsPropertyMigrationValues migrationValues)
		{
			switch (migrationValues.PropertyName)
			{
				case "HostName":
				case "AETitle":
				case "Port":
				case "AllowUnknownCaller":
				case "QueryResponsesInUtf8":
					migrationValues.CurrentValue = migrationValues.PreviousValue;
					break;
				default: //Don't migrate the storage sop classes or transfer syntaxes
					break;
			}
		}

		#endregion
	}

	#region Custom Configuration classes

	[XmlType("SopClass")]
	public class SopClassConfigurationElement : IEquatable<SopClassConfigurationElement>
	{
		public SopClassConfigurationElement() {}

		public SopClassConfigurationElement(string uid, string description)
		{
			Uid = uid;
			Description = description;
		}

		[XmlAttribute("Uid")]
		public string Uid { get; set; }

		[XmlAttribute("Description")]
		public string Description { get; set; }

		public bool Equals(SopClassConfigurationElement other)
		{
			return other != null && Uid == other.Uid;
		}

		public override bool Equals(object obj)
		{
			return obj is SopClassConfigurationElement && Equals((SopClassConfigurationElement) obj);
		}

		public override int GetHashCode()
		{
			return 0x444CB7C9 ^ (Uid != null ? Uid.GetHashCode() : 0);
		}

		public override string ToString()
		{
			return string.Format(@"{0}={1}", Uid, Description);
		}
	}

	[XmlType("ImageSopClassCollection")]
	public class ImageSopClassConfigurationElementCollection : SopClassConfigurationElementCollection {}

	[XmlType("NonImageSopClassCollection")]
	public class NonImageSopClassConfigurationElementCollection : SopClassConfigurationElementCollection {}

	[XmlType("SopClassCollection")]
	public abstract class SopClassConfigurationElementCollection : ConfigurationElementCollection<SopClassConfigurationElement>
	{
		[XmlArray(@"SopClasses")]
		public SopClassConfigurationElement[] SopClasses
		{
			get { return Items; }
			set { Items = value; }
		}
	}

	[XmlType("TransferSyntax")]
	public class TransferSyntaxConfigurationElement : IEquatable<TransferSyntaxConfigurationElement>
	{
		public TransferSyntaxConfigurationElement() {}

		public TransferSyntaxConfigurationElement(string uid, string description)
		{
			Uid = uid;
			Description = description;
		}

		[XmlAttribute("Uid")]
		public string Uid { get; set; }

		[XmlAttribute("Description")]
		public string Description { get; set; }

		public bool Equals(TransferSyntaxConfigurationElement other)
		{
			return other != null && Uid == other.Uid;
		}

		public override bool Equals(object obj)
		{
			return obj is TransferSyntaxConfigurationElement && Equals((TransferSyntaxConfigurationElement) obj);
		}

		public override int GetHashCode()
		{
			return 0x0898858D ^ (Uid != null ? Uid.GetHashCode() : 0);
		}

		public override string ToString()
		{
			return string.Format(@"{0}={1}", Uid, Description);
		}
	}

	[XmlType("TransferSyntaxCollection")]
	public class TransferSyntaxConfigurationElementCollection : ConfigurationElementCollection<TransferSyntaxConfigurationElement>
	{
		[XmlArray(@"TransferSyntaxes")]
		public TransferSyntaxConfigurationElement[] TransferSyntaxes
		{
			get { return Items; }
			set { Items = value; }
		}
	}

	public abstract class ConfigurationElementCollection<T> : IList<T>
	{
		private readonly List<T> _items = new List<T>();

		protected T[] Items
		{
			get { return _items.ToArray(); }
			set
			{
				_items.Clear();
				if (value != null) _items.AddRange(value);
			}
		}

		public override string ToString()
		{
			return '{' + string.Join(@", ", _items.Select(i => i.ToString()).ToArray()) + '}';
		}

		#region Implementation of IList<T>

		public T this[int index]
		{
			get { return _items[index]; }
			set { _items[index] = value; }
		}

		public int Count
		{
			get { return _items.Count; }
		}

		bool ICollection<T>.IsReadOnly
		{
			get { return false; }
		}

		public void Add(T item)
		{
			_items.Add(item);
		}

		public void Clear()
		{
			_items.Clear();
		}

		public bool Contains(T item)
		{
			return _items.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_items.CopyTo(array, arrayIndex);
		}

		public int IndexOf(T item)
		{
			return _items.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			_items.Insert(index, item);
		}

		public bool Remove(T item)
		{
			return _items.Remove(item);
		}

		public void RemoveAt(int index)
		{
			_items.RemoveAt(index);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}

	#endregion
}