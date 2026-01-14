namespace RackingSystem.Models.Slot
{
    public class SlotReelDTO
    {
        public long Slot_Id { get; set; } = 0;
        public string SlotCode { get; set; } = "";
        public int ColNo { get; set; } = 0;
        public int RowNo { get; set; } = 0;
        public bool IsLeft { get; set; } = true;

        public string ReelId { get; set; } = "";
        public string ReelCode { get; set; } = "";
        public int ActualHeight { get; set; } = 0;

        public long Detail_Id { get; set; } = 0;

        public string Id { get; set; } = "";
    }
}
