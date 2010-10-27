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
using System.Collections;

namespace ClearCanvas.Web.Common
{
	[DataContract(Namespace = Namespace.Value)]
    public abstract class Message
    {
        [DataMember(IsRequired = true)]
        public Guid Identifier { get; set; }

        [DataMember(IsRequired = false)]
        public Guid TargetId { get; set; }

        #region Equality

        public override bool Equals(object obj)
        {
            if (obj is Message)
                return Equals((Message)obj);

            return false;
        }

        public static bool operator ==(Message e1, Message e2)
        {
            return Object.Equals(e1, e2);
        }

        public static bool operator !=(Message e1, Message e2)
        {
            return !Object.Equals(e1, e2);
        }

        #region IEquatable<Message> Members

        public bool Equals(Message other)
        {
            return other.Identifier == Identifier;
        }

        #endregion
        #endregion

        public override int GetHashCode()
        {
            unchecked
            {
                return Identifier.GetHashCode();
            }
        }
	}

    [DataContract(Namespace = Namespace.Value)]
    public class MessageSet : IEnumerable
    {
		[DataMember(IsRequired = true)]
		public Guid ApplicationId { get; set; }
		
		[DataMember(IsRequired = true)]
        public Message[] Messages { get; set; }

		[DataMember(IsRequired = true)]
		public int Number { get; set; }

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return (Messages ?? new Message[0]).GetEnumerator();
		}

		#endregion
	}
}