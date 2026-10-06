namespace RackingSystem.Models.Slot
{
    public class SlotUsageDTO
    {
        public string title { get; set; } = String.Empty;
        public bool available { get; set; } = false;
        public int slotQty { get; set; } = 0;
        public double percentage { get; set; } = 0.00;
         
    }
}
