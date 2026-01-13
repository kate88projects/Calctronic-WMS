using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Models.Loader
{
    public class LoaderReelDtlDTO
    {
        public long Loader_Id { get; set; } = 0;
        public string LoaderCode { get; set; } = "";
        public int ColHeight { get; set; } = 0;
        public int BalanceHeight { get; set; } = 0;
        public int ColNo { get; set; } = 0;

        [Key]
        public Guid Reel_Id { get; set; }
        public string ReelCode { get; set; } = "";
        public int Qty { get; set; } = 0;
        public string ItemCode { get; set; } = "";
        public string ItemDesc { get; set; } = "";
        public string ItemGroupCode { get; set; } = "";
    }
}
