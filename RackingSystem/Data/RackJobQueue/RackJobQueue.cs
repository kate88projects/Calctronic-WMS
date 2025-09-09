using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.RackJobQueue
{
    public class RackJobQueue
    {
        [Key]
        public long RackJobQueue_Id { get; set; }

        [Required]
        public long Doc_Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string DocType { get; set; } = "";

        [Required]
        public int Idx { get; set; } = 0;

        [MaxLength(500)]
        public string Remark { get; set; } = "";

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(50)]
        public string CreatedBy { get; set; } = "";

        [Required]
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(50)]
        public string UpdatedBy { get; set; } = "";

    }
}
