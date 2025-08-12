using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.Maintenances
{
    public class Item
    {
        [Key]
        public long Item_Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ItemCode { get; set; } = "";

        [Required]
        [MaxLength(50)]
        public string UOM { get; set; } = "";

        [Required]
        [MaxLength(255)]
        public string Description { get; set; } = "";

        [MaxLength(255)]
        public string Desc2 { get; set; } = "";

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public bool IsFinishGood { get; set; } = true;

        [Required]
        public int Thickness { get; set; } = 0;

        public int Width { get; set; } = 0;

        [Required]
        public int MaxThickness { get; set; } = 0;

        [Required]
        public bool AlarmOverMaxThickness { get; set; } = true;

    }
}
