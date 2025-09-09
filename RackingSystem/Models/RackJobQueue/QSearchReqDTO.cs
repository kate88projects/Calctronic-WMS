namespace RackingSystem.Models.RackJobQueue
{
    public class QSearchReqDTO
    {
        public string DateType { get; set; } = "C";
        public DateTime DateFrom { get; set; } = DateTime.Now;
        public DateTime DateTo { get; set; } = DateTime.Now;
        public string DocNo { get; set; } = "";
        public string DocType { get; set; } = "";
        public string ItemCode { get; set; } = "";
        public string ReelCode { get; set; } = "";
        public string CustomerSupplierName { get; set; } = "";
        public string CustomerSupplierRefNo { get; set; } = "";

        public int pageSize { get; set; } = 50;
        public int page { get; set; } = 1;
    }
}
