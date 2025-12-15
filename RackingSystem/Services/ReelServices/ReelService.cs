using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.Models.Slot;
using RackingSystem.Models;
using RackingSystem.Models.Reel;
using Microsoft.Data.SqlClient;
using RackingSystem.Models.GRN;

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

    }
}
