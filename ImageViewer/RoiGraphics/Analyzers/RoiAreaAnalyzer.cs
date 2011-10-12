#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using ClearCanvas.Common;
using ClearCanvas.ImageViewer.RoiGraphics;

namespace ClearCanvas.ImageViewer.RoiGraphics.Analyzers
{
	/// <summary>
	/// An <see cref="IRoiAnalyzer"/> that displays the area of a <see cref="RoiGraphic"/>.
	/// </summary>
	[ExtensionOf(typeof (RoiAnalyzerExtensionPoint))]
	public class RoiAreaAnalyzer : IRoiAnalyzer
	{
		private Units _units = Units.Centimeters;
	    private RoiAnalyzerUpdateCallback _updateCallback;

	    /// <summary>
		/// Gets or sets the base unit of measurement in which analysis is performed.
		/// </summary>
		public Units Units
		{
			get { return _units; }
			set { _units = value; }
		}

		/// <summary>
		/// Checks if this analyzer class can analyze the given ROI.
		/// </summary>
		/// <param name="roi">The ROI to analyze.</param>
		/// <returns>True if this class can analyze the given ROI; False otherwise.</returns>
		public bool SupportsRoi(Roi roi)
		{
			return roi is IRoiAreaProvider;
		}

		/// <summary>
		/// Analyzes the given ROI.
		/// </summary>
		/// <param name="roi">The ROI being analyzed.</param>
		/// <param name="mode">The analysis mode.</param>
		/// <returns>A string containing the analysis results, which can be appended to the analysis
		/// callout of the associated <see cref="RoiGraphic"/>, if one exists.</returns>
		public string Analyze(Roi roi, RoiAnalysisMode mode)
		{
			if (!SupportsRoi(roi))
				return null;

			// performance enhancement to restrict excessive computation of polygon area.
			if (mode == RoiAnalysisMode.Responsive)
			{
				if (_units == Units.Pixels)
					return String.Format(SR.FormatAreaPixels, SR.StringNoValue);
				else if (_units == Units.Millimeters)
					return String.Format(SR.FormatAreaSquareMm, SR.StringNoValue);
				else
					return String.Format(SR.FormatAreaSquareCm, SR.StringNoValue);
			}

			IRoiAreaProvider areaProvider = (IRoiAreaProvider) roi;

			string text;

			Units oldUnits = areaProvider.Units;
			areaProvider.Units = areaProvider.IsCalibrated ? _units : Units.Pixels;

			if (!areaProvider.IsCalibrated || _units == Units.Pixels)
				text = String.Format(SR.FormatAreaPixels, areaProvider.Area);
			else if (_units == Units.Millimeters)
				text = String.Format(SR.FormatAreaSquareMm, areaProvider.Area);
			else
				text = String.Format(SR.FormatAreaSquareCm, areaProvider.Area);

			areaProvider.Units = oldUnits;

			return text;
		}

        public void SetRoiAnalyzerUpdateCallback(RoiAnalyzerUpdateCallback callback)
        {
            _updateCallback = callback;
        }
	}
}