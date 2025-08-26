using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.Maintenances
{
    public class Slot
    {
        [Key]
        public long Slot_Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string SlotCode { get; set; } = "";

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public bool IsLeft { get; set; } = false;

        [Required]
        public bool ForEmptyTray { get; set; } = false;

        [Required]
        public bool HasEmptyTray { get; set; } = false;

        [Required]
        public bool HasReel { get; set; } = false;

        [MaxLength(1)]
        public string ReelNo { get; set; } = "";

        [Required]
        public int ColNo { get; set; } = 0;

        [Required]
        public int RowNo { get; set; } = 0;

        [Required]
        public int XPulse { get; set; } = 0;

        [Required]
        public int YPulse { get; set; } = 0;

        [Required]
        public int QRXPulse { get; set; } = 0;

        [Required]
        public int QRYPulse { get; set; } = 0;

        [Required]
        public int Add1Pulse { get; set; } = 0;

        [Required]
        public int Add2Pulse { get; set; } = 0;

        [Required]
        public int Add3Pulse { get; set; } = 0;

        [Required]
        public int Add4Pulse { get; set; } = 0;

        [Required]
        public int Add5Pulse { get; set; } = 0;

        [Required]
        public int Add6Pulse { get; set; } = 0;

    }
}
