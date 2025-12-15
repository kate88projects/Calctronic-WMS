using RackingSystem.Models;
using RackingSystem.Models.Reel;

namespace RackingSystem.Services.ReelServices
{
    public interface IReelService
    {
        public Task<ServiceResponseModel<List<ReelListDTO>>> GetReelList();
        public Task<ServiceResponseModel<ReelAvailableListDTO>> GetAvailableReelTotalCount(ReelAvailableSearchReqDTO req);
        public Task<ServiceResponseModel<List<ReelAvailableListDTO>>> GetAvailableReelList(ReelAvailableSearchReqDTO req);
    }
}
