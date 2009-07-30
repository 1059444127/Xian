#region License

// Copyright (c) 2009, ClearCanvas Inc.
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
    using System.Collections.Generic;
    using ClearCanvas.ImageServer.Model.EntityBrokers;
    using ClearCanvas.ImageServer.Enterprise;
    using System.Reflection;

[Serializable]
public partial class WorkQueueTypeEnum : ServerEnum
{
      #region Private Static Members
      private static readonly WorkQueueTypeEnum _StudyProcess = GetEnum("StudyProcess");
      private static readonly WorkQueueTypeEnum _AutoRoute = GetEnum("AutoRoute");
      private static readonly WorkQueueTypeEnum _DeleteStudy = GetEnum("DeleteStudy");
      private static readonly WorkQueueTypeEnum _WebDeleteStudy = GetEnum("WebDeleteStudy");
      private static readonly WorkQueueTypeEnum _WebMoveStudy = GetEnum("WebMoveStudy");
      private static readonly WorkQueueTypeEnum _WebEditStudy = GetEnum("WebEditStudy");
      private static readonly WorkQueueTypeEnum _CleanupStudy = GetEnum("CleanupStudy");
      private static readonly WorkQueueTypeEnum _CompressStudy = GetEnum("CompressStudy");
      private static readonly WorkQueueTypeEnum _MigrateStudy = GetEnum("MigrateStudy");
      private static readonly WorkQueueTypeEnum _PurgeStudy = GetEnum("PurgeStudy");
      private static readonly WorkQueueTypeEnum _ReprocessStudy = GetEnum("ReprocessStudy");
      private static readonly WorkQueueTypeEnum _ReconcileStudy = GetEnum("ReconcileStudy");
      private static readonly WorkQueueTypeEnum _ReconcileCleanup = GetEnum("ReconcileCleanup");
      private static readonly WorkQueueTypeEnum _ReconcilePostProcess = GetEnum("ReconcilePostProcess");
      private static readonly WorkQueueTypeEnum _ProcessDuplicate = GetEnum("ProcessDuplicate");
      private static readonly WorkQueueTypeEnum _DeleteSeries = GetEnum("DeleteSeries");
      #endregion

      #region Public Static Properties
      /// <summary>
      /// Processing of a new incoming study.
      /// </summary>
      public static WorkQueueTypeEnum StudyProcess
      {
          get { return _StudyProcess; }
      }
      /// <summary>
      /// DICOM Auto-route request.
      /// </summary>
      public static WorkQueueTypeEnum AutoRoute
      {
          get { return _AutoRoute; }
      }
      /// <summary>
      /// Automatic deletion of a Study.
      /// </summary>
      public static WorkQueueTypeEnum DeleteStudy
      {
          get { return _DeleteStudy; }
      }
      /// <summary>
      /// Manual study delete via the Web UI.
      /// </summary>
      public static WorkQueueTypeEnum WebDeleteStudy
      {
          get { return _WebDeleteStudy; }
      }
      /// <summary>
      /// Manual DICOM move of a study via the Web UI.
      /// </summary>
      public static WorkQueueTypeEnum WebMoveStudy
      {
          get { return _WebMoveStudy; }
      }
      /// <summary>
      /// Manual study edit via the Web UI.
      /// </summary>
      public static WorkQueueTypeEnum WebEditStudy
      {
          get { return _WebEditStudy; }
      }
      /// <summary>
      /// Cleanup all unprocessed or failed instances within a study.
      /// </summary>
      public static WorkQueueTypeEnum CleanupStudy
      {
          get { return _CleanupStudy; }
      }
      /// <summary>
      /// Compress a study.
      /// </summary>
      public static WorkQueueTypeEnum CompressStudy
      {
          get { return _CompressStudy; }
      }
      /// <summary>
      /// Migrate studies between tiers.
      /// </summary>
      public static WorkQueueTypeEnum MigrateStudy
      {
          get { return _MigrateStudy; }
      }
      /// <summary>
      /// Purge archived study and place offline.
      /// </summary>
      public static WorkQueueTypeEnum PurgeStudy
      {
          get { return _PurgeStudy; }
      }
      /// <summary>
      /// Reprocess an entire study.
      /// </summary>
      public static WorkQueueTypeEnum ReprocessStudy
      {
          get { return _ReprocessStudy; }
      }
      /// <summary>
      /// Reconcile images.
      /// </summary>
      public static WorkQueueTypeEnum ReconcileStudy
      {
          get { return _ReconcileStudy; }
      }
      /// <summary>
      /// Cleanup a failed Reconcile Study entry
      /// </summary>
      public static WorkQueueTypeEnum ReconcileCleanup
      {
          get { return _ReconcileCleanup; }
      }
      /// <summary>
      /// Process reconciled images.
      /// </summary>
      public static WorkQueueTypeEnum ReconcilePostProcess
      {
          get { return _ReconcilePostProcess; }
      }
      /// <summary>
      /// Process duplicate.
      /// </summary>
      public static WorkQueueTypeEnum ProcessDuplicate
      {
          get { return _ProcessDuplicate; }
      }
      /// <summary>
      /// Delete Series.
      /// </summary>
      public static WorkQueueTypeEnum DeleteSeries
      {
          get { return _DeleteSeries; }
      }

      #endregion

      #region Constructors
      public WorkQueueTypeEnum():base("WorkQueueTypeEnum")
      {}
      #endregion
      #region Public Members
      public override void SetEnum(short val)
      {
          ServerEnumHelper<WorkQueueTypeEnum, IWorkQueueTypeEnumBroker>.SetEnum(this, val);
      }
      static public List<WorkQueueTypeEnum> GetAll()
      {
          return ServerEnumHelper<WorkQueueTypeEnum, IWorkQueueTypeEnumBroker>.GetAll();
      }
      static public WorkQueueTypeEnum GetEnum(string lookup)
      {
          return ServerEnumHelper<WorkQueueTypeEnum, IWorkQueueTypeEnumBroker>.GetEnum(lookup);
      }
      #endregion
}
}
