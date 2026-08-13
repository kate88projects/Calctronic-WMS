namespace RackingSystem.Models.Reel
{
    public class StockAgingReqDTO
    {
        public string ItemCode { get; set; } = "";

        public string ItemGroup { get; set; } = "";

        public DateTime? ToExpiryDate { get; set; }

        public bool? FilterExpired { get; set; }

        public bool? FilterInSRMS { get; set; }

        public int pageSize { get; set; } = 50;

        public int page { get; set; } = 1;
    }
}
