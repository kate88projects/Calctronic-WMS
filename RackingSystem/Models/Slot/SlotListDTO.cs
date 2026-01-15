using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Models.Slot
{
    public class SlotListDTO
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

        public bool NeedCheck { get; set; } = false;
        public string CheckRemark { get; set; } = "";
        public int Priority { get; set; } = 0;

        public Guid Reel_Id { get; set; }
        public string ReelCode { get; set; } = "";

    }
}
