using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.Maintenances
{
    public class Loader
    {
        [Key]
        public long Loader_Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string LoaderCode { get; set; } = "";

        [Required]
        [MaxLength(255)]
        public string Description { get; set; } = "";

        [Required]
        public bool IsActive { get; set; } = true;

        [MaxLength(20)]
        public string Status { get; set; } = "";

        [MaxLength(50)]
        public string Remark { get; set; } = "";

        [Required]
        public int TotalCol { get; set; } = 0;

        [Required]
        public int ColHeight { get; set; } = 0;

        [MaxLength(50)]
        public string IPAddr { get; set; } = "";

        [MaxLength(50)]
        public string LockedDeviceID { get; set; } = "";

        [MaxLength(50)]
        public string LockedBy { get; set; } = "";

    }
}
