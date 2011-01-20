#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using ClearCanvas.Dicom;
using ClearCanvas.Enterprise.Core;
using ClearCanvas.ImageServer.Common.CommandProcessor;
using ClearCanvas.ImageServer.Enterprise;
using ClearCanvas.ImageServer.Model;
using ClearCanvas.ImageServer.Model.Brokers;
using ClearCanvas.ImageServer.Model.Parameters;

namespace ClearCanvas.ImageServer.Services.Archiving
{
	/// <summary>
	/// Command to insert a <see cref="FilesystemStudyStorage"/> record in the database
	/// and update the Study status.
	/// </summary>
	public class InsertFilesystemStudyStorageCommand : ServerDatabaseCommand
	{
		private readonly ServerEntityKey _serverPartitionKey;
		private readonly string _studyInstanceUid;
		private readonly string _folder;
		private readonly ServerEntityKey _filesystemKey;
		private readonly TransferSyntax _transfersyntax;
		private StudyStorageLocation _location;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="serverPartitionKey">The <see cref="ServerPartition"/> the study belongs to.</param>
		/// <param name="studyInstanceUid">The Study</param>
		/// <param name="folder">The folder (typically the study date) where the study is stored.</param>
		/// <param name="filesystemKey">The filesystem the study is stored on.</param>
		/// <param name="transferSyntax">The <see cref="TransferSyntax"/> of the study.</param>
		public InsertFilesystemStudyStorageCommand(ServerEntityKey serverPartitionKey,
													 string studyInstanceUid,
													 string folder,
													 ServerEntityKey filesystemKey,
													 TransferSyntax transferSyntax)
			: base("Insert FilesystemStudyStorage", true)
		{
			_serverPartitionKey = serverPartitionKey;
			_studyInstanceUid = studyInstanceUid;
			_folder = folder;
			_filesystemKey = filesystemKey;
			_transfersyntax = transferSyntax;
		}

		/// <summary>
		/// The <see cref="StudyStorageLocation"/> inserted.
		/// </summary>
		public StudyStorageLocation Location
		{
			get { return _location; }
		}

		/// <summary>
		/// Execute the insert.
		/// </summary>
		/// <param name="updateContext">The persistent store connection to use for the update.</param>
		protected override void OnExecute(ServerCommandProcessor theProcessor, IUpdateContext updateContext)
		{
			IInsertStudyStorage locInsert = updateContext.GetBroker<IInsertStudyStorage>();
			InsertStudyStorageParameters insertParms = new InsertStudyStorageParameters();
			insertParms.ServerPartitionKey = _serverPartitionKey;
			insertParms.StudyInstanceUid = _studyInstanceUid;
			insertParms.Folder = _folder;
			insertParms.FilesystemKey = _filesystemKey;
			insertParms.QueueStudyStateEnum = QueueStudyStateEnum.Idle;

			if (_transfersyntax.LosslessCompressed)
			{
				insertParms.TransferSyntaxUid = _transfersyntax.UidString;
				insertParms.StudyStatusEnum = StudyStatusEnum.OnlineLossless;
			}
			else if (_transfersyntax.LossyCompressed)
			{
				insertParms.TransferSyntaxUid = _transfersyntax.UidString;
				insertParms.StudyStatusEnum = StudyStatusEnum.OnlineLossy;
			}
			else
			{
				insertParms.TransferSyntaxUid = TransferSyntax.ExplicitVrLittleEndianUid;
				insertParms.StudyStatusEnum = StudyStatusEnum.Online;
			}

			// Find one so we don't uselessly process all the results.
			_location = locInsert.FindOne(insertParms);
		}
	}
}
