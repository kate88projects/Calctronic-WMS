namespace RackingSystem.Models.Slot
{
    public class SlotPulsesExcelDTO
    {
        public string SlotCode { get; set; }
        public int ColNo { get; set; }
        public int RowNo { get; set; }
        public int QRXPulse { get; set; }
        public int QRZPulse { get; set; }
        public string LastReadingTime { get; set; }
        public int PreviousQRXPulse { get; set; }
        public int PreviousQRZPulse { get; set; }
    }
}
