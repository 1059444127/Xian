#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

#if	UNIT_TESTS
#pragma warning disable 1591,0419,1574,1587

using System;
using NUnit.Framework;

namespace ClearCanvas.ImageViewer.Imaging.Tests
{
	[TestFixture]
	public class VoiLutLinearTests
	{
		[Test]
		public void BasicTest()
		{
			const double windowWidth = 4096;
			const double windowLevel = 0;

			BasicVoiLutLinear lut = new BasicVoiLutLinear();
			lut.MinInputValue = -2048;
			lut.MaxInputValue = 2047;

			lut.WindowWidth = windowWidth;
			lut.WindowCenter = windowLevel;

			Assert.AreEqual(-2048, lut[-2048]);
			Assert.AreEqual(0, lut[0]);
			Assert.AreEqual(2047, lut[2047]);
		}

		[Test]
		public void AlterWindowLevel()
		{
			const double windowWidth = 4096;
			const double windowLevel = 2047;

			BasicVoiLutLinear lut = new BasicVoiLutLinear();
			lut.MinInputValue = 0;
			lut.MaxInputValue = 4095;

			lut.WindowWidth = windowWidth;
			lut.WindowCenter = windowLevel;

			Assert.AreEqual(1, lut[0]);
			Assert.AreEqual(2048, lut[2047]);
			Assert.AreEqual(4095, lut[4095]);

			lut.WindowWidth = 512;
			lut.WindowCenter = 1023;

			Assert.AreEqual(512, lut.WindowWidth);
			Assert.AreEqual(1023, lut.WindowCenter);
			Assert.AreEqual(0, lut[767]);
			Assert.AreEqual(4095, lut[1279]);
		}

		[Test]
		public void Threshold()
		{
			const double windowWidth = 1;
			const double windowLevel = 0;

			BasicVoiLutLinear lut = new BasicVoiLutLinear();
			lut.MinInputValue = -2048;
			lut.MaxInputValue = 2047;

			lut.WindowWidth = windowWidth;
			lut.WindowCenter = windowLevel;

			Assert.AreEqual(-2048, lut[-2]);
			Assert.AreEqual(-2048, lut[-1]);
			Assert.AreEqual(2047, lut[0]);
			Assert.AreEqual(2047, lut[1]);
		}

		[Test(Description = "Compares the VOI LUT output against the first example provided in DICOM 2011 PS 3.3 Section C.11.2.1.2 Note 3")]
		public void TestDicomExampleA()
		{
			const double windowCentre = 2048;
			const double windowWidth = 4096;

			var lut = new BasicVoiLutLinear(windowWidth, windowCentre) {MinInputValue = short.MinValue, MaxInputValue = short.MaxValue};
			Assert.AreEqual((int) lut.MinInputValue, lut.MinOutputValue, "LUT output range min");
			Assert.AreEqual((int) lut.MaxInputValue, lut.MaxOutputValue, "LUT output range max");

			Assert.AreEqual(DicomReferenceFunction(0 - 1e-6, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[0 - 1e-6], "LUT output for value X < C-0.5 - (W-1)/2");
			Assert.AreEqual(DicomReferenceFunction(0, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[0], "LUT output for value X == C-0.5 - (W-1)/2");

			Assert.AreEqual(DicomReferenceFunction(4095 + 1e-6, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[4095 + 1e-6], "LUT output for value X > C-0.5 + (W-1)/2");

			Assert.AreEqual(DicomReferenceFunction(0.1, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[0.1], "LUT output for value between C-0.5 - (W-1)/2 and C-0.5 + (W-1)/2");
			Assert.AreEqual(DicomReferenceFunction(1, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[1], "LUT output for value between C-0.5 - (W-1)/2 and C-0.5 + (W-1)/2");
			Assert.AreEqual(DicomReferenceFunction(5, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[5], "LUT output for value between C-0.5 - (W-1)/2 and C-0.5 + (W-1)/2");
			Assert.AreEqual(DicomReferenceFunction(2048, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[2048], "LUT output for value between C-0.5 - (W-1)/2 and C-0.5 + (W-1)/2");
			Assert.AreEqual(DicomReferenceFunction(4090, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[4090], "LUT output for value between C-0.5 - (W-1)/2 and C-0.5 + (W-1)/2");
			Assert.AreEqual(DicomReferenceFunction(4094.9, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[4094.9], "LUT output for value between C-0.5 - (W-1)/2 and C-0.5 + (W-1)/2");
			Assert.AreEqual(DicomReferenceFunction(4095, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[4095], "LUT output for value between C-0.5 - (W-1)/2 and C-0.5 + (W-1)/2");
		}

		[Test(Description = "Compares the VOI LUT output against the second example provided in DICOM 2011 PS 3.3 Section C.11.2.1.2 Note 3")]
		public void TestDicomExampleB()
		{
			const double windowCentre = 2048;
			const double windowWidth = 1;

			var lut = new BasicVoiLutLinear(windowWidth, windowCentre) {MinInputValue = short.MinValue, MaxInputValue = short.MaxValue};
			Assert.AreEqual((int) lut.MinInputValue, lut.MinOutputValue, "LUT output range min");
			Assert.AreEqual((int) lut.MaxInputValue, lut.MaxOutputValue, "LUT output range max");

			Assert.AreEqual(DicomReferenceFunction(2047.5 - 1e-6, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[2047.5 - 1e-6], "LUT output for value X < C-0.5 - (W-1)/2");
			Assert.AreEqual(DicomReferenceFunction(2047.5, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[2047.5], "LUT output for value X == C-0.5 - (W-1)/2");

			Assert.AreEqual(DicomReferenceFunction(2047.5 + 1e-6, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[2047.5 + 1e-6], "LUT output for value X > C-0.5 + (W-1)/2");
		}

		[Test(Description = "Compares the VOI LUT output against the third example provided in DICOM 2011 PS 3.3 Section C.11.2.1.2 Note 3")]
		public void TestDicomExampleC()
		{
			const double windowCentre = 0;
			const double windowWidth = 100;

			var lut = new BasicVoiLutLinear(windowWidth, windowCentre) {MinInputValue = short.MinValue, MaxInputValue = short.MaxValue};
			Assert.AreEqual((int) lut.MinInputValue, lut.MinOutputValue, "LUT output range min");
			Assert.AreEqual((int) lut.MaxInputValue, lut.MaxOutputValue, "LUT output range max");

			Assert.AreEqual(DicomReferenceFunction(-50 - 1e-6, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[-50 - 1e-6], "LUT output for value X < C-0.5 - (W-1)/2");
			Assert.AreEqual(DicomReferenceFunction(-50, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[-50], "LUT output for value X == C-0.5 - (W-1)/2");

			Assert.AreEqual(DicomReferenceFunction(49 + 1e-6, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[49 + 1e-6], "LUT output for value X > C-0.5 + (W-1)/2");

			Assert.AreEqual(DicomReferenceFunction(-49.9, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[-49.9], "LUT output for value between C-0.5 - (W-1)/2 and C-0.5 + (W-1)/2");
			Assert.AreEqual(DicomReferenceFunction(-49, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[-49], "LUT output for value between C-0.5 - (W-1)/2 and C-0.5 + (W-1)/2");
			Assert.AreEqual(DicomReferenceFunction(-44, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[-44], "LUT output for value between C-0.5 - (W-1)/2 and C-0.5 + (W-1)/2");
			Assert.AreEqual(DicomReferenceFunction(0, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[0], "LUT output for value between C-0.5 - (W-1)/2 and C-0.5 + (W-1)/2");
			Assert.AreEqual(DicomReferenceFunction(43, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[43], "LUT output for value between C-0.5 - (W-1)/2 and C-0.5 + (W-1)/2");
			Assert.AreEqual(DicomReferenceFunction(48.9, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[48.9], "LUT output for value between C-0.5 - (W-1)/2 and C-0.5 + (W-1)/2");
			Assert.AreEqual(DicomReferenceFunction(49, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[49], "LUT output for value between C-0.5 - (W-1)/2 and C-0.5 + (W-1)/2");
		}

		[Test(Description = "Compares the VOI LUT output against the fourth example provided in DICOM 2011 PS 3.3 Section C.11.2.1.2 Note 3")]
		public void TestDicomExampleD()
		{
			const double windowCentre = 0;
			const double windowWidth = 1;

			var lut = new BasicVoiLutLinear(windowWidth, windowCentre) {MinInputValue = short.MinValue, MaxInputValue = short.MaxValue};
			Assert.AreEqual((int) lut.MinInputValue, lut.MinOutputValue, "LUT output range min");
			Assert.AreEqual((int) lut.MaxInputValue, lut.MaxOutputValue, "LUT output range max");

			Assert.AreEqual(DicomReferenceFunction(-0.5 - 1e-6, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[-0.5 - 1e-6], "LUT output for value X < C-0.5 - (W-1)/2");
			Assert.AreEqual(DicomReferenceFunction(-0.5, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[-0.5], "LUT output for value X == C-0.5 - (W-1)/2");

			Assert.AreEqual(DicomReferenceFunction(-0.5 + 1e-6, windowWidth, windowCentre, lut.MinOutputValue, lut.MaxOutputValue), lut[-0.5 + 1e-6], "LUT output for value X > C-0.5 + (W-1)/2");
		}

		/// <summary>
		/// The VOI linear window function as defined in DICOM 2011 PS 3.3 Section C.11.2.1.2 (with output rounded)
		/// </summary>
		private static int DicomReferenceFunction(double value, double windowWidth, double windowCentre, int minOutput, int maxOutput)
		{
			if (value <= windowCentre - 0.5 - (windowWidth - 1)/2)
				return minOutput;
			else if (value > windowCentre - 0.5 + (windowWidth - 1)/2)
				return maxOutput;
			else
				return (int) Math.Round(((value - (windowCentre - 0.5))/(windowWidth - 1) + 0.5)*(maxOutput - minOutput) + minOutput);
		}
	}
}

#endif