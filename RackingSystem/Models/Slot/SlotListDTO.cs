using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Models.Slot
{
    public class SlotListDTO
    {
        public long Slot_Id { get; set; }

        public string SlotCode { get; set; }

        public bool IsActive { get; set; } = true;

        public bool ForEmptyDrawer { get; set; } = false;

        public bool HasEmptyDrawer { get; set; } = false;

        public bool HasReel { get; set; } = false;
        public string ReelNo { get; set; } = "";

        public int ColNo { get; set; } = 0;

        public int RowNo { get; set; } = 0;

        public int XPulse { get; set; } = 0;
        public int YPulse { get; set; } = 0;
        public int QRXPulse { get; set; } = 0;
        public int QRYPulse { get; set; } = 0;
        public int Add1Pulse { get; set; } = 0;
        public int Add2Pulse { get; set; } = 0;
        public int Add3Pulse { get; set; } = 0;
        public int Add4Pulse { get; set; } = 0;
        public int Add5Pulse { get; set; } = 0;
        public int Add6Pulse { get; set; } = 0;

        public string? ErrorMsg { get; set; }
        public bool IsLeft { get; set; } = false;

    }
}
