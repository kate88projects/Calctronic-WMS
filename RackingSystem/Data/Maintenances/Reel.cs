using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RackingSystem.Data.Maintenances
{
    public class Reel
    {
        [Key]
        public Guid Reel_Id { get; set; }

        [MaxLength(25)]
        public string ReelCode { get; set; } = "";

        [Required]
        public long Item_Id { get; set; } = 0;

        [Required]
        public int Qty { get; set; } = 0;

        [Required]
        public DateTime ExpiryDate { get; set; } = DateTime.Now;

        [Required]
        public bool IsReady { get; set; } = false;

        [Required]
        public int StatusIdx { get; set; } = 1;

        [MaxLength(20)]
        public string Status { get; set; } = "";

        [MaxLength(50)]
        public string OnHoldRemark { get; set; } = "";

        [Required]
        public int ActualHeight { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,3)")]
        public decimal ActualHeightDec { get; set; } = 0;

        [MaxLength(50)]
        public string ItemCode { get; set; } = "";

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public long Slot_Id { get; set; }
    }
}
