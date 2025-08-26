using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.Models;
using RackingSystem.Models.Item;
using RackingSystem.Data.Maintenances;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RackingSystem.Models.GRN;
using RackingSystem.Models.Slot;

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

        public async Task<ServiceResponseModel<List<ItemGroupListDTO>>> GetItemGroupList()
        {
            ServiceResponseModel<List<ItemGroupListDTO>> result = new ServiceResponseModel<List<ItemGroupListDTO>>();

            try
            {
                var itemList = await _dbContext.ItemGroup.OrderBy(x => x.ItemGroupCode).ToListAsync();
                var itemListDTO = _mapper.Map<List<ItemGroupListDTO>>(itemList).ToList();
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

        public async Task<ServiceResponseModel<ItemGroupDTO>> SaveItemGroup(ItemGroupDTO itemReq)
        {
            ServiceResponseModel<ItemGroupDTO> result = new ServiceResponseModel<ItemGroupDTO>();

            try
            {
                // 1. checking Data
                if (itemReq == null)
                {
                    result.errMessage = "Please insert Item Group Code.";
                    return result;
                }
                if (string.IsNullOrEmpty(itemReq.ItemGroupCode))
                {
                    result.errMessage = "Please insert Item Group Code.";
                    return result;
                }
                if (itemReq.Description == null)
                {
                    result.errMessage = "Please insert Description.";
                    return result;
                }
                if (itemReq.ItemGroup_Id == 0)
                {
                    ItemGroup? iExist = _dbContext.ItemGroup.FirstOrDefault(x => x.ItemGroupCode == itemReq.ItemGroupCode);
                    if (iExist != null)
                    {
                        result.errMessage = "This item group code has exist.";
                        return result;
                    }
                }
                else
                {
                    ItemGroup? iExist = _dbContext.ItemGroup.FirstOrDefault(x => x.ItemGroupCode == itemReq.ItemGroupCode && x.ItemGroup_Id != itemReq.ItemGroup_Id);
                    if (iExist != null)
                    {
                        result.errMessage = "This item group code has been used.";
                        return result;
                    }
                }

                // 2. save Data
                if (itemReq.ItemGroup_Id == 0)
                {
                    ItemGroup _item = new ItemGroup()
                    {
                        ItemGroupCode = itemReq.ItemGroupCode,
                        Description = itemReq.Description,
                        IsActive = itemReq.IsActive,
                    };
                    _dbContext.ItemGroup.Add(_item);
                }
                else
                {
                    ItemGroup? _item = _dbContext.ItemGroup.Find(itemReq.ItemGroup_Id);
                    if (_item == null)
                    {
                        result.errMessage = "Cannot find this item group, please refresh the list.";
                        return result;
                    }
                    _item.ItemGroupCode = itemReq.ItemGroupCode;
                    _item.Description = itemReq.Description;
                    _item.IsActive = itemReq.IsActive;
                    _dbContext.ItemGroup.Update(_item);
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

        public async Task<ServiceResponseModel<ItemGroupDTO>> DeleteItemGroup(ItemGroupDTO itemReq)
        {
            ServiceResponseModel<ItemGroupDTO> result = new ServiceResponseModel<ItemGroupDTO>();

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
                ItemGroup? _item = _dbContext.ItemGroup.Find(itemReq.ItemGroup_Id);
                if (_item == null)
                {
                    result.errMessage = "Cannot find this item group, please refresh the list.";
                    return result;
                }
                _dbContext.ItemGroup.Remove(_item);
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

        public async Task<ServiceResponseModel<List<ItemGroupListDTO>>> GetActiveItemGroupList()
        {
            ServiceResponseModel<List<ItemGroupListDTO>> result = new ServiceResponseModel<List<ItemGroupListDTO>>();

            try
            {
                var itemList = await _dbContext.ItemGroup.Where(x => x.IsActive == true).OrderBy(x => x.ItemGroupCode).ToListAsync();
                var itemListDTO = _mapper.Map<List<ItemGroupListDTO>>(itemList).ToList();
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

        public async Task<ServiceResponseModel<int>> GetItemTotalCount(ItemSearchReqDTO req)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();

            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@GetTotal", "1"),
                    new SqlParameter("@ItemCode", req.ItemCode),
                    new SqlParameter("@ItemDesc", req.ItemDesc),
                    new SqlParameter("@ItemGroup", req.ItemGroup),
                    new SqlParameter("@Thickness", req.Thickness),
                    new SqlParameter("@pageSize", req.pageSize),
                    new SqlParameter("@page", req.page)
                };

                string sql = "EXECUTE dbo.Item_GET_SEARCHLIST @GetTotal,@ItemCode,@ItemDesc,@ItemGroup,@Thickness,@pageSize,@page";
                var listDTO = await _dbContext.SP_ItemSearchList.FromSqlRaw(sql, parameters).ToListAsync();

                int totalCount = 0;
                if (listDTO != null)
                {
                    totalCount = listDTO.First().totalRecord;
                }

                result.success = true;
                result.data = totalCount;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<List<ItemListDTO>>> GetItemList(ItemSearchReqDTO req)
        {
            ServiceResponseModel<List<ItemListDTO>> result = new ServiceResponseModel<List<ItemListDTO>>();

            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@GetTotal", "0"),
                    new SqlParameter("@ItemCode", req.ItemCode),
                    new SqlParameter("@ItemDesc", req.ItemDesc),
                    new SqlParameter("@ItemGroup", req.ItemGroup),
                    new SqlParameter("@Thickness", req.Thickness),
                    new SqlParameter("@pageSize", req.pageSize),
                    new SqlParameter("@page", req.page)
                };

                string sql = "EXECUTE dbo.Item_GET_SEARCHLIST @GetTotal,@ItemCode,@ItemDesc,@ItemGroup,@Thickness,@pageSize,@page";
                var listDTO = await _dbContext.SP_ItemSearchList.FromSqlRaw(sql, parameters).ToListAsync();

                result.success = true;
                result.data = listDTO;
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
                if (itemReq.ReelDimension_Id == 0)
                {
                    result.errMessage = "Please select Reel Dimension.";
                    return result;
                }
                //if (itemReq.Thickness == null)
                //{
                //    result.errMessage = "Please insert Thickness.";
                //    return result;
                //}
                //if (itemReq.Width == null)
                //{
                //    result.errMessage = "Please insert Width.";
                //    return result;
                //}
                //if (itemReq.MaxThickness == null)
                //{
                //    result.errMessage = "Please insert Max Thickness.";
                //    return result;
                //}
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
                        ItemGroup_Id = itemReq.ItemGroup_Id,
                        ReelDimension_Id = itemReq.ReelDimension_Id,
                        //Thickness = itemReq.Thickness ?? 0,
                        //Width = itemReq.Width ?? 0,
                        //MaxThickness = itemReq.MaxThickness ?? 0,
                        AlarmOverMaxThickness = itemReq.AlarmOverMaxThickness,
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
                    _item.ItemGroup_Id = itemReq.ItemGroup_Id;
                    _item.ReelDimension_Id = itemReq.ReelDimension_Id;
                    //_item.Thickness = itemReq.Thickness ?? 0;
                    //_item.Width = itemReq.Width ?? 0;
                    //_item.MaxThickness = itemReq.MaxThickness ?? 0;
                    _item.AlarmOverMaxThickness = itemReq.AlarmOverMaxThickness;
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

        public async Task<ServiceResponseModel<List<ItemExcelReqDTO>>> SaveExcelItem(List<ItemExcelReqDTO> items)
        {
            ServiceResponseModel<List<ItemExcelReqDTO>> result = new ServiceResponseModel<List<ItemExcelReqDTO>>();
            List<ItemExcelReqDTO> errorsLine = new List<ItemExcelReqDTO>(); // Change List<object> to List<SlotListDTO>

            try
            {
                if (items != null)
                {
                    foreach (var itm in items)
                    {
                        bool isError = false;

                        Item? iExist = _dbContext.Item.FirstOrDefault(x => x.ItemCode == itm.ItemCode);
                        ItemGroup? igExist = null;
                        if (!string.IsNullOrEmpty(itm.ItemGroupCode))
                        {
                            igExist = _dbContext.ItemGroup.FirstOrDefault(x => x.ItemGroupCode == itm.ItemGroupCode);
                        }
                        ReelDimension? rExist = _dbContext.ReelDimension.FirstOrDefault(x => x.Thickness == itm.Thickness);

                        if (iExist != null)
                        {
                            result.errMessage = $"Item code: {itm.ItemCode} has been used";
                            itm.ErrorMsg = result.errMessage;
                            isError = true;
                        }
                        if (string.IsNullOrEmpty(itm.ItemCode))
                        {
                            result.errMessage = $"Item Code cannot be empty.";
                            itm.ErrorMsg = result.errMessage;
                            isError = true;
                        }
                        if (string.IsNullOrEmpty(itm.UOM))
                        {
                            result.errMessage = $"UOM cannot be empty.";
                            itm.ErrorMsg = result.errMessage;
                            isError = true;
                        }                        
                        if (!string.IsNullOrEmpty(itm.ItemGroupCode))
                        {
                            if (igExist == null)
                            {
                                result.errMessage = $"Item Code: {itm.ItemCode} Item Group {itm.ItemGroupCode} has not found.";
                                itm.ErrorMsg = result.errMessage;
                                isError = true;
                            }
                        }
                        if (rExist == null)
                        {
                            result.errMessage = $"Item Code: {itm.ItemCode} Thickness {itm.Thickness} has not found.";
                            itm.ErrorMsg = result.errMessage;
                            isError = true;
                        }

                        if (!isError)
                        {
                            Item _item = new Item()
                            {
                                ItemCode = itm.ItemCode,
                                UOM = itm.UOM,
                                Description = itm.Description,
                                Desc2 = itm.Desc2,
                                IsActive = itm.IsActive,
                                IsFinishGood = itm.IsFinishGood,
                                AlarmOverMaxThickness = itm.AlarmOverMaxThickness,
                                ReelDimension_Id = rExist.ReelDimension_Id,
                            };
                            if (!string.IsNullOrEmpty(itm.ItemGroupCode))
                            {
                                _item.ItemGroup_Id = igExist.ItemGroup_Id;
                            }
                            _dbContext.Item.Add(_item);
                        }
                        else
                        {
                            errorsLine.Add(itm);
                        }
                    }

                    if (errorsLine.Any())
                    {
                        result.success = false;
                        result.errMessage = "Some rows failed validation.";
                        result.data = errorsLine;
                    }
                    else
                    {
                        await _dbContext.SaveChangesAsync();
                        result.success = true;
                    }
                }
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
