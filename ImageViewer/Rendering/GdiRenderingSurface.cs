#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

#pragma warning disable 1591,0419,1574,1587

using System;
using System.Drawing;
using ClearCanvas.Common;

namespace ClearCanvas.ImageViewer.Rendering
{
	internal sealed class GdiRenderingSurface : IRenderingSurface
	{
		private ImageBuffer _imageBuffer;
		private BackBuffer _finalBuffer;

		private IntPtr _windowID;
		private IntPtr _contextID;
		private Rectangle _clientRectangle;
		private Rectangle _clipRectangle;

		public GdiRenderingSurface(IntPtr windowID, int width, int height)
		{
			_imageBuffer = new ImageBuffer();
			_finalBuffer = new BackBuffer();

			_windowID = windowID;
			this.ClientRectangle = new Rectangle(0, 0, width, height);
		}

		#region IRenderingSurface Members

		public IntPtr WindowID
		{
			get { return _windowID; }
			set { _windowID = value; }
		}

		public IntPtr ContextID 
		{
			get { return _contextID; }
			set 
			{ 
				_contextID = value;
				FinalBuffer.ContextID = _contextID;
			}
		}

		/// <summary>
		/// Gets or sets the rectangle of the surface.
		/// </summary>
		/// <remarks>
		/// This is the rectangle of the view onto the <see cref="ITile"/>.
		/// The top-left corner is always (0,0).  This rectangle changes as the
		/// view (i.e., the hosting window) changes size.
		/// </remarks>
		public Rectangle ClientRectangle
		{
			get { return _clientRectangle; }
			set
			{
				if (value.Width == 0 || value.Height == 0)
					return;

				if (_clientRectangle != value)
				{
					_clientRectangle = value;
					_imageBuffer.Size = new Size(_clientRectangle.Width, _clientRectangle.Height);
					_finalBuffer.ClientRectangle = _clientRectangle;
				}
			}
		}

		/// <summary>
		/// Gets or sets the rectangle that requires repainting.
		/// </summary>
		/// <remarks>
		/// The implementer of <see cref="IRenderer"/> should use this rectangle
		/// to intelligently perform the <see cref="DrawMode.Refresh"/> operation.
		/// </remarks>
		public Rectangle ClipRectangle
		{
			get { return _clipRectangle; }
			set { _clipRectangle = value; }
		}

		#endregion

		public ImageBuffer ImageBuffer
		{
			get { return _imageBuffer; }
		}

		public BackBuffer FinalBuffer
		{
			get { return _finalBuffer; }
		}

		#region IDisposable Members

		public void Dispose()
		{
			try
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
			catch (Exception e)
			{
				// shouldn't throw anything from inside Dispose()
				Platform.Log(LogLevel.Error, e);
			}
		}

		#endregion

		/// <summary>
		/// Implementation of the <see cref="IDisposable"/> pattern
		/// </summary>
		/// <param name="disposing">True if this object is being disposed, false if it is being finalized</param>
		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_imageBuffer != null)
				{
					_imageBuffer.Dispose();
					_imageBuffer = null;
				}

				if (_finalBuffer != null)
				{
					_finalBuffer.Dispose();
					_finalBuffer = null;
				}
			}
		}
	}
}
