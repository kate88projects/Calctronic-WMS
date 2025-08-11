namespace RackingSystem.Models.Item
{
    public class ItemDTO
    {
        public long Item_Id { get; set; } = 0;

        public string ItemCode { get; set; } = "";

        public string UOM { get; set; } = "";

        public string Description { get; set; } = "";

        public string Desc2 { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public bool IsFinishGood { get; set; } = true;

        public int? Height { get; set; } = 0;

        public int? Width { get; set; } = 0;

        public int? MaxHeight { get; set; } = 0;

        public bool AlarmOverMaxHeight { get; set; } = true;
    }
}
