using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Models.Log
{
    public class LogDTO
    {
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string EventName { get; set; } = "";

        public long Id { get; set; }

        public string Remark1 { get; set; } = "";

        public string Remark2 { get; set; } = "";

    }
}
