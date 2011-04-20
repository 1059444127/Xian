﻿#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using ClearCanvas.Common;
using ClearCanvas.Common.Configuration;

namespace ClearCanvas.Desktop
{
	[SettingsGroupDescription("Configures the installed localizations available for use in the application.")]
	[SettingsProvider(typeof (StandardSettingsProvider))]
	internal sealed partial class LocaleSettings
	{
		public LocaleSettings()
		{
			ApplicationSettingsRegistry.Instance.RegisterInstance(this);
		}
	}

	/// <summary>
	/// Represents the collection of installed localizations available for use in the application.
	/// </summary>
	public sealed class InstalledLocales : IXmlSerializable
	{
		private const string _elementLocale = "Locale";
		private const string _attributeCulture = "Culture";
		private const string _attributeDisplayName = "DisplayName";

		private readonly List<Locale> _installedLocales = new List<Locale>();

		/// <summary>
		/// Gets a <see cref="Locale"/> representing the default invariant locale.
		/// </summary>
		public static readonly Locale InvariantLocale = new Locale(string.Empty, "English");

		/// <summary>
		/// Gets the default instance of <see cref="InstalledLocales"/> from the application settings.
		/// </summary>
		public static InstalledLocales Default
		{
			get { return LocaleSettings.Default.InstalledLocales ?? new InstalledLocales(); }
		}

		/// <summary>
		/// Gets the count of installed localizations other than the <see cref="InvariantLocale"/>.
		/// </summary>
		public int Count
		{
			get { return _installedLocales.Count; }
		}

		/// <summary>
		/// Enumerates the installed localizations.
		/// </summary>
		/// <param name="includeInvariantLocale">A value indicating whether or not the <see cref="InvariantLocale"/> should be included in the enumeration.</param>
		public IEnumerable<Locale> Enumerate(bool includeInvariantLocale)
		{
			if (includeInvariantLocale)
				yield return InvariantLocale;
			foreach (var locale in _installedLocales)
				yield return locale;
		}

		#region IXmlSerializable Members

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			if (reader.MoveToContent() != XmlNodeType.Element)
				throw new InvalidOperationException();

			var emptyCollection = reader.IsEmptyElement; // check if container element is empty
			var locales = new List<Locale>();

			reader.ReadStartElement(); // consume the container element tag
			if (!emptyCollection)
			{
				// children of container should each be elements representing individual collection items
				// at the end of this loop, we must be positioned past each item, whether an end element or an empty element tag
				while (reader.MoveToContent() == XmlNodeType.Element)
				{
					var emptyItem = reader.IsEmptyElement;
					var locale = (Locale) null;

					if (reader.Name == _elementLocale)
					{
						try
						{
							var culture = reader.GetAttribute(_attributeCulture);
							var displayName = reader.GetAttribute(_attributeDisplayName);
							locale = new Locale(culture, displayName);
						}
						catch (Exception ex)
						{
							Platform.Log(LogLevel.Debug, ex, "Invalid installed locale definition");
						}

						// consume the entire element
						if (!emptyItem)
						{
							reader.ReadSubtree().Close(); // read start tag and all descendants
							reader.ReadEndElement(); // read end tag
						}
						else
						{
							reader.ReadStartElement(); // read empty element tag
						}
					}
					else
					{
						// if the child element is unrecognized, we still have to consume it!
						//  otherwise we will not be at the right position to pick up the next item
						if (!emptyItem)
						{
							reader.ReadSubtree().Close(); // read start tag and all descendants
							reader.ReadEndElement(); // read end tag
						}
						else
						{
							reader.ReadStartElement(); // read empty element tag
						}
					}

					if (locale != null)
						locales.Add(locale);
				}
			}

			_installedLocales.Clear();
			_installedLocales.AddRange(locales);
			_installedLocales.Sort((x, y) => string.Compare(x.DisplayName, y.DisplayName, StringComparison.InvariantCultureIgnoreCase));
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			foreach (var locale in _installedLocales)
			{
				writer.WriteStartElement(_elementLocale);
				writer.WriteAttributeString(_attributeCulture, locale.Culture);
				writer.WriteAttributeString(_attributeDisplayName, locale.DisplayName);
				writer.WriteEndElement();
			}
		}

		XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		#endregion

		#region Locale Class

		/// <summary>
		/// Represents a single installed localization.
		/// </summary>
		public sealed class Locale : IEquatable<Locale>
		{
			/// <summary>
			/// Gets the culture code of the localization.
			/// </summary>
			public readonly string Culture;

			/// <summary>
			/// Gets the name of the localization in the localization's own language and script.
			/// </summary>
			public readonly string Name;

			/// <summary>
			/// Gets the display name of the localization.
			/// </summary>
			/// <remarks>
			/// This value is equal to <see cref="Name"/> by default, but can be overridden in the settings.
			/// </remarks>
			public readonly string DisplayName;

			/// <summary>
			/// Gets the name of the localization in the invariant language (i.e. English).
			/// </summary>
			public readonly string InvariantName;

			internal Locale(string culture, string displayName)
			{
				var cultureInfo = CultureInfo.GetCultureInfo(culture ?? string.Empty);
				Culture = culture;
				Name = cultureInfo.NativeName;
				InvariantName = cultureInfo.EnglishName;
				DisplayName = !string.IsNullOrEmpty(displayName) ? displayName : Name;
			}

			/// <summary>
			/// Gets a <see cref="CultureInfo"/> representing this locale.
			/// </summary>
			public CultureInfo GetCultureInfo()
			{
				return CultureInfo.GetCultureInfo(Culture);
			}

			public override int GetHashCode()
			{
				return 0x0F5E0071 ^ Culture.GetHashCode();
			}

			/// <summary>
			/// Checks if this locale is equivalent to <paramref name="object"/>.
			/// </summary>
			public override bool Equals(object @object)
			{
				if (@object is Locale)
					return Equals((Locale) @object);
				return base.Equals(@object);
			}

			/// <summary>
			/// Checks if this locale is equivalent to <paramref name="other"/>.
			/// </summary>
			public bool Equals(Locale other)
			{
				if (other == null)
					return false;
				return string.Equals(Culture, other.Culture, StringComparison.InvariantCultureIgnoreCase);
			}

			/// <summary>
			/// Checks if the locales <paramref name="x"/> and <paramref name="y"/> are equivalent.
			/// </summary>
			public static bool operator ==(Locale x, Locale y)
			{
				if (!ReferenceEquals(x, null))
					return x.Equals(y);
				return ReferenceEquals(y, null) || y.Equals(x);
			}

			/// <summary>
			/// Checks if the locales <paramref name="x"/> and <paramref name="y"/> are not equivalent.
			/// </summary>
			public static bool operator !=(Locale x, Locale y)
			{
				return !(x == y);
			}
		}

		#endregion
	}
}