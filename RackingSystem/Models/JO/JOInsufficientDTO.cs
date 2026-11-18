namespace RackingSystem.Models.JO
{
    public class JOInsufficientDTO
    {
        public long Item_Id { get; set; }

        public string ItemCode { get; set; } = "";

        public string Description { get; set; } = "";

        public int Qty { get; set; } = 0;

        public int BufferQty { get; set; } = 0;

        public int BalQty { get; set; } = 0;

        public bool RetrieveFailed { get; set; } = false;
    }
}
