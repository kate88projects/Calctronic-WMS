using RackingSystem.Models.Loader;

namespace RackingSystem.Models.RackJob
{
    public class RackJobHubInDTO
    {
        public LoaderDTO LoaderInfo { get; set; } = new LoaderDTO();

        public bool SystemStart { get; set; } = false;

        public string LoaderCode { get; set; } = "";
        public long LoaderId { get; set; } = 0;

        public string DrawerStatus { get; set; } = "";
        public string GantryStatus { get; set; } = "";

        public int SetUnload { get; set; } = 0;
        public string ReelId { get; set; } = "";
        public string ReelCode { get; set; } = "";
        public string ActualHeight { get; set; } = "";

        public string CartesianRobotState { get; set; } = "";
        public string CartesianLocation { get; set; } = "";

        public string RetrieveSlot { get; set; } = "";
        public string PutAwaySlot { get; set; } = "";
        public string SlotTake { get; set; } = "";
        public string ReelIdC { get; set; } = "";
        public string ReelCodeC { get; set; } = "";
        public string ActualHeightC { get; set; } = "";

    }

    public class RackJobHubInJsonDTO
    {
        public bool SystemStart { get; set; } = false;

        public string LoaderCode { get; set; } = "";
        public long LoaderId { get; set; } = 0;

        public string DrawerStatus { get; set; } = "";
        public string GantryStatus { get; set; } = "";

        public int SetUnload { get; set; } = 0;
        public string ReelId { get; set; } = "";
        public string ReelCode { get; set; } = "";
        public int ActualHeight { get; set; } = 0;

        public string CartesianRobotState { get; set; } = "";
        public string CartesianLocation { get; set; } = "";

        public string RetrieveSlot { get; set; } = "";
        public string PutAwaySlot { get; set; } = "";
        public int SlotTake { get; set; } = 0;
        public string ReelIdC { get; set; } = "";
        public string ReelCodeC { get; set; } = "";
        public int ActualHeightC { get; set; } = 0;

        public bool HalfPickMove { get; set; } = false;
        public int HalfPickMoveStep { get; set; } = 0;
        public bool HalfPlaceMove { get; set; } = false;
        public int HalfPlaceMoveStep { get; set; } = 0;

        public int ColNum { get; set; } = 1;

        public int Col1TotalReels { get; set; } = 0;
        public int Col2TotalReels { get; set; } = 0;
        public int Col3TotalReels { get; set; } = 0;
        public int Col4TotalReels { get; set; } = 0;

        public string RackLeftSlot { get; set; } = "";
        public string RackRightSlot { get; set; } = "";

        public string RDot1 { get; set; } = "waiting";
        public string RDot2 { get; set; } = "waiting";
        public string RDot3 { get; set; } = "waiting";
        public string RDot4 { get; set; } = "waiting";
        public string RDot5 { get; set; } = "waiting";
        public string RDot6 { get; set; } = "waiting";
        public string RDot7 { get; set; } = "waiting";
        public string RDot8 { get; set; } = "waiting";

        public string PDot1 { get; set; } = "waiting";
        public string PDot2 { get; set; } = "waiting";
        public string PDot3 { get; set; } = "waiting";
        public string PDot4 { get; set; } = "waiting";
        public string PDot5 { get; set; } = "waiting";
        public string PDot6 { get; set; } = "waiting";
    }
}
