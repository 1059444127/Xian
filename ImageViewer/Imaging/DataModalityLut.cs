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
	/// Base implementation of a <see cref="IDataModalityLut"/> lookup table mapping input stored pixel values to output modality-independent values.
	/// </summary>
	/// <remarks>
	/// Normally, you should not have to inherit directly from this class.
	/// <see cref="SimpleDataModalityLut"/> or <see cref="GeneratedDataModalityLut"/> should cover
	/// most, if not all, common use cases.
	/// </remarks>
	[Cloneable(true)]
	public abstract class DataModalityLut : IDataModalityLut
	{
		private event EventHandler _lutChanged;

		private int _minInputValue;
		private int _maxInputValue;
		private double _minOutputValue;
		private double _maxOutputValue;

		public virtual int MinInputValue
		{
			get { return _minInputValue; }
			set
			{
				if (value == _minInputValue)
					return;

				_minInputValue = value;
				OnLutChanged();
			}
		}

		public virtual int MaxInputValue
		{
			get { return _maxInputValue; }
			set
			{
				if (value == _maxInputValue)
					return;

				_maxInputValue = value;
				OnLutChanged();
			}
		}

		public virtual double MinOutputValue
		{
			get { return _minOutputValue; }
			protected set
			{
				if (_minOutputValue == value)
					return;

				_minOutputValue = value;
				OnLutChanged();
			}
		}

		public virtual double MaxOutputValue
		{
			get { return _maxOutputValue; }
			protected set
			{
				if (value == _maxOutputValue)
					return;

				_maxOutputValue = value;
				OnLutChanged();
			}
		}

		public virtual double this[int index]
		{
			get
			{
				if (index <= FirstMappedPixelValue)
					return Data[0];
				else if (index >= LastMappedPixelValue)
					return Data[Length - 1];
				else
					return Data[index - FirstMappedPixelValue];
			}
			protected set
			{
				if (index < FirstMappedPixelValue || index > LastMappedPixelValue)
					return;

				Data[index - FirstMappedPixelValue] = value;
			}
		}

		double IComposableLut.MinInputValue
		{
			get { return MinInputValue; }
			set { MinInputValue = (int) Math.Round(value); }
		}

		double IComposableLut.MaxInputValue
		{
			get { return MaxInputValue; }
			set { MaxInputValue = (int) Math.Round(value); }
		}

		double IComposableLut.MinOutputValue
		{
			get { return MinOutputValue; }
		}

		double IComposableLut.MaxOutputValue
		{
			get { return MaxOutputValue; }
		}

		double IComposableLut.this[double input]
		{
			get { return this[(int) Math.Round(input)]; }
		}

		public event EventHandler LutChanged
		{
			add { _lutChanged += value; }
			remove { _lutChanged -= value; }
		}

		///<summary>
		/// Gets the length of <see cref="Data"/>.
		///</summary>
		/// <remarks>
		/// The reason for this member's existence is that <see cref="Data"/> may
		/// not yet exist; this value is based solely on <see cref="FirstMappedPixelValue"/>
		/// and <see cref="LastMappedPixelValue"/>.
		/// </remarks>
		public int Length
		{
			get { return 1 + LastMappedPixelValue - FirstMappedPixelValue; }
		}

		public abstract int FirstMappedPixelValue { get; }

		/// <summary>
		/// Gets the last mapped pixel value.
		/// </summary>
		public abstract int LastMappedPixelValue { get; }

		public abstract double[] Data { get; }

		public abstract string GetKey();

		public abstract string GetDescription();

		IComposableLut IComposableLut.Clone()
		{
			return Clone();
		}

		IModalityLut IModalityLut.Clone()
		{
			return Clone();
		}

		public IDataModalityLut Clone()
		{
			return CloneBuilder.Clone(this) as IDataModalityLut;
		}

		/// <summary>
		/// Fires the <see cref="LutChanged"/> event.
		/// </summary>
		/// <remarks>
		/// Inheritors should call this method when any property of the lookup table has changed.
		/// </remarks>
		protected virtual void OnLutChanged()
		{
			EventsHelper.Fire(_lutChanged, this, EventArgs.Empty);
		}

		#region IMemorable Members

		/// <summary>
		/// Captures the state of the lookup table.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The implementation should return an object containing enough state information so that,
		/// when <see cref="SetMemento"/> is called, the lookup table can be restored to the original state.
		/// </para>
		/// <para>
		/// If the method is implemented, <see cref="SetMemento"/> must also be implemented.
		/// </para>
		/// </remarks>
		public virtual object CreateMemento()
		{
			return null;
		}

		/// <summary>
		/// Restores the state of the lookup table.
		/// </summary>
		/// <param name="memento">An object that was originally created by <see cref="CreateMemento"/>.</param>
		/// <remarks>
		/// <para>
		/// The implementation should return the lookup table to the original state captured by <see cref="CreateMemento"/>.
		/// </para>
		/// <para>
		/// If you implement <see cref="CreateMemento"/> to capture the lookup table's state, you must also implement this method
		/// to allow the state to be restored. Failure to do so will result in a <see cref="InvalidOperationException"/>.
		/// </para>
		/// </remarks>
		public virtual void SetMemento(object memento)
		{
			if (memento != null)
				throw new InvalidOperationException(SR.ExceptionMustOverrideSetMemento);
		}

		#endregion
	}
}