using RackingSystem.Models.RackJobQueue;
using RackingSystem.Models;
using RackingSystem.Data.JO;

namespace RackingSystem.Services.RackJobQueueServices
{
    public interface IRackJobQueueService
    {
        public Task<ServiceResponseModel<List<QListDTO>>> GetQueueList();
        public Task<ServiceResponseModel<QReqDTO>> AddQueue(QReqDTO req);
        public Task<ServiceResponseModel<QReqDTO>> DeleteQueue(QReqDTO req);
        public Task<ServiceResponseModel<QReqDTO>> ChangeQueue(QReqDTO req);

        public Task<ServiceResponseModel<List<JobOrder>>> GetJOList_PendingToQueue();

    }
}
