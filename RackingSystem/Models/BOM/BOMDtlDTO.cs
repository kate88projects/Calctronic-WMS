namespace RackingSystem.Models.BOM
{
    public class BOMDtlDTO
    {
        public long BOMDetail_Id { get; set; } 
        public long BOM_Id { get; set; } = 0;
        public long Item_Id { get; set; }
        public int Qty { get; set; } = 0;
        public string Remark { get; set; } = "";
        public DateTime CreatedDate { get; set; } 
        public string CreatedBy { get; set; } = "";
        public DateTime UpdatedDate { get; set; } 
        public string UpdatedBy { get; set; } = "";
    }
}
