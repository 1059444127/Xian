﻿#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using ClearCanvas.Common;
using ClearCanvas.Common.Actions;
using ClearCanvas.ImageServer.Rules.AutoRouteAction;

namespace ClearCanvas.ImageServer.Rules.GrantAccessAction
{
    /// <summary>
    /// Class for implementing auto-route action as specified by <see cref="IActionItem{T}"/>
    /// </summary>
    public class GrantAccessActionItem : ServerActionItemBase
    {
        readonly private string _device;
        #region Constructors

        public GrantAccessActionItem(string device)
            : base("GrantAcccess Action")
        {
            _device = device;
        }

        #endregion

        #region Public Properties

        #endregion

        #region Public Methods

        protected override bool OnExecute(ServerActionContext context)
        {
            InsertStudyDataAccessCommand command;

            command = new InsertStudyDataAccessCommand(context, Guid.NewGuid());

            if (context.CommandProcessor != null)
                context.CommandProcessor.AddCommand(command);
            else
            {
                try
                {
                    command.Execute(context.CommandProcessor);
                }
                catch (Exception e)
                {
                    Platform.Log(LogLevel.Error, e, "Unexpected exception when inserting grant-access request");

                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
