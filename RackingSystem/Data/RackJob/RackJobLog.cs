using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.RackJob
{
    public class RackJobLog
    {
        [Key]
        public long RackJobLog_Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string CurrentJobType { get; set; } = "";

        [Required]
        public long Loader_Id { get; set; } = 0;

        [Required]
        public long Trolley_Id { get; set; } = 0;

        [Required]
        public long RackJobQueue_Id { get; set; }

        [Required]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime EndDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(50)]
        public string LoginIP { get; set; } = "";

    }
}
