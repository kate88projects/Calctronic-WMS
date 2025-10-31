using Microsoft.AspNetCore.Mvc;
using RackingSystem.Models;
using RackingSystem.Models.GRN;

namespace RackingSystem.Services.GRNServices
{
    public interface IGRNService
    {
        public Task<ServiceResponseModel<GRNDtlDTO>> SaveGRNDtl(GRNDtlReqDTO req);
        public Task<ServiceResponseModel<GRNDtlDTO>> DeleteGRNDtl(GRNDtlDTO req);

        public Task<ServiceResponseModel<int>> GetGRNDetailTotalCount(GRNSearchReqDTO req);
        public Task<ServiceResponseModel<List<GRNDtlListDTO>>> GetGRNDetailList(GRNSearchReqDTO req);
        public Task<ServiceResponseModel<GRNDtlDTO>> GetLatestGRNDetail(Guid detailId);

    }
}
