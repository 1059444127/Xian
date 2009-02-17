#region License

// Copyright (c) 2006-2009, ClearCanvas Inc.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met:
//
//    * Redistributions of source code must retain the above copyright notice, 
//      this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, 
//      this list of conditions and the following disclaimer in the documentation 
//      and/or other materials provided with the distribution.
//    * Neither the name of ClearCanvas Inc. nor the names of its contributors 
//      may be used to endorse or promote products derived from this software without 
//      specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR 
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
// OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
// ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY 
// OF SUCH DAMAGE.

#endregion

// This file is auto-generated by the ClearCanvas.Model.SqlServer2005.CodeGenerator project.

namespace ClearCanvas.ImageServer.Model
{
    using System;
    using ClearCanvas.Enterprise.Core;
    using ClearCanvas.ImageServer.Enterprise;
    using ClearCanvas.ImageServer.Model.EntityBrokers;

    [Serializable]
    public partial class PartitionArchive: ServerEntity
    {
        #region Constructors
        public PartitionArchive():base("PartitionArchive")
        {}
        public PartitionArchive(
             System.Int32 _archiveDelayHours_
            ,ArchiveTypeEnum _archiveTypeEnum_
            ,System.Xml.XmlDocument _configurationXml_
            ,System.String _description_
            ,System.Boolean _enabled_
            ,System.Boolean _readOnly_
            ,ClearCanvas.ImageServer.Enterprise.ServerEntityKey _serverPartitionKey_
            ):base("PartitionArchive")
        {
            _archiveDelayHours = _archiveDelayHours_;
            _archiveTypeEnum = _archiveTypeEnum_;
            _configurationXml = _configurationXml_;
            _description = _description_;
            _enabled = _enabled_;
            _readOnly = _readOnly_;
            _serverPartitionKey = _serverPartitionKey_;
        }
        #endregion

        #region Private Members
        private System.Int32 _archiveDelayHours;
        private ArchiveTypeEnum _archiveTypeEnum;
        private System.Xml.XmlDocument _configurationXml;
        private System.String _description;
        private System.Boolean _enabled;
        private System.Boolean _readOnly;
        private ClearCanvas.ImageServer.Enterprise.ServerEntityKey _serverPartitionKey;
        #endregion

        #region Public Properties
        [EntityFieldDatabaseMappingAttribute(TableName="PartitionArchive", ColumnName="ArchiveDelayHours")]
        public System.Int32 ArchiveDelayHours
        {
        get { return _archiveDelayHours; }
        set { _archiveDelayHours = value; }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="PartitionArchive", ColumnName="ArchiveTypeEnum")]
        public ArchiveTypeEnum ArchiveTypeEnum
        {
        get { return _archiveTypeEnum; }
        set { _archiveTypeEnum = value; }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="PartitionArchive", ColumnName="ConfigurationXml")]
        public System.Xml.XmlDocument ConfigurationXml
        {
        get { return _configurationXml; }
        set { _configurationXml = value; }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="PartitionArchive", ColumnName="Description")]
        public System.String Description
        {
        get { return _description; }
        set { _description = value; }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="PartitionArchive", ColumnName="Enabled")]
        public System.Boolean Enabled
        {
        get { return _enabled; }
        set { _enabled = value; }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="PartitionArchive", ColumnName="ReadOnly")]
        public System.Boolean ReadOnly
        {
        get { return _readOnly; }
        set { _readOnly = value; }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="PartitionArchive", ColumnName="ServerPartitionGUID")]
        public ClearCanvas.ImageServer.Enterprise.ServerEntityKey ServerPartitionKey
        {
        get { return _serverPartitionKey; }
        set { _serverPartitionKey = value; }
        }
        #endregion

        #region Static Methods
        static public PartitionArchive Load(ServerEntityKey key)
        {
            using (IReadContext read = PersistentStoreRegistry.GetDefaultStore().OpenReadContext())
            {
                return Load(read, key);
            }
        }
        static public PartitionArchive Load(IReadContext read, ServerEntityKey key)
        {
            IPartitionArchiveEntityBroker broker = read.GetBroker<IPartitionArchiveEntityBroker>();
            PartitionArchive theObject = broker.Load(key);
            return theObject;
        }
        static public PartitionArchive Insert(PartitionArchive table)
        {
            using (IUpdateContext update = PersistentStoreRegistry.GetDefaultStore().OpenUpdateContext(UpdateContextSyncMode.Flush))
            {
                return Insert(update, table);
            }
        }
        static public PartitionArchive Insert(IUpdateContext update, PartitionArchive table)
        {
            IPartitionArchiveEntityBroker broker = update.GetBroker<IPartitionArchiveEntityBroker>();
            PartitionArchiveUpdateColumns updateColumns = new PartitionArchiveUpdateColumns();
            updateColumns.ArchiveDelayHours = table.ArchiveDelayHours;
            updateColumns.ArchiveTypeEnum = table.ArchiveTypeEnum;
            updateColumns.ConfigurationXml = table.ConfigurationXml;
            updateColumns.Description = table.Description;
            updateColumns.Enabled = table.Enabled;
            updateColumns.ReadOnly = table.ReadOnly;
            updateColumns.ServerPartitionKey = table.ServerPartitionKey;
            PartitionArchive theObject = broker.Insert(updateColumns);
            update.Commit();
            return theObject;
        }
        #endregion
    }
}
