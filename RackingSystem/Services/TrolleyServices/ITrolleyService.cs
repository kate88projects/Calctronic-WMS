using RackingSystem.Models;
using RackingSystem.Models.Slot;
using RackingSystem.Models.Trolley;

namespace RackingSystem.Services.TrolleyServices
{
    public interface ITrolleyService
    {
        public Task<ServiceResponseModel<List<TrolleyListDTO>>> GetTrolleyList();
        public Task<ServiceResponseModel<List<TrolleyListDTO>>> GetActiveTrolleyList();
        public Task<ServiceResponseModel<TrolleyListDTO>> SaveTrolley(TrolleyListDTO trolley);
        public Task<ServiceResponseModel<TrolleyListDTO>> DeleteTrolley(TrolleyListDTO trolley);
        public Task<ServiceResponseModel<TrolleySlotDTO>> GetTrolleySlot(long id);
        public Task<ServiceResponseModel<List<TrolleySlotDTO>>> GetTrolleySlotList();
        public Task<ServiceResponseModel<TrolleySlotDTO>> SaveTrolleySlot(TrolleySlotDTO trolleySlot);
        public Task<ServiceResponseModel<TrolleySlotDTO>> DeleteTrolleySlot(TrolleySlotDTO trolleySlot);
        public Task<ServiceResponseModel<TrolleySlotRangeDTO>> SaveRangeOfTrolleySlot(TrolleySlotRangeDTO tsRanges);
        public Task<ServiceResponseModel<List<TrolleySlotDTO>>> SaveExcelTrolleySlot(List<TrolleySlotDTO> trolleySlot);
        public Task<ServiceResponseModel<List<TrolleySlotDTO>>> UpdateExcelTSPulses(List<TrolleySlotDTO> tsPulses);

        public Task<ServiceResponseModel<TrolleyDTO>> GetTrolleyInfo(string req);
        public Task<ServiceResponseModel<SlotFreeDTO>> GetFreeTrolleySlot_BySlot_ASC(SlotFreeReqDTO req);

        public Task<ServiceResponseModel<List<TrolleyReelDtlDTO>>> GetTrolleyReelDtlList(long id);

        public Task<ServiceResponseModel<TrolleySlotDTO>> UpdateTrolleySlotStatus(SlotStatusReqDTO slotReq);
    }
}
