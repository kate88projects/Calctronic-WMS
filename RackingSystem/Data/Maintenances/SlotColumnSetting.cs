using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.Maintenances
{
    public class SlotColumnSetting
    {
        [Key]
        public long SlotColumnSetting_Id { get; set; }

        [Required]
        public int ColNo { get; set; } = 0;

        [Required]
        public int EmptyDrawer_IN_Idx { get; set; } = 0;

        [Required]
        public int Reel_IN_Idx { get; set; } = 0;

        [Required]
        public int EmptyDrawer_OUT_Idx { get; set; } = 0;

        [Required]
        public int Reel_OUT_Idx { get; set; } = 0;

    }
}
