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
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using ClearCanvas.Common.Utilities;

namespace ClearCanvas.Desktop.View.WinForms
{
	/// <summary>
	/// Represents a dropdown control from which the user can select a UI locale.
	/// </summary>
	public class LocalePicker : UserControl
	{
		private static readonly Size _defaultSize = new Size(136, 21);
		private event EventHandler _selectedLocaleChanged;
		private readonly ComboBox _dropDown;

		/// <summary>
		/// Initializes a new instance of a <see cref="LocalePicker"/>.
		/// </summary>
		public LocalePicker()
		{
			SuspendLayout();
			try
			{
				_dropDown = new ComboBox {Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, FormattingEnabled = true, Location = new Point(0, 0)};
				_dropDown.SelectedIndexChanged += OnSelectedIndexChanged;

				Name = "LocalePicker";
				Controls.Add(_dropDown);
				Size = _defaultSize;

				base.BackColor = Color.Transparent;
			}
			finally
			{
				ResumeLayout(false);
			}

			AvailableLocales = new List<InstalledLocales.Locale>(InstalledLocales.Instance.Locales).AsReadOnly();
			AvailableCultures = CollectionUtils.Map<InstalledLocales.Locale, CultureInfo>(AvailableLocales, x => x.GetCultureInfo()).AsReadOnly();
			DefaultLocale = InstalledLocales.Instance.Default;
			DefaultCulture = DefaultLocale.GetCultureInfo();

			foreach (var locale in AvailableLocales)
				_dropDown.Items.Add(locale);
			_dropDown.SelectedItem = DefaultLocale;
		}

		protected override Size DefaultSize
		{
			get { return _defaultSize; }
		}

		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore(x, y, width, _dropDown.PreferredHeight, specified);
		}

		/// <summary>
		/// Gets a collection of installed locales.
		/// </summary>
		[Browsable(false)]
		public ICollection<InstalledLocales.Locale> AvailableLocales { get; private set; }

		/// <summary>
		/// Gets a collection of cultures associated with <see cref="AvailableLocales"/>.
		/// </summary>
		[Browsable(false)]
		public ICollection<CultureInfo> AvailableCultures { get; private set; }

		/// <summary>
		/// Gets the default locale.
		/// </summary>
		[Browsable(false)]
		public InstalledLocales.Locale DefaultLocale { get; private set; }

		/// <summary>
		/// Gets the culture associated with <see cref="DefaultLocale"/>.
		/// </summary>
		[Browsable(false)]
		public CultureInfo DefaultCulture { get; private set; }

		/// <summary>
		/// Gets or sets the selected locale.
		/// </summary>
		[Browsable(false)]
		public InstalledLocales.Locale SelectedLocale
		{
			get { return (InstalledLocales.Locale) _dropDown.SelectedItem; }
			set
			{
				if ((InstalledLocales.Locale) _dropDown.SelectedItem != value)
					_dropDown.SelectedItem = value;
			}
		}

		/// <summary>
		/// Gets or sets the culture associated with <see cref="SelectedLocale"/>.
		/// </summary>
		[Browsable(false)]
		public CultureInfo SelectedCulture
		{
			get { return SelectedLocale.GetCultureInfo(); }
			set
			{
				if (value != null)
				{
					InstalledLocales.Locale locale;
					if (TryGetLocaleByName(value.Name, out locale))
						SelectedLocale = locale;
				}
			}
		}

		/// <summary>
		/// Occurs when the value of <see cref="SelectedLocale"/> has changed.
		/// </summary>
		public event EventHandler SelectedLocaleChanged
		{
			add { _selectedLocaleChanged += value; }
			remove { _selectedLocaleChanged -= value; }
		}

		/// <summary>
		/// Called when the value of <see cref="SelectedLocale"/> has changed.
		/// </summary>
		protected virtual void OnSelectedLocaleChanged()
		{
			EventsHelper.Fire(_selectedLocaleChanged, this, EventArgs.Empty);
		}

		public override Size GetPreferredSize(Size proposedSize)
		{
			return _dropDown.GetPreferredSize(proposedSize);
		}

		/// <summary>
		/// Gets the locale associated with the specified culture code.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if a matching locale does not exist.</exception>
		public InstalledLocales.Locale GetLocaleByName(string cultureCode)
		{
			InstalledLocales.Locale locale;
			if (!TryGetLocaleByName(cultureCode, out locale))
				throw new ArgumentOutOfRangeException("cultureCode");
			return locale;
		}

		/// <summary>
		/// Gets the locale associated with the specified culture code.
		/// </summary>
		/// <returns>True if a matching locale exists; False otherwise.</returns>
		public bool TryGetLocaleByName(string cultureCode, out InstalledLocales.Locale result)
		{
			foreach (var locale in AvailableLocales)
			{
				if (string.Equals(locale.Culture, cultureCode, StringComparison.InvariantCultureIgnoreCase))
				{
					result = locale;
					return true;
				}
			}
			result = null;
			return false;
		}

		private void OnSelectedIndexChanged(object sender, EventArgs e)
		{
			var locale = _dropDown.SelectedItem as InstalledLocales.Locale;
			if (locale != null)
				OnSelectedLocaleChanged();
		}
	}
}
