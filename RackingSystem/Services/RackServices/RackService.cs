using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.Models;
using RackingSystem.Models.RackJob;

namespace RackingSystem.Services.RackServices
{
    public class RackService : IRackService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public RackService(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<ServiceResponseModel<RackJobDTO>> GetRackJob()
        {
            ServiceResponseModel<RackJobDTO> result = new ServiceResponseModel<RackJobDTO>();

            try
            {
                var rackJob = await _dbContext.RackJob.OrderBy(r => r.StartDate).FirstAsync();
                var rackJobDTO = _mapper.Map<RackJobDTO>(rackJob);

                result.success = true;
                result.data = rackJobDTO;
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
