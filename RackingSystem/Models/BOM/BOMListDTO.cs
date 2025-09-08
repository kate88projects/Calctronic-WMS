using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Models.BOM
{
    public class BOMListDTO
    {
        [Key]
        public long BOM_Id { get; set; }
        public long Item_Id { get; set; } = 0;  
        public string Description { get; set; } = "";
        public bool IsActive { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = "";
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public string UpdatedBy { get; set; } = "";
        public int totalRecord { get; set; } = 0;
        public int page { get; set; } = 1;
    }
}
