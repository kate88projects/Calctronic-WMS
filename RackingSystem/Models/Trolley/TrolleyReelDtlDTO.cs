using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Models.Trolley
{
    public class TrolleyReelDtlDTO
    {
        public long Trolley_Id { get; set; } = 0;
        public string TrolleyCode { get; set; }

        [Key]
        public long TrolleySlot_Id { get; set; } = 0;
        public string TrolleySlotCode { get; set; } = "";
        public int ColNo { get; set; } = 0;
        public int RowNo { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public bool IsLeft { get; set; } = false;
        public int Priority { get; set; } = 0;
        public bool NeedCheck { get; set; } = false;
        public string CheckRemark { get; set; } = "";
        public bool HasReel { get; set; } = false;
        public string ReelNo { get; set; } = "";

        public Guid Reel_Id { get; set; }
        public string ReelCode { get; set; } = "";
        public int Qty { get; set; } = 0;
        public string ItemCode { get; set; } = "";
        public string ItemDesc { get; set; } = "";
        public string ItemGroupCode { get; set; } = "";

    }
}
