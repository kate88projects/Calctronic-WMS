using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Models.JO
{
    public class JOListDTO
    {
        public long JobOrder_Id { get; set; }

        public DateTime DocDate { get; set; } = new DateTime();

        public string DocDateDisplay { get; set; } = "";

        public string DocNo { get; set; } = "";

        public string Description { get; set; } = "";

        public string Status { get; set; } = "";

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string CreatedDateDisplay { get; set; } = "";

        public string CreatedBy { get; set; } = "";

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        public string UpdatedDateDisplay { get; set; } = "";

        public string UpdatedBy { get; set; } = "";

        public int totalRecord { get; set; } = 0;

        public int page { get; set; } = 1;
    }
}
