using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.RackJob
{
    public class RackJob
    {
        [Key]
        public long Rack_Id { get; set; }

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
        [MaxLength(50)]
        public string LoginIP { get; set; } = "";

        [Required]
        public string Json { get; set; } = "";

        [Required]
        public int TotalCount { get; set; } = 0;
    }
}
