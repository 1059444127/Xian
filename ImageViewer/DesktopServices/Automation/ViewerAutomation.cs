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
using System.ServiceModel;
using System.Text;
using System.Threading;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Desktop;
using ClearCanvas.Dicom.ServiceModel;
using ClearCanvas.Dicom.ServiceModel.Query;
using ClearCanvas.ImageViewer.Configuration;
using ClearCanvas.ImageViewer.Services.Automation;
using ClearCanvas.ImageViewer.Services.ServerTree;
using ClearCanvas.ImageViewer.StudyManagement;
using ApplicationEntity=ClearCanvas.ImageViewer.StudyManagement.ApplicationEntity;

namespace ClearCanvas.ImageViewer.DesktopServices.Automation
{
	/// <summary>
	/// For internal use only.
	/// </summary>
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, UseSynchronizationContext = true, ConfigurationName = "ViewerAutomation", Namespace = AutomationNamespace.Value)]
	public class ViewerAutomation: IViewerAutomation
	{
		private static readonly string _viewerNotFoundReason = "The specified viewer was not found.";

		public ViewerAutomation()
		{
		}

		private static IStudyRootQuery GetStudyRootQuery()
		{
			return Platform.GetService<IStudyRootQuery>();
		}

		#region IViewerAutomation Members

		public GetActiveViewersResult GetActiveViewers()
		{
			List<Viewer> viewers = new List<Viewer>();

			//The tool stores the viewer ids in order of activation, most recent first
			foreach (Guid viewerId in ViewerAutomationTool.GetViewerIds())
			{
				IImageViewer viewer = ViewerAutomationTool.GetViewer(viewerId);
				if (viewer != null && GetViewerWorkspace(viewer) != null)
					viewers.Add(new Viewer(viewerId, GetPrimaryStudyInstanceUid(viewer)));
			}

			if (viewers.Count == 0)
				throw new FaultException<NoActiveViewersFault>(new NoActiveViewersFault(), "No active viewers were found.");

			GetActiveViewersResult result = new GetActiveViewersResult();
			result.ActiveViewers = viewers;
			return result;
		}

		public GetViewerInfoResult GetViewerInfo(GetViewerInfoRequest request)
		{
			if (request == null)
			{
				string message = "The get viewer info request cannot be null.";
				Platform.Log(LogLevel.Debug, message);
				throw new FaultException(message);
			}

			if (request.Viewer == null || request.Viewer.Identifier.Equals(Guid.Empty))
			{
				string message = "A valid viewer id must be specified.";
				Platform.Log(LogLevel.Debug, message);
				throw new FaultException(message);
			}

			IImageViewer viewer = ViewerAutomationTool.GetViewer(request.Viewer.Identifier);
			if (viewer == null)
			{
				string message = String.Format("The specified viewer ({0}) was not found, " +
									"likely because it has already been closed by the user.", request.Viewer.Identifier);
				Platform.Log(LogLevel.Debug, message);

				throw new FaultException<ViewerNotFoundFault>(new ViewerNotFoundFault(message), _viewerNotFoundReason);
			}

			GetViewerInfoResult result = new GetViewerInfoResult();
			result.AdditionalStudyInstanceUids = GetAdditionalStudyInstanceUids(viewer);
			return result;
		}

		public OpenStudiesResult OpenStudies(OpenStudiesRequest request)
		{
			if (request == null)
			{
				string message = "The open studies request cannot be null.";
				Platform.Log(LogLevel.Debug, message);
				throw new FaultException(message);
			}

			if (request.StudiesToOpen == null || request.StudiesToOpen.Count == 0)
			{
				string message = "At least one study must be specified.";
				Platform.Log(LogLevel.Debug, message);
				throw new FaultException(message);
			}

			OpenStudiesResult result = new OpenStudiesResult();
			bool activateIfOpen = request.ActivateIfAlreadyOpen ?? true;

			try
			{
				string primaryStudyInstanceUid = request.StudiesToOpen[0].StudyInstanceUid;
				IImageViewer viewer = null;
				if (activateIfOpen)
				{
					Workspace workspace = GetViewerWorkspace(primaryStudyInstanceUid);
					if (workspace != null)
					{
						viewer = ImageViewerComponent.GetAsImageViewer(workspace);
						workspace.Activate();
					}
				}
				
				if (viewer == null)
					viewer = LaunchViewer(request, primaryStudyInstanceUid);

				Guid? viewerId = ViewerAutomationTool.GetViewerId(viewer);
				if (viewerId == null)
					throw new FaultException("Failed to retrieve the id of the specified viewer.");

				result.Viewer = new Viewer(viewerId.Value, primaryStudyInstanceUid);
				return result;
			}
			catch(FaultException)
			{
				throw;
			}
			catch(Exception e)
			{
				string message = "An unexpected error has occurred while attempting to open the study(s).";
				Platform.Log(LogLevel.Error, e, message);
				throw new FaultException(message);
			}
		}

		public void ActivateViewer(ActivateViewerRequest request)
		{
			if (request == null)
			{
				string message = "The activate viewer request cannot be null.";
				Platform.Log(LogLevel.Debug, message);
				throw new FaultException(message);
			}

			if (request.Viewer == null || request.Viewer.Identifier.Equals(Guid.Empty))
			{
				string message = "A valid viewer id must be specified.";
				Platform.Log(LogLevel.Debug, message);
				throw new FaultException(message);
			}

			IImageViewer viewer = ViewerAutomationTool.GetViewer(request.Viewer.Identifier);
			if (viewer == null)
			{
				string message = String.Format("The specified viewer ({0}) was not found, " +
					"likely because it has already been closed by the user.", request.Viewer.Identifier);
				Platform.Log(LogLevel.Debug, message);

				throw new FaultException<ViewerNotFoundFault>(new ViewerNotFoundFault(message), _viewerNotFoundReason);
			}

			IWorkspace workspace = GetViewerWorkspace(viewer);
			if (workspace == null)
			{
				string message = String.Format("The specified viewer ({0}) was found, " + 
					"but does not appear to be hosted in one of the active workspaces.", request.Viewer.Identifier);
				Platform.Log(LogLevel.Error, message);

				throw new FaultException<ViewerNotFoundFault>(new ViewerNotFoundFault(message), _viewerNotFoundReason);
			}

			try
			{
				workspace.Activate();
			}
			catch(Exception e)
			{
				string message = String.Format("An unexpected error has occurred while attempting " + 
					"to activate the specified viewer ({0}).", request.Viewer.Identifier);
				Platform.Log(LogLevel.Error, e, message);
				throw new FaultException(message);
			}
		}

		public void CloseViewer(CloseViewerRequest request)
		{
			if (request == null)
			{
				string message = "The close viewer request cannot be null.";
				Platform.Log(LogLevel.Debug, message);
				throw new FaultException(message);
			}

			if (request.Viewer == null || request.Viewer.Identifier.Equals(Guid.Empty))
			{
				string message = "A valid viewer id must be specified.";
				Platform.Log(LogLevel.Debug, message);
				throw new FaultException(message);
			}

			IImageViewer viewer = ViewerAutomationTool.GetViewer(request.Viewer.Identifier);
			if (viewer == null)
			{
				string message = String.Format("The specified viewer ({0}) was not found, " +
					"likely because it has already been closed by the user.", request.Viewer.Identifier);
				Platform.Log(LogLevel.Debug, message);

				throw new FaultException<ViewerNotFoundFault>(new ViewerNotFoundFault(message), _viewerNotFoundReason);
			}

			IWorkspace workspace = GetViewerWorkspace(viewer);
			if (workspace == null)
			{
				string message = String.Format("The specified viewer ({0}) was found, " +
					"but it does not appear to be hosted in one of the active workspaces.", request.Viewer.Identifier);
				Platform.Log(LogLevel.Error, message);

				throw new FaultException<ViewerNotFoundFault>(new ViewerNotFoundFault(message), _viewerNotFoundReason);
			}

			try
			{
				workspace.Close(UserInteraction.NotAllowed);
			}
			catch (Exception e)
			{
				string message = String.Format("An unexpected error has occurred while attempting " +
					"to close the specified viewer ({0}).", request.Viewer.Identifier);
				Platform.Log(LogLevel.Error, e, message);
				throw new FaultException(message);
			}
		}

		#endregion

		private static void CompleteOpenStudyInfo(List<OpenStudyInfo> openStudyInfo)
		{
			List<OpenStudyInfo> incomplete = CollectionUtils.Select(openStudyInfo,
						delegate(OpenStudyInfo info) { return String.IsNullOrEmpty(info.SourceAETitle); });
			
			//only go looking for studies if the source ae title is unspecified.
			if (incomplete.Count == 0)
				return;

			List<string> incompleteStudyUids = CollectionUtils.Map<OpenStudyInfo, string>(incomplete,
				delegate(OpenStudyInfo info) { return info.StudyInstanceUid; });

			using (StudyRootQueryBridge bridge = new StudyRootQueryBridge(GetStudyRootQuery()))
			{
				IList<StudyRootStudyIdentifier> foundStudies = bridge.QueryByStudyInstanceUid(incompleteStudyUids);
				foreach (StudyRootStudyIdentifier study in foundStudies)
				{
					foreach (OpenStudyInfo info in openStudyInfo)
					{
						if (info.StudyInstanceUid == study.StudyInstanceUid)
						{
							info.SourceAETitle = study.RetrieveAeTitle;
							break;
						}
					}
				}
			}
		}

		private static IImageViewer LaunchViewer(OpenStudiesRequest request, string primaryStudyInstanceUid)
		{
			CompleteOpenStudyInfo(request.StudiesToOpen);
			IDictionary<string, ApplicationEntity> serverMap = GetServerMap(request.StudiesToOpen);

			ImageViewerComponent viewer = new ImageViewerComponent(LayoutManagerCreationParameters.Extended);
			List<LoadStudyArgs> loadStudyArgs = new List<LoadStudyArgs>();

			foreach (OpenStudyInfo info in request.StudiesToOpen)
			{
				//None of the servers should be empty now, but if they are, assume local.
				//The worst that will happen is it will fail to load when it doesn't exist.
				ApplicationEntity server = null;
				string loader = "DICOM_LOCAL";

				if (!String.IsNullOrEmpty(info.SourceAETitle) && serverMap.ContainsKey(info.SourceAETitle))
				{
					server = serverMap[info.SourceAETitle];
					if (server != null)
						loader = "CC_STREAMING";
				}

				loadStudyArgs.Add(new LoadStudyArgs(info.StudyInstanceUid, server, loader));
			}

			try
			{
				viewer.LoadStudies(loadStudyArgs);
			}
			catch (Exception e)
			{
				bool faultThrown = false;
				try
				{
					HandleLoadStudiesException(e, primaryStudyInstanceUid, viewer);
				}
				catch
				{
					faultThrown = true;
					viewer.Dispose();
					throw;
				}
				finally
				{
					if (!faultThrown || request.ReportFaultToUser)
						SynchronizationContext.Current.Post(ReportLoadFailures, e);
				}
			}

			ImageViewerComponent.Launch(viewer, new LaunchImageViewerArgs(ViewerLaunchSettings.WindowBehaviour));
			return viewer;
		}

		/// <summary>
		/// As long as the primary study is loaded, even partially, we continue opening the viewer and
		/// just report the loading errors to the user.  If other studies failed to load, we still just
		/// open the viewer and report to the user.
		/// </summary>
		private static void HandleLoadStudiesException(Exception e, string primaryStudyInstanceUid, IImageViewer viewer)
		{
			if (GetPrimaryStudyInstanceUid(viewer) == primaryStudyInstanceUid)
				return; //the primary study was at least partiallly loaded.

			if (e is NotFoundLoadStudyException)
				throw new FaultException<StudyNotFoundFault>(new StudyNotFoundFault(), "The study was not found.");
			if (e is NearlineLoadStudyException)
				throw new FaultException<StudyNearlineFault>(new StudyNearlineFault(), "The study is nearline.");
			if (e is OfflineLoadStudyException)
				throw new FaultException<StudyOfflineFault>(new StudyOfflineFault(), "The study is offline.");
			if (e is InUseLoadStudyException)
				throw new FaultException<StudyInUseFault>(new StudyInUseFault(), "The study is in use.");
			if (e is StudyLoaderNotFoundException)
			{
				const string reason = "The study cannot be loaded directly from the specified server/location.";
				throw new FaultException<OpenStudiesFault>(new OpenStudiesFault { FailureDescription = reason }, reason);
			}

			throw new FaultException<OpenStudiesFault>(new OpenStudiesFault(), "The primary study could not be loaded.");
		}

		private static IDictionary<string, ApplicationEntity> GetServerMap(IEnumerable<OpenStudyInfo> openStudies)
		{
			Dictionary<string, ApplicationEntity> serverMap = new Dictionary<string, ApplicationEntity>();

			string localAE = ServerTree.GetClientAETitle();
			serverMap[localAE] = null;

			ServerTree serverTree = new ServerTree();
			List<IServerTreeNode> servers = serverTree.FindChildServers(serverTree.RootNode.ServerGroupNode);

			foreach (OpenStudyInfo info in openStudies)
			{
				if (!String.IsNullOrEmpty(info.SourceAETitle) && !serverMap.ContainsKey(info.SourceAETitle))
				{
					Server server = servers.Find(delegate(IServerTreeNode node)
								{
									return ((Server)node).AETitle == info.SourceAETitle;
								}) as Server;

					//only add streaming servers.
					if (server != null && server.IsStreaming)
					{
						serverMap[info.SourceAETitle] =
							new ApplicationEntity(server.Host, server.AETitle, server.Name, server.Port, 
							server.IsStreaming, server.HeaderServicePort, server.WadoServicePort);
					}
				}
			}

			return serverMap;
		}

		private static void ReportLoadFailures(object loadFailures)
		{
			ExceptionHandler.Report((Exception)loadFailures, Application.ActiveDesktopWindow);
		}

		private static string GetPrimaryStudyInstanceUid(IImageViewer viewer)
		{
			foreach (Patient patient in viewer.StudyTree.Patients)
			{
				foreach (Study study in patient.Studies)
				{
					return study.StudyInstanceUid;
				}
			}

			return null;
		}

		private static List<string> GetAdditionalStudyInstanceUids(IImageViewer viewer)
		{
			List<string> studyInstanceUids = new List<string>();

			foreach (Patient patient in viewer.StudyTree.Patients)
			{
				foreach (Study study in patient.Studies)
				{
					studyInstanceUids.Add(study.StudyInstanceUid);
				}
			}

			if (studyInstanceUids.Count > 0)
				studyInstanceUids.RemoveAt(0);

			return studyInstanceUids;
		}

		private static Workspace GetViewerWorkspace(IImageViewer viewer)
		{
			foreach (Workspace workspace in GetViewerWorkspaces())
			{
				IImageViewer workspaceViewer = ImageViewerComponent.GetAsImageViewer(workspace);
				if (viewer == workspaceViewer)
					return workspace;
			}

			return null;
		}

		private static Workspace GetViewerWorkspace(string primaryStudyUid)
		{
			foreach (Workspace workspace in GetViewerWorkspaces())
			{
				IImageViewer viewer = ImageViewerComponent.GetAsImageViewer(workspace);
				if (primaryStudyUid == GetPrimaryStudyInstanceUid(viewer))
					return workspace;
			}

			return null;
		}

		private static IEnumerable<Workspace> GetViewerWorkspaces()
		{
			foreach (DesktopWindow desktopWindow in Application.DesktopWindows)
			{
				foreach (Workspace workspace in desktopWindow.Workspaces)
				{
					IImageViewer viewer = ImageViewerComponent.GetAsImageViewer(workspace);
					if (viewer != null)
						yield return workspace;
				}
			}
		}
	}
}
