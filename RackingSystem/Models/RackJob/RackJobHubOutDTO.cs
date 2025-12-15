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

        public string GantryStatus { get; set; } = "";

        public string ReelId { get; set; } = "";
        public string ReelCode { get; set; } = "";

        public string CartesianRobotState { get; set; } = "";
        public string CartesianLocation { get; set; } = "";

        public string RetrieveSlot { get; set; } = "";
        public string PutAwaySlot { get; set; } = "";

        public List<RackJobHubOutDtlDTO> DtlList { get; set; } = new List<RackJobHubOutDtlDTO>();
        public List<RackJobHubOutEDtlDTO> EDtlList { get; set; } = new List<RackJobHubOutEDtlDTO>();

    }

    public class RackJobHubOutDtlDTO
    {
        public long JobOrderDetail_Id { get; set; }
        public long JobOrder_Id { get; set; }
        public long Item_Id { get; set; }
        public int Qty { get; set; } = 0;
    }

    public class RackJobHubOutEDtlDTO
    {
        public long JobOrderEmergencyDetail_Id { get; set; }
        public long JobOrderEmergency_Id { get; set; }
        public long Item_Id { get; set; }
        public int Qty { get; set; } = 0;
    }

    public class RackJobHubOutJsonDTO
    {
        public bool SystemStart { get; set; } = false;

        public string TrolleyCode { get; set; } = "";
        public long TrolleyId { get; set; } = 0;

        public string GantryStatus { get; set; } = "";

        public string ReelId { get; set; } = "";
        public string ReelCode { get; set; } = "";

        public string CartesianRobotState { get; set; } = "";
        public string CartesianLocation { get; set; } = "";

        public string RetrieveSlot { get; set; } = "";
        public string PutAwaySlot { get; set; } = "";
    }
}
