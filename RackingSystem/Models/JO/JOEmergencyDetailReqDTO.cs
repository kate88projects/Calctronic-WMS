namespace RackingSystem.Models.JO
{
    public class JOEmergencyDetailReqDTO
    {
        public long JobOrderEmergencyDetail_Id { get; set; }
        public long JobOrderEmergency_Id { get; set; }
        public long Item_Id { get; set; }
        public int Qty { get; set; } = 0;
        public int BufferQty { get; set; } = 0;
        public string ItemCode { get; set; } = "";
        public string ErrorMsg { get; set; } = "";
        public int ExcelRow { get; set; } = 0;
        public List<string> ErrorFields { get; set; } = new List<string>();
    }
}
