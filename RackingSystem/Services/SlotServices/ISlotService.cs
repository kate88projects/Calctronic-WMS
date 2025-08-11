
using RackingSystem.Models;
using RackingSystem.Models.Slot;

namespace RackingSystem.Services.SlotServices
{
    public interface ISlotService
    {
        public Task<ServiceResponseModel<List<SlotListDTO>>> GetSlotList();
        public Task<ServiceResponseModel<SlotDTO>> SaveSlot(SlotDTO slotReq);
        public Task<ServiceResponseModel<SlotDTO>> DeleteSlot(SlotDTO slotReq);
        public Task<ServiceResponseModel<List<SlotListDTO>>> GetSlotStatus_ByColumn(int req);
    }
}
