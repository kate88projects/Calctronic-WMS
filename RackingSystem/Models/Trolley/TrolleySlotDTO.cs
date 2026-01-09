using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Models.Trolley
{
    public class TrolleySlotDTO
    {
        public long TrolleySlot_Id { get; set; } 
        public long Trolley_Id { get; set; }
        public string TrolleySlotCode { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsLeft { get; set; } = false;

        //public bool ForEmptyDrawer { get; set; } = false;

        //public bool HasEmptyDrawer { get; set; } = false;

        public bool HasReel { get; set; } = false;
        public string ReelNo { get; set; } = "";
        public int ColNo { get; set; } = 0;
        public int RowNo { get; set; } = 0;
        public int XPulse { get; set; } = 0;
        public int YPulse { get; set; } = 0;
        public int QRXPulse { get; set; } = 0;
        public int QRYPulse { get; set; } = 0;
        public int LastQRXPulse { get; set; } = 0;
        public int LastQRYPulse { get; set; } = 0;
        public DateTime LastQRReadTime { get; set; }
        public int Add1Pulse { get; set; } = 0;
        public int Add2Pulse { get; set; } = 0;
        public int Add3Pulse { get; set; } = 0;
        public int Add4Pulse { get; set; } = 0;
        public int Add5Pulse { get; set; } = 0;
        public int Add6Pulse { get; set; } = 0;

        public string? ErrorMsg { get; set; }

        public Guid Reel_Id { get; set; }

        public bool NeedCheck { get; set; } = false;
        public string CheckRemark { get; set; } = "";
        public int Priority { get; set; } = 0;
        public int TotalSlot { get; set; } = 1;

    }
}
