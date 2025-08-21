using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.Maintenances
{
    public class LoaderColumn
    {
        [Key]
        public long LoaderColumn_Id { get; set; }

        [Required]
        public long Loader_Id { get; set; }

        [Required]
        public int ColNo { get; set; } = 0;

        [Required]
        public int BalanceHeight { get; set; } = 0;

    }
}
