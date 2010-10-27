#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using ClearCanvas.Common.Utilities;

namespace ClearCanvas.ImageViewer.Volume.Mpr
{
	//TODO (cr Oct 2009): if the slice sets were display set descriptors, 
	//you could just attach them to the display set and this class could be removed.
	[Cloneable]
	public class MprDisplaySet : DisplaySet, IDisplaySet
	{
		[CloneCopyReference]
		private IMprSliceSet _sliceSet;

		public MprDisplaySet(string name, IMprSliceSet sliceSet)
			: base(name, sliceSet.Uid)
		{
			_sliceSet = sliceSet;
			_sliceSet.SliceSopsChanged += sliceSet_SliceSopsChanged;

			FillPresentationImages();
		}

		protected MprDisplaySet(MprDisplaySet source, ICloningContext context)
		{
			context.CloneFields(source, this);
		}

		[OnCloneComplete]
		private void OnCloneComplete()
		{
			_sliceSet.SliceSopsChanged += sliceSet_SliceSopsChanged;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_sliceSet.SliceSopsChanged -= sliceSet_SliceSopsChanged;
				_sliceSet = null;
			}
			base.Dispose(disposing);
		}

		public IMprSliceSet SliceSet
		{
			get { return _sliceSet; }
		}

		IDisplaySet IDisplaySet.CreateFreshCopy()
		{
			return (IDisplaySet) CloneBuilder.Clone(this);
		}

		IDisplaySet IDisplaySet.Clone()
		{
			return (IDisplaySet) CloneBuilder.Clone(this);
		}

		private void sliceSet_SliceSopsChanged(object sender, EventArgs e)
		{
			// clear old presentation images
			List<IPresentationImage> images = new List<IPresentationImage>(this.PresentationImages);
			this.PresentationImages.Clear();
			foreach (IPresentationImage image in this.PresentationImages)
				image.Dispose();

			// repopulate with new slices
			this.FillPresentationImages();
		}

		private void FillPresentationImages()
		{
			foreach (MprSliceSop sop in _sliceSet.SliceSops)
			{
				foreach (IPresentationImage image in PresentationImageFactory.Create(sop))
					base.PresentationImages.Add(image);
			}
		}

		public override object CreateMemento()
		{
			object baseDisplaySetMemento = base.CreateMemento();

			return new MprDisplaySetMemento(this, baseDisplaySetMemento);
		}

		public override void SetMemento(object memento)
		{
			MprDisplaySetMemento mprDisplaySetMemento = memento as MprDisplaySetMemento;
			if (mprDisplaySetMemento == null)
				return;

			IMprStandardSliceSet sliceSet = _sliceSet as IMprStandardSliceSet;
			if (sliceSet != null && !sliceSet.IsReadOnly && mprDisplaySetMemento.SlicerParams != null)
				sliceSet.SlicerParams = mprDisplaySetMemento.SlicerParams;

			if (this.ImageBox != null && mprDisplaySetMemento.SliceIndex >= 0 && mprDisplaySetMemento.SliceIndex < this.PresentationImages.Count)
				this.ImageBox.TopLeftPresentationImage = this.PresentationImages[mprDisplaySetMemento.SliceIndex];

			base.SetMemento(mprDisplaySetMemento.DisplaySetMemento);
		}

		private class MprDisplaySetMemento
		{
			public readonly IVolumeSlicerParams SlicerParams;
			public readonly int SliceIndex;
			public readonly object DisplaySetMemento;

			public MprDisplaySetMemento(MprDisplaySet mprDisplaySet, object displaySetMemento)
			{
				if (mprDisplaySet.ImageBox != null)
					this.SliceIndex = mprDisplaySet.ImageBox.TopLeftPresentationImageIndex;

				if (mprDisplaySet.SliceSet is IMprStandardSliceSet)
					this.SlicerParams = ((IMprStandardSliceSet) mprDisplaySet.SliceSet).SlicerParams;

				this.DisplaySetMemento = displaySetMemento;
			}
		}
	}
}