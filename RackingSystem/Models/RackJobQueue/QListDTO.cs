using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Models.RackJobQueue
{
    public class QListDTO
    {
        [Key]
        public long RackJobQueue_Id { get; set; }

        public long Doc_Id { get; set; }

        public string DocType { get; set; } = "";

        public string DocNo { get; set; } = "";

        public int Idx { get; set; } = 0;

        public int TotalLines { get; set; } = 0;

        public string Remark { get; set; } = "";

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string CreatedDateDisplay { get; set; } = "";

        public string CreatedBy { get; set; } = "";

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        public string UpdatedDateDisplay { get; set; } = "";

        public string UpdatedBy { get; set; } = "";
    }
}
