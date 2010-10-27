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
using ClearCanvas.Common.Specifications;
using ClearCanvas.Common;
using ClearCanvas.ImageViewer.Common;

namespace ClearCanvas.ImageViewer
{
	internal class PatientInfoSpecification : ImageSetSpecification
	{
		private readonly IImageSet _referenceImageSet;

		public PatientInfoSpecification(IImageSet referenceImageSet)
		{
			_referenceImageSet = referenceImageSet;
		}

		public override TestResult Test(IImageSet imageSet)
		{
			if (imageSet.PatientInfo == _referenceImageSet.PatientInfo)
				return new TestResult(true);
			else
				return new TestResult(false);
		}
	}

	/// <summary>
	/// A <see cref="ISpecification"/> class for use with <see cref="IImageSet"/>s.
	/// </summary>
	public abstract class ImageSetSpecification : ISpecification
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public ImageSetSpecification()
		{
		}

		#region ISpecification Members

		TestResult ISpecification.Test(object obj)
		{
			if (obj is IImageSet)
				return Test(obj as IImageSet);
			else 
				return new TestResult(false);
		}

		#endregion

		/// <summary>
		/// Tests the given <see cref="IImageSet"/> against this specification.
		/// </summary>
		public abstract TestResult Test(IImageSet imageSet);
	}

	internal class PatientImageSetGroup : FilteredGroup<IImageSet>
	{
		internal PatientImageSetGroup(IImageSet sourceImageSet)
			: base("Patient", sourceImageSet.PatientInfo, new PatientInfoSpecification(sourceImageSet))
		{
		}
	}

	internal class PatientImageSetGroupFactory : IFilteredGroupFactory<IImageSet>
	{
		internal PatientImageSetGroupFactory()
		{
		}

		#region IFilteredGroupFactory<IImageSet> Members

		public FilteredGroup<IImageSet> Create(IImageSet item)
		{
			return new PatientImageSetGroup(item);
		}

		#endregion
	}

	/// <summary>
	/// A convenient class that can be used to filter <see cref="IImageSet"/>s into related groups.
	/// </summary>
	/// <remarks>
	/// The real power of this class is that it responds to changes in <see cref="SourceImageSets"/>,
	/// which is an <see cref="ObservableList{TItem}">observable list</see> of <see cref="IImageSet"/>s.
	/// </remarks>
	public class ImageSetGroups : IDisposable
	{
		private readonly RootFilteredGroup<IImageSet> _root;
		private ObservableList<IImageSet> _sourceImageSets;

		/// <summary>
		/// Constructor.
		/// </summary>
		public ImageSetGroups()
		{
			_root = new RootFilteredGroup<IImageSet>("Root", "All Patients", new PatientImageSetGroupFactory());
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public ImageSetGroups(ObservableList<IImageSet> sourceImageSets)
			: this()
		{
			SourceImageSets = sourceImageSets;
		}

		/// <summary>
		/// Gets the <see cref="RootFilteredGroup{T}">root</see> <see cref="FilteredGroup{T}">filtered group</see>.
		/// </summary>
		public RootFilteredGroup<IImageSet> Root
		{
			get { return _root; }	
		}

		/// <summary>
		/// Gets or sets the underlying list of <see cref="IImageSet"/>s to be observed and filtered.
		/// </summary>
		public ObservableList<IImageSet> SourceImageSets
		{
			set
			{
				if (_sourceImageSets == value)
					return;

				if (_sourceImageSets != null)
				{
					_sourceImageSets.ItemAdded -= OnImageSetAdded;
					_sourceImageSets.ItemChanging -= OnImageSetChanging;
					_sourceImageSets.ItemChanged -= OnImageSetChanged;
					_sourceImageSets.ItemRemoved -= OnImageSetRemoved;

					_root.Clear();
				}

				_sourceImageSets = value;
				if (_sourceImageSets != null)
				{
					_root.Add(_sourceImageSets);

					_sourceImageSets.ItemAdded += OnImageSetAdded;
					_sourceImageSets.ItemChanging += OnImageSetChanging;
					_sourceImageSets.ItemChanged += OnImageSetChanged;
					_sourceImageSets.ItemRemoved += OnImageSetRemoved;
				}
			}
			get
			{
				return _sourceImageSets;
			}
		}

		/// <summary>
		/// Implementation of the <see cref="IDisposable"/> pattern.
		/// </summary>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				SourceImageSets = null;
			}
		}

		#region IDisposable Members

		/// <summary>
		/// Releases any resources used by this object.
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
				Platform.Log(LogLevel.Warn, e, "An unexpected error has occurred.");
			}
		}

		#endregion

		private void OnImageSetAdded(object sender, ListEventArgs<IImageSet> e)
		{
			_root.Add(e.Item);
		}
		private void OnImageSetChanging(object sender, ListEventArgs<IImageSet> e)
		{
			_root.Remove(e.Item);
		}
		private void OnImageSetChanged(object sender, ListEventArgs<IImageSet> e)
		{
			_root.Add(e.Item);
		}
		private void OnImageSetRemoved(object sender, ListEventArgs<IImageSet> e)
		{
			_root.Remove(e.Item);
		}
	}
}