using RackingSystem.Models.Loader;
using RackingSystem.Models.Trolley;

namespace RackingSystem.Models.RackJob
{
    public class RackJobHubOutDTO
    {
        public TrolleyDTO TrolleyInfo { get; set; } = new TrolleyDTO();

        public bool SystemStart { get; set; } = false;

        public string TrolleyCode { get; set; } = "";
        public long TrolleyId { get; set; } = 0;

        public string DrawerStatus { get; set; } = "";

        public string ReelId { get; set; } = "";
        public string ReelCode { get; set; } = "";
        public int ActualHeight { get; set; } = 0;

        public string CartesianRobotState { get; set; } = "";
        public string CartesianLocation { get; set; } = "";

        public string RetrieveSlot { get; set; } = "";
        public string PutAwaySlot { get; set; } = "";
        public int SlotTake { get; set; } = 0;

        public string TrolleyLocation { get; set; } = "";

        public List<RackJobHubOutDtlDTO> DtlList { get; set; } = new List<RackJobHubOutDtlDTO>();

    }

    public class RackJobHubOutDtlDTO
    {
        public string Detail_Id { get; set; }
        public long Id { get; set; }
        public long Item_Id { get; set; }
        public int Qty { get; set; } = 0;
        public string Reel_Id { get; set; } = "";
        public string ItemCode { get; set; } = "";
    }

    public class RackJobHubOutJsonDTO
    {
        public bool SystemStart { get; set; } = false;

        public string TrolleyCode { get; set; } = "";
        public long TrolleyId { get; set; } = 0;
        public string TrolleySide { get; set; } = "A";

        public string DrawerStatus { get; set; } = "";

        public string ReelId { get; set; } = "";
        public string ReelCode { get; set; } = "";
        public int ActualHeight { get; set; } = 0;
        public long DtlId { get; set; } = 0;

        public string CartesianRobotState { get; set; } = "";
        public string CartesianLocation { get; set; } = "";

        public string RetrieveSlot { get; set; } = "";
        public string PutAwaySlot { get; set; } = "";
        public int SlotTake { get; set; } = 0;

        public string TrolleyLocation { get; set; } = "";

        public int ReelBalance { get; set; } = 0;

        public string ArmPosition { get; set; } = "center";
        public int ArmHeight { get; set; } = 0;
        public bool GripperOpen { get; set; } = false;
        public bool HeldReel { get; set; } = false;

        public string RackLeftSlot { get; set; } = "";
        public string RackRightSlot { get; set; } = "";
        public string TrolleySlot { get; set; } = "";

        public string RDot1 { get; set; } = "waiting";
        public string RDot2 { get; set; } = "waiting";
        public string RDot3 { get; set; } = "waiting";
        public string PDot1 { get; set; } = "waiting";
        public string PDot2 { get; set; } = "waiting";
        public string PDot3 { get; set; } = "waiting";

    }
}
