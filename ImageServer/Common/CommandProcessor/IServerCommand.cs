﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

namespace ClearCanvas.ImageServer.Common.CommandProcessor
{
    /// <summary>
	/// Defines the interface of a command used by the <see cref="ServerCommandProcessor"/>
    /// </summary>
    public interface IServerCommand
    {
        /// <summary>
        /// Gets and sets the execution context for the command.
        /// </summary>
        ExecutionContext ExecutionContext { set; get; }

        /// <summary>
        /// Gets and sets a value describing what the command is doing.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets a value describing if the ServerCommand requires a rollback of the operation its included in if it fails during execution.
        /// </summary>
        bool RequiresRollback
        {
            get;
            set;
        }

        /// <summary>
        /// Execute the ServerCommand.
        /// </summary>
        void Execute(ServerCommandProcessor theProcessor);

        /// <summary>
        /// Undo the operation done by <see cref="Execute"/>.
        /// </summary>
        void Undo();
    }


}