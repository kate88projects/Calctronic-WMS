namespace RackingSystem.Models.RackJob
{
    public class RackJobSlotInfo
    {
        public string SlotCode { get; set; } = "";
        public int QRXPulse { get; set; } = 0;
        public int QRYPulse { get; set; } = 0;
        public int QRXPulseDiffer { get; set; } = 0;
        public int QRYPulseDiffer { get; set; } = 0;
        public string data { get; set; } = "";
    }
}
