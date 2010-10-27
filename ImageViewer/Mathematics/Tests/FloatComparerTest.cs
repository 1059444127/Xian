#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

#if	UNIT_TESTS

#pragma warning disable 1591,0419,1574,1587

using NUnit.Framework;

namespace ClearCanvas.ImageViewer.Mathematics.Tests
{
	[TestFixture]
	public class FloatComparerTest
	{
		public FloatComparerTest()
		{
		}

		[TestFixtureSetUp]
		public void Init()
		{
		}
		
		[TestFixtureTearDown]
		public void Cleanup()
		{
		}

		[Test]
		public void AreEqual()
		{
			float x = 1.0f;
			float y = 2.0f;
			float z = x + y;

			Assert.IsTrue(FloatComparer.AreEqual(z, 3.0f));
		}

		[Test]
		public void IsGreaterThan()
		{
			float x = 1.001f;
			float y = 1.0f;

			Assert.IsTrue(FloatComparer.IsGreaterThan(x, y));
		}

		[Test]
		public void IsLessThan()
		{
			float x = 1.001f;
			float y = 1.0f;

			Assert.IsTrue(FloatComparer.IsLessThan(y, x));
		}
	}
}

#endif