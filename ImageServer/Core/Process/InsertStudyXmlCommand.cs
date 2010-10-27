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
using ClearCanvas.Dicom;
using ClearCanvas.Dicom.Utilities.Xml;
using ClearCanvas.ImageServer.Common;
using ClearCanvas.ImageServer.Common.CommandProcessor;
using ClearCanvas.ImageServer.Common.Utilities;
using ClearCanvas.ImageServer.Model;

namespace ClearCanvas.ImageServer.Core.Process
{
	/// <summary>
	/// Insert DICOM file into a <see cref="StudyXml"/> file and save to disk.
	/// </summary>
	public class InsertStudyXmlCommand : ServerCommand
	{
		#region Private Members

		private readonly DicomFile _file;
		private readonly StudyXml _stream;
		private readonly StudyStorageLocation _studyStorageLocation;

		#endregion

		#region Private Static Members
		private static readonly StudyXmlOutputSettings _outputSettings = ImageServerCommonConfiguration.DefaultStudyXmlOutputSettings;
		#endregion

		#region Constructors

		public InsertStudyXmlCommand(DicomFile file, StudyXml stream, StudyStorageLocation storageLocation)
			: base("Insert into Study XML", true)
		{
			Platform.CheckForNullReference(file, "Dicom File object");
			Platform.CheckForNullReference(stream, "StudyStream object");
			Platform.CheckForNullReference(storageLocation, "Study Storage Location");

			_file = file;
			_stream = stream;
			_studyStorageLocation = storageLocation;
		}

		#endregion

		#region Private Methods

		private static void WriteStudyStream(string streamFile, string gzStreamFile, StudyXml theStream)
		{
			XmlDocument doc = theStream.GetMemento(_outputSettings);

			// allocate the random number generator here, in case we need it below
			Random rand = new Random();
			string tmpStreamFile = streamFile + "_tmp";
			string tmpGzStreamFile =  gzStreamFile + "_tmp";
			for (int i = 0; ; i++)
				try
				{
					if (File.Exists(tmpStreamFile))
						FileUtils.Delete(tmpStreamFile);
					if (File.Exists(tmpGzStreamFile))
						FileUtils.Delete(tmpGzStreamFile);

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
					if (File.Exists(gzStreamFile))
						FileUtils.Delete(gzStreamFile);
					File.Move(tmpGzStreamFile,gzStreamFile);
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
			long fileSize = 0;
			if (File.Exists(_file.Filename))
			{
				FileInfo finfo = new FileInfo(_file.Filename);

				fileSize = finfo.Length;
			}

			// Setup the insert parameters
			if (false == _stream.AddFile(_file, fileSize, _outputSettings))
			{
				Platform.Log(LogLevel.Error, "Unexpected error adding SOP to XML Study Descriptor for file {0}",
				             _file.Filename);
				throw new ApplicationException("Unexpected error adding SOP to XML Study Descriptor for SOP: " +
				                               _file.MediaStorageSopInstanceUid);
			}

			// Write it back out.  We flush it out with every added image so that if a failure happens,
			// we can recover properly.
			string streamFile =
				Path.Combine(_studyStorageLocation.GetStudyPath(), _studyStorageLocation.StudyInstanceUid + ".xml");
			string gzStreamFile = streamFile + ".gz";

			WriteStudyStream(streamFile, gzStreamFile, _stream);
		}

		protected override void OnUndo()
		{
		    Platform.Log(LogLevel.Info, "Undoing InsertStudyXmlCommand");
			_stream.RemoveFile(_file);

			string streamFile =
				Path.Combine(_studyStorageLocation.GetStudyPath(), _studyStorageLocation.StudyInstanceUid + ".xml");
			string gzStreamFile = streamFile + ".gz";
			WriteStudyStream(streamFile, gzStreamFile, _stream);
		}

		#endregion
	}
}