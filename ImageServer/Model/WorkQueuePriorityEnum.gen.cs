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
public partial class WorkQueuePriorityEnum : ServerEnum
{
      #region Private Static Members
      private static readonly WorkQueuePriorityEnum _Low = GetEnum("Low");
      private static readonly WorkQueuePriorityEnum _Medium = GetEnum("Medium");
      private static readonly WorkQueuePriorityEnum _High = GetEnum("High");
      private static readonly WorkQueuePriorityEnum _Stat = GetEnum("Stat");
      #endregion

      #region Public Static Properties
      /// <summary>
      /// Low priority
      /// </summary>
      public static WorkQueuePriorityEnum Low
      {
          get { return _Low; }
      }
      /// <summary>
      /// Medium priority
      /// </summary>
      public static WorkQueuePriorityEnum Medium
      {
          get { return _Medium; }
      }
      /// <summary>
      /// High priority
      /// </summary>
      public static WorkQueuePriorityEnum High
      {
          get { return _High; }
      }
      /// <summary>
      /// Stat priority
      /// </summary>
      public static WorkQueuePriorityEnum Stat
      {
          get { return _Stat; }
      }

      #endregion

      #region Constructors
      public WorkQueuePriorityEnum():base("WorkQueuePriorityEnum")
      {}
      #endregion
      #region Public Members
      public override void SetEnum(short val)
      {
          ServerEnumHelper<WorkQueuePriorityEnum, IWorkQueuePriorityEnumBroker>.SetEnum(this, val);
      }
      static public List<WorkQueuePriorityEnum> GetAll()
      {
          return ServerEnumHelper<WorkQueuePriorityEnum, IWorkQueuePriorityEnumBroker>.GetAll();
      }
      static public WorkQueuePriorityEnum GetEnum(string lookup)
      {
          return ServerEnumHelper<WorkQueuePriorityEnum, IWorkQueuePriorityEnumBroker>.GetEnum(lookup);
      }
      #endregion
}
}
