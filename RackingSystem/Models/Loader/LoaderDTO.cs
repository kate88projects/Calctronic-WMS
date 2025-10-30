namespace RackingSystem.Models.Loader
{
    public class LoaderDTO
    {
        public long Loader_Id { get; set; }

        public string LoaderCode { get; set; } = "";

        public string Description { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public string Status { get; set; } = "";

        public string Remark { get; set; } = "";

        public int TotalCol { get; set; } = 0;

        public int ColHeight { get; set; } = 0;

        public string IPAddr { get; set; } = "";

        public int Col1UsedHeight { get; set; } = 0;
        public int Col2UsedHeight { get; set; } = 0;
        public int Col3UsedHeight { get; set; } = 0;
        public int Col4UsedHeight { get; set; } = 0;

        public int Col1TotalReels { get; set; } = 0;
        public int Col2TotalReels { get; set; } = 0;
        public int Col3TotalReels { get; set; } = 0;
        public int Col4TotalReels { get; set; } = 0;

        public int Col1UsedPercentage { get; set; } = 0;
        public int Col2UsedPercentage { get; set; } = 0;
        public int Col3UsedPercentage { get; set; } = 0;
        public int Col4UsedPercentage { get; set; } = 0;

    }
}
