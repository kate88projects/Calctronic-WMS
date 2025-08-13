using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.Maintenances
{
    public class SlotCalculation
    {
        [Key]
        public long SlotCalculation_Id { get; set; }

        [Required]
        public int MaxThickness { get; set; } = 0;

        [Required]
        public int ReserveSlot { get; set; } = 0;

    }
}
