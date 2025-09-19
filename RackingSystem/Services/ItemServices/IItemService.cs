using RackingSystem.Models.Slot;
using RackingSystem.Models;
using RackingSystem.Models.Item;

namespace RackingSystem.Services.ItemServices
{
    public interface IItemService
    {
        public Task<ServiceResponseModel<List<ItemGroupListDTO>>> GetItemGroupList();
        public Task<ServiceResponseModel<ItemGroupDTO>> SaveItemGroup(ItemGroupDTO itemReq);
        public Task<ServiceResponseModel<ItemGroupDTO>> DeleteItemGroup(ItemGroupDTO itemReq);

        public Task<ServiceResponseModel<List<ItemGroupListDTO>>> GetActiveItemGroupList();

        public Task<ServiceResponseModel<int>> GetItemTotalCount(ItemSearchReqDTO req);
        public Task<ServiceResponseModel<List<ItemListDTO>>> GetItemList(ItemSearchReqDTO req);
        public Task<ServiceResponseModel<ItemDTO>> SaveItem(ItemDTO itemReq);
        public Task<ServiceResponseModel<ItemDTO>> DeleteItem(ItemDTO itemReq);
        public Task<ServiceResponseModel<List<ItemExcelReqDTO>>> SaveExcelItem(List<ItemExcelReqDTO> slots);

        public Task<ServiceResponseModel<List<ItemListDTO>>> GetActiveItemList();
        public Task<ServiceResponseModel<List<ItemListDTO>>> GetFinishedItemList();
        public Task<ServiceResponseModel<List<ItemListDTO>>> GetRawItemList();
    }
}
