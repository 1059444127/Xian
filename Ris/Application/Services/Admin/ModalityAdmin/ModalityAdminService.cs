#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System.Collections.Generic;
using System.Security.Permissions;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Enterprise.Core.Modelling;
using ClearCanvas.Healthcare;
using ClearCanvas.Healthcare.Brokers;
using ClearCanvas.Enterprise.Core;
using ClearCanvas.Enterprise.Common;
using ClearCanvas.Ris.Application.Common;
using ClearCanvas.Ris.Application.Common.Admin.ModalityAdmin;
using AuthorityTokens=ClearCanvas.Ris.Application.Common.AuthorityTokens;

namespace ClearCanvas.Ris.Application.Services.Admin.ModalityAdmin
{
	[ExtensionOf(typeof(ApplicationServiceExtensionPoint))]
	[ServiceImplementsContract(typeof(IModalityAdminService))]
	public class ModalityAdminService : ApplicationServiceBase, IModalityAdminService
	{
		#region IModalityAdminService Members

		[ReadOperation]
		public ListAllModalitiesResponse ListAllModalities(ListAllModalitiesRequest request)
		{
			var criteria = new ModalitySearchCriteria();
			criteria.Id.SortAsc(0);
			if (!request.IncludeDeactivated)
				criteria.Deactivated.EqualTo(false);

			var assembler = new ModalityAssembler();
			return new ListAllModalitiesResponse(
				CollectionUtils.Map<Modality, ModalitySummary, List<ModalitySummary>>(
					PersistenceContext.GetBroker<IModalityBroker>().Find(criteria, request.Page),
					assembler.CreateModalitySummary));
		}

		[ReadOperation]
		public LoadModalityForEditResponse LoadModalityForEdit(LoadModalityForEditRequest request)
		{
			// note that the version of the ModalityRef is intentionally ignored here (default behaviour of ReadOperation)
			var m = PersistenceContext.Load<Modality>(request.ModalityRef);
			var assembler = new ModalityAssembler();
			return new LoadModalityForEditResponse(assembler.CreateModalityDetail(m));
		}

		[ReadOperation]
		public LoadModalityEditorFormDataResponse LoadModalityEditorFormData(LoadModalityEditorFormDataRequest request)
		{
			return new LoadModalityEditorFormDataResponse
			{
				DicomModalityChoices = EnumUtils.GetEnumValueList<DicomModalityEnum>(this.PersistenceContext)
			};
		}

		[UpdateOperation]
		[PrincipalPermission(SecurityAction.Demand, Role = AuthorityTokens.Admin.Data.Modality)]
		public AddModalityResponse AddModality(AddModalityRequest request)
		{
			var modality = new Modality();
			var assembler = new ModalityAssembler();
			assembler.UpdateModality(request.ModalityDetail, modality, this.PersistenceContext);

			PersistenceContext.Lock(modality, DirtyState.New);

			// ensure the new modality is assigned an OID before using it in the return value
			PersistenceContext.SynchState();

			return new AddModalityResponse(assembler.CreateModalitySummary(modality));
		}

		[UpdateOperation]
		[PrincipalPermission(SecurityAction.Demand, Role = AuthorityTokens.Admin.Data.Modality)]
		public UpdateModalityResponse UpdateModality(UpdateModalityRequest request)
		{
			var modality = PersistenceContext.Load<Modality>(request.ModalityDetail.ModalityRef, EntityLoadFlags.CheckVersion);
			var assembler = new ModalityAssembler();
			assembler.UpdateModality(request.ModalityDetail, modality, this.PersistenceContext);

			return new UpdateModalityResponse(assembler.CreateModalitySummary(modality));
		}

		[UpdateOperation]
		[PrincipalPermission(SecurityAction.Demand, Role = AuthorityTokens.Admin.Data.Modality)]
		public DeleteModalityResponse DeleteModality(DeleteModalityRequest request)
		{
			try
			{
				var broker = PersistenceContext.GetBroker<IModalityBroker>();
				var item = broker.Load(request.ModalityRef, EntityLoadFlags.Proxy);
				broker.Delete(item);
				PersistenceContext.SynchState();
				return new DeleteModalityResponse();
			}
			catch (PersistenceException)
			{
				throw new RequestValidationException(string.Format(SR.ExceptionFailedToDelete, TerminologyTranslator.Translate(typeof(Modality))));
			}
		}

		#endregion
	}
}
