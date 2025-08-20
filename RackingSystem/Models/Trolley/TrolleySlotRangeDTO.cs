namespace RackingSystem.Models.Trolley
{
    public class TrolleySlotRangeDTO
    {
        public long Trolley_Id { get; set; }
        public string TrolleySlotFormat { get; set; }
        public int StartCol { get; set; } = 0;
        public int TotalCols { get; set; } = 0;
        public int StartRow { get; set; } = 0;
        public int TotalRows { get; set; } = 0;
        public int XPulse { get; set; } = 0;
        public int XPulseIncrement { get; set; } = 0;
        public int YPulse { get; set; } = 0;
        public int YPulseIncrement { get; set; } = 0;
        public int QRXPulse { get; set; } = 0;
        public int QRXPulseIncrement { get; set; } = 0;
        public int QRYPulse { get; set; } = 0;
        public int QRYPulseIncrement { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public bool IsLeft { get; set; } = true;
        //public bool HasEmptyDrawer { get; set; } = false;
        public bool HasReel { get; set; } = false;
        public int Add1Pulse { get; set; } = 0;
        public int Add1PulseIncrement { get; set; } = 0;
        public int Add2Pulse { get; set; } = 0;
        public int Add2PulseIncrement { get; set; } = 0;
        public int Add3Pulse { get; set; } = 0;
        public int Add3PulseIncrement { get; set; } = 0;
        public int Add4Pulse { get; set; } = 0;
        public int Add4PulseIncrement { get; set; } = 0;
        public int Add5Pulse { get; set; } = 0;
        public int Add5PulseIncrement { get; set; } = 0;
        public int Add6Pulse { get; set; } = 0;
        public int Add6PulseIncrement { get; set; } = 0;
    }
}