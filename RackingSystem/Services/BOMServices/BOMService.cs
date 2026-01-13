using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.Data.Maintenances;
using RackingSystem.Models;
using RackingSystem.Models.BOM;

namespace RackingSystem.Services.BOMServices
{
    public class BOMService : IBOMService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public BOMService(AppDbContext context, IMapper mapper)
        {
            _dbContext = context;
            _mapper = mapper;
        }

        public async Task<ServiceResponseModel<BOMListDTO>> GetBOM(long BomId)
        {
            ServiceResponseModel<BOMListDTO> result = new ServiceResponseModel<BOMListDTO>();

            try
            {
                var bomList = await _dbContext.BOM.Where(x => x.BOM_Id == BomId).FirstOrDefaultAsync();
                var bomListDTO = _mapper.Map<BOMListDTO>(bomList);
                result.success = true;
                result.data = bomListDTO;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            return result;
        }
        public async Task<ServiceResponseModel<List<BOMListDTO>>> GetBOMList(BOMSearchReqDTO req)
        {
            ServiceResponseModel<List<BOMListDTO>> result = new ServiceResponseModel<List<BOMListDTO>>();

            try
            {
                //var bomList = await _dbContext.BOM.OrderBy(x => x.BOM_Id).ToListAsync();
                //var bomListDTO = _mapper.Map<List<BOMListDTO>>(bomList);
                var parameters = new[]
               {
                    new SqlParameter("@GetTotal", "0"),
                    new SqlParameter("@ItemCode", req.ItemCode),
                    new SqlParameter("@CreatedBy", req.CreatedBy),
                    new SqlParameter("@pageSize", req.pageSize),
                    new SqlParameter("@page", req.page),
                };

                string sql = "EXECUTE dbo.BOM_GET_SEARCHLIST @GetTotal, @ItemCode, @CreatedBy, @pageSize, @page";
                var listDTO = await _dbContext.SP_BOMSearchList.FromSqlRaw(sql, parameters).ToListAsync();

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

        public async Task<ServiceResponseModel<List<BOMDtlDTO>>> GetBOMDetail(long bomId)
        {
            ServiceResponseModel<List<BOMDtlDTO>> result = new ServiceResponseModel<List<BOMDtlDTO>>();

            try
            {
                var bomDtl = _dbContext.BOMDetail.Where(d => d.BOM_Id == bomId).ToList();
                var bomDtlDTO = _mapper.Map<List<BOMDtlDTO>>(bomDtl);
                result.success = true;
                result.data = bomDtlDTO;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            return result;
        }
        public async Task<ServiceResponseModel<BOMDtlReqDTO>> SaveBOM(BOMDtlReqDTO bom)
        {
            ServiceResponseModel<BOMDtlReqDTO> result = new ServiceResponseModel<BOMDtlReqDTO>();

            try
            {
                if (bom.Item_Id == 0)
                {
                    result.errMessage = "Please select an BOM Item.";
                    return result;
                }
                foreach (var subitem in bom.SubItems)
                {
                    if (subitem.Item_Id == 0)
                    {
                        result.errMessage = "Please select an BOM Item.";
                        return result;
                    }
                    else if (subitem.Qty == 0)
                    {
                        result.errMessage = "Please insert item quantity";
                        return result;
                    }
                }

                var aggregatedSubItems = bom.SubItems
                    .GroupBy(x => x.Item_Id)
                    .Select(g => new
                    {
                        Item_Id = g.Key,
                        Qty = g.Sum(x => x.Qty),
                        Remark = string.Join(",", g.Select(x => x.Remark).Where(r => !string.IsNullOrEmpty(r))),
                        BOMDetail_Id = g.First().BOMDetail_Id,
                        BOM_Id = g.First().BOM_Id
                    })
                    .ToList();

                if (bom.BOM_Id == 0)
                {
                    BOM _bom = new BOM()
                    {
                        Item_Id = bom.Item_Id,
                        Description = bom.Description,
                        IsActive = bom.IsActive,
                        CreatedBy = bom.CreatedBy,
                        CreatedDate = DateTime.Now,
                    };
                    _dbContext.BOM.Add(_bom);
                    await _dbContext.SaveChangesAsync();

                    //Console.WriteLine($"Generated BOM Id: {_bom.BOM_Id}");

                    foreach (var subitem in aggregatedSubItems)
                    {
                        BOMDetail _bomDtl = new BOMDetail()
                        {
                            BOM_Id = _bom.BOM_Id,
                            Item_Id = subitem.Item_Id,
                            Qty = subitem.Qty,
                            Remark = subitem.Remark,
                            CreatedBy = bom.CreatedBy,
                            CreatedDate = DateTime.Now,
                        };
                        _dbContext.BOMDetail.Add(_bomDtl);
                    }
                    await _dbContext.SaveChangesAsync();
                    result.success = true;
                }
                else
                {
                    BOM? _bom = _dbContext.BOM.Find(bom.BOM_Id);
                    if (_bom == null)
                    {
                        result.errMessage = "Cannot find this specified BOM. Please refersh the list and try again.";
                        return result;
                    }

                    //get all detail where bom id is same
                    var existingDetails = _dbContext.BOMDetail.Where(d => d.BOM_Id == _bom.BOM_Id).ToList();
                    if (existingDetails.Count == 0)
                    {
                        result.errMessage = "Cannot find this specified BOM Detail. Please refersh the list and try again.";
                        return result;
                    }

                    _bom.Item_Id = bom.Item_Id;
                    _bom.Description = bom.Description;
                    _bom.IsActive = bom.IsActive;
                    _bom.CreatedBy = bom.CreatedBy;
                    _bom.UpdatedBy = bom.CreatedBy;
                    _bom.UpdatedDate = DateTime.Now;
                    _dbContext.BOM.Update(_bom);

                    //remove the previous if not found on the latest detail
                    var itemsToRemove = new List<BOMDetail>();
                    foreach (var exist in existingDetails)
                    {
                        var updated = bom.SubItems.FirstOrDefault(s => s.BOMDetail_Id == exist.BOMDetail_Id);
                        if (updated == null)
                        {
                            itemsToRemove.Add(exist);
                        }
                    }

                    //update the latest detail
                    foreach (var subitem in aggregatedSubItems)
                    {
                        if (subitem.BOMDetail_Id == 0)
                        {
                            BOMDetail _bomDtl = new BOMDetail()
                            {
                                BOM_Id = _bom.BOM_Id,
                                Item_Id = subitem.Item_Id,
                                Qty = subitem.Qty,
                                Remark = subitem.Remark,
                                CreatedBy = bom.CreatedBy,
                                CreatedDate = DateTime.Now,
                            };
                            _dbContext.BOMDetail.Add(_bomDtl);
                        }
                        else
                        {
                            BOMDetail? _bomDtl = existingDetails.FirstOrDefault(d => d.BOM_Id == subitem.BOM_Id && d.BOMDetail_Id == subitem.BOMDetail_Id);
                            if (_bomDtl == null)
                            {
                                result.errMessage = "Cannot find the specified BOM Detail for update.";
                                return result;
                            }

                            _bomDtl.Item_Id = subitem.Item_Id;
                            _bomDtl.Qty = subitem.Qty;
                            _bomDtl.Remark = subitem.Remark;
                            _bomDtl.UpdatedBy = bom.CreatedBy;
                            _bomDtl.UpdatedDate = DateTime.Now;
                            _dbContext.BOMDetail.Update(_bomDtl);
                        }
                    }

                    if (itemsToRemove.Any())
                    {
                        _dbContext.BOMDetail.RemoveRange(itemsToRemove);
                    }

                    await _dbContext.SaveChangesAsync();
                    result.success = true;
                }
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            return result;
        }

        public async Task<ServiceResponseModel<BOMListDTO>> DeleteBOM(BOMListDTO bom)
        {
            ServiceResponseModel<BOMListDTO> result = new ServiceResponseModel<BOMListDTO>();

            try
            {
                if (bom == null)
                {
                    result.errMessage = "Something wrong. Please refresh the list and try again.";
                    return result;
                }

                BOM? _bom = _dbContext.BOM.Find(bom.BOM_Id);
                if (_bom == null)
                {
                    result.errMessage = "Cannot find this BOM, please refresh the list.";
                    return result;
                }
                _dbContext.BOM.Remove(_bom);

                var existingDetails = _dbContext.BOMDetail.Where(d => d.BOM_Id == _bom.BOM_Id).ToList();
                foreach (var dtl in existingDetails)
                {
                    _dbContext.BOMDetail.Remove(dtl);
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

        public async Task<ServiceResponseModel<int>> GetBOMTotalCount(BOMSearchReqDTO req)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@GetTotal", "1"),
                    new SqlParameter("@ItemCode", req.ItemCode),
                    new SqlParameter("@CreatedBy", req.CreatedBy),
                    new SqlParameter("@pageSize", req.pageSize),
                    new SqlParameter("@page", req.page),
                };
                string sql = "EXECUTE dbo.BOM_GET_SEARCHLIST @GetTotal, @ItemCode, @CreatedBy, @pageSize, @page";
                var listDTO = await _dbContext.SP_BOMSearchList.FromSqlRaw(sql, parameters).ToListAsync();

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

        public async Task<ServiceResponseModel<List<BOMListReqDTO>>> GetActiveBOMList()
        {
            var result = new ServiceResponseModel<List<BOMListReqDTO>>();

            try
            {
                //var bomList = await _dbContext.BOM.Where(x => x.IsActive == true).OrderBy(x => x.BOM_Id).ToListAsync();
                //var bomListDTO = _mapper.Map<List<BOMListDTO>>(bomList);

                var joinResult = await (
                    from i in _dbContext.Item
                    join b in _dbContext.BOM on i.Item_Id equals b.Item_Id
                    where i.IsActive && i.IsFinishGood && b.IsActive
                    select new BOMListReqDTO
                    {
                        Item_Id = i.Item_Id,
                        ItemCode = i.ItemCode,
                        BOM_Id = b.BOM_Id
                    }).ToListAsync();

                var dtoList = _mapper.Map<List<BOMListReqDTO>>(joinResult);

                result.success = true;
                result.data = dtoList;
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
