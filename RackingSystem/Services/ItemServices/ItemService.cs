using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.Models;
using RackingSystem.Models.Item;
using RackingSystem.Data.Maintenances;

namespace RackingSystem.Services.ItemServices
{
    public class ItemService : IItemService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public ItemService(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<ServiceResponseModel<List<ItemListDTO>>> GetItemList()
        {
            ServiceResponseModel<List<ItemListDTO>> result = new ServiceResponseModel<List<ItemListDTO>>();

            try
            {
                var itemList = await _dbContext.Item.OrderBy(x => x.ItemCode).ToListAsync();
                var itemListDTO = _mapper.Map<List<ItemListDTO>>(itemList).ToList();
                result.success = true;
                result.data = itemListDTO;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<ItemDTO>> SaveItem(ItemDTO itemReq)
        {
            ServiceResponseModel<ItemDTO> result = new ServiceResponseModel<ItemDTO>();

            try
            {
                // 1. checking Data
                if (itemReq == null)
                {
                    result.errMessage = "Please insert Item Code.";
                    return result;
                }
                if (string.IsNullOrEmpty(itemReq.ItemCode))
                {
                    result.errMessage = "Please insert Item Code.";
                    return result;
                }
                if (itemReq.UOM == null)
                {
                    result.errMessage = "Please insert UOM.";
                    return result;
                }
                if (itemReq.Description == null)
                {
                    result.errMessage = "Please insert Description.";
                    return result;
                }
                if (itemReq.Height == null)
                {
                    result.errMessage = "Please insert Height.";
                    return result;
                }
                if (itemReq.Width == null)
                {
                    result.errMessage = "Please insert Width.";
                    return result;
                }
                if (itemReq.MaxHeight == null)
                {
                    result.errMessage = "Please insert Max Height.";
                    return result;
                }
                if (itemReq.Item_Id == 0)
                {
                    Item? iExist = _dbContext.Item.FirstOrDefault(x => x.ItemCode == itemReq.ItemCode);
                    if (iExist != null)
                    {
                        result.errMessage = "This item code has exist.";
                        return result;
                    }
                }
                else
                {
                    Item? iExist = _dbContext.Item.FirstOrDefault(x => x.ItemCode == itemReq.ItemCode && x.Item_Id != itemReq.Item_Id);
                    if (iExist != null)
                    {
                        result.errMessage = "This item code has been used.";
                        return result;
                    }
                }

                // 2. save Data
                if (itemReq.Item_Id == 0)
                {
                    Item _item = new Item()
                    {
                        ItemCode = itemReq.ItemCode,
                        UOM = itemReq.UOM,
                        Description = itemReq.Description,
                        Desc2 = itemReq.Desc2 ?? "",
                        IsActive = itemReq.IsActive,
                        IsFinishGood = itemReq.IsFinishGood,
                        Height = itemReq.Height ?? 0,
                        Width = itemReq.Width ?? 0,
                        MaxHeight = itemReq.MaxHeight ?? 0,
                        AlarmOverMaxHeight = itemReq.AlarmOverMaxHeight,
                    };
                    _dbContext.Item.Add(_item);
                }
                else
                {
                    Item? _item = _dbContext.Item.Find(itemReq.Item_Id);
                    if (_item == null)
                    {
                        result.errMessage = "Cannot find this item, please refresh the list.";
                        return result;
                    }
                    _item.ItemCode = itemReq.ItemCode;
                    _item.UOM = itemReq.UOM;
                    _item.Description = itemReq.Description;
                    _item.Desc2 = itemReq.Desc2 ?? "";
                    _item.IsActive = itemReq.IsActive;
                    _item.IsFinishGood = itemReq.IsFinishGood;
                    _item.Height = itemReq.Height ?? 0;
                    _item.Width = itemReq.Width ?? 0;
                    _item.MaxHeight = itemReq.MaxHeight ?? 0;
                    _item.AlarmOverMaxHeight = itemReq.AlarmOverMaxHeight;
                    _dbContext.Item.Update(_item);
                }
                await _dbContext.SaveChangesAsync();

                result.success = true;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<ItemDTO>> DeleteItem(ItemDTO itemReq)
        {
            ServiceResponseModel<ItemDTO> result = new ServiceResponseModel<ItemDTO>();

            try
            {
                // 1. checking Data
                if (itemReq == null)
                {
                    result.errMessage = "Please refresh the list.";
                    return result;
                }
                //Bin? binExist2 = _dbContext.Bin.FirstOrDefault(x => x.ColNo == binReq.ColNo && x.RowNo != binReq.RowNo && x.Bin_Id != binReq.Bin_Id);
                //if (binExist2 != null)
                //{
                //    result.errMessage = "This Column No and Row No has been used.";
                //    return result;
                //}

                // 2. save Data
                Item? _item = _dbContext.Item.Find(itemReq.Item_Id);
                if (_item == null)
                {
                    result.errMessage = "Cannot find this item, please refresh the list.";
                    return result;
                }
                _dbContext.Item.Remove(_item);
                await _dbContext.SaveChangesAsync();

                result.success = true;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<List<ItemListDTO>>> GetActiveItemList()
        {
            ServiceResponseModel<List<ItemListDTO>> result = new ServiceResponseModel<List<ItemListDTO>>();

            try
            {
                var itemList = await _dbContext.Item.Where(x => x.IsActive == true).OrderBy(x => x.ItemCode).ToListAsync();
                var itemListDTO = _mapper.Map<List<ItemListDTO>>(itemList).ToList();
                result.success = true;
                result.data = itemListDTO;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

    }
}
