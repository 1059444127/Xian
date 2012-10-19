﻿#region License

// Copyright (c) 2012, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClearCanvas.ImageServer.Model;
using ClearCanvas.ImageServer.Common;
using System.IO;
using ClearCanvas.Web.Enterprise.Authentication;
using ClearCanvas.Common;

namespace ClearCanvas.ImageServer.Core.ModelExtensions
{
    public static class ServerPartitionExtensions
    {

        /// <summary>
        /// Return the absolute path to the currently active Incoming folder for this partition
        /// or null if there's no incoming folder (eg, Import Service is not running)
        /// </summary>
        /// <returns></returns>
        public static string GetIncomingFolder(this ServerPartition partition)
        {
            var importServices = ServiceLock.FindServicesOfType(ServiceLockTypeEnum.ImportFiles);
            if (importServices == null || importServices.Count == 0)
                return null;

            var activeService = importServices.SingleOrDefault(s => s.Enabled);
            if (activeService == null)
                return null;

            var filesystem = FilesystemMonitor.Instance.GetFilesystemInfo(activeService.FilesystemKey);

            // Note: Import Service doesn't care if the filesystem is disabled or readonly so we don't need to care either
            var incomingPath = Path.Combine(filesystem.Filesystem.FilesystemPath, String.Format("{0}_{1}", partition.PartitionFolder, FilesystemMonitor.ImportDirectorySuffix));
            return incomingPath;
        }


        /// <summary>
        /// Checks if the specified user is allowed to access this partition.
        /// </summary>
        /// <returns></returns>
        public static bool IsUserAccessAllowed(this ServerPartition partition, CustomPrincipal user)
        {
            Platform.CheckForNullReference(user, "user cannot be null");

            // If user has the "access all" token, return true
            if (user.IsInRole(ClearCanvas.Enterprise.Common.AuthorityTokens.DataAccess.AllPartitions))
                return true;

            // If user belongs to any data access authority group which can access the partition, return true
            var isAllowed = user.Credentials.DataAccessAuthorityGroups != null
                && user.Credentials.DataAccessAuthorityGroups.Any(g => partition.IsAuthorityGroupAllowed(g.ToString()));

            return isAllowed;
        }
    }
}

