using RackingSystem.Data.Maintenances;
using RackingSystem.Models;
using RackingSystem.Models.BOM;
using RackingSystem.Models.Item;

namespace RackingSystem.Services.BOMServices
{
    public interface IBOMService
    {
        public Task<ServiceResponseModel<BOMListDTO>> GetBOM(long bomId);
        public Task<ServiceResponseModel<List<BOMListDTO>>> GetBOMList(BOMSearchReqDTO req);
        public Task<ServiceResponseModel<List<BOMDtlDTO>>> GetBOMDetail(long bomId);
        public Task<ServiceResponseModel<BOMDtlReqDTO>> SaveBOM(BOMDtlReqDTO bom);
        public Task<ServiceResponseModel<BOMListDTO>> DeleteBOM(BOMListDTO bom);
        public Task<ServiceResponseModel<int>> GetBOMTotalCount(BOMSearchReqDTO req);

        public Task<ServiceResponseModel<List<BOMListReqDTO>>> GetActiveBOMList();
        public Task<ServiceResponseModel<BOMExcelReqDTO>> SaveExcelBOM(BOMExcelReqDTO boms);


    }
}
