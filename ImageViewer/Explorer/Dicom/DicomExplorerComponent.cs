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
using System.Security.Policy;
using ClearCanvas.Desktop;
using ClearCanvas.ImageViewer.Configuration.ServerTree;
using ClearCanvas.ImageViewer.StudyManagement;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Common;
using ClearCanvas.ImageViewer.Services.ServerTree;
using ClearCanvas.ImageViewer.Configuration;
using ClearCanvas.ImageViewer.Common;

namespace ClearCanvas.ImageViewer.Explorer.Dicom
{
	internal class DicomExplorerComponent : SplitComponentContainer
	{
		private static readonly object _syncLock = new object();
		private static readonly List<DicomExplorerComponent> _activeComponents = new List<DicomExplorerComponent>();

		private ServerTreeComponent _serverTreeComponent;
		private StudyBrowserComponent _studyBrowserComponent;

		private DicomExplorerComponent(SplitPane pane1, SplitPane pane2)
			: base(pane1, pane2, Desktop.SplitOrientation.Horizontal)
		{
		}

		public ServerTreeComponent ServerTreeComponent
		{
			get { return _serverTreeComponent; }
		}

		public StudyBrowserComponent StudyBrowserComponent
		{
			get { return _studyBrowserComponent; }
		}

		public SearchPanelComponent SearchPanelComponent
		{
			get { return _studyBrowserComponent.SearchPanelComponent; }
		}

		public override void Start()
		{
			base.Start();

			lock (_syncLock)
			{
				_activeComponents.Add(this);
			}
		}

		public override void Stop()
		{
			lock (_syncLock)
			{
				_activeComponents.Remove(this);
			}

			base.Stop();
		}

		public static List<DicomExplorerComponent> GetActiveComponents()
		{
			lock (_syncLock)
			{
				return new List<DicomExplorerComponent>(_activeComponents);
			}
		}

		public static DicomExplorerComponent Create()
		{
			ServerTreeComponent serverTreeComponent = new ServerTreeComponent();
			serverTreeComponent.ShowLocalDataStoreNode = HasLocalDatastoreSupport();

			bool hasEditPermission = PermissionsHelper.IsInRole(AuthorityTokens.Configuration.MyServers);
			serverTreeComponent.IsReadOnly = !hasEditPermission;

			StudyBrowserComponent studyBrowserComponent = new StudyBrowserComponent();

			serverTreeComponent.SelectedServerChanged +=
				delegate { studyBrowserComponent.SelectServerGroup(serverTreeComponent.SelectedServers); };

			SearchPanelComponent searchPanel = new SearchPanelComponent(studyBrowserComponent);

			SelectDefaultServerNode(serverTreeComponent);

			try
			{
				//explicitly check and make sure we're querying local only.
				if (serverTreeComponent.ShowLocalDataStoreNode && serverTreeComponent.SelectedServers.IsLocalDatastore)
					studyBrowserComponent.Search();
			}
			catch (PolicyException)
			{
				//TODO: ignore this on startup or show message?
			}
			catch (Exception e)
			{
				ExceptionHandler.Report(e, Application.ActiveDesktopWindow);
			}

			SplitPane leftPane = new SplitPane(SR.TitleServerTreePane, serverTreeComponent, 0.25f);
			SplitPane rightPane = new SplitPane(SR.TitleStudyBrowserPane, studyBrowserComponent, 0.75f);

			SplitComponentContainer bottomContainer =
				new SplitComponentContainer(
				leftPane,
				rightPane,
				SplitOrientation.Vertical);

			SplitPane topPane = new SplitPane(SR.TitleSearchPanelPane, searchPanel, true);
			SplitPane bottomPane = new SplitPane(SR.TitleStudyNavigatorPane, bottomContainer, false);

			DicomExplorerComponent component = new DicomExplorerComponent(topPane, bottomPane);
			component._studyBrowserComponent = studyBrowserComponent;
			component._serverTreeComponent = serverTreeComponent;
			return component;
		}

		internal void SelectDefaultServers()
		{
			SelectDefaultServers(_serverTreeComponent);
		}

		private static void SelectDefaultServerNode(ServerTreeComponent serverTreeComponent)
		{
			if (serverTreeComponent.ShowLocalDataStoreNode &&
				!DicomExplorerConfigurationSettings.Default.SelectDefaultServerOnStartup)
			{
				serverTreeComponent.SetSelection(serverTreeComponent.ServerTree.RootNode.LocalDataStoreNode);
			}
			else
			{
				SelectDefaultServers(serverTreeComponent);
			}
		}

		private static void SelectDefaultServers(ServerTreeComponent serverTreeComponent)
		{
			ServerTree serverTree = serverTreeComponent.ServerTree;

			List<Server> defaultServers = DefaultServers.SelectFrom(serverTree);
			CheckDefaultServers(serverTree, defaultServers);
			IServerTreeNode initialSelection = GetFirstDefaultServerOrGroup(serverTree.RootNode.ServerGroupNode);
			UncheckAllServers(serverTree);

			if (initialSelection == null)
			{
				if (serverTreeComponent.ShowLocalDataStoreNode)
					initialSelection = serverTreeComponent.ServerTree.RootNode.LocalDataStoreNode;
				else
					initialSelection = serverTreeComponent.ServerTree.RootNode.ServerGroupNode;
			}

			serverTreeComponent.SetSelection(initialSelection);
		}

		private static IServerTreeNode GetFirstDefaultServerOrGroup(ServerGroup serverGroup)
		{
			if (serverGroup.IsEntireGroupChecked())
				return serverGroup;

			//consider groups and servers at this level
			foreach (ServerGroup group in serverGroup.ChildGroups)
			{
				if (group.IsEntireGroupChecked())
					return group;
			}

			foreach (Server server in serverGroup.ChildServers)
			{
				if (server.IsChecked)
					return server;
			}

			//repeat for children of the groups at this level
			foreach (ServerGroup group in serverGroup.ChildGroups)
			{
				IServerTreeNode defaultServerOrGroup = GetFirstDefaultServerOrGroup(group);
				if (defaultServerOrGroup != null)
					return defaultServerOrGroup;
			}

			return null;
		}

		private static void CheckDefaultServers(ServerTree serverTree, List<Server> defaultServers)
		{
			foreach (Server server in serverTree.FindChildServers())
			{
				if (defaultServers.Contains(server))
					server.IsChecked = true;
			}
		}

		private static void UncheckAllServers(ServerTree serverTree)
		{
			foreach (Server server in serverTree.FindChildServers())
				server.IsChecked = false;
		}

		internal static bool HasLocalDatastoreSupport()
		{
			try
			{
				StudyFinderExtensionPoint finders = new StudyFinderExtensionPoint();
				return null != CollectionUtils.SelectFirst(finders.CreateExtensions(),
								delegate(object extension) { return ((IStudyFinder)extension).Name == "DICOM_LOCAL"; });
			}
			catch (NotSupportedException)
			{
				Platform.Log(LogLevel.Warn, "Local data store study finder not found.");
				return false;
			}
		}
	}
}
