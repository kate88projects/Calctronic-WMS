namespace RackingSystem.Models.Item
{
    public class ItemExcelReqDTO
    {
        public long Item_Id { get; set; } = 0;

        public string ItemCode { get; set; } = "";

        public string UOM { get; set; } = "";

        public string Description { get; set; } = "";

        public string Desc2 { get; set; } = "";

        public long ItemGroup_Id { get; set; } = 0;
        public string ItemGroupCode { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public bool IsFinishGood { get; set; } = true;

        public long ReelDimension_Id { get; set; } = 0;

        public int Thickness { get; set; } = 0;

        public int Width { get; set; } = 0;

        public int MaxThickness { get; set; } = 0;

        public bool AlarmOverMaxThickness { get; set; } = true;

        public int totalRecord { get; set; } = 0;

        public int page { get; set; } = 1;

        public string ErrorMsg { get; set; } = "";
    }
}
