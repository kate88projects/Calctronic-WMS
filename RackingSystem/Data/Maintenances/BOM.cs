using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.Maintenances
{
    public class BOM
    {
        [Key]
        public long BOM_Id { get; set; }

        [Required]
        public long Item_Id { get; set; } = 0;

        [MaxLength(500)]
        public string Description { get; set; } = "";

        [Required]
        public bool IsActive { get; set; } = false;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(50)]
        public string CreatedBy { get; set; } = "";

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string UpdatedBy { get; set; } = "";
    }
}
