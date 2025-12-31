namespace RackingSystem.Models.RackJob
{
    public class RackJobDrawerInDTO
    {
    }

    public class RackJobDrawerInJsonDTO
    {
        public bool SystemStart { get; set; } = false;

        public string TrolleyCode { get; set; } = "";
        public long TrolleyId { get; set; } = 0;

        public string DrawerStatus { get; set; } = "";

        public string CartesianRobotState { get; set; } = "";
        public string CartesianLocation { get; set; } = "";

        public string TrolleyLocation { get; set; } = "";

        public string RetrieveSlot { get; set; } = "";
        public string PutAwaySlot { get; set; } = "";
    }
}
