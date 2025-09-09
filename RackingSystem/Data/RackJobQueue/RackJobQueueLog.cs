using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.RackJobQueue
{
    public class RackJobQueueLog
    {
        [Key]
        public long RackJobQueueLog_Id { get; set; }

        [Required]
        public long RackJobQueue_Id { get; set; }

        [Required]
        public long Doc_Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string DocType { get; set; } = "";

        [Required]
        public int Idx { get; set; } = 0;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(50)]
        public string CreatedBy { get; set; } = "";

        [MaxLength(20)]
        public string EndStatus { get; set; } = "";

    }
}
