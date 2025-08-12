using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.Maintenances
{
    public class SlotCalculation
    {
        [Key]
        public long SlotCalculation_Id { get; set; }

        [Required]
        public int Thickness { get; set; } = 0;

        [Required]
        public int Width { get; set; } = 0;

    }
}
