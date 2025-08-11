namespace RackingSystem.Models.GRN
{
    public class GRNSearchReqDTO
    {
        public string DateType { get; set; } = "C";
        public DateTime DateFrom { get; set; } = DateTime.Now;
        public DateTime DateTo { get; set; } = DateTime.Now;
        public string GRNBatchNo { get; set; } = "";
        public string ItemCode { get; set; } = "";
        public string ItemDesc { get; set; } = "";
        public string ItemDesc2 { get; set; } = "";
        public string Remark { get; set; } = "";
        public string ReelCode { get; set; } = "";
        public string SupplierName { get; set; } = "";
        public string SupplierRefNo { get; set; } = "";

        public int pageSize { get; set; } = 50;
        public int page { get; set; } = 1;
    }
}
