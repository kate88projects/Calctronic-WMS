namespace RackingSystem.Models.BOM
{
    public class BOMExcelReqDTO
    {
        public string FinishedGoods { get; set; }
        public string Description { get; set; } = "";
        public List<BOMExcelDtlDTO> SubItems { get; set; } = new();
        public string? ErrorMsg { get; set; }

    }

    public class BOMExcelDtlDTO
    {
        public int ExcelRowNo { get; set; }
        public string ItemCode { get; set; }
        public int Quantity { get; set; } = 0;
        public string Remark { get; set; } = "";
        public string? DtlErrorMsg { get; set; }

    }
}
