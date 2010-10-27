#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System.Collections.Generic;
using ClearCanvas.Common.Utilities;
using ClearCanvas.ImageViewer.Graphics;

namespace ClearCanvas.ImageViewer.Tools.Synchronization
{
	// NOTE: Because the synchronization tools act based on events coming from the viewer event broker,
	// they cannot be responsible for drawing the images because that would result in repeated (unnecessary)
	// Draw() calls.  The reference line tool is dependent on the stacking synchronization tool in that 
	// it needs to synchronize the images first, before the reference lines are drawn, otherwise images
	// that are about to become invisible will be unnecessarily drawn.
	//
	// This class acts as a mediator, listening to events from the viewer event broker and coordinating
	// the synchronization tools (the order they are called in, as well as drawing the affected images
	// such that none are redrawn unnecessarily).
	//
	// There may be a more general solution for coordinating draws, but this problem may be 
	// more or less isolated to synchronization tools, so we will just use this class for
	// now rather than spend the time to develop a general solution unnecessarily.

	internal class SynchronizationToolCoordinator
	{
		#region SynchronizationToolCompositeGraphic class

		// The mediator provides each tool with its own CompositeGraphic to work with, while
		// maintaining control over the order in which these graphics appear.
		private class SynchronizationToolCompositeGraphic : CompositeGraphic
		{
			public SynchronizationToolCompositeGraphic()
			{
				// reference lines on the bottom
				base.Graphics.Add(new ReferenceLineCompositeGraphic());
				// reference points on top
				base.Graphics.Add(new CompositeGraphic());
			}

			public ReferenceLineCompositeGraphic ReferenceLineCompositeGraphic
			{
				get { return (ReferenceLineCompositeGraphic)base.Graphics[0]; }
			}

			public CompositeGraphic SpatialLocatorCompositeGraphic
			{
				get { return (CompositeGraphic)base.Graphics[1]; }
			}
		}

		#endregion

		#region Private Fields

		private static readonly Dictionary<IImageViewer, SynchronizationToolCoordinator> _coordinators;

		private readonly IImageViewer _viewer;
		
		private StackingSynchronizationTool _stackingSynchronizationTool;
		private ReferenceLineTool _referenceLineTool;
		private SpatialLocatorTool _spatialLocatorTool;

		private int _referenceCount = 0;

		#endregion

		static SynchronizationToolCoordinator()
		{
			_coordinators = new Dictionary<IImageViewer, SynchronizationToolCoordinator>();
		}

		private SynchronizationToolCoordinator(IImageViewer viewer)
		{
			_viewer = viewer;
		}

		public void SetStackingSynchronizationTool(StackingSynchronizationTool tool)
		{
			_stackingSynchronizationTool = tool;
		}

		public void SetReferenceLineTool(ReferenceLineTool tool)
		{
			_referenceLineTool = tool;
		}

		public void SetSpatialLocatorTool(SpatialLocatorTool tool)
		{
			_spatialLocatorTool = tool;
		}

		private static void Draw<T>(IEnumerable<T> itemsToDraw)
			where T : IDrawable
		{
			foreach (IDrawable drawable in itemsToDraw)
				drawable.Draw();
		}

		private void DrawImageBoxes(IEnumerable<IImageBox> imageBoxes)
		{
			List<IImageBox> imageBoxesToDraw = new List<IImageBox>(imageBoxes);

			//Then calculate the reference lines.
			_referenceLineTool.RefreshAllReferenceLines();

			foreach (IPresentationImage image in _referenceLineTool.GetImagesToRedraw())
			{
				//Only draw images that won't be drawn as a result of the image boxes being drawn.
				if (!imageBoxesToDraw.Contains(image.ParentDisplaySet.ImageBox))
					image.Draw();
			}

			Draw(imageBoxesToDraw);
		}

		private void OnTileSelected(object sender, TileSelectedEventArgs e)
		{
			if (e.SelectedTile.PresentationImage != null)
			{
				//the presentation image selected event will fire and take care of this.
				return;
			}

			_referenceLineTool.RefreshAllReferenceLines();
			Draw(_referenceLineTool.GetImagesToRedraw());
		}

		private void OnPresentationImageSelected(object sender, PresentationImageSelectedEventArgs e)
		{
			_stackingSynchronizationTool.SynchronizeImageBoxes();
			OnSynchronizedImageBoxes();
		}

		private static SynchronizationToolCompositeGraphic GetSynchronizationToolCompositeGraphic(IPresentationImage image)
		{
			if (image is IApplicationGraphicsProvider)
			{
				GraphicCollection overlayGraphics = ((IApplicationGraphicsProvider)image).ApplicationGraphics;
				SynchronizationToolCompositeGraphic container = CollectionUtils.SelectFirst(overlayGraphics,
																	delegate(IGraphic graphic)
																	{
																		return graphic is SynchronizationToolCompositeGraphic;
																	}) as SynchronizationToolCompositeGraphic;

				if (container == null)
					overlayGraphics.Insert(0, container = new SynchronizationToolCompositeGraphic());

				container.Visible = true;
				return container;
			}

			return null;
		}


		public static SynchronizationToolCoordinator Get(IImageViewer viewer)
		{
			if (!_coordinators.ContainsKey(viewer))
			{
				SynchronizationToolCoordinator coordinator = new SynchronizationToolCoordinator(viewer);

				viewer.EventBroker.PresentationImageSelected += coordinator.OnPresentationImageSelected;
				viewer.EventBroker.TileSelected += coordinator.OnTileSelected;
			
				_coordinators.Add(viewer, coordinator);
			}

			DicomImagePlane.InitializeCache();

			++_coordinators[viewer]._referenceCount;
			return _coordinators[viewer];
		}

		public void Release()
		{
			DicomImagePlane.ReleaseCache();

			--_referenceCount;
			if (_referenceCount <= 0)
			{
				_viewer.EventBroker.PresentationImageSelected -= OnPresentationImageSelected;
				_viewer.EventBroker.TileSelected -= OnTileSelected;

				_coordinators.Remove(_viewer);
			}
		}

		public ReferenceLineCompositeGraphic GetReferenceLineCompositeGraphic(IPresentationImage image)
		{
			SynchronizationToolCompositeGraphic container = GetSynchronizationToolCompositeGraphic(image);
			if (container == null)
				return null;

			return container.ReferenceLineCompositeGraphic;
		}

		public CompositeGraphic GetSpatialLocatorCompositeGraphic(IPresentationImage image)
		{
			SynchronizationToolCompositeGraphic container = GetSynchronizationToolCompositeGraphic(image);
			if (container == null)
				return null;

			return container.SpatialLocatorCompositeGraphic;
		}

		public void OnSynchronizedImageBoxes()
		{
			DrawImageBoxes(_stackingSynchronizationTool.GetImageBoxesToDraw());
		}

		public void OnSpatialLocatorCrosshairsUpdated()
		{
			//The spatial locator and stacking sync tool conflict.
			_stackingSynchronizationTool.SynchronizeActive = false;

			DrawImageBoxes(_spatialLocatorTool.GetImageBoxesToRedraw());
		}

		public void OnSpatialLocatorStopped()
		{
			Draw(_spatialLocatorTool.GetImageBoxesToRedraw());
		}
		
		public void OnRefreshedReferenceLines()
		{
			Draw(_referenceLineTool.GetImagesToRedraw());
		}
	}
}
