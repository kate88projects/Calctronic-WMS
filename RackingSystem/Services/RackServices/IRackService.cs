using RackingSystem.Models;
using RackingSystem.Models.RackJob;
using RackingSystem.Models.RackJobQueue;

namespace RackingSystem.Services.RackServices
{
    public interface IRackService
    {
        public Task<ServiceResponseModel<RackJobDTO>> GetRackJob();

    }
}
