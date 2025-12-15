namespace RackingSystem.Models.Trolley
{
    public class TrolleyDTO
    {
        public long Trolley_Id { get; set; }

        public string TrolleyCode { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public string Status { get; set; } = "";

        public string Remark { get; set; } = "";

        public int TotalCol { get; set; } = 0;

        public int TotalRow { get; set; } = 0;

        public Side Side { get; set; }

        public int Col1TotalUsed { get; set; } = 0;
        public int Col2TotalUsed { get; set; } = 0;
        public int Col3TotalUsed { get; set; } = 0;
        public int Col4TotalUsed { get; set; } = 0;
        public int Col5TotalUsed { get; set; } = 0;
        public int Col6TotalUsed { get; set; } = 0;

        public int Col1Balance { get; set; } = 0;
        public int Col2Balance { get; set; } = 0;
        public int Col3Balance { get; set; } = 0;
        public int Col4Balance { get; set; } = 0;
        public int Col5Balance { get; set; } = 0;
        public int Col6Balance { get; set; } = 0;

    }
}
