#region License

// Copyright (c) 2006-2007, ClearCanvas Inc.
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

using System;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Desktop;
using ClearCanvas.Ris.Application.Common.ModalityWorkflow;
using System.Collections;

namespace ClearCanvas.Ris.Client.Adt
{
    public class TechnologistDocumentationDocument : Document
    {
        private readonly ModalityWorklistItem _item;
        private readonly IEnumerable _folders;

        public TechnologistDocumentationDocument(string accessionNumber, ModalityWorklistItem item, IEnumerable folders, IDesktopWindow desktopWindow)
            : base(accessionNumber, desktopWindow)
        {
            if(string.IsNullOrEmpty(accessionNumber))
            {
                throw new ArgumentException("Cannot be null or empty", "accessionNumber");
            }
            if(item == null)
            {
                throw new ArgumentNullException("item");
            }

            _item = item;
            _folders = folders;
        }

        protected override string GetTitle()
        {
            return string.Format("A# {0} - {1}, {2}", _item.AccessionNumber, _item.PatientName.FamilyName, _item.PatientName.GivenName);
        }

        protected override IApplicationComponent GetComponent()
        {
            TechnologistDocumentationComponent component = new TechnologistDocumentationComponent(_item);

            component.DocumentSaved += DocumentSaved;
            component.DocumentCompleted += DocumentCompleted;

            return component;
        }

        private void DocumentSaved(object sender, EventArgs e)
        {
            this.Close();
        }

        private void DocumentCompleted(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}