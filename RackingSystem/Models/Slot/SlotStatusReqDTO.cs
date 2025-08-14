namespace RackingSystem.Models.Slot
{
    public class SlotStatusReqDTO
    {
        public long Slot_Id { get; set; }

        public bool IsActive { get; set; } = true;

        public bool ForEmptyTray { get; set; } = false;

        public bool HasEmptyTray { get; set; } = false;

        public bool HasReel { get; set; } = false;
        public string ReelNo { get; set; } = "";

        public int RowNo { get; set; } = 0;
    }
}
