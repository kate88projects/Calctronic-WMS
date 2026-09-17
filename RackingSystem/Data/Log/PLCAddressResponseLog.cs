using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.Log
{
    public class PLCAddressResponseLog
    {
        [Key]
        public Guid PLCAddressResponseLog_Id { get; set; }

        public long RackJobQueue_Id { get; set; } = 0;

        public int Address { get; set; }

        [MaxLength(10)]
        public string Action { get; set; } = "";

        public int Value { get; set; }

        [MaxLength(50)]
        public string MethodName { get; set; } = "";

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsErr { get; set; } = false;

    }
}
