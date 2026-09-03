namespace RackingSystem.Models.Item
{
    public class SlotSearchReqDTO
    {
        public string ItemCode { get; set; } = "";

        public DateTime ExpiryDate { get; set; } = DateTime.Today;
    }

    public class SlotItemDTO
    {
        public long Item_Id { get; set; }

        public string ItemCode { get; set; } = "";

        public Guid Reel_Id { get; set; }

        public string ReelCode { get; set; } = "";

        public DateTime ExpiryDate { get; set; } = DateTime.Now;

        public string SlotCode { get; set; } = "";

        public int Qty { get; set; } = 0;

        public bool IsReady { get; set; } = true;

        public string Status { get; set; } = "";
    }
}
