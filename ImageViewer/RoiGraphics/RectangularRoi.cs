﻿#region License

// Copyright (c) 2009, ClearCanvas Inc.
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

using System;
using System.Drawing;
using ClearCanvas.ImageViewer.Graphics;
using ClearCanvas.ImageViewer.StudyManagement;

namespace ClearCanvas.ImageViewer.RoiGraphics
{
	/// <summary>
	/// Represents a rectangular region of interest.
	/// </summary>
	public class RectangularRoi : Roi, IRoiAreaProvider, IRoiStatisticsProvider
	{
		private readonly RectangleF _rectangle;

		/// <summary>
		/// Constructs a new rectangular region of interest, specifying an <see cref="IBoundableGraphic"/> as the source of the definition and pixel data.
		/// </summary>
		/// <param name="rectangle">The rectangular graphic that represents the region of interest.</param>
		public RectangularRoi(IBoundableGraphic rectangle) : base(rectangle.ParentPresentationImage)
		{
			rectangle.CoordinateSystem = CoordinateSystem.Source;
			try
			{
				// we need a normalized rectangle, and rectangles can sometimes
				// be defined negatively (negative width or height)
				_rectangle = rectangle.BoundingBox;
			}
			finally
			{
				rectangle.ResetCoordinateSystem();
			}
		}

		/// <summary>
		/// Constructs a new rectangular region of interest.
		/// </summary>
		/// <param name="rectangle">The region of interest.</param>
		/// <param name="presentationImage">The image containing the source pixel data.</param>
		public RectangularRoi(RectangleF rectangle, IPresentationImage presentationImage) : base(presentationImage)
		{
			_rectangle = rectangle;
		}

		/// <summary>
		/// Gets the region of interest.
		/// </summary>
		public RectangleF Rectangle
		{
			get { return _rectangle; }
		}

		/// <summary>
		/// Called by <see cref="Roi.BoundingBox"/> to compute the tightest bounding box of the region of interest.
		/// </summary>
		/// <remarks>
		/// <para>This method is only called once and the result is cached for future accesses.</para>
		/// <para>
		/// Regions of interest have no notion of coordinate system. All coordinates are inherently
		/// given relative to the image pixel space (i.e. <see cref="CoordinateSystem.Source"/>.)
		/// </para>
		/// </remarks>
		/// <returns>A rectangle defining the bounding box.</returns>
		protected override RectangleF ComputeBounds()
		{
			return _rectangle;
		}

		/// <summary>
		/// Creates a copy of this <see cref="RectangularRoi"/> using the same region of interest shape but using a different image as the source pixel data.
		/// </summary>
		/// <param name="presentationImage">The image upon which to copy this region of interest.</param>
		/// <returns>A new <see cref="RectangularRoi"/> of the same shape as the current region of interest.</returns>
		public override Roi CopyTo(IPresentationImage presentationImage)
		{
			return new RectangularRoi(_rectangle, presentationImage);
		}

		/// <summary>
		/// Tests to see if the given point is contained within the region of interest.
		/// </summary>
		/// <remarks>
		/// Regions of interest have no notion of coordinate system. All coordinates are inherently
		/// given relative to the image pixel space (i.e. <see cref="CoordinateSystem.Source"/>.)
		/// </remarks>
		/// <param name="point">The point to test.</param>
		/// <returns>True if the point is defined as within the region of interest; False otherwise.</returns>
		public override bool Contains(PointF point)
		{
			return _rectangle.Contains(point);
		}

		/// <summary>
		/// Gets a value indicating that the image has pixel spacing information or has
		/// previously been calibrated, and hence the <see cref="IRoiAreaProvider.Area"/> property is available.
		/// </summary>
		public bool IsCalibrated
		{
			get { return !base.NormalizedPixelSpacing.IsNull; }
		}

		#region IRoiStatisticsProvider Members

		private RoiStatistics _statistics;

		/// <summary>
		/// Gets the standard deviation of the values over the <see cref="Roi"/>.
		/// </summary>
		public double StandardDeviation
		{
			get
			{
				if (_statistics == null)
				{
					_statistics = RoiStatistics.Calculate(this);
				}
				return _statistics.StandardDeviation;
			}
		}

		/// <summary>
		/// Gets the mean of the values over the <see cref="Roi"/>.
		/// </summary>
		public double Mean
		{
			get
			{
				if (_statistics == null)
				{
					_statistics = RoiStatistics.Calculate(this);
				}
				return _statistics.Mean;
			}
		}

		#endregion

		#region IRoiAreaProvider Members

		private double? _pixelArea;
		private double? _area;

		/// <summary>
		/// Gets the area of the <see cref="Roi"/> in square pixels.
		/// </summary>
		public double PixelArea
		{
			get
			{
				if (!_pixelArea.HasValue)
				{
					_pixelArea = Math.Abs(_rectangle.Width*_rectangle.Height);
				}
				return _pixelArea.Value;
			}
		}

		/// <summary>
		/// Gets the area of the <see cref="Roi"/> in square millimetres.
		/// </summary>
		/// <exception cref="UncalibratedImageException">If the image has no pixel spacing
		/// information and has not been calibrated.</exception>
		public double Area
		{
			get
			{
				if (!_area.HasValue)
				{
					if (base.NormalizedPixelSpacing.IsNull)
						throw new UncalibratedImageException();

					_area = this.PixelArea*base.NormalizedPixelSpacing.Column*base.NormalizedPixelSpacing.Row;
				}
				return _area.Value;
			}
		}

		#endregion
	}
}