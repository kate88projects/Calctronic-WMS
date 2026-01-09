using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Models.Slot
{
    public class SlotDTO
    {
        public long Slot_Id { get; set; }

        public string SlotCode { get; set; }

        public bool IsActive { get; set; } = true;

        public bool ForEmptyTray { get; set; } = false;

        public bool HasEmptyTray { get; set; } = false;

        public bool HasReel { get; set; } = false;
        public string ReelNo { get; set; } = "";

        public int ColNo { get; set; } = 0;

        public int RowNo { get; set; } = 0;

        public int XPulse { get; set; } = 0;
        public int YPulse { get; set; } = 0;
        public int LastQRXPulse { get; set; } = 0;
        public int LastQRYPulse { get; set; } = 0;

        public DateTime LastQRReadTime { get; set; }
        public int QRXPulse { get; set; } = 0;
        public int QRYPulse { get; set; } = 0;
        public int Add1Pulse { get; set; } = 0;
        public int Add2Pulse { get; set; } = 0;
        public int Add3Pulse { get; set; } = 0;
        public int Add4Pulse { get; set; } = 0;
        public int Add5Pulse { get; set; } = 0;
        public int Add6Pulse { get; set; } = 0;
        public bool IsLeft { get; set; } = false;

        public int TotalSlot { get; set; } = 1;
        public int Priority { get; set; } = 0;

    }
}
