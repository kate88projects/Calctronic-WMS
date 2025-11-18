using RackingSystem.Models;
using RackingSystem.Models.JO;

namespace RackingSystem.Services.JOServices
{
    public interface IJOService
    {
        public Task<ServiceResponseModel<JOListDTO>> GetJO(long id);
        public Task<ServiceResponseModel<List<JOListDTO>>> GetJOList(); //JOSearchReqDTO req)
        public Task<ServiceResponseModel<List<JODetailReqDTO>>> GetJODetail(long jobId);
        public Task<ServiceResponseModel<JOReqDTO>> SaveJob(JOReqDTO job);
        public Task<ServiceResponseModel<JOListDTO>> DeleteJob(JOListDTO job);

        public Task<ServiceResponseModel<JOEmergencyReqDTO>> GetJOEmergency(long id);
        public Task<ServiceResponseModel<List<JOEmergencyReqDTO>>> GetJOEmergencyList();
        public Task<ServiceResponseModel<List<JOEmergencyDetailReqDTO>>> GetJOEmergencyDetail(long jobEId);
        public Task<ServiceResponseModel<JOEmergencyReqDTO>> SaveEmergency(JOEmergencyReqDTO jobEmergency);
        public Task<ServiceResponseModel<JOEmergencyReqDTO>> DeleteEmergency(JOEmergencyReqDTO jobEmergency);


    }
}
