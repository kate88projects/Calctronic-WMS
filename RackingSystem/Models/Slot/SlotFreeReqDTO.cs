namespace RackingSystem.Models.Slot
{
    public class SlotFreeReqDTO
    {
        public string SortExp { get; set; } = "";

        public int TotalSlot { get; set; } = 0;

        public int ColNo { get; set; } = 0;

        public bool IsLeft { get; set; } = true;
    }
}
