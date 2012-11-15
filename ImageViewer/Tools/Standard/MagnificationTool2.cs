﻿#region License

// Copyright (c) 2012, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Drawing;
using ClearCanvas.Common;
using ClearCanvas.Desktop;
using ClearCanvas.ImageViewer.Annotations;
using ClearCanvas.ImageViewer.Graphics;
using ClearCanvas.ImageViewer.Rendering;

namespace ClearCanvas.ImageViewer.Tools.Standard
{
    public partial class MagnificationTool
    {
        private string _lastRenderExceptionMessage = null;
        private PresentationImage _magnificationImage;
        private bool _firstRender;
        private Point _tileLocation;

        private void RenderImage(DrawArgs args)
        {
            if (args.DrawMode == DrawMode.Refresh)
            {
                RefreshImage(args);
                return;
            }

            if (_firstRender)
            {
                // the first time we try to render a freshly cloned image, we need to draw it twice
                // this is to make sure the client rectangle is updated when we try to compute the correct point of interest
                _firstRender = false;
                RenderImage(args);
            }

            try
            {
                var sourceTransform = (ImageSpatialTransform)((ISpatialTransformProvider)SelectedPresentationImage).SpatialTransform;
                var transform = (ImageSpatialTransform)((ISpatialTransformProvider)_magnificationImage).SpatialTransform;

                float scale = sourceTransform.Scale * ToolSettings.Default.MagnificationFactor;
                transform.ScaleToFit = false;
                transform.Scale = scale;
                transform.TranslationX = 0;
                transform.TranslationY = 0;

                var midPoint = new PointF(args.RenderingSurface.ClientRectangle.Width / 2f, args.RenderingSurface.ClientRectangle.Height / 2f);
                var sourcePointOfInterest = sourceTransform.ConvertToSource(_tileLocation);
                // compute translation required to move the point of interest on the magnified image to the centre of the client area
                var translation = transform.ConvertToSource(midPoint) - new SizeF(sourcePointOfInterest);
                transform.TranslationX = translation.X;
                transform.TranslationY = translation.Y;

                _magnificationImage.Draw(args);

                // clear the rendering exception message
                _lastRenderExceptionMessage = null;
            }
            catch (Exception ex)
            {
                Platform.Log(LogLevel.Error, ex,
                             "An error has occured while rendering the magnified contents of the tile.");

                // a rendering exception was encountered, so set the message field
                _lastRenderExceptionMessage = ex is RenderingException
                                                  ? ((RenderingException)ex).UserMessage
                                                  : ex.Message;

                // we cannot simply pass the existing Graphics because we haven't released its hDC yet
                // if we do, we'll get a "Object is currently in use elsewhere" exception
                DrawErrorMessage(_lastRenderExceptionMessage, args.RenderingSurface.ContextID, args.RenderingSurface.ClientRectangle);
            }
        }

        private void RefreshImage(DrawArgs args)
        {
            try
            {
                // if there was an exception the last time we rendered the buffer, don't refresh from the buffer and instead redraw the error message
                if (string.IsNullOrEmpty(_lastRenderExceptionMessage))
                {
                    _magnificationImage.Draw(args);
                }
                else
                {
                    // we cannot simply pass the existing Graphics because we haven't released its hDC yet
                    // if we do, we'll get a "Object is currently in use elsewhere" exception
                    DrawErrorMessage(_lastRenderExceptionMessage, args.RenderingSurface.ContextID, args.RenderingSurface.ClientRectangle);
                }
            }
            catch (Exception ex)
            {
                Platform.Log(LogLevel.Error, ex, "An error has occured while refreshing the magnified contents of the tile.");

                var exceptionMessage = ex is RenderingException ? ((RenderingException)ex).UserMessage : ex.Message;

                // we cannot simply pass the Graphics because we haven't released its hDC yet
                // if we do, we'll get a "Object is currently in use elsewhere" exception
                DrawErrorMessage(exceptionMessage, args.RenderingSurface.ContextID, args.RenderingSurface.ClientRectangle);
            }
        }

        private static void DrawErrorMessage(string errorMessage, IntPtr hDC, Rectangle bounds)
        {
            using (var errorGraphics = System.Drawing.Graphics.FromHdc(hDC))
            {
                // don't give the user any false expectation of the validity of the magnified output by clearing any partially rendered results
                errorGraphics.FillRectangle(Brushes.Black, bounds);

                using (var format = new StringFormat
                {
                    Trimming = StringTrimming.EllipsisCharacter,
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoClip
                })
                {
                    // use the system-determined default font to ensure we can't fail at drawing error messages (cause some systems might not have Arial)
                    using (var font = new Font(SystemFonts.DefaultFont.Name, 12.0f))
                    {
                        errorGraphics.DrawString(errorMessage, font, Brushes.WhiteSmoke, bounds, format);
                    }
                }
            }
        }

        private void HideOverlays()
        {
            if (_magnificationImage is IAnnotationLayoutProvider)
            {
                string magFactor = String.Format("{0:F1}x", ToolSettings.Default.MagnificationFactor);
                AnnotationLayout layout = new AnnotationLayout();
                BasicTextAnnotationItem item = new BasicTextAnnotationItem("mag", "mag", "mag", magFactor);
                AnnotationBox box = new AnnotationBox(new RectangleF(0.8F, 0F, .2F, .05F), item);
                box.Justification = AnnotationBox.JustificationBehaviour.Right;
                box.VerticalAlignment = AnnotationBox.VerticalAlignmentBehaviour.Top;
                layout.AnnotationBoxes.Add(box);
                ((BasicPresentationImage)_magnificationImage).AnnotationLayout = layout;
            }

            if (_magnificationImage is IOverlayGraphicsProvider)
            {
                GraphicCollection graphics = ((IOverlayGraphicsProvider)_magnificationImage).OverlayGraphics;
                foreach (IGraphic graphic in graphics)
                    graphic.Visible = false;
            }

            if (_magnificationImage is IApplicationGraphicsProvider)
            {
                GraphicCollection graphics = ((IApplicationGraphicsProvider)_magnificationImage).ApplicationGraphics;
                foreach (IGraphic graphic in graphics)
                    graphic.Visible = false;
            }

            //we want the Dicom graphics to be visible (e.g. shutter and embedded overlays)

            //if (_magnificationImage is IDicomPresentationImage)
            //{
            //    GraphicCollection graphics = ((IDicomPresentationImage)_magnificationImage).DicomGraphics;
            //    foreach (IGraphic graphic in graphics)
            //        graphic.Visible = false;
            //}
        }

        private void InitializeMagnificationImage()
        {
            if (_magnificationImage != null)
                return;

            if (SelectedPresentationImage == null)
                throw new ArgumentException("The image cannot be null", "value");

            if (!(SelectedPresentationImage is ISpatialTransformProvider))
                throw new ArgumentException("The image must implement ISpatialTransformProvider", "value");

            if (!(((ISpatialTransformProvider)SelectedPresentationImage).SpatialTransform is ImageSpatialTransform))
                throw new ArgumentException("The image must provide an IImageSpatialTransform", "value");

            DisposeMagnificationImage();

            _firstRender = true;
            _magnificationImage = (PresentationImage)SelectedPresentationImage.Clone();

            HideOverlays();
        }

        private void DisposeMagnificationImage()
        {
            if (_magnificationImage != null)
            {
                _magnificationImage.Dispose();
                _magnificationImage = null;
            }
        }

        /*
        //text doesn't end up looking very good due to interpolation effects.
        private void AddMagnificationIndicator()
        {
            //Ideally, I would just replace the IAnnotationLayoutProvider with a new one that
            //showed only the mag factor ... but it would actually require framework changes,
            //so I'll do this for now.
            string magFactor = String.Format("{0:F2}x", _magnificationFactor);

            SizeF size;
            Bitmap bitmap = new Bitmap(Width, Height);
            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
            {
                using (Font font = new Font("Arial", 10, FontStyle.Bold, GraphicsUnit.Point))
                {
                    size = graphics.MeasureString(magFactor, font);
                }
            }

            bitmap.Dispose();

            int width = (int) (size.Width + 1) + 4;
            int height = (int)(size.Height + 1) + 4;
            int stride = 4*width;

            byte[] buffer = new byte[stride * height];
            GCHandle bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                bitmap = new Bitmap(width, height, stride, PixelFormat.Format32bppArgb, bufferHandle.AddrOfPinnedObject());
                using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
                {
                    graphics.Clear(Color.FromArgb(0, Color.Black));

                    using (Font font = new Font("Arial", 10, FontStyle.Regular, GraphicsUnit.Point))
                    {
                        //drop-shadow
                        using (Brush brush = new SolidBrush(Color.Black))
                        {
                            graphics.DrawString(magFactor, font, brush, 1, 1);
                        }
						
                        using (Brush brush = new SolidBrush(Color.WhiteSmoke))
                        {
                            graphics.DrawString(magFactor, font, brush, 0, 0);
                        }
                    }
                }
            }
            finally
            {
                bufferHandle.Free();
            }

            ColorImageGraphic graphic = new ColorImageGraphic(bitmap.Height, bitmap.Width, buffer);
            _magnificationImage.SceneGraph.Graphics.Add(graphic);
            graphic.SpatialTransform.TranslationX = Width - graphic.Columns - 5;
            graphic.SpatialTransform.TranslationY = 5;
        }
        */

    }
}
