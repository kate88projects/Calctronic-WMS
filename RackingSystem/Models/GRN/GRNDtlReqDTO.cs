using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Models.GRN
{
    public class GRNDtlReqDTO
    {
        public Guid? GRNDetail_Id { get; set; }

        public string GRNBatchNo { get; set; } = "";

        public long Item_Id { get; set; } = 0;

        public string ItemCode { get; set; } = "";

        public int Qty { get; set; } = 0;

        public DateTime? ExpiryDate { get; set; } = DateTime.Now;

        public string Remark { get; set; } = "";

        public string UserId { get; set; } = "";

    }
}
