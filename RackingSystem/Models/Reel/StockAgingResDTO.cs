namespace RackingSystem.Models.Reel
{
    public class StockAgingResDTO
    {
        public List<StockAgingDTO> Items { get; set; } = new List<StockAgingDTO>();

        public int TotalRecords { get; set; }

        public int TotalNonExpired { get; set; }

        public int TotalExpired { get; set; }

        public int TotalNonExpiredInSRMS { get; set; }

        public int TotalExpiredInSRMS { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }
    }
}
