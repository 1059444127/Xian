#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Drawing;
using ClearCanvas.ImageViewer.Imaging;
using ClearCanvas.ImageViewer.RoiGraphics;
using ClearCanvas.ImageViewer.Mathematics;

namespace ClearCanvas.ImageViewer.RoiGraphics
{
	/// <summary>
	/// Defines the properties to get the mean and standard deviation of a
	/// region of interest.
	/// </summary>
	public interface IRoiStatisticsProvider
	{
		/// <summary>
		/// Gets the standard deviation of the values over the <see cref="Roi"/>.
		/// </summary>
		double StandardDeviation { get; }

		/// <summary>
		/// Gets the mean of the values over the <see cref="Roi"/>.
		/// </summary>
		double Mean { get; }
	}

	internal class RoiStatistics
	{
		public readonly double Mean;
		public readonly double StandardDeviation;
		public readonly bool Valid;

		private RoiStatistics()
		{
			this.Valid = false;
		}

		private RoiStatistics(double mean, double stddev)
		{
			this.Valid = true;
			this.Mean = mean;
			this.StandardDeviation = stddev;
		}

		private delegate bool IsPointInRoiDelegate(int x, int y);

		public static RoiStatistics Calculate(Roi roi)
		{
			if (!(roi.PixelData is GrayscalePixelData))
				return new RoiStatistics();

			double mean = CalculateMean(
				roi.BoundingBox, 
				(GrayscalePixelData)roi.PixelData, 
				roi.ModalityLut, 
				roi.Contains);

			double stdDev = CalculateStandardDeviation(
				mean, 
				roi.BoundingBox, 
				(GrayscalePixelData)roi.PixelData, 
				roi.ModalityLut, 
				roi.Contains);

			return new RoiStatistics(mean, stdDev);
		}

		private static double CalculateMean
			(
			RectangleF roiBoundingBox,
			GrayscalePixelData pixelData,
			IModalityLut modalityLut,
			IsPointInRoiDelegate isPointInRoi
			)
		{
			double sum = 0;
			int pixelCount = 0;

            var boundingBox = RectangleUtilities.RoundInflate(RectangleUtilities.ConvertToPositiveRectangle(roiBoundingBox));
			pixelData.ForEachPixel(
                boundingBox.Left,
                boundingBox.Top,
                boundingBox.Right,
                boundingBox.Bottom,
				delegate(int i, int x, int y, int pixelIndex)
					{
						if (isPointInRoi(x, y))
						{
							++pixelCount;
							// Make sure we run the raw pixel through the modality LUT
							// when doing the calculation. Note that the modality LUT
							// can be something other than a rescale intercept, so we can't
							// just run the mean through the LUT.
							int storedValue = pixelData.GetPixel(pixelIndex);
							double realValue = modalityLut != null ? modalityLut[storedValue] : storedValue;
							sum += realValue;
						}
					});

			if (pixelCount == 0)
				return 0;

			return sum/pixelCount;
		}

		private static double CalculateStandardDeviation
			(
			double mean,
			RectangleF roiBoundingBox,
			GrayscalePixelData pixelData,
			IModalityLut modalityLut,
			IsPointInRoiDelegate isPointInRoi
			)
		{
			double sum = 0;
			int pixelCount = 0;

            var boundingBox = RectangleUtilities.RoundInflate(RectangleUtilities.ConvertToPositiveRectangle(roiBoundingBox));
            pixelData.ForEachPixel(
                boundingBox.Left,
                boundingBox.Top,
                boundingBox.Right,
                boundingBox.Bottom,
                delegate(int i, int x, int y, int pixelIndex)
                {
					if (isPointInRoi(x, y)) {
						++pixelCount;
						int storedValue = pixelData.GetPixel(pixelIndex);
						double realValue = modalityLut != null ? modalityLut[storedValue] : storedValue;

						double deviation = realValue - mean;
						sum += deviation*deviation;
					}
				});

			if (pixelCount == 0)
				return 0;

			return Math.Sqrt(sum/pixelCount);
		}
	}
}