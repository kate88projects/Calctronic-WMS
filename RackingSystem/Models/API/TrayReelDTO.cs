namespace RackingSystem.Models.API
{
    public class TrayReelDTO
    {

        public bool successTray { get; set; } = false;
        public string errMessageTray { get; set; } = "";

        public bool successStartScan { get; set; } = false;
        public string errMessageStartScan { get; set; } = "";

        public bool successScan { get; set; } = false;
        public string errMessageScan { get; set; } = "";

        public bool successReel { get; set; } = false;
        public string errMessageReel { get; set; } = "";

        public bool successSetH { get; set; } = false;
        public string errMessageSetH { get; set; } = "";

        public string ScannedBarcode { get; set; } = "";

        public Guid Reel_Id { get; set; }

        public string ReelCode { get; set; } = "";

        public long Item_Id { get; set; } = 0;

        public string ItemCode { get; set; } = "";

        public string UOM { get; set; } = "";

        public string Description { get; set; } = "";

        public int Thickness { get; set; } = 0;

        public int Qty { get; set; } = 0;

        public int ActualHeight { get; set; } = 0;

        public string Status { get; set; } = "";

    }
}
