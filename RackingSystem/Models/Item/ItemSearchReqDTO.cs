namespace RackingSystem.Models.Item
{
    public class ItemSearchReqDTO
    {
        public string ItemCode { get; set; } = "";
        public string ItemDesc { get; set; } = "";
        public string ItemGroup { get; set; } = "";
        public string Thickness { get; set; } = "";

        public int pageSize { get; set; } = 50;
        public int page { get; set; } = 1;
    }
}
