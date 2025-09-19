using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.JO
{
    public class JobOrderDetail
    {
        [Key]
        public long JobOrderDetail_Id { get; set; }

        public long JobOrder_Id { get; set; }

        public long Item_Id { get; set; }

        public int Qty { get; set; } = 0;

    }
}
