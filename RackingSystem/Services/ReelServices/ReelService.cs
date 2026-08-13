using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.Models;
using RackingSystem.Models.Reel;

namespace RackingSystem.Services.ReelServices
{
    public class ReelService : IReelService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public ReelService(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<ServiceResponseModel<List<ReelListDTO>>> GetReelList()
        {
            ServiceResponseModel<List<ReelListDTO>> result = new ServiceResponseModel<List<ReelListDTO>>();

            try
            {
                var reelList = await _dbContext.Reel.OrderBy(x => x.ReelCode).ToListAsync();
                var reelListDTO = _mapper.Map<List<ReelListDTO>>(reelList).ToList();
                result.success = true;
                result.data = reelListDTO;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<ReelAvailableListDTO>> GetAvailableReelTotalCount(ReelAvailableSearchReqDTO req)
        {
            ServiceResponseModel<ReelAvailableListDTO> result = new ServiceResponseModel<ReelAvailableListDTO>();

            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@GetTotal", "1"),
                    new SqlParameter("@DateType", req.DateType),
                    new SqlParameter("@DateFrom", req.DateFrom.ToString("yyyy-MM-dd")),
                    new SqlParameter("@DateTo", req.DateTo.ToString("yyyy-MM-dd")),
                    new SqlParameter("@ItemCode", req.ItemCode),
                    new SqlParameter("@ReelCode", req.ReelCode),
                    new SqlParameter("@StatusIdxList", req.StatusIdxList),
                    new SqlParameter("@pageSize", req.pageSize),
                    new SqlParameter("@page", req.page)
                };

                string sql = "EXECUTE dbo.Reel_GET_AVAILABLE_SEARCHLIST @GetTotal,@DateType,@DateFrom,@DateTo,@ItemCode,@ReelCode,@StatusIdxList,@pageSize,@page";
                var listDTO = await _dbContext.SP_ReelGetAvailSearchList.FromSqlRaw(sql, parameters).ToListAsync();

                ReelAvailableListDTO data = new ReelAvailableListDTO();
                if (listDTO != null)
                {
                    data.totalRecord = listDTO.First().totalRecord;
                    data.TotalWaiting = listDTO.First().TotalWaiting;
                    data.TotalInLoader = listDTO.First().TotalInLoader;
                    data.TotalSRMS = listDTO.First().TotalSRMS;
                    data.TotalInTrolley = listDTO.First().TotalInTrolley;
                }


                result.success = true;
                result.data = data;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<List<ReelAvailableListDTO>>> GetAvailableReelList(ReelAvailableSearchReqDTO req)
        {
            ServiceResponseModel<List<ReelAvailableListDTO>> result = new ServiceResponseModel<List<ReelAvailableListDTO>>();

            try
            {
                //List<int> aStatus = new List<int>();
                //aStatus.Add(1);
                //aStatus.Add(2);
                //aStatus.Add(3);
                //var reelList = await _dbContext.Reel.Where(x => x.StatusIdx != 5).OrderBy(x => x.ReelCode).ToListAsync();
                //var reelListDTO = _mapper.Map<List<ReelListDTO>>(reelList).ToList();
                //result.success = true;
                //result.data = reelListDTO;
                //return result;

                var parameters = new[]
                {
                    new SqlParameter("@GetTotal", "0"),
                    new SqlParameter("@DateType", req.DateType),
                    new SqlParameter("@DateFrom", req.DateFrom.ToString("yyyy-MM-dd")),
                    new SqlParameter("@DateTo", req.DateTo.ToString("yyyy-MM-dd")),
                    new SqlParameter("@ItemCode", req.ItemCode),
                    new SqlParameter("@ReelCode", req.ReelCode),
                    new SqlParameter("@StatusIdxList", req.StatusIdxList),
                    new SqlParameter("@pageSize", req.pageSize),
                    new SqlParameter("@page", req.page)
                };

                string sql = "EXECUTE dbo.Reel_GET_AVAILABLE_SEARCHLIST @GetTotal,@DateType,@DateFrom,@DateTo,@ItemCode,@ReelCode,@StatusIdxList,@pageSize,@page";
                var listDTO = await _dbContext.SP_ReelGetAvailSearchList.FromSqlRaw(sql, parameters).ToListAsync();

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

        public async Task<ServiceResponseModel<StockAgingResDTO>> GetExpiredStockAging(StockAgingReqDTO req)
        {
            ServiceResponseModel<StockAgingResDTO> result = new ServiceResponseModel<StockAgingResDTO>();
            result.data = new StockAgingResDTO();
            result.data.Items = new List<StockAgingDTO>();

            try
            {
                var query = from reel in _dbContext.Reel
                            join item in _dbContext.Item on reel.Item_Id equals item.Item_Id
                            join itemGroup in _dbContext.ItemGroup on item.ItemGroup_Id equals itemGroup.ItemGroup_Id
                            join slot in _dbContext.Slot on reel.Slot_Id equals slot.Slot_Id into slotJoin
                            from slot in slotJoin.DefaultIfEmpty()
                            select new { reel, item, itemGroup, slot };

                // Apply filters
                if (!string.IsNullOrWhiteSpace(req.ItemCode))
                {
                    query = query.Where(x => x.item.ItemCode.Contains(req.ItemCode));
                }

                if (!string.IsNullOrWhiteSpace(req.ItemGroup))
                {
                    query = query.Where(x => x.itemGroup.ItemGroupCode.Contains(req.ItemGroup));
                }

                if (req.ToExpiryDate.HasValue)
                {
                    query = query.Where(x => x.reel.ExpiryDate <= req.ToExpiryDate.Value.AddDays(1));
                }

                if (req.FilterExpired != null)
                {
                    if (req.FilterExpired == true)
                    {
                        query = query.Where(x => x.reel.ExpiryDate < DateTime.Now.Date);
                    }
                    else
                    {
                        query = query.Where(x => x.reel.ExpiryDate >= DateTime.Now.Date);
                    }
                }

                if (req.FilterInSRMS != null)
                {
                    if (req.FilterInSRMS == true)
                    {
                        query = query.Where(x => x.slot != null);
                    }
                    else
                    {
                        query = query.Where(x => x.slot == null);
                    }
                }

                // Sort by expiry date (oldest first)
                query = query.OrderBy(x => x.reel.ExpiryDate);

                // Get total count before pagination
                int totalCount = await query.CountAsync();
                int totalExpired = await query.Where(x => x.reel.ExpiryDate < DateTime.Now.Date).CountAsync();
                int totalNonExpired = await query.Where(x => x.reel.ExpiryDate >= DateTime.Now.Date).CountAsync();
                int totalExpiredInSRMS = await query.Where(x => x.reel.ExpiryDate < DateTime.Now.Date && x.slot != null).CountAsync(); 
                int totalNonExpiredInSRMS = await query.Where(x => x.reel.ExpiryDate >= DateTime.Now.Date && x.slot != null).CountAsync();

                // Apply pagination
                var dataList = await query
                    .Skip((req.page - 1) * req.pageSize)
                    .Take(req.pageSize)
                    .ToListAsync();

                var itemList = new List<StockAgingDTO>();
                foreach (var item in dataList)
                {
                    int daysExpired = (int)(DateTime.Now.Date - item.reel.ExpiryDate.Date).TotalDays;

                    itemList.Add(new StockAgingDTO
                    {
                        Item_Id = item.item.Item_Id,
                        ItemCode = item.item.ItemCode,
                        ItemDescription = item.item.Description,
                        ItemDesc2 = item.item.Desc2,
                        ItemGroupName = item.itemGroup.ItemGroupCode,
                        Reel_Id = item.reel.Reel_Id,
                        ReelCode = item.reel.ReelCode,
                        SlotCode = item.slot?.SlotCode ?? "",
                        ExpiryDate = item.reel.ExpiryDate,
                        Qty = item.reel.Qty,
                        IsReady = item.reel.IsReady,
                        Status = item.reel.Status,
                        DaysExpired = daysExpired
                    });
                }

                int totalPages = (int)Math.Ceiling((double)totalCount / req.pageSize);

                result.data = new StockAgingResDTO
                {
                    Items = itemList,
                    TotalRecords = totalCount,
                    TotalExpired = totalExpired,
                    TotalNonExpired = totalNonExpired,
                    TotalExpiredInSRMS = totalExpiredInSRMS,
                    TotalNonExpiredInSRMS = totalNonExpiredInSRMS,
                    Page = req.page,
                    PageSize = req.pageSize,
                    TotalPages = totalPages
                };
                result.totalRecords = totalCount;
                result.success = true;
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
