using RackingSystem.Models.Trolley;
using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.Maintenances
{
    public class Trolley
    {
        [Key]
        public long Trolley_Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string TrolleyCode { get; set; } = "";

        [Required]
        [MaxLength(15)]
        public string IPAddress { get; set; } = "";

        [Required]
        public bool IsActive { get; set; } = true;

        [MaxLength(20)]
        public string Status { get; set; } = "";

        [MaxLength(50)]
        public string Remark { get; set; } = "";

        [Required]
        public int TotalCol { get; set; } = 0;

        [Required]
        public int TotalRow { get; set; } = 0;

        [Required]
        public int Side { get; set; } 

    }
}
