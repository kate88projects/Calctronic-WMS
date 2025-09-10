namespace RackingSystem.Models.JO
{
    public class JOReqDTO
    {
        public long JobOrder_Id { get; set; }
        public string DocNo { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = "";
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public string UpdatedBy { get; set; } = "";
        public List<JODetailReqDTO> Details { get; set; }
    }
}
