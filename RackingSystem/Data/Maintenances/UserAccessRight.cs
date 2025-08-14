using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.Maintenances
{
    public class UserAccessRight
    {
        [Key]
        public long UserAccessRight_Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string UAC { get; set; } = "";

        [Required]
        public int UAC_Id { get; set; } = 0;

        [Required]
        [MaxLength(50)]
        public string User_Id { get; set; } = "";

    }
}
