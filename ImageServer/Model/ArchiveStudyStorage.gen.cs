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
    public partial class ArchiveStudyStorage: ServerEntity
    {
        #region Constructors
        public ArchiveStudyStorage():base("ArchiveStudyStorage")
        {}
        public ArchiveStudyStorage(
             System.DateTime _archiveTime_
            ,System.Xml.XmlDocument _archiveXml_
            ,ClearCanvas.ImageServer.Enterprise.ServerEntityKey _partitionArchiveKey_
            ,ClearCanvas.ImageServer.Enterprise.ServerEntityKey _serverTransferSyntaxKey_
            ,ClearCanvas.ImageServer.Enterprise.ServerEntityKey _studyStorageKey_
            ):base("ArchiveStudyStorage")
        {
            _archiveTime = _archiveTime_;
            _archiveXml = _archiveXml_;
            _partitionArchiveKey = _partitionArchiveKey_;
            _serverTransferSyntaxKey = _serverTransferSyntaxKey_;
            _studyStorageKey = _studyStorageKey_;
        }
        #endregion

        #region Private Members
        private System.DateTime _archiveTime;
        private System.Xml.XmlDocument _archiveXml;
        private ClearCanvas.ImageServer.Enterprise.ServerEntityKey _partitionArchiveKey;
        private ClearCanvas.ImageServer.Enterprise.ServerEntityKey _serverTransferSyntaxKey;
        private ClearCanvas.ImageServer.Enterprise.ServerEntityKey _studyStorageKey;
        #endregion

        #region Public Properties
        [EntityFieldDatabaseMappingAttribute(TableName="ArchiveStudyStorage", ColumnName="ArchiveTime")]
        public System.DateTime ArchiveTime
        {
        get { return _archiveTime; }
        set { _archiveTime = value; }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="ArchiveStudyStorage", ColumnName="ArchiveXml")]
        public System.Xml.XmlDocument ArchiveXml
        {
        get { return _archiveXml; }
        set { _archiveXml = value; }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="ArchiveStudyStorage", ColumnName="PartitionArchiveGUID")]
        public ClearCanvas.ImageServer.Enterprise.ServerEntityKey PartitionArchiveKey
        {
        get { return _partitionArchiveKey; }
        set { _partitionArchiveKey = value; }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="ArchiveStudyStorage", ColumnName="ServerTransferSyntaxGUID")]
        public ClearCanvas.ImageServer.Enterprise.ServerEntityKey ServerTransferSyntaxKey
        {
        get { return _serverTransferSyntaxKey; }
        set { _serverTransferSyntaxKey = value; }
        }
        [EntityFieldDatabaseMappingAttribute(TableName="ArchiveStudyStorage", ColumnName="StudyStorageGUID")]
        public ClearCanvas.ImageServer.Enterprise.ServerEntityKey StudyStorageKey
        {
        get { return _studyStorageKey; }
        set { _studyStorageKey = value; }
        }
        #endregion

        #region Static Methods
        static public ArchiveStudyStorage Load(ServerEntityKey key)
        {
            using (IReadContext read = PersistentStoreRegistry.GetDefaultStore().OpenReadContext())
            {
                return Load(read, key);
            }
        }
        static public ArchiveStudyStorage Load(IReadContext read, ServerEntityKey key)
        {
            IArchiveStudyStorageEntityBroker broker = read.GetBroker<IArchiveStudyStorageEntityBroker>();
            ArchiveStudyStorage theObject = broker.Load(key);
            return theObject;
        }
        static public ArchiveStudyStorage Insert(ArchiveStudyStorage table)
        {
            using (IUpdateContext update = PersistentStoreRegistry.GetDefaultStore().OpenUpdateContext(UpdateContextSyncMode.Flush))
            {
                return Insert(update, table);
            }
        }
        static public ArchiveStudyStorage Insert(IUpdateContext update, ArchiveStudyStorage table)
        {
            IArchiveStudyStorageEntityBroker broker = update.GetBroker<IArchiveStudyStorageEntityBroker>();
            ArchiveStudyStorageUpdateColumns updateColumns = new ArchiveStudyStorageUpdateColumns();
            updateColumns.ArchiveTime = table.ArchiveTime;
            updateColumns.ArchiveXml = table.ArchiveXml;
            updateColumns.PartitionArchiveKey = table.PartitionArchiveKey;
            updateColumns.ServerTransferSyntaxKey = table.ServerTransferSyntaxKey;
            updateColumns.StudyStorageKey = table.StudyStorageKey;
            ArchiveStudyStorage theObject = broker.Insert(updateColumns);
            update.Commit();
            return theObject;
        }
        #endregion
    }
}
