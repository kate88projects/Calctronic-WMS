namespace RackingSystem.Models.Reel
{
    public class ReelAvailableResponseDTO
    {
        public int TotalWaiting { get; set; } = 0;
        public int TotalInLoader { get; set; } = 0;
        public int TotalSRMS { get; set; } = 0;
        public int TotalInTrolley { get; set; } = 0;

        public List<ReelAvailableListDTO> ReelList { get; set; } = new List<ReelAvailableListDTO>();
    }
}
