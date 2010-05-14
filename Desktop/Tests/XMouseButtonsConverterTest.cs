﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met:
//
//    * Redistributions of source code must retain the above copyright notice, 
//      this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, 
//      this list of conditions and the following disclaimer in the documentation 
//      and/or other materials provided with the distribution.
//    * Neither the name of ClearCanvas Inc. nor the names of its contributors 
//      may be used to endorse or promote products derived from this software without 
//      specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR 
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
// OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
// ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY 
// OF SUCH DAMAGE.

#endregion

#if	UNIT_TESTS
#pragma warning disable 1591,0419,1574,1587

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using NUnit.Framework;

namespace ClearCanvas.Desktop.Tests
{
	[TestFixture]
	public class XMouseButtonsConverterTest
	{
		// these should be any two unique, non-invariant cultures
		private readonly CultureInfo _dummyCulture = CultureInfo.GetCultureInfo("en-us");
		private readonly CultureInfo _dummyCulture2 = CultureInfo.GetCultureInfo("en-ca");
		private TypeConverter _converter;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			if (_dummyCulture == null)
				throw new Exception("Error setting up test - dummyCulture should not be NULL");
			if (CultureInfo.InvariantCulture.Equals(_dummyCulture))
				throw new Exception("Error setting up test - dummyCulture should not be invariant");
			if (_dummyCulture2 == null)
				throw new Exception("Error setting up test - dummyCulture2 should not be NULL");
			if (CultureInfo.InvariantCulture.Equals(_dummyCulture2))
				throw new Exception("Error setting up test - dummyCulture2 should not be invariant");
			if (_dummyCulture2.Equals(_dummyCulture))
				throw new Exception("Error setting up test - dummyCulture2 should not be the same as dummyCulture");

			// for testing purposes, set up the converter for a specific culture to have a custom mapping
			// normally, you would use TypeDescriptor.GetConverter, but we want to keep the test appdomain clean of these testing mods
			XMouseButtonsConverter converter = new XMouseButtonsConverter(_dummyCulture);
			IDictionary<XMouseButtons, string> relocalizedNames = new Dictionary<XMouseButtons, string>();
			relocalizedNames[XMouseButtons.Left] = "LMouse";
			relocalizedNames[XMouseButtons.Right] = "RMouse";
			relocalizedNames[XMouseButtons.Middle] = "Mouse3";
			relocalizedNames[XMouseButtons.XButton1] = "Mouse4";
			relocalizedNames[XMouseButtons.XButton2] = XMouseButtonsConverter.ButtonSeparator.ToString();
			converter.LocalizedNames = relocalizedNames;

			_converter = converter;
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown() {}

		[Test]
		public void TestXMouseButtonsValueUniqueness()
		{
			// this isn't necessarily required, but does make for a better behaved enumeration
			Dictionary<int, XMouseButtons> uniques = new Dictionary<int, XMouseButtons>();
			foreach (XMouseButtons value in Enum.GetValues(typeof (XMouseButtons)))
			{
				Assert.IsFalse(uniques.ContainsKey((int) value), "There should really only be one enumeration value for each button");
				uniques.Add((int) value, value);
			}
		}

		[Test]
		public void TestTypeConverterAttribute()
		{
			// tests that the XMouseButtonsConverter is properly defined on the XMouseButtons type
			Assert.IsAssignableFrom(typeof (XMouseButtonsConverter), TypeDescriptor.GetConverter(typeof (XMouseButtons)),
			                        "XMouseButtons should be marked with a TypeConverterAttribute that specifies XMouseButtonsConverter");
		}

		[Test]
		public void TestConversionTableFallback()
		{
			// tests that a single converter instance can correctly perform the conversions
			// in either its cached culture, the invariant culture, or a third culture altogether
			AssertEquivalency("LMouse", (XMouseButtons.Left), _dummyCulture, "Conversion using cached culture");
			AssertEquivalency("Left Mouse Button", (XMouseButtons.Left), CultureInfo.InvariantCulture, "Conversion using invariant culture fallback");
			AssertEquivalency("Left Mouse Button", (XMouseButtons.Left), _dummyCulture2, "Conversion using different culture fallback");
		}

		[Test]
		public void TestEmptyStringParse()
		{
			// empty strings should parse to XMouseButtons.None
			XMouseButtons buttonCode = (XMouseButtons) _converter.ConvertFromString(string.Empty);
			Assert.AreEqual(XMouseButtons.None, buttonCode, "empty strings should parse to XMouseButtons.None");
		}

		[Test]
		public void TestStringParseOrderIndifference()
		{
			// ordering of individual buttons in the string should not matter
			XMouseButtons buttonCode = (XMouseButtons) _converter.ConvertFromString("Left Mouse Button+Middle Mouse Button+Right Mouse Button");
			Assert.AreEqual(XMouseButtons.Left | XMouseButtons.Right | XMouseButtons.Middle, buttonCode, "Order of button string elements should not matter");

			buttonCode = (XMouseButtons) _converter.ConvertFromString("Left Mouse Button+Right Mouse Button+Middle Mouse Button");
			Assert.AreEqual(XMouseButtons.Left | XMouseButtons.Right | XMouseButtons.Middle, buttonCode, "Order of button string elements should not matter");

			buttonCode = (XMouseButtons) _converter.ConvertFromString("Middle Mouse Button+Left Mouse Button+Right Mouse Button");
			Assert.AreEqual(XMouseButtons.Left | XMouseButtons.Right | XMouseButtons.Middle, buttonCode, "Order of button string elements should not matter");
		}

		[Test]
		public void TestStringParseWhitespaceIndifference()
		{
			// whitespace between individual buttons in the string should not matter
			XMouseButtons buttonCode = (XMouseButtons) _converter.ConvertFromString("Left Mouse Button\t +Middle Mouse Button \t+Right Mouse Button");
			Assert.AreEqual(XMouseButtons.Left | XMouseButtons.Right | XMouseButtons.Middle, buttonCode, "Whitespace between button string elements should not matter");

			buttonCode = (XMouseButtons) _converter.ConvertFromString("Left Mouse Button+ \tRight Mouse Button+ Middle Mouse Button");
			Assert.AreEqual(XMouseButtons.Left | XMouseButtons.Right | XMouseButtons.Middle, buttonCode, "Whitespace between button string elements should not matter");

			buttonCode = (XMouseButtons) _converter.ConvertFromString("Middle Mouse Button + \t Left Mouse Button\t+  Right Mouse Button");
			Assert.AreEqual(XMouseButtons.Left | XMouseButtons.Right | XMouseButtons.Middle, buttonCode, "Whitespace between button string elements should not matter");

			buttonCode = (XMouseButtons) _converter.ConvertFromString("\t  Middle Mouse Button  ");
			Assert.AreEqual(XMouseButtons.Middle, buttonCode, "Whitespace between button string elements should not matter");
		}

		[Test]
		[ExpectedException(typeof (FormatException))]
		public void TestInvalidStringParse()
		{
			// on the other hand, invalid button strings should throw exceptions during conversion
			XMouseButtons buttonCode = (XMouseButtons) _converter.ConvertFromString("\n");
			Assert.Fail("Expected an exception because the parsed string has an invalid character");
		}

		[Test]
		[ExpectedException(typeof (FormatException))]
		public void TestInvalidButtonStringParse()
		{
			// on the other hand, invalid button strings should throw exceptions during conversion
			XMouseButtons buttonCode = (XMouseButtons) _converter.ConvertFromString("NonExistentButton");
			Assert.Fail("Expected an exception because the parsed string has an invalid button name");
		}

		[Test]
		[ExpectedException(typeof (FormatException))]
		public void TestInvalidButtonWithNonButtonStringParse()
		{
			// on the other hand, invalid button strings should throw exceptions during conversion
			XMouseButtons buttonCode = (XMouseButtons) _converter.ConvertFromString("LMouse+NonExistentButton");
			Assert.Fail("Expected an exception because the parsed string has an invalid button name");
		}

		[Test]
		[ExpectedException(typeof (FormatException))]
		public void TestInvalidNonButtonWithButtonStringParse()
		{
			// on the other hand, invalid button strings should throw exceptions during conversion
			XMouseButtons buttonCode = (XMouseButtons) _converter.ConvertFromString("NonExistentButton+Right");
			Assert.Fail("Expected an exception because the parsed string has an invalid button name");
		}

		[Test]
		[ExpectedException(typeof (FormatException))]
		public void TestInvalidButtonWithButtonsStringParse()
		{
			// on the other hand, invalid button strings should throw exceptions during conversion
			XMouseButtons buttonCode = (XMouseButtons) _converter.ConvertFromString("LMouse+NonExistentButton+Right");
			Assert.Fail("Expected an exception because the parsed string has an invalid button name");
		}

		[Test]
		[ExpectedException(typeof (FormatException))]
		public void TestInvalidButtonTrailingSeparatorStringParse()
		{
			// on the other hand, invalid button strings should throw exceptions during conversion
			XMouseButtons buttonCode = (XMouseButtons) _converter.ConvertFromString("LMouse+");
			Assert.Fail("Expected an exception because of a trailing separator in the parse string");
		}

		[Test]
		[ExpectedException(typeof (FormatException))]
		public void TestInvalidButtonLeadingSeparatorStringParse()
		{
			// on the other hand, invalid button strings should throw exceptions during conversion
			XMouseButtons buttonCode = (XMouseButtons) _converter.ConvertFromString("+Right");
			Assert.Fail("Expected an exception because of a leading separator in the parse string");
		}

		[Test]
		public void TestInvariantLocalizationMapping()
		{
			// our invariant fallback should be well behaved (1-to-1 mapping to unique non-null values)
			// localizations of the mapping may do as they will - they will encounter funny parse results, but that's THEIR PROBLEM
			Dictionary<string, XMouseButtons> uniques = new Dictionary<string, XMouseButtons>();
			foreach (KeyValuePair<XMouseButtons, string> pair in XMouseButtonsConverter.InvariantNames)
			{
				if (string.IsNullOrEmpty(pair.Value))
				{
					Assert.Fail("Invariant mapping for {0} should not be NULL", pair.Key);
					break;
				}

				if (uniques.ContainsKey(pair.Value))
				{
					Assert.Fail("Invariant mapping for {0} should be unique (conflicts with existing mapping for {1})", pair.Key, uniques[pair.Value]);
					break;
				}

				uniques.Add(pair.Value, pair.Key);
			}
		}

		[Test]
		public void TestSpecialCase()
		{
			// test special case where the button separator is a button name on its own
			const string message = "Special Case";
			const XMouseButtons button = XMouseButtons.XButton2;
			CultureInfo culture = _dummyCulture;
			string actualButtonName = XMouseButtonsConverter.ButtonSeparator.ToString();
			AssertEquivalency(string.Format("{0}", actualButtonName), (XMouseButtons) button, culture, message);
			AssertEquivalency(string.Format("LMouse+{0}", actualButtonName), XMouseButtons.Left | (XMouseButtons) button, culture, message);
			AssertEquivalency(string.Format("RMouse+{0}", actualButtonName), XMouseButtons.Right | (XMouseButtons) button, culture, message);
			AssertEquivalency(string.Format("LMouse+RMouse+{0}", actualButtonName), XMouseButtons.Left | XMouseButtons.Right | (XMouseButtons) button, culture, message);

			AssertStringParse(string.Format("{0}   ", actualButtonName), (XMouseButtons) button, culture, message);
			AssertStringParse(string.Format("   {0}", actualButtonName), (XMouseButtons) button, culture, message);
			AssertStringParse(string.Format("   {0}   ", actualButtonName), (XMouseButtons) button, culture, message);
			AssertStringParse(string.Format("LMouse    +   {0}", actualButtonName), XMouseButtons.Left | (XMouseButtons) button, culture, message);
			AssertStringParse(string.Format("LMouse+     {0}", actualButtonName), XMouseButtons.Left | (XMouseButtons) button, culture, message);
			AssertStringParse(string.Format("{0}+LMouse", actualButtonName), XMouseButtons.Left | (XMouseButtons) button, culture, message);
			AssertStringParse(string.Format("{0}    +LMouse", actualButtonName), XMouseButtons.Left | (XMouseButtons) button, culture, message);
			AssertStringParse(string.Format("RMouse+{0}+LMouse", actualButtonName), XMouseButtons.Left | XMouseButtons.Right | (XMouseButtons) button, culture, message);
			AssertStringParse(string.Format("RMouse+{0}   +LMouse", actualButtonName), XMouseButtons.Left | XMouseButtons.Right | (XMouseButtons) button, culture, message);
			AssertStringParse(string.Format("RMouse+    {0}+LMouse", actualButtonName), XMouseButtons.Left | XMouseButtons.Right | (XMouseButtons) button, culture, message);
			AssertStringParse(string.Format("RMouse+    {0}   +LMouse", actualButtonName), XMouseButtons.Left | XMouseButtons.Right | (XMouseButtons) button, culture, message);
		}

		[Test]
		public void TestLocalizedButtonCombos()
		{
			// test combining buttons with buttons in the localized case
			const string message = "Localized Combinations";
			CultureInfo culture = _dummyCulture;

			AssertEquivalency(string.Format("LMouse"), XMouseButtons.Left, culture, message);
			AssertEquivalency(string.Format("RMouse"), XMouseButtons.Right, culture, message);
			AssertEquivalency(string.Format("+"), XMouseButtons.XButton2, culture, message);
			AssertEquivalency(string.Format("LMouse+RMouse"), XMouseButtons.Left | XMouseButtons.Right, culture, message);
			AssertEquivalency(string.Format("RMouse++"), XMouseButtons.Right | XMouseButtons.XButton2, culture, message);
			AssertEquivalency(string.Format("LMouse++"), XMouseButtons.Left | XMouseButtons.XButton2, culture, message);
			AssertEquivalency(string.Format("LMouse+RMouse++"), XMouseButtons.Left | XMouseButtons.Right | XMouseButtons.XButton2, culture, message);
		}

		[Test]
		public void TestInvariantButtonCombos()
		{
			// test combining buttons with buttons in the invariant case
			const string message = "Invariant Combinations";
			CultureInfo culture = CultureInfo.InvariantCulture;

			AssertEquivalency(string.Format("Left Mouse Button"), XMouseButtons.Left, culture, message);
			AssertEquivalency(string.Format("Right Mouse Button"), XMouseButtons.Right, culture, message);
			AssertEquivalency(string.Format("Middle Mouse Button"), XMouseButtons.Middle, culture, message);
			AssertEquivalency(string.Format("Left Mouse Button+Right Mouse Button"), XMouseButtons.Left | XMouseButtons.Right, culture, message);
			AssertEquivalency(string.Format("Right Mouse Button+Middle Mouse Button"), XMouseButtons.Right | XMouseButtons.Middle, culture, message);
			AssertEquivalency(string.Format("Left Mouse Button+Middle Mouse Button"), XMouseButtons.Left | XMouseButtons.Middle, culture, message);
			AssertEquivalency(string.Format("Left Mouse Button+Right Mouse Button+Middle Mouse Button"), XMouseButtons.Left | XMouseButtons.Right | XMouseButtons.Middle, culture, message);
		}

		private void AssertEquivalency(string sButtons, XMouseButtons eButtons, CultureInfo culture, string message)
		{
			AssertStringFormat(sButtons, eButtons, culture, message);
			AssertStringParse(sButtons, eButtons, culture, message);
		}

		private void AssertStringFormat(string sButtons, XMouseButtons eButtons, CultureInfo culture, string message)
		{
			string actualString = _converter.ConvertToString(null, culture, eButtons);
			//System.Diagnostics.Trace.WriteLine(actualString);
			Assert.AreEqual(sButtons, actualString, message + ": converting " + (int) eButtons + " which is " + eButtons.ToString());
		}

		private void AssertStringParse(string sButtons, XMouseButtons eButtons, CultureInfo culture, string message)
		{
			XMouseButtons actualEnum = (XMouseButtons) _converter.ConvertFromString(null, culture, sButtons);
			//System.Diagnostics.Trace.WriteLine(actualEnum);
			Assert.AreEqual((int) eButtons, (int) actualEnum, message + ": converting " + sButtons + " which is " + actualEnum.ToString());
		}
	}
}

#endif