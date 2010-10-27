﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

// This file is auto-generated by the ClearCanvas.Model.SqlServer2005.CodeGenerator project.

namespace ClearCanvas.ImageServer.Model
{
    using System;
    using System.Xml;
    using ClearCanvas.Enterprise.Core;
    using ClearCanvas.ImageServer.Enterprise;
    using ClearCanvas.ImageServer.Model.EntityBrokers;

    [Serializable]
    public partial class CannedText: ServerEntity
    {
        #region Constructors
        public CannedText():base("CannedText")
        {}
        public CannedText(
             String _label_
            ,String _category_
            ,String _text_
            ):base("CannedText")
        {
            Label = _label_;
            Category = _category_;
            Text = _text_;
        }
        #endregion

        #region Public Properties
        [EntityFieldDatabaseMappingAttribute(TableName="CannedText", ColumnName="Label")]
        public String Label
        { get; set; }
        [EntityFieldDatabaseMappingAttribute(TableName="CannedText", ColumnName="Category")]
        public String Category
        { get; set; }
        [EntityFieldDatabaseMappingAttribute(TableName="CannedText", ColumnName="Text")]
        public String Text
        { get; set; }
        #endregion

        #region Static Methods
        static public CannedText Load(ServerEntityKey key)
        {
            using (IReadContext read = PersistentStoreRegistry.GetDefaultStore().OpenReadContext())
            {
                return Load(read, key);
            }
        }
        static public CannedText Load(IPersistenceContext read, ServerEntityKey key)
        {
            ICannedTextEntityBroker broker = read.GetBroker<ICannedTextEntityBroker>();
            CannedText theObject = broker.Load(key);
            return theObject;
        }
        static public CannedText Insert(CannedText entity)
        {
            using (IUpdateContext update = PersistentStoreRegistry.GetDefaultStore().OpenUpdateContext(UpdateContextSyncMode.Flush))
            {
                CannedText newEntity = Insert(update, entity);
                update.Commit();
                return newEntity;
            }
        }
        static public CannedText Insert(IUpdateContext update, CannedText entity)
        {
            ICannedTextEntityBroker broker = update.GetBroker<ICannedTextEntityBroker>();
            CannedTextUpdateColumns updateColumns = new CannedTextUpdateColumns();
            updateColumns.Label = entity.Label;
            updateColumns.Category = entity.Category;
            updateColumns.Text = entity.Text;
            CannedText newEntity = broker.Insert(updateColumns);
            return newEntity;
        }
        #endregion
    }
}
