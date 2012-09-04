#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.IO;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Dicom.Utilities.Command;
using ClearCanvas.Enterprise.Core;

namespace ClearCanvas.ImageServer.Common.Command
{
    /// <summary>
    /// Represents the execution context of the current operation.
    /// </summary>
    public class ServerExecutionContext: ICommandProcessorContext
    {
        #region Private Fields

        [ThreadStatic]
        private static ServerExecutionContext _current;
        protected string _backupDirectory;
        protected string _tempDirectory;
        private readonly object _sync = new object();
        private readonly ServerExecutionContext _inheritFrom;
        private IReadContext _readContext;
        private readonly string _contextId;
        private IUpdateContext _updateContext;

        private bool _ownsUpdateContext;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an execution scope in current thread
        /// where persistence context can be re-used.
        /// </summary>
        public ServerExecutionContext()
            : this(Guid.NewGuid().ToString())
        {

        }


        /// <summary>
        /// Creates an instance of <see cref="ServerExecutionContext"/> inherited
        /// the current ExecutionContext in the thread, if it exists. This context
        /// becomes the current context of the thread until it is disposed.
        /// </summary>
        public ServerExecutionContext(String contextId)
            : this(contextId, Current)
        {
         
        }

        public ServerExecutionContext(String contextId, ServerExecutionContext inheritFrom)
        {
            Platform.CheckForNullReference(contextId, "contextId");
            
            _contextId = contextId;
            _inheritFrom = inheritFrom;
            _current = this;
        }

        #endregion

        #region Virtual Protected Methods

        protected virtual string GetTemporaryPath()
        {
            string path = _inheritFrom != null
                              ? Path.Combine(_inheritFrom.GetTemporaryPath(), _contextId)
                              : Path.Combine(ServerPlatform.TempDirectory, _contextId);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        #endregion

        protected object SyncRoot
        {
            get { return _sync; }
        }

        public String TempDirectory
        {
            get { return _tempDirectory ?? (_tempDirectory = GetTemporaryPath()); }
        }

        public virtual String BackupDirectory
        {
            get
            {
                if (_backupDirectory == null)
                {
                    lock (SyncRoot)
                    {
                        String baseBackupDirectory = Path.Combine(TempDirectory, "BackupFiles");
                        _backupDirectory = baseBackupDirectory;
                    }
                }

                if (!Directory.Exists(_backupDirectory))
                    Directory.CreateDirectory(_backupDirectory);

                return _backupDirectory;
            }
            set
            {
                _backupDirectory = value;
            }
        }

        public static ServerExecutionContext Current
        {
            get { return _current; }
        }

        public IPersistenceContext ReadContext
        {
            get
            {
                lock(SyncRoot)
                {
					if (_inheritFrom != null)
						return _inheritFrom.ReadContext;

                    if (_readContext == null)
                        _readContext = PersistentStoreRegistry.GetDefaultStore().OpenReadContext();
                }

                return _readContext;
            }
        }

        public IPersistenceContext PersistenceContext
        {
            get
            {
            	if (_updateContext != null)
                    return _updateContext;

            	return ReadContext;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                if (!DirectoryUtility.DeleteIfEmpty(_backupDirectory))
                {
                    Platform.Log(LogLevel.Warn, "Some backup files can be found left in {0}", BackupDirectory);
                }

                if (Platform.IsLogLevelEnabled(LogLevel.Debug) && Directory.Exists(_tempDirectory))
                    Platform.Log(LogLevel.Debug, "Deleting temp folder: {0}", _tempDirectory);
                
                DirectoryUtility.DeleteIfEmpty(_tempDirectory);
            }
            finally
            {
                if (_updateContext != null)
                    Rollback();

            	if (_readContext!=null)
            	{
            		_readContext.Dispose();
            		_readContext = null;
            	}
                // reset the current context for the thread
                _current = _inheritFrom;
            }            
        }

        #endregion

        protected static string GetTempPathRoot()
        {
            String basePath = String.Empty;
            if (!String.IsNullOrEmpty(ImageServerCommonConfiguration.TemporaryPath))
            {
                if (Directory.Exists(ImageServerCommonConfiguration.TemporaryPath))
                    basePath = ImageServerCommonConfiguration.TemporaryPath;
                else
                {
                    // try to create it
                    try
                    {
                        Directory.CreateDirectory(ImageServerCommonConfiguration.TemporaryPath);
                    }
                    catch(Exception ex)
                    {
                        Platform.Log(LogLevel.Error, ex);
                    }
                }
            }
            return basePath;
        }

        public void PreExecute(ICommand command)
        {
            var dbCommand = command as ServerDatabaseCommand;
            if (dbCommand!= null)
            {
                    if (_updateContext == null)
                    {
                        _updateContext =
                            PersistentStoreRegistry.GetDefaultStore().OpenUpdateContext(UpdateContextSyncMode.Flush);
                        _ownsUpdateContext = true;
                    }

                dbCommand.UpdateContext = _updateContext;
            }            
        }

        public void Commit()
        {
            if (_updateContext != null)
            {
                _updateContext.Commit();
                _updateContext.Dispose();
                _updateContext = null;
            }
        }

        public void Rollback()
        {
            if (_updateContext != null && _ownsUpdateContext)
            {
                _updateContext.Dispose(); // Rollback the db
                _updateContext = null;
            }
        }
    }
}