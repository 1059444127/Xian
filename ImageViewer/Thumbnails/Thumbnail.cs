#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Drawing;
using System.Threading;
using ClearCanvas.Common;
using ClearCanvas.Desktop;
using ClearCanvas.ImageViewer.Annotations;
using ClearCanvas.ImageViewer.Graphics;
using ClearCanvas.ImageViewer.StudyManagement;

namespace ClearCanvas.ImageViewer.Thumbnails
{
	public partial class ThumbnailComponent
	{
		public delegate void ThumbnailLoadedCallback(Thumbnail thumbnail);

		public class Thumbnail : IGalleryItem, IDisposable
		{
			private static readonly int DefaultIconWidth = 100;
			private static readonly int DefaultIconHeight = 100;

			private readonly IDisplaySet _displaySet;

			private volatile IPresentationImage _image;
			private volatile Image _icon;

			private volatile SynchronizationContext _uiThreadContext;
			private volatile bool _loading = false;
			private volatile bool _disposed = false;

			private ThumbnailLoadedCallback _loadedCallback;

			private int _iconWidth;
			private int _iconHeight;

			public Thumbnail(IDisplaySet displaySet, ThumbnailLoadedCallback loadedCallback)
				: this(displaySet, loadedCallback, DefaultIconWidth, DefaultIconHeight)
			{
			}

			public Thumbnail(IDisplaySet displaySet, ThumbnailLoadedCallback loadedCallback, int width, int height)
			{
				_displaySet = displaySet;

				_iconWidth = width;
				_iconHeight = height;

				_image = GetMiddlePresentationImage(displaySet);
				if (_image != null)
				{
					_image = _image.CreateFreshCopy();
					_icon = CreateDummyBitmap(SR.MessageLoading, _iconWidth, _iconHeight);
				}
				else
				{
					_icon = CreateDummyBitmap(SR.MessageNoImages, _iconWidth, _iconHeight);
				}

				_uiThreadContext = SynchronizationContext.Current;
				_loadedCallback = loadedCallback;
				_loading = false;
				_disposed = false;
			}

			#region IGalleryItem Members

			public Image Image
			{
				get { return _icon; }
			}

			public string Name
			{
				get
				{
					string name = _displaySet.Name;
					name = name.Replace("\r\n", " ");
					name = name.Replace("\r", " ");
					name = name.Replace("\n", " ");

					int number = _displaySet.PresentationImages.Count;
					if (number <= 1)
						return String.Format(SR.FormatThumbnailName1Image, name);
					
					return String.Format(SR.FormatThumbnailName, number, name);
				}
				set { throw new NotSupportedException("Renaming thumbnails is not allowed."); }
			}

			public string Description
			{
				get { return String.Empty; }
			}

			public object Item
			{
				get { return _displaySet; }
			}

			#endregion

			public void LoadAsync()
			{
				if (_image == null)
					return;

				_loading = true;
				ThreadPool.QueueUserWorkItem(LoadAsync);
			}

			#region IDisposable Members

			public void Dispose()
			{
				_disposed = true;
				if (_loading)
					return;

				DisposeInternal();
			}

			#endregion

			#region Private Methods

			private void DisposeInternal()
			{
				_disposed = true;

				_uiThreadContext = null;
				_loadedCallback = null;

				if (_image != null)
				{
					_image.Dispose();
					_image = null;
				}

				if (_icon != null)
				{
					_icon.Dispose();
					_icon = null;
				}
			}

			private void LoadAsync(object ignored)
			{
				Bitmap icon;
				try
				{
					icon = CreateBitmap(_image, _iconWidth, _iconHeight);
				}
				catch (Exception e)
				{
					Platform.Log(LogLevel.Error, e);
					icon = CreateDummyBitmap(SR.MessageLoadFailed, _iconWidth, _iconHeight);
				}

				_uiThreadContext.Post(this.OnLoaded, icon);
			}

			private void OnLoaded(object icon)
			{
				_icon.Dispose();
				_icon = (Bitmap) icon;

				_loading = false;

				if (!_disposed)
					_loadedCallback(this);
				else
					DisposeInternal();
			}

			private static Bitmap CreateDummyBitmap(string message, int width, int height)
			{
				Bitmap bmp = new Bitmap(width, height);
				System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bmp);

				Brush brush = new SolidBrush(Color.WhiteSmoke);
				Font font = new Font("Arial", 10.0f);

				StringFormat format = new StringFormat();
				format.Alignment = StringAlignment.Center;
				format.LineAlignment = StringAlignment.Center;

				graphics.DrawString(message, font, brush, new Rectangle(0, 0, width, height), format);
				DrawBorder(graphics, width, height);
				graphics.Dispose();

				format.Dispose();
				font.Dispose();
				brush.Dispose();

				return bmp;
			}

			private static Bitmap CreateBitmap(IPresentationImage image, int width, int height)
			{
				image = image.CreateFreshCopy();

				if (image is IAnnotationLayoutProvider)
					((IAnnotationLayoutProvider) image).AnnotationLayout.Visible = false;

				Bitmap bmp = DrawToThumbnail(image, width, height);
				System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bmp);

				DrawBorder(graphics, width, height);

				image.Dispose();
				graphics.Dispose();

				return bmp;
			}

			private static void DrawBorder(System.Drawing.Graphics graphics, int width, int height)
			{
				using (Pen pen = new Pen(Color.DarkGray))
				{
					graphics.DrawRectangle(pen, 0, 0, width - 1, height - 1);
				}
			}

			private static Bitmap DrawToThumbnail(IPresentationImage image, int width, int height)
			{
				const int rasterResolution = 256;

				var bitmap = new Bitmap(width, height);
				using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
				{
					try
					{
						var imageAspectRatio = 1f;
						var thumbnailAspectRatio = (float) height/width;
						var thumbnailBounds = new RectangleF(0, 0, width, height);

						if (image is IImageGraphicProvider)
						{
							var imageGraphic = ((IImageGraphicProvider) image).ImageGraphic;
							imageAspectRatio = (float) imageGraphic.Rows/imageGraphic.Columns;
						}
						if (image is IImageSopProvider)
						{
							var ig = ((IImageSopProvider) image).Frame;
							if (!ig.PixelAspectRatio.IsNull)
								imageAspectRatio *= ig.PixelAspectRatio.Value;
							else if (!ig.PixelSpacing.IsNull)
								imageAspectRatio *= (float) ig.PixelSpacing.AspectRatio;
						}

						if (thumbnailAspectRatio >= imageAspectRatio)
						{
							thumbnailBounds.Width = width;
							thumbnailBounds.Height = width*imageAspectRatio;
							thumbnailBounds.Y = (height - thumbnailBounds.Height)/2;
						}
						else
						{
							thumbnailBounds.Width = height/imageAspectRatio;
							thumbnailBounds.Height = height;
							thumbnailBounds.X = (width - thumbnailBounds.Width)/2;
						}

						// rasterize any invariant vector graphics at a semi-normal image box resolution first before rendering as a thumbnail
						using (var raster = image.DrawToBitmap(rasterResolution, (int) (rasterResolution*imageAspectRatio)))
						{
							graphics.DrawImage(raster, thumbnailBounds);
						}
					}
					catch (Exception ex)
					{
						// rendering the error text to a 100x100 icon is useless, so we'll just paint a placeholder error icon and log the icon error
						Platform.Log(LogLevel.Warn, ex, "Failed to render thumbnail with dimensions {0}x{1}", width, height);

						graphics.FillRectangle(Brushes.Black, 0, 0, width, height);
						graphics.DrawLine(Pens.WhiteSmoke, 0, 0, width, height);
						graphics.DrawLine(Pens.WhiteSmoke, 0, height, width, 0);
					}
				}
				return bitmap;
			}

			private static IPresentationImage GetMiddlePresentationImage(IDisplaySet displaySet)
			{
				if (displaySet.PresentationImages.Count == 0)
					return null;

				if (displaySet.PresentationImages.Count <= 2)
					return displaySet.PresentationImages[0];

				return displaySet.PresentationImages[displaySet.PresentationImages.Count/2];
			}

			#endregion
		}
	}
}