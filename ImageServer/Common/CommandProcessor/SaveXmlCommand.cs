﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.IO;
using System.Threading;
using System.Xml;
using ClearCanvas.Common;
using ClearCanvas.Dicom.Utilities.Xml;
using ClearCanvas.ImageServer.Common;
using ClearCanvas.ImageServer.Common.CommandProcessor;
using ClearCanvas.ImageServer.Common.Utilities;
using ClearCanvas.ImageServer.Model;

namespace ClearCanvas.ImageServer.Common.CommandProcessor
{
    /// <summary>
    /// Insert DICOM file into a <see cref="StudyXml"/> file and save to disk.
    /// </summary>
    public class SaveXmlCommand : ServerCommand, IDisposable
    {
        #region Private Members

        private readonly StudyXml _stream;
        private readonly string _xmlPath;
        private readonly string _gzPath;
        private string _xmlBackupPath;
        private string _gzBackupPath;
        private bool _fileSaved = false;
        #endregion

        #region Private Static Members
        private static readonly StudyXmlOutputSettings _outputSettings = ImageServerCommonConfiguration.DefaultStudyXmlOutputSettings;
        #endregion

        #region Constructors

        public SaveXmlCommand(StudyXml stream, StudyStorageLocation storageLocation)
            : base("Insert into Study XML", true)
        {
            Platform.CheckForNullReference(stream, "StudyStream object");
            Platform.CheckForNullReference(storageLocation, "Study Storage Location");

            _stream = stream;
            _xmlPath = Path.Combine(storageLocation.GetStudyPath(), storageLocation.StudyInstanceUid + ".xml");
            _gzPath = _xmlPath + ".gz";
        }

		public SaveXmlCommand(StudyXml stream, string rootStudyPath, string studyInstanceUid)
			: base("Insert into Study XML", true)
		{
			Platform.CheckForNullReference(stream, "StudyStream object");
			Platform.CheckForNullReference(rootStudyPath, "Study folder path");
			Platform.CheckForNullReference(studyInstanceUid, "Study Instance Uid");

			_stream = stream;
			_xmlPath = Path.Combine(rootStudyPath, studyInstanceUid + ".xml");
			_gzPath = _xmlPath + ".gz";
		}

        #endregion

        #region Private Methods

        private void Backup()
        {
            if (File.Exists(_xmlPath))
            {
                try
                {
                    Random random = new Random();
                    _xmlBackupPath = String.Format("{0}.bak.{1}", _xmlPath, random.Next());
                    File.Copy(_xmlPath, _xmlBackupPath);
                }
                catch (IOException)
                {
                    _xmlBackupPath = null;
                    throw;
                }
            }
            if (File.Exists(_gzPath))
            {
                try
                {
                    Random random = new Random();
                    _gzBackupPath = String.Format("{0}.bak.{1}", _gzPath, random.Next());
                    File.Copy(_gzPath, _gzBackupPath);
                }
                catch (IOException)
                {
                    _gzBackupPath = null;
                    throw;
                }
            }
        }

        private void WriteStudyStream(string streamFile, string gzStreamFile, StudyXml theStream)
        {
            XmlDocument doc = theStream.GetMemento(_outputSettings);

            // allocate the random number generator here, in case we need it below
            Random rand = new Random();
            string tmpStreamFile = streamFile + "_tmp";
            string tmpGzStreamFile = gzStreamFile + "_tmp";
            for (int i = 0; ; i++)
                try
                {
                    if (File.Exists(tmpStreamFile))
                        FileUtils.Delete(tmpStreamFile);
                    if (File.Exists(tmpGzStreamFile))
                        FileUtils.Delete(tmpGzStreamFile);

                    _fileSaved = true;

                    using (FileStream xmlStream = FileStreamOpener.OpenForSoleUpdate(tmpStreamFile, FileMode.CreateNew),
                                      gzipStream = FileStreamOpener.OpenForSoleUpdate(tmpGzStreamFile, FileMode.CreateNew))
                    {
                        StudyXmlIo.WriteXmlAndGzip(doc, xmlStream, gzipStream);
                        xmlStream.Close();
                        gzipStream.Close();
                    }

                    if (File.Exists(streamFile))
                        FileUtils.Delete(streamFile);
                    File.Move(tmpStreamFile, streamFile);
                    if (File.Exists(_gzPath))
                        FileUtils.Delete(_gzPath);
                    File.Move(tmpGzStreamFile, _gzPath);
                    return;
                }
                catch (IOException)
                {
                    if (i < 5)
                    {
                        Thread.Sleep(rand.Next(5, 50)); // Sleep 5-50 milliseconds
                        continue;
                    }

                    throw;
                }
        }

        #endregion

        #region Overridden Protected Methods

		protected override void OnExecute(ServerCommandProcessor theProcessor)
        {
            Backup();

            WriteStudyStream(_xmlPath, _gzBackupPath, _stream);
        }

        protected override void OnUndo()
        {
            if (File.Exists(_xmlPath) && _fileSaved)
                FileUtils.Delete(_xmlPath);

            if (false == String.IsNullOrEmpty(_xmlBackupPath) && File.Exists(_xmlBackupPath))
            {
                // restore original file
                File.Copy(_xmlBackupPath, _xmlPath, true);
            }

            if (File.Exists(_gzPath) && _fileSaved)
                FileUtils.Delete(_gzPath);

            if (false == String.IsNullOrEmpty(_gzBackupPath) && File.Exists(_gzBackupPath))
            {
                // restore original file
                File.Copy(_gzBackupPath, _gzPath, true);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (false == String.IsNullOrEmpty(_xmlBackupPath) && File.Exists(_xmlBackupPath))
            {
                FileUtils.Delete(_xmlBackupPath);
            }
            if (false == String.IsNullOrEmpty(_gzBackupPath) && File.Exists(_gzBackupPath))
            {
                FileUtils.Delete(_gzBackupPath);
            }
        }

        #endregion

        #endregion
    }
}