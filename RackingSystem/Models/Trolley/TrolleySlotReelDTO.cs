using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Models.Trolley
{
    public class TrolleySlotReelDTO
    {
        public long Trolley_Id { get; set; } = 0;
        public string TrolleyCode { get; set; }

        public string IPAdd1 { get; set; } = "";
        public string IPAdd2 { get; set; } = "";
        public string IPAdd3 { get; set; } = "";

        [Key]
        public long TrolleySlot_Id { get; set; } = 0;
        public string TrolleySlotCode { get; set; } = "";
        public int ColNo { get; set; } = 0;
        public int RowNo { get; set; } = 0;
        public bool IsLeft { get; set; } = false;

        public Guid Reel_Id { get; set; }
        public string ReelCode { get; set; } = "";
        public int Qty { get; set; } = 0;
        public string ItemCode { get; set; } = "";
        public string ItemDesc { get; set; } = "";

    }
}
