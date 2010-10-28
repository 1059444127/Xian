#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System.Collections.Generic;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Enterprise.Core;
using ClearCanvas.Healthcare;
using ClearCanvas.Ris.Application.Common;

namespace ClearCanvas.Ris.Application.Services
{
    public class OrderAttachmentAssembler
    {
        class OrderAttachmentSynchronizeHelper : CollectionSynchronizeHelper<OrderAttachment, OrderAttachmentSummary>
        {
            private readonly OrderAttachmentAssembler _assembler;
            private readonly IPersistenceContext _context;
            private readonly Staff _currentUserStaff;

            public OrderAttachmentSynchronizeHelper(OrderAttachmentAssembler assembler, Staff currentUserStaff, IPersistenceContext context)
                : base(true, true)
            {
                _assembler = assembler;
                _context = context;
                _currentUserStaff = currentUserStaff;
            }

            protected override bool CompareItems(OrderAttachment domainItem, OrderAttachmentSummary sourceItem)
            {
                return Equals(domainItem.Document.GetRef(), sourceItem.Document.DocumentRef);
            }

            protected override void AddItem(OrderAttachmentSummary sourceItem, ICollection<OrderAttachment> domainList)
            {
                OrderAttachment attachment = _assembler.CreateOrderAttachment(sourceItem, _currentUserStaff, _context);
                attachment.Document.Attach();
                domainList.Add(attachment);
            }

            protected override void UpdateItem(OrderAttachment domainItem, OrderAttachmentSummary sourceItem, ICollection<OrderAttachment> domainList)
            {
                _assembler.UpdateOrderAttachment(domainItem, sourceItem, _context);
            }

            protected override void RemoveItem(OrderAttachment domainItem, ICollection<OrderAttachment> domainList)
            {
                domainList.Remove(domainItem);
                domainItem.Document.Detach();
            }
        }

        public void Synchronize(IList<OrderAttachment> domainList, IList<OrderAttachmentSummary> sourceList, Staff currentUserStaff, IPersistenceContext context)
        {
            OrderAttachmentSynchronizeHelper synchronizer = new OrderAttachmentSynchronizeHelper(this, currentUserStaff, context);
            synchronizer.Synchronize(domainList, sourceList);
        }

        public OrderAttachmentSummary CreateOrderAttachmentSummary(OrderAttachment attachment, IPersistenceContext context)
        {
            AttachedDocumentAssembler attachedDocAssembler = new AttachedDocumentAssembler();
            StaffAssembler staffAssembler = new StaffAssembler();

            return new OrderAttachmentSummary(
                EnumUtils.GetEnumValueInfo(attachment.Category),
                staffAssembler.CreateStaffSummary(attachment.AttachedBy, context),
                attachment.AttachedTime,
                attachedDocAssembler.CreateAttachedDocumentSummary(attachment.Document));
        }

        public OrderAttachment CreateOrderAttachment(OrderAttachmentSummary summary, Staff currentUserStaff, IPersistenceContext context)
        {
            return new OrderAttachment(
                EnumUtils.GetEnumValue<OrderAttachmentCategoryEnum>(summary.Category, context),
                summary.AttachedBy == null ? currentUserStaff : context.Load<Staff>(summary.AttachedBy.StaffRef),
                Platform.Time,
                context.Load<AttachedDocument>(summary.Document.DocumentRef));
        }

        public void UpdateOrderAttachment(OrderAttachment attachment, OrderAttachmentSummary summary, IPersistenceContext context)
        {
            AttachedDocumentAssembler mimeDocAssembler = new AttachedDocumentAssembler();
            attachment.Category = EnumUtils.GetEnumValue<OrderAttachmentCategoryEnum>(summary.Category, context);
            mimeDocAssembler.UpdateAttachedDocumentSummary(attachment.Document, summary.Document);
        }
    }
}
