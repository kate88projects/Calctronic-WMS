using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.Models.Slot;
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

    }
}
