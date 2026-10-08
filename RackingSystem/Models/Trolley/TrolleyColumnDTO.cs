namespace RackingSystem.Models.Trolley
{
    public class TrolleyColumnDTO
    {
        //public long TrolleyColumn_Id { get; set; }
        public int colNo { get; set; } = 0;
        public int reelQty { get; set; } = 0;
        public bool isLeft { get; set; } = false;
        public double percentage { get; set; } = 0;
        public string side { get; set; } = "";
        public int totalCount { get; set; } = 0;

    }
}
