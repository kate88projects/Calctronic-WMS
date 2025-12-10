using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.RackJob
{
    public class RackJobReport
    {
        [Key]
        public Guid RackJobReport_Id { get; set; }

        [MaxLength(50)]
        public string CurrentJobType { get; set; } = "";

        public long Loader_Id { get; set; } = 0;

        public long Trolley_Id { get; set; } = 0;

        public long RackJobQueue_Id { get; set; } = 0;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(50)]
        public string LoginIP { get; set; } = "";

        [Required]
        [MaxLength(50)]
        public string InfoType { get; set; } = "";

        [MaxLength(255)]
        public string InfoEvent { get; set; } = "";

        public string InfoMessage1 { get; set; } = "";

        public string InfoMessage2 { get; set; } = "";

    }
}
