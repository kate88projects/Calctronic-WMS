using RackingSystem.Models.Setting;
using RackingSystem.Models;

namespace RackingSystem.Services.SettingServices
{
    public interface ISettingService
    {
        public Task<ServiceResponseModel<List<ReelDimensionListDTO>>> GetReelDimensionList();
        public Task<ServiceResponseModel<ReelDimensionDTO>> SaveReelDimension(ReelDimensionDTO req);
        public Task<ServiceResponseModel<ReelDimensionDTO>> DeleteReelDimension(ReelDimensionDTO itemReq);
        public Task<ServiceResponseModel<List<ReelDimensionListDTO>>> GetReelDimensionList_DDL();

        public Task<ServiceResponseModel<List<SlotCalculationListDTO>>> GetSlotCalculationList();
        public Task<ServiceResponseModel<SlotCalculationDTO>> SaveSlotCalculation(SlotCalculationDTO req);
        public Task<ServiceResponseModel<SlotCalculationDTO>> DeleteSlotCalculation(SlotCalculationDTO itemReq);
    }
}
