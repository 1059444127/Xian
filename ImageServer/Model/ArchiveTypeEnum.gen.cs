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
    using System.Collections.Generic;
    using ClearCanvas.ImageServer.Model.EntityBrokers;
    using ClearCanvas.ImageServer.Enterprise;
    using System.Reflection;

[Serializable]
public partial class ArchiveTypeEnum : ServerEnum
{
      #region Private Static Members
      private static readonly ArchiveTypeEnum _HsmArchive = GetEnum("HsmArchive");
      #endregion

      #region Public Static Properties
      /// <summary>
      /// Hierarchical storage management archive such as StorageTek QFS
      /// </summary>
      public static ArchiveTypeEnum HsmArchive
      {
          get { return _HsmArchive; }
      }

      #endregion

      #region Constructors
      public ArchiveTypeEnum():base("ArchiveTypeEnum")
      {}
      #endregion
      #region Public Members
      public override void SetEnum(short val)
      {
          ServerEnumHelper<ArchiveTypeEnum, IArchiveTypeEnumBroker>.SetEnum(this, val);
      }
      static public List<ArchiveTypeEnum> GetAll()
      {
          return ServerEnumHelper<ArchiveTypeEnum, IArchiveTypeEnumBroker>.GetAll();
      }
      static public ArchiveTypeEnum GetEnum(string lookup)
      {
          return ServerEnumHelper<ArchiveTypeEnum, IArchiveTypeEnumBroker>.GetEnum(lookup);
      }
      #endregion
}
}
