#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using ClearCanvas.Common;
using ClearCanvas.Dicom;
using ClearCanvas.Dicom.DataStore;
using ClearCanvas.Dicom.Network;
using ClearCanvas.Dicom.Network.Scp;
using ClearCanvas.ImageViewer.Services.Auditing;
using System.Net;

namespace ClearCanvas.ImageViewer.Shreds.DicomServer
{
	[ExtensionOf(typeof(DicomScpExtensionPoint<IDicomServerContext>))]
	public class FindScpExtension : ScpExtension, IDicomScp<IDicomServerContext>
	{
		public FindScpExtension()
			: base(GetSupportedSops())
		{
		}

		private static IEnumerable<SupportedSop> GetSupportedSops()
		{
			SupportedSop sop = new SupportedSop();
			sop.SopClass = SopClass.StudyRootQueryRetrieveInformationModelFind;
			sop.SyntaxList.Add(TransferSyntax.ExplicitVrLittleEndian);
			sop.SyntaxList.Add(TransferSyntax.ImplicitVrLittleEndian);
			yield return sop;
		}

		private static string GetRemoteHostName(AssociationParameters association)
		{
			string remoteHostName = null;
			try
			{
				if (association.RemoteEndPoint != null)
				{
					try
					{
						IPHostEntry entry = Dns.GetHostEntry(association.RemoteEndPoint.Address);
						remoteHostName = entry.HostName;
					}
					catch
					{
						remoteHostName = association.RemoteEndPoint.Address.ToString();
					}
				}
			}
			catch (Exception e)
			{
				remoteHostName = null;
				Platform.Log(LogLevel.Warn, e, "Unable to resolve remote host name for auditing.");
			}

			return remoteHostName;
		}

		public override bool OnReceiveRequest(Dicom.Network.DicomServer server, ServerAssociationParameters association, byte presentationID, DicomMessage message)
		{
			String level = message.DataSet[DicomTags.QueryRetrieveLevel].GetString(0, "").Trim();

			if (message.AffectedSopClassUid.Equals(SopClass.StudyRootQueryRetrieveInformationModelFindUid))
			{
				try
				{
					using (IDataStoreReader reader = DataAccessLayer.GetIDataStoreReader())
					{
						IEnumerable<DicomAttributeCollection> results = reader.Query(message.DataSet);
						foreach (DicomAttributeCollection result in results)
						{
							DicomMessage response = new DicomMessage();
							foreach (DicomAttribute attribute in result)
								response.DataSet[attribute.Tag] = attribute.Copy();

							//Add these to each response.
							message.DataSet[DicomTags.RetrieveAeTitle].SetStringValue(Context.AETitle);
							message.DataSet[DicomTags.InstanceAvailability].SetStringValue("ONLINE");

							response.DataSet[DicomTags.QueryRetrieveLevel].SetStringValue(level);
							server.SendCFindResponse(presentationID, message.MessageId, response,
													 DicomStatuses.Pending);
						}
					}
				}
				catch (Exception e)
				{
					Platform.Log(LogLevel.Error, e, "Unexpected exception when processing FIND request.");

					try
					{
						DicomMessage errorResponse = new DicomMessage();
						server.SendCFindResponse(presentationID, message.MessageId, errorResponse,
						                         DicomStatuses.QueryRetrieveUnableToProcess);

						return true;
					}
					finally
					{
						AuditHelper.LogQueryReceived(association.CallingAE, GetRemoteHostName(association), EventResult.SeriousFailure,
						                             message.AffectedSopClassUid, message.DataSet);
					}
				}

				try
				{
					DicomMessage finalResponse = new DicomMessage();
					server.SendCFindResponse(presentationID, message.MessageId, finalResponse, DicomStatuses.Success);

					AuditHelper.LogQueryReceived(association.CallingAE, GetRemoteHostName(association), EventResult.Success,
					                             message.AffectedSopClassUid, message.DataSet);
					return true;
				}
				catch
				{
					AuditHelper.LogQueryReceived(association.CallingAE, GetRemoteHostName(association), EventResult.SeriousFailure,
					                             message.AffectedSopClassUid, message.DataSet);
					throw;
				}
			}

			try
			{
				Platform.Log(LogLevel.Error, "Unexpected Study Root Query/Retrieve level: {0}", level);
				server.SendCFindResponse(presentationID, message.MessageId, new DicomMessage(),
										 DicomStatuses.QueryRetrieveIdentifierDoesNotMatchSOPClass);
				return true;
			}
			finally
			{
				AuditHelper.LogQueryReceived(association.CallingAE, GetRemoteHostName(association), EventResult.SeriousFailure,
				                             message.AffectedSopClassUid, message.DataSet);
			}
		}
	}
}
