namespace RackingSystem.Models.JO
{
    public class JOEmergencyDetailReqDTO
    {
        public long JobOrderEmergencyDetail_Id { get; set; }
        public long JobOrderEmergency_Id { get; set; }
        public long Item_Id { get; set; }
        public int Qty { get; set; } = 0;
    }
}
