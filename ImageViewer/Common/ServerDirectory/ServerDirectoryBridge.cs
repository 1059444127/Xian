﻿using System;
using System.Collections.Generic;
using System.Linq;
using ClearCanvas.Common;
using ClearCanvas.ImageViewer.Common.DicomServer;

namespace ClearCanvas.ImageViewer.Common.ServerDirectory
{
    public class ServerDirectoryBridge : IDisposable
    {
        private IServerDirectory _serverDirectory;

        public ServerDirectoryBridge()
            : this(Platform.GetService<IServerDirectory>())
        {
        }

        public ServerDirectoryBridge(IServerDirectory serverDirectory)
        {
            Platform.CheckForNullReference(serverDirectory, "serverDirectory");
            _serverDirectory = serverDirectory;
        }

        public IDicomServiceNode GetLocalServer()
        {
            try
            {
                DicomServerConfigurationHelper.Refresh(true);
                return new DicomServiceNode(DicomServerConfigurationHelper.DicomServerConfiguration);
            }
            catch (Exception)
            {
                return new DicomServiceNode(new DicomServerConfiguration
                                             {AETitle = DicomServerConfigurationHelper.GetOfflineAETitle(false)});
            }
        }

        public List<IDicomServiceNode> GetServers()
        {
            var servers = _serverDirectory.GetServers(new GetServersRequest()).DirectoryEntries;
            return servers.Select(s => s.ToServiceNode()).OfType<IDicomServiceNode>().ToList();
        }

        public List<IDicomServiceNode> GetServersByAETitle(string aeTitle)
        {
            var servers = _serverDirectory.GetServers(new GetServersRequest{AETitle = aeTitle}).DirectoryEntries;
            return servers.Select(s => s.ToServiceNode()).OfType<IDicomServiceNode>().ToList();
        }

        public List<IDicomServiceNode> GetServerByName(string name)
        {
            var servers = _serverDirectory.GetServers(new GetServersRequest { Name = name}).DirectoryEntries;
            return servers.Select(s => s.ToServiceNode()).OfType<IDicomServiceNode>().ToList();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!(_serverDirectory is IDisposable))
                return;

            ((IDisposable)_serverDirectory).Dispose();
            _serverDirectory = null;
        }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Warn, e);
            }
        }

        #endregion
    }
}
