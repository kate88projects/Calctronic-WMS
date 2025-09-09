namespace RackingSystem.Models.JO
{
    public class JOSearchReqDTO
    {
        public string DateType { get; set; } = "C";
        public DateTime DateFrom { get; set; } = DateTime.Now;
        public DateTime DateTo { get; set; } = DateTime.Now;
        public string DocNo { get; set; } = "";
        public string Desc { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string CustomerRefNo { get; set; } = "";
        public string ProductCode { get; set; } = "";
        public string RawCode { get; set; } = "";

        public int pageSize { get; set; } = 50;
        public int page { get; set; } = 1;
    }
}
