namespace RackingSystem.Models.JO
{
    public class JOStockCheckDTO
    {
        public long JobOrderDetail_Id { get; set; }

        public long Item_Id { get; set; }

        public string ItemCode { get; set; } = "";

        public string ItemDescription { get; set; } = "";

        public int RequiredQty { get; set; } = 0;

        public int AvailableQty { get; set; } = 0;

        public int AllocatedQty { get; set; } = 0;

        public int ShortageQty { get; set; } = 0;

        public bool IsSufficientStock { get; set; } = false;

        public List<JOReelAllocationDTO> AllocatedReels { get; set; } = new List<JOReelAllocationDTO>();

        public List<JOReelAllocationDTO> AvailableReels { get; set; } = new List<JOReelAllocationDTO>();
    }

    public class JOReelAllocationDTO
    {
        public Guid Reel_Id { get; set; }

        public string ReelCode { get; set; } = "";

        public int Qty { get; set; } = 0;

        public DateTime ExpiryDate { get; set; }

        public string SlotCode { get; set; } = "";

        public string Status { get; set; } = "";

        public bool IsExpired { get; set; }
    }
}
