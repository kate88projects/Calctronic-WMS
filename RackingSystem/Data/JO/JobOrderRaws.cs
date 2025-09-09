using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.JO
{
    public class JobOrderRaws
    {
        [Key]
        public Guid JobOrderRaws_Id { get; set; }

        public long JobOrderDetail_Id { get; set; }

        public long JobOrder_Id { get; set; }

        public long BOM_Id { get; set; }

        public long Item_Id { get; set; } = 0;

        public int BaseQty { get; set; } = 0;

        public int Qty { get; set; } = 0;

        public int BalQty { get; set; } = 0;

    }
}
