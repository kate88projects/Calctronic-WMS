using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.Maintenances
{
    public class ItemGroup
    {
        [Key]
        public long ItemGroup_Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ItemGroupCode { get; set; } = "";

        [Required]
        [MaxLength(255)]
        public string Description { get; set; } = "";

        [Required]
        public bool IsActive { get; set; } = true;

    }
}
