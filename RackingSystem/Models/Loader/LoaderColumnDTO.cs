namespace RackingSystem.Models.Loader
{
    public class LoaderColumnDTO
    {
        public long LoaderColumn_Id { get; set; }

        public long Loader_Id { get; set; }

        public int ColNo { get; set; } = 0;

        public int BalanceHeight { get; set; } = 0;
        public int ColHeight { get; set; } = 0; 
        public int BalancePercentage { get; set; } = 0;
        public int UsagePercentage { get; set; } = 0;

    }
}
