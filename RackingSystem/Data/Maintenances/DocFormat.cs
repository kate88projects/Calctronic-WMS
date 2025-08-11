using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.Maintenances
{
    public class DocFormat
    {
        [Key]
        public long DocFormat_Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string DocFormatType { get; set; } = "";

        [Required]
        [MaxLength(25)]
        public string DocumentFormat { get; set; } = "";

        [Required]
        public int NumberLength { get; set; } = 8;

        [Required]
        public int NextRoundingNum { get; set; } = 1;

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public bool IsResetMonthly { get; set; } = false;

    }
}
