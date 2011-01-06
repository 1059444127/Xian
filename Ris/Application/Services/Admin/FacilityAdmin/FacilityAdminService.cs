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
using ClearCanvas.Enterprise.Common;
using ClearCanvas.Enterprise.Core;
using ClearCanvas.Enterprise.Core.Modelling;
using ClearCanvas.Healthcare;
using ClearCanvas.Healthcare.Brokers;
using ClearCanvas.Ris.Application.Common;
using ClearCanvas.Ris.Application.Common.Admin.FacilityAdmin;
using AuthorityTokens=ClearCanvas.Ris.Application.Common.AuthorityTokens;

namespace ClearCanvas.Ris.Application.Services.Admin.FacilityAdmin
{
    [ExtensionOf(typeof(ApplicationServiceExtensionPoint))]
    [ServiceImplementsContract(typeof(IFacilityAdminService))]
    public class FacilityAdminService : ApplicationServiceBase, IFacilityAdminService
    {
        #region IFacilityAdminService Members

        [ReadOperation]
        public ListAllFacilitiesResponse ListAllFacilities(ListAllFacilitiesRequest request)
        {
            FacilitySearchCriteria criteria = new FacilitySearchCriteria();
			criteria.Code.SortAsc(0);
			if (!request.IncludeDeactivated)
				criteria.Deactivated.EqualTo(false);

            FacilityAssembler assembler = new FacilityAssembler();
            return new ListAllFacilitiesResponse(
                CollectionUtils.Map<Facility, FacilitySummary, List<FacilitySummary>>(
                    PersistenceContext.GetBroker<IFacilityBroker>().Find(criteria, request.Page),
                    delegate(Facility f)
                    {
                        return assembler.CreateFacilitySummary(f);
                    }));
        }

        [ReadOperation]
        public GetFacilityEditFormDataResponse GetFacilityEditFormData(GetFacilityEditFormDataRequest request)
        {
            return new GetFacilityEditFormDataResponse(EnumUtils.GetEnumValueList<InformationAuthorityEnum>(PersistenceContext));
        }

        [ReadOperation]
        public LoadFacilityForEditResponse LoadFacilityForEdit(LoadFacilityForEditRequest request)
        {
            // note that the version of the FacilityRef is intentionally ignored here (default behaviour of ReadOperation)
            Facility f = PersistenceContext.Load<Facility>(request.FacilityRef);
            FacilityAssembler assembler = new FacilityAssembler();

            return new LoadFacilityForEditResponse(assembler.CreateFacilityDetail(f));
        }

        [UpdateOperation]
        [PrincipalPermission(SecurityAction.Demand, Role = AuthorityTokens.Admin.Data.Facility)]
        public AddFacilityResponse AddFacility(AddFacilityRequest request)
        {
            Facility facility = new Facility();
            FacilityAssembler assembler = new FacilityAssembler();
            assembler.UpdateFacility(request.FacilityDetail, facility, this.PersistenceContext);

            PersistenceContext.Lock(facility, DirtyState.New);

            // ensure the new facility is assigned an OID before using it in the return value
            PersistenceContext.SynchState();

            return new AddFacilityResponse(assembler.CreateFacilitySummary(facility));
        }

        [UpdateOperation]
		[PrincipalPermission(SecurityAction.Demand, Role = AuthorityTokens.Admin.Data.Facility)]
		public UpdateFacilityResponse UpdateFacility(UpdateFacilityRequest request)
        {
            Facility facility = PersistenceContext.Load<Facility>(request.FacilityDetail.FacilityRef, EntityLoadFlags.CheckVersion);

            FacilityAssembler assembler = new FacilityAssembler();
            assembler.UpdateFacility(request.FacilityDetail, facility, this.PersistenceContext);

            return new UpdateFacilityResponse(assembler.CreateFacilitySummary(facility));
        }

		[UpdateOperation]
		[PrincipalPermission(SecurityAction.Demand, Role = AuthorityTokens.Admin.Data.Facility)]
		public DeleteFacilityResponse DeleteFacility(DeleteFacilityRequest request)
		{
			try
			{
				IFacilityBroker broker = PersistenceContext.GetBroker<IFacilityBroker>();
				Facility item = broker.Load(request.FacilityRef, EntityLoadFlags.Proxy);
				broker.Delete(item);
				PersistenceContext.SynchState();
				return new DeleteFacilityResponse();
			}
			catch (PersistenceException)
			{
				throw new RequestValidationException(string.Format(SR.ExceptionFailedToDelete, TerminologyTranslator.Translate(typeof(Facility))));
			}
		}

		#endregion

    }
}
