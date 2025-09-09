namespace RackingSystem.Models.BOM
{
    public class BOMListReqDTO
    {
        public long Item_Id { get; set; }
        public long BOM_Id { get; set; }
        public string ItemCode { get; set; } = "";
    }
}
