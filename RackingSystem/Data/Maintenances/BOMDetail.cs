using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.Maintenances
{
    public class BOMDetail
    {
        [Key]
        public long BOMDetail_Id { get; set; }

        [Required]
        public long BOM_Id { get; set; } = 0;

        [Required]
        public long Item_Id { get; set; } = 0;

        [Required]
        public int Qty { get; set; } = 0;

        [MaxLength(500)]
        public string Remark { get; set; } = "";

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(50)]
        public string CreatedBy { get; set; } = "";

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string UpdatedBy { get; set; } = "";
    }
}
