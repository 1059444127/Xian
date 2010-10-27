#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Runtime.Serialization;
using ClearCanvas.Web.Common;

namespace ClearCanvas.ImageViewer.Web.Common.Messages
{
    [DataContract(Namespace = ViewerNamespace.Value)]
    public class MouseMessage : Message
    {
        [DataMember(IsRequired = false)]
        public Position MousePosition { get; set; }

        [DataMember(IsRequired = false)]
        public MouseButton Button { get; set; }

		[DataMember(IsRequired = false)]
		public int ClickCount { get; set; }

        [DataMember(IsRequired = false)]
        public MouseButtonState MouseButtonState { get; set; }

        public override string ToString()
        {
            return String.Format("Mouse Message {0}: {1} {2}", Identifier, Button, MouseButtonState);
        }
    }
}