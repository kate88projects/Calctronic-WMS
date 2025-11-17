using RackingSystem.Models.Loader;

namespace RackingSystem.Models.RackJob
{
    public class RackJobHubInDTO
    {
        public LoaderDTO LoaderInfo { get; set; } = new LoaderDTO();
        public string LoaderCode { get; set; } = "";
        public long LoaderId { get; set; } = 0;

        public string DrawerStatus { get; set; } = "";
        public string GantryStatus { get; set; } = "";

        public int SetUnload { get; set; } = 0;
        public string ReelId { get; set; } = "";
        public string ReeLCode { get; set; } = "";
        public string ActualHeight { get; set; } = "";

        public string CartesianStatus { get; set; } = "";

        public string RetrieveSlotSide { get; set; } = "";
        public string RetrieveSlot { get; set; } = "";
        public string PutAwaySlotSide { get; set; } = "";
        public string PutAwaySlot { get; set; } = "";
        public string SlotTake { get; set; } = "";
        public string ReelIdC { get; set; } = "";
        public string ReeLCodeC { get; set; } = "";
        public string ActualHeightC { get; set; } = "";

    }
}
