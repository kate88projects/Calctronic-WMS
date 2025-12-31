using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.JO
{
    public class JobOrderEmergencyDetail
    {
        [Key]
        public long JobOrderEmergencyDetail_Id { get; set; }

        [Required]
        public long JobOrderEmergency_Id { get; set; }

        [Required]
        public long Item_Id { get; set; }

        [Required]
        public int Qty { get; set; } = 0;

        [Required]
        public int BalQty { get; set; } = 0;
    }
}
