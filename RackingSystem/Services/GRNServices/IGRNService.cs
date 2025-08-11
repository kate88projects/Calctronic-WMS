using RackingSystem.Models.GRN;
using RackingSystem.Models;

namespace RackingSystem.Services.GRNServices
{
    public interface IGRNService
    {
        public Task<ServiceResponseModel<GRNDtlDTO>> SaveGRNDtl(GRNDtlReqDTO req); 

        public Task<ServiceResponseModel<int>> GetGRNDetailTotalCount(GRNSearchReqDTO req);
        public Task<ServiceResponseModel<List<GRNDtlListDTO>>> GetGRNDetailList(GRNSearchReqDTO req);
    }
}
