namespace RackingSystem.Models.Reel
{
    public class StockAgingDTO
    {
        public long Item_Id { get; set; }

        public string ItemCode { get; set; } = "";

        public string ItemDescription { get; set; } = "";

        public string ItemDesc2 { get; set; } = "";

        public string ItemGroupName { get; set; } = "";

        public Guid Reel_Id { get; set; }

        public string ReelCode { get; set; } = "";

        public string SlotCode { get; set; } = "";

        public DateTime ExpiryDate { get; set; } = DateTime.Now;

        public int Qty { get; set; } = 0;

        public bool IsReady { get; set; } = true;

        public string Status { get; set; } = "";

        public int DaysExpired { get; set; } = 0;
    }
}
