namespace RackingSystem.Models.BOM
{
    public class BOMSearchReqDTO
    {
        public string ItemCode { get; set; } = "";
        public string CreatedBy { get; set; } = "";
        public int pageSize { get; set; } = 50;
        public int page { get; set; } = 1;
    }
}
