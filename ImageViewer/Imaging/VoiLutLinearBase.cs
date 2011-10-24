#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using ClearCanvas.Common.Utilities;

namespace ClearCanvas.ImageViewer.Imaging
{
	/// <summary>
	/// Abstract class providing all the base implementation for Linear Voi Luts.
	/// </summary>
	/// <remarks>
	/// A simple Linear Voi Lut class (<see cref="BasicVoiLutLinear"/>) and other 
	/// abstract base classes (<see cref="CalculatedVoiLutLinear"/>, <see cref="AlgorithmCalculatedVoiLutLinear"/>)
	/// are provided that cover most, if not all, Linear Voi Lut use cases.  You should not need
	/// to derive directly from this class.
	/// </remarks>
	/// <seealso cref="ComposableVoiLut"/>
	/// <seealso cref="IVoiLut"/>
	[Cloneable(true)]
	public abstract class VoiLutLinearBase : ComposableVoiLut
	{
		#region Private Fields

		private int _minInputValue;
		private int _maxInputValue;
		private double _windowRegionStart;
		private double _windowRegionEnd;
		private bool _recalculate;

		#endregion

		#region Protected Constructor

		/// <summary>
		/// Default constructor.
		/// </summary>
		protected VoiLutLinearBase()
		{
			_recalculate = true;
			_minInputValue = int.MinValue;
			_maxInputValue = int.MaxValue;
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Gets the Window Width.
		/// </summary>
		protected abstract double GetWindowWidth();

		/// <summary>
		/// Gets the Window Center.
		/// </summary>
		protected abstract double GetWindowCenter();

		#endregion

		#region Overrides
		#region Public Properties

		/// <summary>
		/// Gets the output value of the Lut at a given input <paramref name="index"/>.
		/// </summary>
		public sealed override int this[int index]
		{
			get
			{
				if (_recalculate)
				{
					Calculate();
					_recalculate = false;
				}

				if (index <= _windowRegionStart)
					return MinOutputValue;
				if (index > _windowRegionEnd)
					return MaxOutputValue;

                double scale = ((index - (GetWindowCenter() - 0.5)) / (GetWindowWidthInternal() - 1)) + 0.5;
				return (int)((scale * (MaxOutputValue - MinOutputValue)) + MinOutputValue);
			}
			protected set
			{
				throw new InvalidOperationException(SR.ExceptionLinearLutDataCannotBeSet);
			}
		}

		/// <summary>
		/// Gets or sets the minimum input value.
		/// </summary>
		/// <remarks>
		/// This value should not be modified by your code.  It will be set internally by the framework.
		/// </remarks>
		public sealed override int MinInputValue
		{
			get { return _minInputValue; }
			set
			{
				if (_minInputValue == value)
					return;

				_minInputValue = value;
				OnLutChanged();
			}
		}

		/// <summary>
		/// Gets the maximum input value.
		/// </summary>
		/// <remarks>
		/// This value should not be modified by your code.  It will be set internally by the framework.
		/// </remarks>
		public sealed override int MaxInputValue
		{
			get { return _maxInputValue; }
			set
			{
				if (_maxInputValue == value)
					return;

				_maxInputValue = value;
				OnLutChanged();
			}
		}

		/// <summary>
		/// Gets the minimum output value.
		/// </summary>
		/// <exception cref="MemberAccessException">Thrown on any attempt to set the value.</exception>
		public sealed override int MinOutputValue
		{
			get { return _minInputValue; }
			protected set { throw new InvalidOperationException(SR.ExceptionMinimumOutputValueIsNotSettable); }
		}

		/// <summary>
		/// Gets the maximum output value.
		/// </summary>
		/// <exception cref="MemberAccessException">Thrown on any attempt to set the value.</exception>
		public sealed override int MaxOutputValue
		{
			get { return _maxInputValue; }
			protected set { throw new InvalidOperationException(SR.ExceptionMaximumOutputValueIsNotSettable); }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets a string key that identifies this particular LUT's characteristics, so that 
		/// an image's <see cref="IComposedLut"/> can be more efficiently determined.
		/// </summary>
		/// <remarks>
		/// This method is not to be confused with <b>equality</b>, since some Luts can be
		/// dependent upon the actual image to which it belongs.
		/// </remarks>
		public sealed override string GetKey()
		{
			return String.Format("{0}_{1}_{2}_{3}",
				this.MinInputValue,
				this.MaxInputValue,
                this.GetWindowWidthInternal(),
				this.GetWindowCenter());
		}

		/// <summary>
		/// Should be called by implementors when the Lut characteristics have changed.
		/// </summary>
		protected override void OnLutChanged()
		{
			_recalculate = true;
			base.OnLutChanged();
		}

		#endregion
		#endregion

		#region Private Methods

		private void Calculate()
		{
			double halfWindow = (GetWindowWidthInternal() - 1) / 2;
			_windowRegionStart = GetWindowCenter() - 0.5 - halfWindow;
			_windowRegionEnd = GetWindowCenter() - 0.5 + halfWindow;
		}
		
        private double GetWindowWidthInternal()
        {
            return Math.Max(1, GetWindowWidth());
        }

	    #endregion
	}
}
