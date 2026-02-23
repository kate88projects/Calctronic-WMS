using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Models.GRN
{
    public class GRNDtlListDTO
    {
        [Key]
        public Guid GRNDetail_Id { get; set; }

        public string GRNBatchNo { get; set; } = "";

        public long Item_Id { get; set; } = 0;

        public string ItemCode { get; set; } = "";

        public string ItemDesc { get; set; } = "";

        public int Qty { get; set; } = 0;

        public DateTime ExpiryDate { get; set; } = DateTime.Now;

        public string ReelCode { get; set; } = "";

        public string ReelStatus { get; set; } = "";

        public string Remark { get; set; } = "";

        public string SupplierName { get; set; } = "";

        public string SupplierRefNo { get; set; } = "";

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
