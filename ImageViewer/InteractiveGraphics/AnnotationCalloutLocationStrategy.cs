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
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.ImageViewer.Graphics;
using ClearCanvas.ImageViewer.Mathematics;

namespace ClearCanvas.ImageViewer.InteractiveGraphics
{
	/// <summary>
	/// A default implementation of <see cref="IAnnotationCalloutLocationStrategy"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This implementation sets the initial callout position to be a location offset
	/// from the top left corner of the ROI measurement's bounding box. Once set, the
	/// callout is not moved along with the measurement.
	/// </para>
	/// <para>
	/// Implementors of <see cref="IAnnotationCalloutLocationStrategy"/> may wish to
	/// derive from this class to take advantage of built-in functionality and only
	/// override methods to implement the desired strategy.
	/// </para>
	/// </remarks>
	[Cloneable(true)]
	public class AnnotationCalloutLocationStrategy : IAnnotationCalloutLocationStrategy
	{
		[CloneIgnore]
		private AnnotationGraphic _annotationGraphic;
		private bool _initialLocationSet;

		/// <summary>
		/// Constructor.
		/// </summary>
		internal protected AnnotationCalloutLocationStrategy()
		{
			_initialLocationSet = false;
		}

		/// <summary>
		/// Releases unmanaged resources held by this object.
		/// </summary>
		public void Dispose()
		{
			try
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
			catch (Exception ex)
			{
				Platform.Log(LogLevel.Error, ex);
			}
		}

		/// <summary>
		/// Gets the owning <see cref="AnnotationGraphic"/>.
		/// </summary>
		protected AnnotationGraphic AnnotationGraphic
		{
			get { return _annotationGraphic; }
		}

		/// <summary>
		/// Gets the <see cref="AnnotationGraphic"/>'s Subject.
		/// </summary>
		protected IGraphic Roi
		{
			get { return _annotationGraphic.Subject; }
		}

		/// <summary>
		/// Gets the <see cref="AnnotationGraphic"/>'s Callout.
		/// </summary>
		protected ICalloutGraphic Callout
		{
			get { return _annotationGraphic.Callout; }
		}

		/// <summary>
		/// Called to release any unmanaged resources held by this object.
		/// </summary>
		/// <param name="disposing">True if <see cref="IDisposable.Dispose"/> was called; False if the object is being finalized.</param>
		protected virtual void Dispose(bool disposing) {}

		/// <summary>
		/// Called when the <see cref="AnnotationGraphic"/> changes.
		/// </summary>
		/// <param name="oldAnnotationGraphic">The former value of <see cref="AnnotationGraphic"/>.</param>
		/// <param name="annotationGraphic">The new value of <see cref="AnnotationGraphic"/>.</param>
		protected virtual void OnAnnotationGraphicChanged(AnnotationGraphic oldAnnotationGraphic, AnnotationGraphic annotationGraphic) {}

		#region IRoiCalloutLocationStrategy Members

		/// <summary>
		/// Sets the <see cref="AnnotationGraphic"/> that owns this strategy.
		/// </summary>
		public void SetAnnotationGraphic(AnnotationGraphic annotationGraphic)
		{
			if (_annotationGraphic != annotationGraphic)
			{
				var oldAnnotationGraphic = _annotationGraphic;
				_annotationGraphic = annotationGraphic;
				OnAnnotationGraphicChanged(oldAnnotationGraphic, annotationGraphic);
			}
		}

		/// <summary>
		/// Called when the <see cref="InteractiveGraphics.AnnotationGraphic"/>'s callout location has been changed externally; for example, by the user.
		/// </summary>
		public virtual void OnCalloutLocationChangedExternally() {}

		/// <summary>
		/// Calculates the initial callout location only; returns false thereafter.
		/// </summary>
		public virtual bool CalculateCalloutLocation(out PointF location, out CoordinateSystem coordinateSystem)
		{
			location = PointF.Empty;
			coordinateSystem = CoordinateSystem.Destination;

			if (!_initialLocationSet)
			{
				_initialLocationSet = true;

				SizeF offset = new SizeF(0, 55);

				// Setup the callout
				this.Roi.CoordinateSystem = CoordinateSystem.Destination;
				location = RectangleUtilities.ConvertToPositiveRectangle(Roi.BoundingBox).Location - offset;
				this.Roi.ResetCoordinateSystem();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Calculates the callout endpoint using the <see cref="IGraphic.GetClosestPoint"/> method.
		/// </summary>
		public virtual void CalculateCalloutEndPoint(out PointF endPoint, out CoordinateSystem coordinateSystem)
		{
			coordinateSystem = this.AnnotationGraphic.CoordinateSystem;
			endPoint = AnnotationGraphic.Subject.GetClosestPoint(AnnotationGraphic.Callout.TextLocation);
		}

		/// <summary>
		/// Creates a deep copy of this strategy object.
		/// </summary>
		/// <remarks>
		/// Implementations should never return null from this method.
		/// </remarks>
		public IAnnotationCalloutLocationStrategy Clone()
		{
			return CloneBuilder.Clone(this) as IAnnotationCalloutLocationStrategy;
		}

		#endregion
	}
}
