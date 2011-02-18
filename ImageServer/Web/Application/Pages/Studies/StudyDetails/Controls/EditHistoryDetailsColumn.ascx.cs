#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using ClearCanvas.ImageServer.Common.Utilities;
using ClearCanvas.ImageServer.Core.Edit;
using ClearCanvas.ImageServer.Model;
using ClearCanvas.ImageServer.Services.WorkQueue.WebEditStudy;
using ClearCanvas.ImageServer.Web.Common.Utilities;
using ClearCanvas.ImageServer.Web.Application.App_GlobalResources;

namespace ClearCanvas.ImageServer.Web.Application.Pages.Studies.StudyDetails.Controls
{
    public partial class EditHistoryDetailsColumn : System.Web.UI.UserControl
    {
        private StudyHistory _historyRecord;
        private WebEditStudyHistoryChangeDescription _description;

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public StudyHistory HistoryRecord
        {
            set { _historyRecord = value; }
        }

        public WebEditStudyHistoryChangeDescription EditHistory
        {
            get
            {
                if (_description == null && _historyRecord != null)
                {
                    _description = XmlUtils.Deserialize<WebEditStudyHistoryChangeDescription>(_historyRecord.ChangeDescription.DocumentElement);
                }
                return _description;
            }
        }

        public string GetReason(string reasonString)
        {
            if (string.IsNullOrEmpty(reasonString)) return SR.NoneSpecified;
            string[] reason = reasonString.Split(ImageServerConstants.ReasonCommentSeparator, StringSplitOptions.None);
            return reason[0];
        }

        public string GetComment(string reasonString)
        {
            if (string.IsNullOrEmpty(reasonString)) return SR.NoneSpecified;
            string[] reason = reasonString.Split(ImageServerConstants.ReasonCommentSeparator, StringSplitOptions.None);
            return reason[1];
        }

        protected string ChangeSummaryText
        {
            get{
                return String.Format(SR.EditBy, EditTypeTranslator.Translate(EditHistory.EditType), EditHistory.UserId ?? SR.Unknown);
            }
        }
    }

    public static class EditTypeTranslator{
        public static string Translate(EditType type)
        {
            switch (type)
            {
                case EditType.WebEdit:
                    return HtmlUtility.Encode(SR.StudyDetails_WebEdit_Description);
            }

            return HtmlUtility.Encode(HtmlUtility.GetEnumInfo(type).LongDescription);
        }
    }
}