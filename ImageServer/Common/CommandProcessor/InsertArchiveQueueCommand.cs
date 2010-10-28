#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using ClearCanvas.Enterprise.Core;
using ClearCanvas.ImageServer.Enterprise;
using ClearCanvas.ImageServer.Model;
using ClearCanvas.ImageServer.Model.Brokers;
using ClearCanvas.ImageServer.Model.Parameters;

namespace ClearCanvas.ImageServer.Common.CommandProcessor
{
	/// <summary>
	/// <see cref="ServerCommand"/> for inserting into the <see cref="ArchiveQueue"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Note that This stored procedure checks to see if a study delete record has been 
	/// inserted into the database, so it should be called after the rules engine has 
	/// been run & appropriate records inserted into the database.
	/// </para>
	/// <para>
	/// Note also that it can be called when we reprocess a study.
	/// </para>
	/// </remarks>
	public class InsertArchiveQueueCommand : ServerDatabaseCommand
	{
		private readonly ServerEntityKey _serverPartitionKey;
		private readonly ServerEntityKey _studyStorageKey;
		

		public InsertArchiveQueueCommand(ServerEntityKey serverPartitionKey, ServerEntityKey studyStorageKey)
			: base("Insert ArchiveQueue record", true)
		{
			_serverPartitionKey = serverPartitionKey;

			_studyStorageKey = studyStorageKey;

		}

		protected override void OnExecute(ServerCommandProcessor theProcessor, IUpdateContext updateContext)
		{
			// Setup the insert parameters
			InsertArchiveQueueParameters parms = new InsertArchiveQueueParameters();
			parms.ServerPartitionKey = _serverPartitionKey;
			parms.StudyStorageKey = _studyStorageKey;
			
			// Get the Insert ArchiveQueue broker and do the insert
			IInsertArchiveQueue insert = updateContext.GetBroker<IInsertArchiveQueue>();

			// Do the insert
            if (!insert.Execute(parms))
                throw new ApplicationException("InsertArchiveQueueCommand failed");
		}
	}
}
