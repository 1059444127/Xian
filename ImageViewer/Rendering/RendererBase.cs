#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.ImageViewer.Annotations;
using ClearCanvas.ImageViewer.Graphics;

namespace ClearCanvas.ImageViewer.Rendering
{
	/// <summary>
	/// A Template class providing the base functionality for an <see cref="IRenderer"/>.
	/// </summary>
	/// <remarks>
	/// See the remarks section for <see cref="RendererFactoryBase"/> regarding the 
	/// thread-safety of this object (it is not thread-safe).  For this reason, you should
	/// use this class in tandem with the <see cref="RendererFactoryBase"/>, although it
	/// is not required that you do so.
	/// </remarks>
	public abstract class RendererBase : IRenderer
	{
		private DrawMode _drawMode;
		private CompositeGraphic _sceneGraph;
		private IRenderingSurface _surface;

		/// <summary>
		/// Constructor.
		/// </summary>
		protected RendererBase()
		{
		}

		/// <summary>
		/// Gets the <see cref="ClearCanvas.ImageViewer.Rendering.DrawMode"/>.
		/// </summary>
		protected DrawMode DrawMode
		{
			get { return _drawMode; }
			set { _drawMode = value; }
		}

		/// <summary>
		/// Gets the <b>SceneGraph</b> that is to be rendered.
		/// </summary>
		protected CompositeGraphic SceneGraph
		{
			get { return _sceneGraph; }
			set { _sceneGraph = value; }
		}

		/// <summary>
		/// Gets the <see cref="IRenderingSurface"/> that is to be rendered upon.
		/// </summary>
		protected IRenderingSurface Surface
		{
			get { return _surface; }
			set { _surface = value; }
		}

		/// <summary>
		/// Renders the specified scene graph to the graphics surface.
		/// </summary>
		/// <remarks>
		/// Calling code should take care to handle any exceptions in a manner suitable to the context of
		/// the rendering operation. For example, the view control for an
		/// <see cref="ITile"/> may wish to display the error message in the tile's client area <i>without
		/// crashing the control</i>, whereas an image export routine may wish to notify the user via an error
		/// dialog and have the export output <i>fail to be created</i>. Automated routines (such as unit
		/// tests) may even wish that the exception bubble all the way to the top for debugging purposes.
		/// </remarks>
		/// <param name="drawArgs">A <see cref="DrawArgs"/> object that specifies the graphics surface and the scene graph to be rendered.</param>
		/// <exception cref="RenderingException">Thrown if any <see cref="Exception"/> is encountered in the rendering pipeline.</exception>
		public virtual void Draw(DrawArgs drawArgs)
		{
			try
			{
				Initialize(drawArgs); 
				
				if (drawArgs.RenderingSurface.ClientRectangle.Width == 0 || drawArgs.RenderingSurface.ClientRectangle.Height == 0)
					return;
								
				if (DrawMode == DrawMode.Render)
					Render();
				else
					Refresh();
			}
			catch (Exception e)
			{
				throw new RenderingException(e, drawArgs);
			}
			finally
			{
				_sceneGraph = null;
				_surface = null;
			}
		}

		/// <summary>
		/// Factory method for an <see cref="IRenderingSurface"/>.
		/// </summary>
		public abstract IRenderingSurface GetRenderingSurface(IntPtr windowID, int width, int height);

		/// <summary>
		/// Initializes the member variables before calling <see cref="Render"/> or <see cref="Refresh"/>.
		/// </summary>
		protected virtual void Initialize(DrawArgs drawArgs)
		{
			_drawMode = drawArgs.DrawMode;
			_sceneGraph = drawArgs.SceneGraph;
			_surface = drawArgs.RenderingSurface;
		}

		/// <summary>
		/// Traverses and draws the scene graph.  
		/// </summary>
		/// <remarks>
		/// Inheritors should override this method to do any necessary work before calling the base method.
		/// </remarks>
		protected virtual void Render()
		{
			DrawSceneGraph(SceneGraph);
			DrawTextOverlay(SceneGraph.ParentPresentationImage);
		}

		/// <summary>
		/// Called when <see cref="DrawArgs.DrawMode"/> is equal to <b>DrawMode.Refresh</b>.
		/// </summary>
		/// <remarks>
		/// Inheritors must implement this method.
		/// </remarks>
		protected abstract void Refresh();

		/// <summary>
		/// Traverses and Renders the Scene Graph.
		/// </summary>
		protected void DrawSceneGraph(CompositeGraphic sceneGraph)
		{
			foreach (Graphic graphic in sceneGraph.Graphics)
			{
				if (graphic.Visible)
				{
					graphic.OnDrawing();

					if (graphic is CompositeGraphic)
						DrawSceneGraph((CompositeGraphic)graphic);
					else if (graphic is ImageGraphic)
						DrawImageGraphic((ImageGraphic)graphic);
					else if (graphic is LinePrimitive)
						DrawLinePrimitive((LinePrimitive)graphic);
					else if (graphic is InvariantLinePrimitive)
						DrawInvariantLinePrimitive((InvariantLinePrimitive)graphic);
					else if (graphic is CurvePrimitive)
						DrawCurvePrimitive((CurvePrimitive)graphic);
					else if (graphic is RectanglePrimitive)
						DrawRectanglePrimitive((RectanglePrimitive)graphic);
					else if (graphic is InvariantRectanglePrimitive)
						DrawInvariantRectanglePrimitive((InvariantRectanglePrimitive)graphic);
					else if (graphic is EllipsePrimitive)
						DrawEllipsePrimitive((EllipsePrimitive)graphic);
					else if (graphic is InvariantEllipsePrimitive)
						DrawInvariantEllipsePrimitive((InvariantEllipsePrimitive)graphic);
					else if (graphic is ArcPrimitive)
						DrawArcPrimitive((IArcGraphic)graphic);
					else if (graphic is InvariantArcPrimitive)
						DrawArcPrimitive((IArcGraphic)graphic);
					else if (graphic is PointPrimitive)
						DrawPointPrimitive((PointPrimitive)graphic);
					else if (graphic is InvariantTextPrimitive)
						DrawTextPrimitive((InvariantTextPrimitive)graphic);
				}
			}
		}

		/// <summary>
		/// Draws the Text Overlay.
		/// </summary>
		protected void DrawTextOverlay(IPresentationImage presentationImage)
		{
			CodeClock clock = new CodeClock();
			clock.Start();

			if (presentationImage == null || !(presentationImage is IAnnotationLayoutProvider))
				return;

			IAnnotationLayout layout = ((IAnnotationLayoutProvider)presentationImage).AnnotationLayout;
			if (layout == null || !layout.Visible)
				return;

			foreach (AnnotationBox annotationBox in layout.AnnotationBoxes)
			{
				if (annotationBox.Visible)
				{
					string annotationText = annotationBox.GetAnnotationText(presentationImage);
					if (!String.IsNullOrEmpty(annotationText))
						DrawAnnotationBox(annotationText, annotationBox);
				}
			}

			clock.Stop();
			PerformanceReportBroker.PublishReport("RendererBase", "DrawTextOverlay", clock.Seconds);
		}

		/// <summary>
		/// Draws an <see cref="ImageGraphic"/>.  Must be overridden and implemented.
		/// </summary>
		protected abstract void DrawImageGraphic(ImageGraphic imageGraphic);

		/// <summary>
		/// Draws a <see cref="LinePrimitive"/>.  Must be overridden and implemented.
		/// </summary>
		protected abstract void DrawLinePrimitive(LinePrimitive line);

		/// <summary>
		/// Draws a <see cref="InvariantLinePrimitive"/>.  Must be overridden and implemented.
		/// </summary>
		protected abstract void DrawInvariantLinePrimitive(InvariantLinePrimitive line);

		/// <summary>
		/// Draws a <see cref="CurvePrimitive"/>. Must be overridden and implemented.
		/// </summary>
		protected abstract void DrawCurvePrimitive(CurvePrimitive curve);

		/// <summary>
		/// Draws a <see cref="RectanglePrimitive"/>.  Must be overridden and implemented.
		/// </summary>
		protected abstract void DrawRectanglePrimitive(RectanglePrimitive rectangle);

		/// <summary>
		/// Draws a <see cref="InvariantRectanglePrimitive"/>.  Must be overridden and implemented.
		/// </summary>
		protected abstract void DrawInvariantRectanglePrimitive(InvariantRectanglePrimitive rectangle);

		/// <summary>
		/// Draws a <see cref="EllipsePrimitive"/>.  Must be overridden and implemented.
		/// </summary>
		protected abstract void DrawEllipsePrimitive(EllipsePrimitive ellipse);

		/// <summary>
		/// Draws a <see cref="InvariantEllipsePrimitive"/>.  Must be overridden and implemented.
		/// </summary>
		protected abstract void DrawInvariantEllipsePrimitive(InvariantEllipsePrimitive ellipse);

		/// <summary>
		/// Draws a <see cref="ArcPrimitive"/>.  Must be overridden and implemented.
		/// </summary>
		protected abstract void DrawArcPrimitive(IArcGraphic arc);

		/// <summary>
		/// Draws a <see cref="PointPrimitive"/>.  Must be overridden and implemented.
		/// </summary>
		protected abstract void DrawPointPrimitive(PointPrimitive pointPrimitive);

		/// <summary>
		/// Draws an <see cref="InvariantTextPrimitive"/>.  Must be overridden and implemented.
		/// </summary>
		protected abstract void DrawTextPrimitive(InvariantTextPrimitive textPrimitive);

		/// <summary>
		/// Draws an <see cref="AnnotationBox"/>.  Must be overridden and implemented.
		/// </summary>
		protected abstract void DrawAnnotationBox(string annotationText, AnnotationBox annotationBox);

		/// <summary>
		/// Draws an error message in the Scene Graph's client area of the screen.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method was deprecated in favour of allowing the render client code to handle errors in
		/// a manner suitable for the context in which it is called. For example, the view control for an
		/// <see cref="ITile"/> may wish to display the error message in the tile's client area <i>without
		/// crashing the control</i>, whereas an image export routine may wish to notify the user via an error
		/// dialog and have the export output <i>fail to be created</i>. Automated routines (such as unit
		/// tests) may even wish that the exception bubble all the way to the top for debugging purposes.
		/// </para>
		/// <para>
		/// For these reasons, this method is no longer called by <see cref="RendererBase"/>, although
		/// individual renderer implementations may still render error messages if, even after consideration
		/// of the above listed scenarios, it is determined that the exception should be handled internally.
		/// </para>
		/// </remarks>
		[Obsolete("Renderer implementations are no longer responsible for handling render pipeline errors.")]
		protected virtual void ShowErrorMessage(string message) {}

		#region Disposal

		/// <summary>
		/// Dispose method.  Inheritors should override this method to do any additional cleanup.
		/// </summary>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
			}
		}

		#region IDisposable Members

		/// <summary>
		/// Dispose method.
		/// </summary>
		public void Dispose()
		{
			try
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
			catch(Exception e)
			{
				Platform.Log(LogLevel.Error, e);
			}
		}

		#endregion
		#endregion
	}
}