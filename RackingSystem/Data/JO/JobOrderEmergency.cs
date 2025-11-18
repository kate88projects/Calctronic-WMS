using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.JO
{
    public class JobOrderEmergency
    {
        [Key]
        public long JobOrderEmergency_Id { get; set; }

        [Required]
        public DateTime DocDate { get; set; } = new DateTime();

        [Required]
        [MaxLength(50)]
        public string DocNo { get; set; } = "";

        [MaxLength(255)]
        public string Description { get; set; } = "";

        [MaxLength(20)]
        public string Status { get; set; } = "";

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(50)]
        public string CreatedBy { get; set; } = "";
    }
}
