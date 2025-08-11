using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.Maintenances
{
    public class Configuration
    {
        [Key]
        public long Configuration_Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ConfigTitle { get; set; } = "";

        [Required]
        [MaxLength(50)]
        public string ConfigValue { get; set; } = "";

    }
}
