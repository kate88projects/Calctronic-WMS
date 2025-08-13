namespace RackingSystem.Models.Setting
{
    public class ReelDimensionListDTO
    {
        public long ReelDimension_Id { get; set; }

        public int Thickness { get; set; } = 0;

        public int Width { get; set; } = 0;

        public int MaxThickness { get; set; } = 0;

        public string Description { get; set; } = "";

    }
}
