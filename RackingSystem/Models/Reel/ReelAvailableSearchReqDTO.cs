namespace RackingSystem.Models.Reel
{
    public class ReelAvailableSearchReqDTO
    {
        public string DateType { get; set; } = "C";
        public DateTime DateFrom { get; set; } = DateTime.Now;
        public DateTime DateTo { get; set; } = DateTime.Now;
        public string ReelCode { get; set; } = "";
        public string ItemCode { get; set; } = "";
        public string ItemDesc { get; set; } = "";
        public string ItemDesc2 { get; set; } = "";
        public string Remark { get; set; } = "";
        public string StatusIdxList { get; set; } = "";

        public int pageSize { get; set; } = 50;
        public int page { get; set; } = 1;
    }
}
