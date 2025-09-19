using RackingSystem.Models;
using RackingSystem.Models.JO;

namespace RackingSystem.Services.JOServices
{
    public interface IJOService
    {
        public Task<ServiceResponseModel<List<JOListDTO>>> GetJOList(); //JOSearchReqDTO req)
        public Task<ServiceResponseModel<List<JODetailReqDTO>>> GetJODetail(long jobId);
        public Task<ServiceResponseModel<JOReqDTO>> SaveJob(JOReqDTO job);
        public Task<ServiceResponseModel<JOListDTO>> DeleteJob(JOListDTO job);
    }
}
