#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections;
using System.Collections.Generic;

namespace ClearCanvas.ImageViewer.PresentationStates.Dicom
{
	internal class DicomPresentationImageCollection<T> : IEnumerable<T> where T : IDicomPresentationImage
	{
		private readonly List<T> _images;
		private string _studyUid = null;
		private Dictionary<string, List<T>> _dictionary = null;

		public DicomPresentationImageCollection()
		{
			_images = new List<T>();
		}

		public DicomPresentationImageCollection(IEnumerable<T> images)
		{
			_images = new List<T>(images);

			if (_images.Count > 0)
				_studyUid = _images[0].ImageSop.StudyInstanceUid;
		}

		private Dictionary<string, List<T>> Dictionary
		{
			get
			{
				if (_dictionary == null)
				{
					_dictionary = new Dictionary<string, List<T>>();
					foreach (T image in _images)
					{
						string seriesUid = image.ImageSop.SeriesInstanceUid;
						if (!_dictionary.ContainsKey(seriesUid))
							_dictionary.Add(seriesUid, new List<T>());
						_dictionary[seriesUid].Add(image);
					}
				}
				return _dictionary;
			}
		}

		public void Add(T image)
		{
			if (_dictionary != null)
				throw new InvalidOperationException();
			if (_studyUid != null && _studyUid != image.ImageSop.StudyInstanceUid)
				throw new ArgumentException();
			else if (_studyUid == null)
				_studyUid = image.ImageSop.StudyInstanceUid;

			_images.Add(image);
		}

		public int Count
		{
			get { return _images.Count; }
		}

		public T FirstImage
		{
			get
			{
				if (_images.Count == 0)
					return default(T);
				return _images[0];
			}
		}

		public IEnumerable<string> EnumerateSeries()
		{
			return this.Dictionary.Keys;
		}

		public IEnumerable<T> EnumerateImages()
		{
			return _images;
		}

		public IEnumerable<T> EnumerateImages(string seriesUid)
		{
			if (_dictionary.ContainsKey(seriesUid))
			{
				foreach (T image in _dictionary[seriesUid])
					yield return image;
			}
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return this.EnumerateImages().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.EnumerateImages().GetEnumerator();
		}
	}
}