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
using ClearCanvas.ImageViewer.PresentationStates.Dicom;
using ClearCanvas.ImageViewer.PresentationStates.Dicom.GraphicAnnotationSerializers;
using ClearCanvas.ImageViewer.RoiGraphics;

namespace ClearCanvas.ImageViewer.Graphics
{
	/// <summary>
	/// Defines <see cref="IGraphic"/>s which follow the <i>decorator pattern</i> to provide
	/// modify and/or add functionality to an existing <see cref="IGraphic"/>.
	/// </summary>
	public interface IDecoratorGraphic : IGraphic
	{
		/// <summary>
		/// Gets the <see cref="IGraphic"/> decorated by this graphic.
		/// </summary>
		IGraphic DecoratedGraphic { get; }
	}

	/// <summary>
	/// Base class for <see cref="IDecoratorGraphic"/> implementations.
	/// </summary>
	[Cloneable]
	[DicomSerializableGraphicAnnotation(typeof (DecoratorGraphicAnnotationSerializer))]
	public abstract class DecoratorCompositeGraphic : CompositeGraphic, IDecoratorGraphic
	{
		[CloneIgnore]
		private IGraphic _decoratedGraphic;

		/// <summary>
		/// Constructs a new <see cref="DecoratorCompositeGraphic"/>.
		/// </summary>
		/// <param name="graphic">The graphic to be decorated.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="graphic"/> is null.</exception>
		protected DecoratorCompositeGraphic(IGraphic graphic)
		{
			Platform.CheckForNullReference(graphic, "graphic");
			base.Graphics.Add(_decoratedGraphic = graphic);
		}

		/// <summary>
		/// Cloning constructor.
		/// </summary>
		/// <param name="source">The source object from which to clone.</param>
		/// <param name="context">The cloning context object.</param>
		protected DecoratorCompositeGraphic(DecoratorCompositeGraphic source, ICloningContext context)
		{
			context.CloneFields(source, this);
		}

		[OnCloneComplete]
		private void OnCloneComplete()
		{
			_decoratedGraphic = CollectionUtils.FirstElement(base.Graphics);
		}

		/// <summary>
		/// Gets the <see cref="IGraphic"/> decorated by this graphic.
		/// </summary>
		IGraphic IDecoratorGraphic.DecoratedGraphic
		{
			get { return this.DecoratedGraphic; }
		}

		/// <summary>
		/// Gets the <see cref="IGraphic"/> decorated by this graphic.
		/// </summary>
		protected IGraphic DecoratedGraphic
		{
			get { return _decoratedGraphic; }
		}

		/// <summary>
		/// Gets an object describing the region of interest on the <see cref="Graphic.ParentPresentationImage"/> selected by the <see cref="DecoratedGraphic"/>.
		/// </summary>
		/// <remarks>
		/// Decorated graphics that do not describe a region of interest may return null.
		/// </remarks>
		/// <returns>A <see cref="Roi"/> describing this region of interest, or null if the decorated graphic does not describe a region of interest.</returns>
		public override Roi GetRoi()
		{
			return this.DecoratedGraphic.GetRoi();
		}
	}
}