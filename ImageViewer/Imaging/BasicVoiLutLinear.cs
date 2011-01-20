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
using ClearCanvas.Common.Utilities;
using ClearCanvas.Desktop;

namespace ClearCanvas.ImageViewer.Imaging
{
	/// <summary>
	/// The most basic of Linear Luts where the <see cref="WindowWidth"/> and <see cref="WindowCenter"/> can be directly set/manipulated.
	/// </summary>
	/// <seealso cref="IBasicVoiLutLinear"/>
	[Cloneable(true)]
	public sealed class BasicVoiLutLinear : VoiLutLinearBase, IBasicVoiLutLinear
	{
		#region Window/Level Memento class

		private class WindowLevelMemento : IEquatable<WindowLevelMemento>
		{
			public readonly double WindowWidth;
			public readonly double WindowCenter;

			public WindowLevelMemento(double windowWidth, double windowCenter)
			{
				WindowWidth = windowWidth;
				WindowCenter = windowCenter;
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}	
		
			public override bool Equals(object obj)
			{
				if (obj == this)
					return true;

				if (obj is WindowLevelMemento)
					return this.Equals((WindowLevelMemento) obj);

				return false;
			}

			#region IEquatable<WindowLevelMemento> Members

			public bool Equals(WindowLevelMemento other)
			{
				if (other == null)
					return false;

				return this.WindowWidth == other.WindowWidth && this.WindowCenter == other.WindowCenter;
			}

			#endregion
		}

		#endregion

		#region Private Fields

		private double _windowWidth;
		private double _windowCenter;

		#endregion

		#region Public Constructors

		/// <summary>
		/// Constructor.  
		/// </summary>
		/// <remarks>
		/// Allows the initial <see cref="WindowWidth"/> and <see cref="WindowCenter"/> to be set.
		/// </remarks>
		/// <param name="windowWidth">The initial Window Width.</param>
		/// <param name="windowCenter">The initial Window Center.</param>
		public BasicVoiLutLinear(double windowWidth, double windowCenter)
			: base()
		{
			this.WindowWidth = windowWidth;
			this.WindowCenter = windowCenter;
		}

		/// <summary>
		/// Default Constructor.
		/// </summary>
		/// <remarks>
		/// The initial <see cref="WindowWidth"/> and <see cref="WindowCenter"/> are 1 and 0, respectively.
		/// </remarks>
		public BasicVoiLutLinear()
			: this(1, 0)
		{
		}

		#endregion

		#region Protected Methods
		#region Overrides

		/// <summary>
		/// Gets the <see cref="WindowWidth"/>.
		/// </summary>
		protected override double GetWindowWidth()
		{
			return this.WindowWidth;
		}

		/// <summary>
		/// Gets the <see cref="WindowCenter"/>.
		/// </summary>
		protected override double GetWindowCenter()
		{
			return this.WindowCenter;
		}

		#endregion
		#endregion

		#region Public Members
		#region Properties

		/// <summary>
		/// Gets or sets the Window Width.
		/// </summary>
		public double WindowWidth
		{
			get { return _windowWidth; }
			set
			{
				if (value == _windowWidth)
					return;

				if (value < 1)
					value = 1;

				_windowWidth = value;
				base.OnLutChanged();
			}
		}

		/// <summary>
		/// Gets or sets the Window Center.
		/// </summary>
		public double WindowCenter
		{
			get { return _windowCenter; }
			set
			{
				if (value == _windowCenter)
					return;

				_windowCenter = value;
				base.OnLutChanged();
			}
		}

		#endregion

		#region Methods
		#region Overrides

		/// <summary>
		/// Gets an abbreviated description of the Lut.
		/// </summary>
		public override string GetDescription()
		{
			return String.Format(SR.FormatDescriptionBasicLinearLut, WindowWidth, WindowCenter);
		}

		/// <summary>
		/// Creates a memento, through which the Lut's state can be restored.
		/// </summary>
		public override object CreateMemento()
		{
			return new WindowLevelMemento(this.WindowWidth, this.WindowCenter);
		}

		/// <summary>
		/// Sets the Lut's state from the input memento object.
		/// </summary>
		/// <exception cref="InvalidCastException">Thrown when the memento is unrecognized, which should never happen.</exception>
		/// <param name="memento">The memento to use to restore a previous state.</param>
		public override void SetMemento(object memento)
		{
			WindowLevelMemento windowLevelMemento = (WindowLevelMemento) memento;

			this.WindowWidth = windowLevelMemento.WindowWidth;
			this.WindowCenter = windowLevelMemento.WindowCenter;
		}

		#endregion
		#endregion
		#endregion
	}
}
