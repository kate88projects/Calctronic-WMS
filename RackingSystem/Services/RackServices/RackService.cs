using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.Data.RackJob;
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
                var rackJob = await _dbContext.RackJob.Where(r => r.RackJobQueue_Id != 0).OrderByDescending(r => r.StartDate).FirstOrDefaultAsync();
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

        public async Task<ServiceResponseModel<RackJobLog>> GetEndRackJob(long jobQId)
        {
            ServiceResponseModel<RackJobLog> result = new ServiceResponseModel<RackJobLog>();

            try
            {
                //check if rackjob log did have record means the rackjob is done
                var rackJobLog = await _dbContext.RackJobLog.SingleOrDefaultAsync(l => l.RackJobQueue_Id == jobQId);
                if (rackJobLog == null)
                {
                    result.errMessage = "Current Rack Job Process yet to end.";
                    return result;
                }
                
                //double check on rackjob queue is this job is remove
                //var rackJobQueue = await _dbContext.RackJobQueue.SingleOrDefaultAsync(q => q.RackJobQueue_Id == jobQId);
                //if (rackJobQueue != null)
                //{
                //    result.errMessage = "Current Rack Job Process Queue still exist.";
                //    return result;
                //}

                var rackJob = await _dbContext.RackJob.Where(r => r.RackJobQueue_Id == jobQId).OrderBy(r => r.StartDate).FirstOrDefaultAsync();
                if (rackJob != null) //job is still in progress
                {
                    result.errMessage = "Current Rack Job Process yet to end.";
                    return result;
                }

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
