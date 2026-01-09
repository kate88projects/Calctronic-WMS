namespace RackingSystem.Models.Item
{
    public class ItemGroupListDTO
    {
        public long ItemGroup_Id { get; set; }

        public string ItemGroupCode { get; set; } = "";

        public string Description { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public int ExpiredYears { get; set; } = 0;
        public int ExpiredMonths { get; set; } = 0;
    }
}
