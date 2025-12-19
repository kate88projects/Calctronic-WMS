using RackingSystem.Models.Trolley;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RackingSystem.Data.Maintenances
{
    public class Trolley
    {
        [Key]
        public long Trolley_Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string TrolleyCode { get; set; } = "";
        [Required]
        [MaxLength(15)]
        public string IPAdd1 { get; set; } = "";
        [Required]
        [MaxLength(15)]
        public string IPAdd2 { get; set; } = "";
        [Required]
        [MaxLength(15)]
        public string IPAdd3 { get; set; } = "";
        [Required]
        public int IPAdd1AColNo { get; set; } = 0;
        [Required]
        public int IPAdd1BColNo { get; set; } = 0;
        [Required]
        public int IPAdd2AColNo { get; set; } = 0;
        [Required]
        public int IPAdd2BColNo { get; set; } = 0;
        [Required]
        public int IPAdd3AColNo { get; set; } = 0;
        [Required]
        public int IPAdd3BColNo { get; set; } = 0;
        [Required]
        public int IPAdd1RowNo { get; set; } = 0;
        [Required]
        public int IPAdd2RowNo { get; set; } = 0;
        [Required]
        public int IPAdd3RowNo { get; set; } = 0;
        [Required]
        public bool IsActive { get; set; } = true;

        [MaxLength(20)]
        public string Status { get; set; } = "";

        [MaxLength(50)]
        public string Remark { get; set; } = "";

        //[Required]
        //public int TotalCol { get; set; } = 0;

        //[Required]
        //public int TotalRow { get; set; } = 0;

        //[Required]
        //public int Side { get; set; } 

    }
}
