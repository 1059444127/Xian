#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.IO;
using ClearCanvas.Common;

namespace ClearCanvas.ImageServer.Common.CommandProcessor
{
	/// <summary>
	/// <see cref="ServerCommand"/> for deleting a file, not that there is no rollback.  Rollback
	/// should be accomplished by other means.  IE, do a rename of the file, then delete when everything
	/// else is done.
	/// </summary>
	public class FileDeleteCommand : ServerCommand, IDisposable
	{
        private string _backupFile;
        private bool _backedUp = false;
        private readonly string _path;

		public FileDeleteCommand(string path, bool requiresRollback) 
            : base(String.Format("Delete {0}", path), requiresRollback)
		{
            Platform.CheckForNullReference(path, "path");
			_path = path;
		}

		protected override void OnExecute(ServerCommandProcessor theProcessor)
		{
            if (RequiresRollback)
            {
                Backup();
            }


			if (File.Exists(_path))
			{
			    File.Delete(_path);
			}
            else
			{
			    Platform.Log(LogLevel.Warn, "Attempted to delete file which doesn't exist: {0}", _path);
			}
		}

	    private void Backup()
	    {
            if (File.Exists(_path))
            {
                _backupFile = _path + ".bak";
                File.Copy(_path, _backupFile);
                _backedUp = true;
            }
            
	    }

	    protected override void OnUndo()
		{
			if (_backedUp)
			{
			    Platform.Log(LogLevel.Info, "Restoring {0} ...", _path);
                File.Copy(_backupFile, _path, true);
			}
		}

        public override string ToString()
        {
            return String.Format("Delete file {0}", _path);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (File.Exists(_backupFile))
            {
                File.Delete(_backupFile);
            }
        }

        #endregion
    }
}