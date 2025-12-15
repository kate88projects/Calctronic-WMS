using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Models.Reel
{
    public class ReelAvailableListDTO
    {
        [Key]
        public Guid Reel_Id { get; set; }

        public string ReelCode { get; set; } = "";

        public int Qty { get; set; } = 0;

        public string Status { get; set; } = "";

        public int StatusIdx { get; set; } = 0;

        public int ActualHeight { get; set; } = 0;

        public decimal ActualHeightDec { get; set; } = 0;

        public bool NeedCheck { get; set; } = false;

        public string CheckRemark { get; set; } = "";

        public string ItemCode { get; set; } = "";

        public string ItemDesc { get; set; } = "";

        public string Desc2 { get; set; } = "";

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string CreatedDateDisplay { get; set; } = "";


        public string LoaderCode { get; set; } = "";

        public int LoaderColumn { get; set; } = 0;

        public bool SRMSIsLeft { get; set; } = false;

        public string SRMSSlotCode { get; set; } = "";

        public int SRMSColNo { get; set; } = 0;

        public int SRMSRowNo { get; set; } = 0;

        public string TrolleyCode { get; set; } = "";

        public string TrolleySlotCode { get; set; } = "";

        public int TrolleyColNo { get; set; } = 0;

        public int TrolleyRowNo { get; set; } = 0;

        public int TotalWaiting { get; set; } = 0;
        public int TotalInLoader { get; set; } = 0;
        public int TotalSRMS { get; set; } = 0;
        public int TotalInTrolley { get; set; } = 0;

        public int totalRecord { get; set; } = 0;

        public int page { get; set; } = 1;
    }
}
