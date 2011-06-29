#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Enterprise.Common;
using ClearCanvas.Enterprise.Common.Configuration;
using ClearCanvas.Enterprise.Configuration.Brokers;
using ClearCanvas.Enterprise.Core;
using ClearCanvas.Common.Configuration;

namespace ClearCanvas.Enterprise.Configuration
{
	[ExtensionOf(typeof(CoreServiceExtensionPoint))]
	[ServiceImplementsContract(typeof(IConfigurationService))]
	public class ConfigurationService : ConfigurationServiceBase, IConfigurationService
	{
		#region IConfigurationService Members

		// because this service is invoked by the framework, rather than by the application,
		// it is safest to use a new persistence scope
		[ReadOperation(PersistenceScopeOption = PersistenceScopeOption.RequiresNew)]
		[ResponseCaching("GetSettingsMetadataCachingDirective")]
		public ListSettingsGroupsResponse ListSettingsGroups(ListSettingsGroupsRequest request)
		{
			var broker = PersistenceContext.GetBroker<IConfigurationSettingsGroupBroker>();
			return new ListSettingsGroupsResponse(
				CollectionUtils.Map(broker.FindAll(), (ConfigurationSettingsGroup g) => g.GetDescriptor()));
		}

		// because this service is invoked by the framework, rather than by the application,
		// it is safest to use a new persistence scope
		[ReadOperation(PersistenceScopeOption = PersistenceScopeOption.RequiresNew)]
		[ResponseCaching("GetSettingsMetadataCachingDirective")]
		public ListSettingsPropertiesResponse ListSettingsProperties(ListSettingsPropertiesRequest request)
		{
			Platform.CheckForNullReference(request, "request");
			Platform.CheckMemberIsSet(request.Group, "Group");

			var where = ConfigurationSettingsGroup.GetCriteria(request.Group);
			var broker = PersistenceContext.GetBroker<IConfigurationSettingsGroupBroker>();
			var group = broker.FindOne(where);

			return new ListSettingsPropertiesResponse(
				CollectionUtils.Map(group.SettingsProperties, (ConfigurationSettingsProperty p) => p.GetDescriptor()));
		}

		public ListConfigurationDocumentsResponse ListConfigurationDocuments(ListConfigurationDocumentsRequest request)
		{
			throw new NotImplementedException();
		}

		[UpdateOperation]
		public ImportSettingsGroupResponse ImportSettingsGroup(ImportSettingsGroupRequest request)
		{
			Platform.CheckForNullReference(request, "request");
			Platform.CheckMemberIsSet(request.Group, "Group");

			var broker = PersistenceContext.GetBroker<IConfigurationSettingsGroupBroker>();
			var where = ConfigurationSettingsGroup.GetCriteria(request.Group);
			var group = CollectionUtils.FirstElement(broker.Find(where));
			if (group == null)
			{
				// group doesn't exist, need to create it
				group = new ConfigurationSettingsGroup();
				group.UpdateFromDescriptor(request.Group);
				PersistenceContext.Lock(group, DirtyState.New);
			}
			else
			{
				// update group from descriptor
				group.UpdateFromDescriptor(request.Group);
			}

			if (request.Properties != null)
			{
				// update properties
				group.SettingsProperties.Clear();
				foreach (var descriptor in request.Properties)
				{
					var property = new ConfigurationSettingsProperty();
					property.UpdateFromDescriptor(descriptor);
					group.SettingsProperties.Add(property);
				}
			}

			PersistenceContext.SynchState();

			return new ImportSettingsGroupResponse();
		}

		// because this service is invoked by the framework, rather than by the application,
		// it is safest to use a new persistence scope
		[ReadOperation(PersistenceScopeOption = PersistenceScopeOption.RequiresNew)]
		[ResponseCaching("GetDocumentCachingDirective")]
		public GetConfigurationDocumentResponse GetConfigurationDocument(GetConfigurationDocumentRequest request)
		{
			Platform.CheckForNullReference(request, "request");
			Platform.CheckMemberIsSet(request.DocumentKey, "DocumentKey");

			return GetConfigurationDocumentHelper(request.DocumentKey);
		}

		// because this service is invoked by the framework, rather than by the application,
		// it is safest to use a new persistence scope
		[UpdateOperation(PersistenceScopeOption = PersistenceScopeOption.RequiresNew)]
		[Audit(typeof(ConfigurationServiceRecorder))]
		public SetConfigurationDocumentResponse SetConfigurationDocument(SetConfigurationDocumentRequest request)
		{
			Platform.CheckForNullReference(request, "request");
			Platform.CheckMemberIsSet(request.DocumentKey, "DocumentKey");

			CheckWriteAccess(request.DocumentKey);

			var broker = PersistenceContext.GetBroker<IConfigurationDocumentBroker>();
			var criteria = BuildCurrentVersionCriteria(request.DocumentKey);
			var documents = broker.Find(criteria, new SearchResultPage(0, 1), new EntityFindOptions { Cache = true });

			var document = CollectionUtils.FirstElement(documents);
			if (document != null)
			{
				document.Body.DocumentText = request.Content;
			}
			else
			{
				// no saved document, create new
				document = NewDocument(request.DocumentKey);
				document.Body.DocumentText = request.Content;
				PersistenceContext.Lock(document, DirtyState.New);
			}

			return new SetConfigurationDocumentResponse();
		}

		// because this service is invoked by the framework, rather than by the application,
		// it is safest to use a new persistence scope
		[UpdateOperation(PersistenceScopeOption = PersistenceScopeOption.RequiresNew)]
		public RemoveConfigurationDocumentResponse RemoveConfigurationDocument(RemoveConfigurationDocumentRequest request)
		{
			Platform.CheckForNullReference(request, "request");
			Platform.CheckMemberIsSet(request.DocumentKey, "DocumentKey");

			CheckWriteAccess(request.DocumentKey);

			try
			{
				var broker = PersistenceContext.GetBroker<IConfigurationDocumentBroker>();
				var criteria = BuildCurrentVersionCriteria(request.DocumentKey);
				var document = broker.FindOne(criteria);
				broker.Delete(document);
			}
			catch (EntityNotFoundException)
			{
				// no document - nothing to remove
			}

			return new RemoveConfigurationDocumentResponse();
		}

		#endregion


		private static ConfigurationDocument NewDocument(ConfigurationDocumentKey key)
		{
			return new ConfigurationDocument(key.DocumentName,
				VersionUtils.ToPaddedVersionString(key.Version, false, false),
				StringUtilities.NullIfEmpty(key.User),
				StringUtilities.NullIfEmpty(key.InstanceKey));
		}
	}
}
