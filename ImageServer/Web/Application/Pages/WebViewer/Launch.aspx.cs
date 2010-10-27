﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using System.Web;
using ClearCanvas.Common;
using ClearCanvas.Enterprise.Common;
using ClearCanvas.Enterprise.Core;
using ClearCanvas.ImageServer.Enterprise.Authentication;
using ClearCanvas.ImageServer.Model;
using ClearCanvas.ImageServer.Model.EntityBrokers;
using ClearCanvas.ImageServer.Web.Common.Data;

namespace ClearCanvas.ImageServer.Web.Application.Pages.WebViewer
{
    public partial class Launch : System.Web.UI.Page
    {
        //private string _sessionId;
        
        public string UserID
        {
            get;
            set;
        }

        public string Password
        {
            get; set;
        }

        public string AppName
        {
            get;
            set;
        }

        public string ListStudies
        {
            get;
            set;
        }


        public string WebViewerInitString
        {
            get;
            set;
        }

        /// <summary>
        /// Set a <see cref="ISearchCondition{T}"/> for DICOM string based (wildcard matching) value.
        /// </summary>
        /// <param name="cond"></param>
        /// <param name="val"></param>
        private static void SetStringCondition(ISearchCondition<string> cond, string val)
        {
            if (val.Length == 0)
                return;

            if (val.Contains("*") || val.Contains("?"))
            {
                //TODO Remove when paging is implemented on queries
                int charCount = 0;
                foreach (char c in val)
                    if (c != '*' && c != '?')
                        charCount++;
                if (charCount < 4)
                    throw new ArgumentException("Wildcard parameters require at least 4 characters.");

                String value = val.Replace("%", "[%]").Replace("_", "[_]");
                value = value.Replace('*', '%');
                value = value.Replace('?', '_');
                cond.Like(value);
            }
            else
                cond.EqualTo(val);
        }

        protected void Page_Load(object sender, EventArgs e)
        {           
            UserID = Request.Params[ImageServerConstants.WebViewerQueryStrings.Username];
            Password = Request.Params[ImageServerConstants.WebViewerQueryStrings.Password];
            AppName = Request.Params[ImageServerConstants.WebViewerQueryStrings.ApplicationName];
            ListStudies = Request.Params[ImageServerConstants.WebViewerQueryStrings.ListStudies];
            WebViewerInitString = Request.Params[ImageServerConstants.WebViewerQueryStrings.WebViewerInitParams];

            //Try to authenticate the user
            if (!string.IsNullOrEmpty(UserID) && !string.IsNullOrEmpty(Password))
            {
                if(String.IsNullOrEmpty(AppName))
                {
                    int start = WebViewerInitString.IndexOf(ImageServerConstants.WebViewerQueryStrings.ApplicationName + "=");

                    if (start < 0) AppName = ImageServerConstants.DefaultApplicationName;
                    else
                    {
                        start += (ImageServerConstants.WebViewerQueryStrings.ApplicationName + "=").Length;
                        AppName = WebViewerInitString.Substring(start);
                        int end = AppName.IndexOf(',');
                        AppName = AppName.Substring(0, end);
                        if (string.IsNullOrEmpty(AppName)) AppName = ImageServerConstants.DefaultApplicationName;
                    }
                }
            
                AppName = String.Format("{0}@{1}", AppName, HttpContext.Current.Request.UserHostName);
                
                try
                {
                    using (LoginService service = new LoginService())
                    {
                        SessionInfo session = service.Login(UserID, Password, AppName);
                        Platform.Log(LogLevel.Info, "[{0}]: {1} has successfully logged in.", AppName, UserID);

                        WebViewerInitString += string.Format(",{0}={1},{2}={3}",
                                                     ImageServerConstants.WebViewerQueryStrings.Username, UserID,
                                                     ImageServerConstants.WebViewerQueryStrings.Session,
                                                     session.Credentials.SessionToken.Id);

                        //_sessionId = session.Credentials.SessionToken.Id;

                        //Add the session information to the context in case we redirect to the studies page.
                        //We need these to properly launch the WebViewer
                        Context.Items.Add(ImageServerConstants.WebViewerQueryStrings.Username, UserID);
                        Context.Items.Add(ImageServerConstants.WebViewerQueryStrings.Session, session.Credentials.SessionToken.Id);
                        Context.Items.Add("Authorized", false);

                        foreach(string role in session.Credentials.Authorities)
                        {
                            if(role.Equals(ImageServerConstants.WebViewerAuthorityToken))
                            {
                                Context.Items["Authorized"] = "true";
                                break;
                            }
                        }
                    }
                }
                catch (PasswordExpiredException)
                {
                    Platform.Log(LogLevel.Info, "[{0}]: {1} encountered PasswordExpiredException.", AppName, UserID);
                    Server.Transfer(ImageServerConstants.PageURLs.WebViewerAuthorizationErrorPage, true);
                }
                catch (UserAccessDeniedException)
                {
                    Platform.Log(LogLevel.Info, "[{0}]: {1} encountered UserAccessDeniedException.", AppName, UserID);
                    Server.Transfer(ImageServerConstants.PageURLs.WebViewerAuthorizationErrorPage, true);
                }        
                catch (Exception ex)
                {
                    Platform.Log(LogLevel.Info, "[{0}]: {1} encountered exception {2} - {3}.", AppName, UserID, ex.GetType(), ex.Message);
                    Server.Transfer(ImageServerConstants.PageURLs.WebViewerAuthorizationErrorPage, true);
                }

                if (!string.IsNullOrEmpty(ListStudies) && ListStudies.Equals("true"))
                {
                    //Extract the WebViewer Init Parameters to determine whether or not we need
                    //to redirect to the Studies page.
                    var initParams = new WebViewerInitParams();
                    string[] vals = HttpUtility.UrlDecode(WebViewerInitString).Split(new[] { '?', ';', '=', ',', '&' });
                    for (int i = 0; i < vals.Length - 1; i++)
                    {
                        if (String.IsNullOrEmpty(vals[i]))
                            continue;

                        if (vals[i].Equals(ImageServerConstants.WebViewerStartupParameters.Study))
                        {
                            i++;
                            initParams.StudyInstanceUids.Add(vals[i]);
                        }
                        else if (vals[i].Equals(ImageServerConstants.WebViewerStartupParameters.PatientID))
                        {
                            i++;
                            initParams.PatientIds.Add(vals[i]);
                        }
                        else if (vals[i].Equals(ImageServerConstants.WebViewerStartupParameters.AeTitle))
                        {
                            i++;
                            initParams.AeTitle = vals[i];
                        }
                        else if (vals[i].Equals(ImageServerConstants.WebViewerStartupParameters.AccessionNumber))
                        {
                            i++; 
                            initParams.AccessionNumbers.Add(vals[i]);
                        }
                    }
                    
                    //Check if there are multiple studies to be displayed. 
                    var controller = new StudyController();
                    var partitionAdapter = new ServerPartitionDataAdapter();
                    var partitionCriteria = new ServerPartitionSelectCriteria();
                    StudySelectCriteria studyCriteria;
                    ServerPartition partition = null;
                    int studyCount = 0;

                    if(!string.IsNullOrEmpty(initParams.AeTitle))
                    {
                        partitionCriteria.AeTitle.EqualTo(initParams.AeTitle);
                        IList<ServerPartition> partitions = partitionAdapter.GetServerPartitions(partitionCriteria);
                        if(partitions.Count == 1)
                        {
                            partition = partitions[0];
                        }
                    }

                    foreach (string patientId in initParams.PatientIds)
                    {
                        studyCriteria = new StudySelectCriteria();
                        if (partition != null) studyCriteria.ServerPartitionKey.EqualTo(partition.Key);
                        SetStringCondition(studyCriteria.PatientId, patientId);
                        studyCount +=controller.GetStudyCount(studyCriteria);
                    }

                    if (studyCount < 2)
                        foreach (string accession in initParams.AccessionNumbers)
                        {
                            studyCriteria = new StudySelectCriteria();
                            if (partition != null) studyCriteria.ServerPartitionKey.EqualTo(partition.Key);
                            SetStringCondition(studyCriteria.AccessionNumber, accession);
                            studyCount += controller.GetStudyCount(studyCriteria);
                        }

                    if (studyCount < 2 && initParams.StudyInstanceUids.Count > 0)
                    {
                        studyCriteria = new StudySelectCriteria();
                        if (partition != null) studyCriteria.ServerPartitionKey.EqualTo(partition.Key);
                        studyCriteria.StudyInstanceUid.In(initParams.StudyInstanceUids);
                        studyCount += controller.GetStudyCount(studyCriteria);
              
                    }

                    if (studyCount > 1) 
                        Server.Transfer(ImageServerConstants.PageURLs.WebViewerStudiesPage, true);
                }
                

                if (string.IsNullOrEmpty(WebViewerInitString))
                {
                    Response.Redirect(ImageServerConstants.PageURLs.WebViewerDefaultPage, true);
                } 
                else
                {
                    Response.Redirect(ImageServerConstants.PageURLs.WebViewerDefaultPage + "?" + ImageServerConstants.WebViewerQueryStrings.WebViewerInitParams + "=" + WebViewerInitString, true);
                }
            } 
            else
            {
                Server.Transfer(ImageServerConstants.PageURLs.WebViewerAuthorizationErrorPage, true);
            }
        }
    }
}
