#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Diagnostics;
using System.Drawing;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.ImageViewer.Graphics;
using ClearCanvas.ImageViewer.Mathematics;

namespace ClearCanvas.ImageViewer.InteractiveGraphics
{
	/// <summary>
	/// A graphical representation of the "handles" that allow 
	/// the user to move and resize graphics decorated by
	/// <see cref="ControlGraphic"/>s.
	/// </summary>
	[Cloneable(true)]
	public class ControlPoint : CompositeGraphic
	{
		#region Private fields

		private PointF _location;
		[CloneIgnore]
		private InvariantRectanglePrimitive _rectangle;
		private event EventHandler _locationChangedEvent;

		#endregion

		/// <summary>
		/// Initializes a new instance of <see cref="ControlPoint"/>.
		/// </summary>
		internal ControlPoint()
		{
		}
		
		private InvariantRectanglePrimitive Rectangle
		{
			get
			{
				if (_rectangle == null)
				{
					_rectangle = new InvariantRectanglePrimitive();
					_rectangle.InvariantTopLeft = new PointF(-4, -4);
					_rectangle.InvariantBottomRight = new PointF(4, 4);
					this.Graphics.Add(_rectangle);
				}

				return _rectangle;
			}
		}

		/// <summary>
		/// Gets or sets the location of the control point.
		/// </summary>
		public PointF Location
		{
			get
			{
				if (base.CoordinateSystem == CoordinateSystem.Source)
					return _location;
				else
					return base.SpatialTransform.ConvertToDestination(_location);
			}
			set
			{
				if (!FloatComparer.AreEqual(this.Location, value))
				{
					Platform.CheckMemberIsSet(base.SpatialTransform, "SpatialTransform");

					if (base.CoordinateSystem == CoordinateSystem.Source)
						_location = value;
					else
						_location = base.SpatialTransform.ConvertToSource(value);

					//Trace.Write(String.Format("Control Point: {0}\n", _location.ToString()));

					this.Rectangle.Location = this.Location;
					EventsHelper.Fire(_locationChangedEvent, this, EventArgs.Empty);
				}
			}
		}

		/// <summary>
		/// Gets or sets the colour of the control point.
		/// </summary>
		public Color Color
		{
			get { return Rectangle.Color; }
			set { Rectangle.Color = value; }
		}
	
		/// <summary>
		/// Occurs when the location of the control point has changed.
		/// </summary>
		public event EventHandler LocationChanged
		{
			add { _locationChangedEvent += value; }
			remove { _locationChangedEvent -= value; }
		}

		/// <summary>
		/// Moves the <see cref="ControlPoint"/> by a specified delta.
		/// </summary>
		/// <param name="delta">The distance to move.</param>
		/// <remarks>
		/// Depending on the value of <see cref="CoordinateSystem"/>,
		/// <paramref name="delta"/> will be interpreted in either source
		/// or destination coordinates.
		/// </remarks>
		public override void Move(SizeF delta)
		{
			this.Location += delta;
		}

		/// <summary>
		/// This method overrides <see cref="Graphic.HitTest"/>.
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public override bool HitTest(Point point)
		{
			return Rectangle.HitTest(point);
		}

		[OnCloneComplete]
		private void OnCloneComplete()
		{
			_rectangle = CollectionUtils.SelectFirst(base.Graphics,
				delegate(IGraphic test) { return test is InvariantRectanglePrimitive; }) as InvariantRectanglePrimitive;
		}
	}
}
