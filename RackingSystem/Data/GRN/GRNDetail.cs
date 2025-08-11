using System;
using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.GRN
{
    public class GRNDetail
    {
        [Key]
        public Guid GRNDetail_Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string GRNBatchNo { get; set; } = "";

        [Required]
        public long Item_Id { get; set; } = 0;

        [Required]
        public int Qty { get; set; } = 0;

        [Required]
        public DateTime ExpiryDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(50)]
        public string Reel_Id { get; set; } = "";

        [Required]
        [MaxLength(25)]
        public string ReelCode { get; set; } = "";

        [Required]
        [MaxLength(255)]
        public string Remark { get; set; } = "";

        [Required]
        [MaxLength(255)]
        public string SupplierName { get; set; } = "";

        [Required]
        [MaxLength(50)]
        public string SupplierRefNo { get; set; } = "";

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(50)]
        public string CreatedBy { get; set; } = "";

        [Required]
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(50)]
        public string UpdatedBy { get; set; } = "";

    }
}
