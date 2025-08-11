using RackingSystem.Models.Slot;
using RackingSystem.Models;
using RackingSystem.Models.Item;

namespace RackingSystem.Services.ItemServices
{
    public interface IItemService
    {
        public Task<ServiceResponseModel<List<ItemListDTO>>> GetItemList();
        public Task<ServiceResponseModel<ItemDTO>> SaveItem(ItemDTO itemReq);
        public Task<ServiceResponseModel<ItemDTO>> DeleteItem(ItemDTO itemReq);

        public Task<ServiceResponseModel<List<ItemListDTO>>> GetActiveItemList();
    }
}
