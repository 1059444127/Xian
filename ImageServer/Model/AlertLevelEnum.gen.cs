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
public partial class AlertLevelEnum : ServerEnum
{
      #region Private Static Members
      private static readonly AlertLevelEnum _Informational = GetEnum("Informational");
      private static readonly AlertLevelEnum _Warning = GetEnum("Warning");
      private static readonly AlertLevelEnum _Error = GetEnum("Error");
      private static readonly AlertLevelEnum _Critical = GetEnum("Critical");
      #endregion

      #region Public Static Properties
      /// <summary>
      /// Informational alert
      /// </summary>
      public static AlertLevelEnum Informational
      {
          get { return _Informational; }
      }
      /// <summary>
      /// Warning alert
      /// </summary>
      public static AlertLevelEnum Warning
      {
          get { return _Warning; }
      }
      /// <summary>
      /// Error alert
      /// </summary>
      public static AlertLevelEnum Error
      {
          get { return _Error; }
      }
      /// <summary>
      /// Critical alert
      /// </summary>
      public static AlertLevelEnum Critical
      {
          get { return _Critical; }
      }

      #endregion

      #region Constructors
      public AlertLevelEnum():base("AlertLevelEnum")
      {}
      #endregion
      #region Public Members
      public override void SetEnum(short val)
      {
          ServerEnumHelper<AlertLevelEnum, IAlertLevelEnumBroker>.SetEnum(this, val);
      }
      static public List<AlertLevelEnum> GetAll()
      {
          return ServerEnumHelper<AlertLevelEnum, IAlertLevelEnumBroker>.GetAll();
      }
      static public AlertLevelEnum GetEnum(string lookup)
      {
          return ServerEnumHelper<AlertLevelEnum, IAlertLevelEnumBroker>.GetEnum(lookup);
      }
      #endregion
}
}
