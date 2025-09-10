namespace RackingSystem.Models.JO
{
    public class JODetailReqDTO
    {
        public long JobOrderDetail_Id { get; set; }
        public long JobOrder_Id { get; set; }
        public long BOM_Id { get; set; }
        public int Qty { get; set; } = 0;
    }
}
