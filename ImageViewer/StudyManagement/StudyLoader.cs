#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;

namespace ClearCanvas.ImageViewer.StudyManagement
{
	/// <summary>
	/// Abstract base class for <see cref="IStudyLoader"/>.
	/// </summary>
	public abstract class StudyLoader : IStudyLoader
	{
		private readonly string _name;
		private IPrefetchingStrategy _prefetchingStrategy;
		private object _currentServer;

		/// <summary>
		/// Constructs a new <see cref="StudyLoader"/> with the given <paramref name="name"/>.
		/// </summary>
		protected StudyLoader(string name)
		{
			_name = name;
		}

		#region IStudyLoader Members

		/// <summary>
		/// Gets the name of the study loader.
		/// </summary>
		public string Name
		{
			get { return _name; }
		}

		/// <summary>
		/// Gets or sets the study loader's pixel data prefetching strategy.
		/// </summary>
		public IPrefetchingStrategy PrefetchingStrategy
		{
			get { return _prefetchingStrategy; }
			protected set { _prefetchingStrategy = value; }
		}

		/// <summary>
		/// Called by <see cref="Start"/> to begin prefetching.
		/// </summary>
		protected abstract int OnStart(StudyLoaderArgs studyLoaderArgs);

		/// <summary>
		/// Creates a <see cref="Sop"/> from the given <see cref="ISopDataSource"/>.
		/// </summary>
		protected virtual Sop CreateSop(ISopDataSource dataSource)
		{
			return Sop.Create(dataSource);
		}

		/// <summary>
		/// Loads the next <see cref="SopDataSource"/> from which a
		/// <see cref="Sop"/> will be created.
		/// </summary>
		/// <returns>The next <see cref="SopDataSource"/> or <b>null</b> if there are none remaining.</returns>
		/// <remarks>
		/// Implementers of <see cref="StudyLoader"/> should avoid loading pixel data
		/// in this method for performance reasons.
		/// </remarks>
		protected abstract SopDataSource LoadNextSopDataSource();


		/// <summary>
		/// Starts the enumeration of images that match the specified
		/// Study Instance UID.
		/// </summary>
		/// <param name="studyLoaderArgs"></param>
		/// <returns>Number of images in study.</returns>
		public int Start(StudyLoaderArgs studyLoaderArgs)
		{
			_currentServer = studyLoaderArgs.Server;

			try
			{
				return OnStart(studyLoaderArgs);
			}
			catch(LoadStudyException)
			{
				throw;
			}
			catch(Exception e)
			{
				throw new LoadStudyException(studyLoaderArgs.StudyInstanceUid, e);
			}
		}

		/// <summary>
		/// Loads the next <see cref="Sop"/>.
		/// </summary>
		/// <returns>The next <see cref="Sop"/> or <b>null</b> if there are none remaining.</returns>
		/// <remarks>
		/// Implementers of <see cref="IStudyLoader"/> should avoid loading pixel data
		/// in this method for performance reasons.
		/// </remarks>
		public Sop LoadNextSop()
		{
				SopDataSource dataSource = LoadNextSopDataSource();
				if (dataSource == null)
				{
					_currentServer = null;
					return null;
				}

				dataSource.StudyLoaderName = Name;
				dataSource.Server = _currentServer;

				return CreateSop(dataSource);
		}

		#endregion

		/// <summary>
		/// Creates all available study loaders.
		/// </summary>
		/// <returns>All the loaders, or an empty array if none exist.</returns>
		public static List<IStudyLoader> CreateAll()
		{
			var studyLoaders = new List<IStudyLoader>();

			try
			{
				var xp = new StudyLoaderExtensionPoint();
				foreach (IStudyLoader loader in xp.CreateExtensions())
					studyLoaders.Add(loader);
			}
			catch (NotSupportedException)
			{
			}

			return studyLoaders;
		}

		/// <summary>
		/// Creates a single study loader, if it exists.
		/// </summary>
		/// <returns>The loader, or null if it doesn't exist.</returns>
		public static IStudyLoader Create(string name)
		{
			return CollectionUtils.SelectFirst(CreateAll(), loader => loader.Name == name);
		}
	}
}
