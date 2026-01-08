using RackingSystem.Models.Trolley;

namespace RackingSystem.Models.RackJob
{
    public class RackJobTrolleyDTO
    {
        public TrolleyDTO TrolleyInfo { get; set; } = new TrolleyDTO();

        public bool SystemStart { get; set; } = false;

        public string TrolleyCode { get; set; } = "";
        public long TrolleyId { get; set; } = 0;

        public string DrawerStatus { get; set; } = "";

        public string CartesianRobotState { get; set; } = "";
        public string CartesianLocation { get; set; } = "";

        public string RetrieveSlot { get; set; } = "";
        public string PutAwaySlot { get; set; } = "";

        public string TrolleyLocation { get; set; } = "";

        public bool chkCol1 { get; set; } = true;
        public bool chkCol2 { get; set; } = true;
        public bool chkCol3 { get; set; } = true;

        public int CurrentRowCol1 { get; set; } = 1;
        public int CurrentRowCol2 { get; set; } = 1;
        public int CurrentRowCol3 { get; set; } = 1;

    }

    public class RackJobTrolleyJsonDTO
    {
        public bool SystemStart { get; set; } = false;

        public string TrolleyCode { get; set; } = "";
        public long TrolleyId { get; set; } = 0;

        public string DrawerStatus { get; set; } = "";

        public string CartesianRobotState { get; set; } = "";
        public string CartesianLocation { get; set; } = "";

        public string RetrieveSlot { get; set; } = "";
        public string PutAwaySlot { get; set; } = "";

        public string TrolleyLocation { get; set; } = "";

        public bool chkCol1 { get; set; } = true;
        public bool chkCol2 { get; set; } = true;
        public bool chkCol3 { get; set; } = true;

        public int CurrentRowCol1 { get; set; } = 1;
        public int CurrentRowCol2 { get; set; } = 1;
        public int CurrentRowCol3 { get; set; } = 1;

    }
}
