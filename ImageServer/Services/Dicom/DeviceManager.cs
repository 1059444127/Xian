﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using ClearCanvas.Common;
using ClearCanvas.Dicom.Network;
using ClearCanvas.Enterprise.Core;
using ClearCanvas.ImageServer.Common;
using ClearCanvas.ImageServer.Model;
using ClearCanvas.ImageServer.Model.EntityBrokers;

namespace ClearCanvas.ImageServer.Services.Dicom
{
    class DeviceManager
    {
    	/// <summary>
    	/// Lookup the device entity in the database corresponding to the remote AE of the association.
    	/// </summary>
    	/// <param name="partition">The partition to look up the devices</param>
    	/// <param name="association">The association</param>
    	/// <param name="isNew">Indicates whether the device returned is created by the call.</param>
    	/// <returns>The device record corresponding to the called AE of the association</returns>
		static public Device LookupDevice(ServerPartition partition, AssociationParameters association, out bool isNew)
    	{
    		isNew = false;

    		Device device;

    		using (
    			IUpdateContext updateContext =
    				PersistentStoreRegistry.GetDefaultStore().OpenUpdateContext(UpdateContextSyncMode.Flush))
    		{
    			IDeviceEntityBroker queryDevice = updateContext.GetBroker<IDeviceEntityBroker>();

    			// Setup the select parameters.
    			DeviceSelectCriteria queryParameters = new DeviceSelectCriteria();
    			queryParameters.AeTitle.EqualTo(association.CallingAE);
    			queryParameters.ServerPartitionKey.EqualTo(partition.GetKey());

    			device = queryDevice.FindOne(queryParameters);
    			if (device == null)
    			{
    				if (!partition.AcceptAnyDevice)
    				{
    					return null;
    				}

    				if (partition.AutoInsertDevice)
    				{
    					// Auto-insert a new entry in the table.
    					DeviceUpdateColumns updateColumns = new DeviceUpdateColumns();

    					updateColumns.AeTitle = association.CallingAE;
    					updateColumns.Enabled = true;
    					updateColumns.Description = String.Format("AE: {0}", association.CallingAE);
    					updateColumns.Dhcp = false;
    					updateColumns.IpAddress = association.RemoteEndPoint.Address.ToString();
    					updateColumns.ServerPartitionKey = partition.GetKey();
    					updateColumns.Port = partition.DefaultRemotePort;
    					updateColumns.AllowQuery = true;
    					updateColumns.AllowRetrieve = true;
    					updateColumns.AllowStorage = true;
    					updateColumns.ThrottleMaxConnections = ImageServerCommonConfiguration.Device.MaxConnections;
    				    updateColumns.DeviceTypeEnum = DeviceTypeEnum.Workstation;
    					IDeviceEntityBroker insert = updateContext.GetBroker<IDeviceEntityBroker>();

    					device = insert.Insert(updateColumns);

    					updateContext.Commit();

    					isNew = true;
    				}
    			}

    			if (device != null)
    			{
    				// For DHCP devices, we always update the remote ip address, if its changed from what is in the DB.
    				if (device.Dhcp && !association.RemoteEndPoint.Address.ToString().Equals(device.IpAddress))
    				{
    					DeviceUpdateColumns updateColumns = new DeviceUpdateColumns();

    					updateColumns.IpAddress = association.RemoteEndPoint.Address.ToString();
    					updateColumns.LastAccessedTime = Platform.Time;

    					IDeviceEntityBroker update = updateContext.GetBroker<IDeviceEntityBroker>();

    					if (!update.Update(device.GetKey(), updateColumns))
    						Platform.Log(LogLevel.Error,
    						             "Unable to update IP Address for DHCP device {0} on partition '{1}'",
    						             device.AeTitle, partition.Description);
    					else
    						updateContext.Commit();
    				}
    				else if (!isNew)
    				{
    					DeviceUpdateColumns updateColumns = new DeviceUpdateColumns();

    					updateColumns.LastAccessedTime = Platform.Time;

    					IDeviceEntityBroker update = updateContext.GetBroker<IDeviceEntityBroker>();

    					if (!update.Update(device.GetKey(), updateColumns))
    						Platform.Log(LogLevel.Error,
    						             "Unable to update LastAccessedTime device {0} on partition '{1}'",
    						             device.AeTitle, partition.Description);
    					else
    						updateContext.Commit();
    				}
    			}
    		}

    		return device;
    	}
    }
}
