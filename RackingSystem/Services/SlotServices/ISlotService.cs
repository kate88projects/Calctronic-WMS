﻿
using RackingSystem.Models;
using RackingSystem.Models.Slot;

namespace RackingSystem.Services.SlotServices
{
    public interface ISlotService
    {
        public Task<ServiceResponseModel<List<SlotListDTO>>> GetSlotList();
        public Task<ServiceResponseModel<SlotDTO>> SaveSlot(SlotDTO slotReq);
        public Task<ServiceResponseModel<SlotDTO>> DeleteSlot(SlotDTO slotReq);
        public Task<ServiceResponseModel<List<SlotListDTO>>> SaveExcelSlot(List<SlotListDTO> slots);
        public Task<ServiceResponseModel<SlotRangeDTO>> SaveRangeOfSlot(SlotRangeDTO slotRanges);

        public Task<ServiceResponseModel<List<SlotListDTO>>> GetSlotStatus_ByColumn(int req);
        public Task<ServiceResponseModel<SlotFreeDTO>> GetFreeSlot_ByColumn_ASC(SlotFreeReqDTO slotReq);
        public Task<ServiceResponseModel<SlotFreeDTO>> GetFreeSlot_ByColumn_DESC(SlotFreeReqDTO req);
        public Task<ServiceResponseModel<SlotDTO>> UpdateSlotStatus(SlotStatusReqDTO slotReq);
        public Task<ServiceResponseModel<List<SlotListDTO>>> UpdateExcelPulses (List<SlotListDTO> slotPulses);
    }
}
