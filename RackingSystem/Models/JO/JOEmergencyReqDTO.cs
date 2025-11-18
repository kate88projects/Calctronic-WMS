using RackingSystem.Data.JO;

namespace RackingSystem.Models.JO
{
    public class JOEmergencyReqDTO
    {
        public long JobOrderEmergency_Id { get; set; }
        public string DocNo { get; set; } = "";
        public DateTime DocDate { get; set; } = new DateTime();
        public string Description { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = "";
        public List<JobOrderEmergencyDetail> EmergencyDetails { get; set; }
    }
}
