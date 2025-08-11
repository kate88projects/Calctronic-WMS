namespace RackingSystem.Models.Reel
{
    public class ReelListDTO
    {
        public Guid Reel_Id { get; set; }

        public string ReelCode { get; set; } = "";

        public long Item_Id { get; set; } = 0;

        public int Qty { get; set; } = 0;

        public DateTime ExpiryDate { get; set; } = DateTime.Now;

        public bool IsReady { get; set; } = true;

        public string Status { get; set; } = "";

        public string OnHoldRemark { get; set; } = "";

        public int ActualHeight { get; set; } = 0;

        public string ItemCode { get; set; } = "";

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
