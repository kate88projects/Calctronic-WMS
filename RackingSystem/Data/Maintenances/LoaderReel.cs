using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.Maintenances
{
    public class LoaderReel
    {
        [Key]
        public Guid LoaderReel_Id { get; set; }

        [Required]
        public long Loader_Id { get; set; }

        [Required]
        public int ColNo { get; set; } = 0;

        [Required]
        public Guid Reel_Id { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

    }
}
