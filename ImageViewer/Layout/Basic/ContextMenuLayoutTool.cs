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
using ClearCanvas.Desktop.Actions;
using ClearCanvas.ImageViewer.BaseTools;
using ClearCanvas.Common.Utilities;
using ClearCanvas.ImageViewer.Common;
using ClearCanvas.ImageViewer.Comparers;
using ClearCanvas.ImageViewer.StudyManagement;
using ClearCanvas.Common;

namespace ClearCanvas.ImageViewer.Layout.Basic
{
	/// <summary>
    /// This tool runs an instance of <see cref="LayoutComponent"/> in a shelf, and coordinates
    /// it so that it reflects the state of the active workspace.
	/// </summary>
	[ActionPlaceholder("display0", "imageviewer-contextmenu/DisplaySets", "DisplaySets")]
	[ExtensionOf(typeof(ImageViewerToolExtensionPoint))]
	public partial class ContextMenuLayoutTool : ImageViewerTool
	{
    	private const string _contextMenuSite = "imageviewer-contextmenu";
		private static readonly List<IActionFactory> _actionFactories = CreateActionFactories();
		private static readonly DefaultContextMenuActionFactory _defaultActionFactory = new DefaultContextMenuActionFactory();

		private List<string> _currentPathElements;

		private ImageSetGroups _imageSetGroups;
		private readonly Dictionary<string, IImageSet> _unavailableImageSets;
		private readonly IComparer<IImageSet> _comparer = new StudyDateComparer();

		private readonly IPatientReconciliationStrategy _patientReconciliationStrategy = new DefaultPatientReconciliationStrategy();

		public ContextMenuLayoutTool()
		{
			_unavailableImageSets = new Dictionary<string, IImageSet>();
		}

		private static List<IActionFactory> CreateActionFactories()
		{
			List<IActionFactory> factories = new List<IActionFactory>();

			try
			{
				foreach (IActionFactory factory in new ActionFactoryExtensionPoint().CreateExtensions())
					factories.Add(factory);
			}
			catch (NotSupportedException)
			{
				throw;
			}
			catch(Exception e)
			{
				Platform.Log(LogLevel.Debug, e, "Exception encountered while trying to create context menu action factories.");
			}

			return factories;
		}

		public override IActionSet Actions
		{
			get { return base.Actions.Union(GetDisplaySetActions()); }
		}
		
		/// <summary>
        /// Overridden to subscribe to workspace activation events
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

			_patientReconciliationStrategy.SetStudyTree(base.ImageViewer.StudyTree);
			_imageSetGroups = new ImageSetGroups(base.Context.Viewer.LogicalWorkspace.ImageSets);

			base.ImageViewer.EventBroker.StudyLoaded += OnStudyLoaded;
			base.ImageViewer.EventBroker.StudyLoadFailed += OnLoadPriorStudyFailed;
		}

		protected override void Dispose(bool disposing)
		{
			base.ImageViewer.EventBroker.StudyLoadFailed -= OnLoadPriorStudyFailed;
			base.ImageViewer.EventBroker.StudyLoaded -= OnStudyLoaded;

			_imageSetGroups.Dispose();

			foreach (IImageSet imageSet in _unavailableImageSets.Values)
				imageSet.Dispose();

			base.Dispose(disposing);
		}

		private void OnStudyLoaded(object sender, StudyLoadedEventArgs e)
		{
			IImageSet unavailableImageSet;
			string studyInstanceUid = e.Study.StudyInstanceUid;
			if (_unavailableImageSets.TryGetValue(studyInstanceUid, out unavailableImageSet))
			{
				_imageSetGroups.Root.Remove(unavailableImageSet);
				_unavailableImageSets.Remove(studyInstanceUid);
			}
		}

		private void OnLoadPriorStudyFailed(object sender, StudyLoadFailedEventArgs e)
		{
			bool notFoundError = e.Error is NotFoundLoadStudyException;
			if (!notFoundError && (e.Error is LoadSopsException || e.Error is StudyLoaderNotFoundException))
			{
				if (null != CollectionUtils.SelectFirst(base.ImageViewer.LogicalWorkspace.ImageSets,
				                                        imageSet => imageSet.Uid == e.Study.StudyInstanceUid))
				{
					return;
				}

				if (_unavailableImageSets.ContainsKey(e.Study.StudyInstanceUid))
					return;

				var reconciled = _patientReconciliationStrategy.ReconcilePatientInformation(e.Study);
				if (reconciled == null)
					return;

				var studyItem = new StudyItem(reconciled, e.Study, e.Study.Server, e.Study.StudyLoaderName);

				ImageSetDescriptor descriptor = new UnavailableImageSetDescriptor(studyItem, e.Error);
				ImageSet unavailableImageSet = new ImageSet(descriptor);

				_imageSetGroups.Root.Add(unavailableImageSet);
				_unavailableImageSets[studyItem.StudyInstanceUid] = unavailableImageSet;
			}
		}

    	/// <summary>
		/// Gets an array of <see cref="IAction"/> objects that allow selection of specific display
		/// sets for display in the currently selected image box.
		/// </summary>
		/// <returns></returns>
		private IActionSet GetDisplaySetActions()
		{
#if TRACEGROUPS
			TraceGroups();
#endif
    		string rootPath = _contextMenuSite;

			_currentPathElements = new List<string>();
			List<IAction> actions = new List<IAction>();

			FilteredGroup<IImageSet> rootGroup = GetRootGroup(_imageSetGroups.Root);
			if (rootGroup != null)
			{
				ActionFactoryContext context = new ActionFactoryContext
				{
					DesktopWindow = Context.DesktopWindow,
					ImageViewer = Context.Viewer,
					Namespace = GetType().FullName,
					ActionPlaceholder = ActionPlaceholder.GetPlaceholderAction(_contextMenuSite, base.Actions, "display0")
				};

				bool showImageSetNames = base.ImageViewer.LogicalWorkspace.ImageSets.Count > 1 || _unavailableImageSets.Count > 0;
				int loadingPriorsNumber = 0;

				foreach (FilteredGroup<IImageSet> group in TraverseImageSetGroups(rootGroup, rootPath))
				{
					string basePath = StringUtilities.Combine(_currentPathElements, "/");

					//not incredibly efficient, but there really aren't that many items.
					List<IImageSet> orderedItems = new List<IImageSet>(group.Items);
					orderedItems.Sort(_comparer);

					foreach (IImageSet imageSet in orderedItems)
					{
						string imageSetPath;
						if (showImageSetNames)
							imageSetPath = String.Format("{0}/{1}", basePath, imageSet.Name.Replace("/", "-"));
						else
							imageSetPath = basePath;

						context.Initialize(imageSet, imageSetPath);
						
						foreach (IActionFactory factory in _actionFactories)
							actions.AddRange(factory.CreateActions(context));

						if (actions.Count == 0 || !context.ExcludeDefaultActions)
							actions.AddRange(_defaultActionFactory.CreateActions(context));
					}

					if (group.Items.Count > 0 && base.ImageViewer.PriorStudyLoader.IsActive)
						actions.Add(CreateLoadingPriorsAction(basePath, ++loadingPriorsNumber));
				}
			}

			return new ActionSet(actions);
		}

		private IEnumerable<FilteredGroup<IImageSet>> TraverseImageSetGroups(FilteredGroup<IImageSet> group, string rootPath)
		{
			List<IImageSet> allItems = group.GetAllItems();
			if (allItems.Count != 0)
			{
				if (_currentPathElements.Count == 0)
					_currentPathElements.Add(rootPath);
				else
					_currentPathElements.Add(group.Label.Replace("/", "-"));

				yield return group;
			}

			foreach (FilteredGroup<IImageSet> child in group.ChildGroups)
			{
				foreach (FilteredGroup<IImageSet> nonEmptyChild in TraverseImageSetGroups(child, rootPath))
					yield return nonEmptyChild;
			}

			if (allItems.Count != 0)
				_currentPathElements.RemoveAt(_currentPathElements.Count - 1);
		}

		private FilteredGroup<IImageSet> GetRootGroup(FilteredGroup<IImageSet> group)
		{
			if (group.HasItems)
				return group;

			int validChildGroups = 0;
    		foreach (FilteredGroup<IImageSet> child in group.ChildGroups)
    		{
    			if (child.GetAllItems().Count > 0)
    				++validChildGroups;
    		}

			//if this group has more than one child group with items anywhere in it's tree, then it's first.
			if (validChildGroups > 1)
				return group;

			foreach (FilteredGroup<IImageSet> child in group.ChildGroups)
			{
				FilteredGroup<IImageSet> rootGroup = GetRootGroup(child);
				if (rootGroup != null)
					return rootGroup;
			}

    		return null;
		}

		private IClickAction CreateLoadingPriorsAction(string basePath, int number)
		{
			string pathString = String.Format("{0}/loadingPriors", basePath);
			ActionPath path = new ActionPath(pathString, null);
			MenuAction action = new MenuAction(string.Format("{0}:loadingPriors{1}", GetType().FullName, number), path, ClickActionFlags.None, null)
			                    	{
			                    		Label = SR.LabelLoadingPriors,
			                    		Persistent = false
			                    	};
			action.SetClickHandler(delegate { });
			return action;
		}

#if TRACEGROUPS

		private void TraceGroups()
		{
			TraceGroup(_imageSetGroups.Root, _imageSetGroups.Root.Name);
		}

		private void TraceGroup(FilteredGroup<IImageSet> group, string currentGroupPath)
		{
			foreach (IImageSet imageSet in group.Items)
			{
				string imageSetPath = String.Format("{0}/{1}", currentGroupPath, imageSet.Name);
				Trace.WriteLine(imageSetPath);
			}

			foreach (FilteredGroup<IImageSet> childGroup in group.ChildGroups)
			{
				string name = childGroup.Label;
				string groupPath = String.Format("{0}/{1}", currentGroupPath, name);
				TraceGroup(childGroup, groupPath);
			}
		}
#endif
	}
}
