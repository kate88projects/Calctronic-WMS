using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.Maintenances
{
    public class DocFormatDetail
    {
        [Key]
        public long DocFormatDetail_Id { get; set; }

        [Required]
        public long DocFormat_Id { get; set; }

        [Required]
        public int Year { get; set; } = 2025;

        [Required]
        public int Month { get; set; } = 1;

        [Required]
        public int NextRoundingNum { get; set; } = 1;

    }
}
