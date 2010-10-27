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

using System;
using System.Collections.Generic;
using ClearCanvas.ImageViewer.Mathematics;

namespace ClearCanvas.ImageViewer.Tests
{
	/// <summary>
	/// Numerical statistic cruncher.
	/// </summary>
	public class Statistics
	{
		public readonly double Mean;
		public readonly double StandardDeviation;
		public readonly double MaxValue;
		public readonly double MinValue;

		public Statistics(IEnumerable<int> data) : this(Enumerate(data, v => v)) {}
		public Statistics(IEnumerable<uint> data) : this(Enumerate(data, v => v)) {}
		public Statistics(IEnumerable<long> data) : this(Enumerate(data, v => v)) {}
		public Statistics(IEnumerable<ulong> data) : this(Enumerate(data, v => v)) {}
		public Statistics(IEnumerable<short> data) : this(Enumerate(data, v => v)) {}
		public Statistics(IEnumerable<ushort> data) : this(Enumerate(data, v => v)) {}
		public Statistics(IEnumerable<byte> data) : this(Enumerate(data, v => v)) {}
		public Statistics(IEnumerable<sbyte> data) : this(Enumerate(data, v => v)) {}
		public Statistics(IEnumerable<float> data) : this(Enumerate(data, v => v)) {}
		public Statistics(IEnumerable<decimal> data) : this(Enumerate(data, v => decimal.ToDouble(v))) {}

		public Statistics(IEnumerable<double> data)
		{
			this.Mean = CalculateMean(data);
			this.StandardDeviation = CalculateStandardDeviation(this.Mean, data);
			ComputeRange(data, out this.MinValue, out this.MaxValue);
		}

		public bool IsEqualTo(double value)
		{
			if (FloatComparer.AreEqual((float) this.Mean, (float) value))
			{
				if (FloatComparer.AreEqual((float) this.StandardDeviation, 0))
					return true;
			}
			return false;
		}

		public bool IsEqualTo(double value, double tolerance)
		{
			if (FloatComparer.AreEqual((float) this.Mean, (float) value, (float) tolerance))
			{
				if (FloatComparer.AreEqual((float) this.StandardDeviation, 0, (float) tolerance))
					return true;
			}
			return false;
		}

		public override string ToString()
		{
			return string.Format("MIN={2:f3}  MAX={3:f3}  \u03BC={0:f3}  \u03C3={1:f3}", this.Mean, this.StandardDeviation, this.MinValue, this.MaxValue);
		}

		private static IEnumerable<double> Enumerate<T>(IEnumerable<T> data, Converter<T, double> converter)
			where T : struct
		{
			foreach (var value in data)
				yield return converter.Invoke(value);
		}

		private static void ComputeRange(IEnumerable<double> data, out double min, out double max)
		{
			bool atLeastOne = false;
			min = double.MaxValue;
			max = double.MinValue;
			foreach (double value in data)
			{
				atLeastOne = true;
				min = Math.Min(value, min);
				max = Math.Max(value, max);
			}
			if (!atLeastOne)
				min = max = double.NaN;
		}

		private static double CalculateMean(IEnumerable<double> data)
		{
			double sum = 0;
			int count = 0;

			foreach (double value in data)
			{
				count++;
				sum += value;
			}

			if (count == 0)
				return 0;

			return sum/count;
		}

		private static double CalculateStandardDeviation(double mean, IEnumerable<double> data)
		{
			double sum = 0;
			int count = 0;

			foreach (double value in data)
			{
				count++;
				double deviation = value - mean;
				sum += deviation*deviation;
			}

			if (count == 0)
				return 0;

			return Math.Sqrt(sum/count);
		}
	}
}

#endif