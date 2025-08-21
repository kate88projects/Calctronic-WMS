using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.Log
{
    public class PLCLoaderLog
    {
        [Key]
        public Guid PLCLoaderLog_Id { get; set; }

        [MaxLength(50)]
        public string EventName { get; set; } = "";

        public long Loader_Id { get; set; }

        public string Remark1 { get; set; } = "";

        public string Remark2 { get; set; } = "";

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
