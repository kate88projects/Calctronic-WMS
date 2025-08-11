namespace RackingSystem.Models.GRN
{
    public class GRNDtlDTO
    {
        public Guid GRNDetail_Id { get; set; }

        public string GRNBatchNo { get; set; } = "";

        public long Item_Id { get; set; } = 0;

        public string ItemCode { get; set; } = "";

        public string ItemDesc { get; set; } = "";

        public int Qty { get; set; } = 0;

        public DateTime? ExpiryDate { get; set; } = DateTime.Now;

        public string Reel_Id { get; set; } = "";

        public string ReelCode { get; set; } = "";

        public string Remark { get; set; } = "";

        public string CreatedDateDisplay { get; set; } = "";

        public string UpdatedDateDisplay { get; set; } = "";
    }
}
