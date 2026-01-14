using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.JO
{
    public class JobOrderDetail
    {
        [Key]
        public long JobOrderDetail_Id { get; set; }
        [Required]
        public long JobOrder_Id { get; set; }
        [Required]
        public long Item_Id { get; set; }
        [Required]
        public int Qty { get; set; } = 0;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
