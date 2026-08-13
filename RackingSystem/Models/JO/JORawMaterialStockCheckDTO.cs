namespace RackingSystem.Models.JO
{
    public class JORawMaterialStockCheckDTO
    {
        public long JobOrderDetail_Id { get; set; }

        public string FinishGoodItemCode { get; set; } = "";

        public string FinishGoodItemDescription { get; set; } = "";

        public int FinishGoodQty { get; set; } = 0;

        public bool IsFulfillable { get; set; } = false;

        public List<RawMaterialStockDTO> RawMaterials { get; set; } = new List<RawMaterialStockDTO>();

        public List<RawMaterialShortageDTO> Shortages { get; set; } = new List<RawMaterialShortageDTO>();
    }

    public class RawMaterialStockDTO
    {
        public long RawMaterial_Id { get; set; }

        public string RawMaterialItemCode { get; set; } = "";

        public string RawMaterialItemDescription { get; set; } = "";

        public int RequiredQty { get; set; } = 0;

        public int AvailableQty { get; set; } = 0;

        public bool HasSufficientStock { get; set; } = false;

        // New properties for reel information
        public string AllocatedReelCodes { get; set; } = ""; // Comma-separated reel codes

        public int AllocatedReelCount { get; set; } = 0; // Number of reels allocated
    }

    public class RawMaterialShortageDTO
    {
        public long RawMaterial_Id { get; set; }

        public string RawMaterialItemCode { get; set; } = "";

        public string RawMaterialItemDescription { get; set; } = "";

        public int RequiredQty { get; set; } = 0;

        public int AvailableQty { get; set; } = 0;

        public int ShortageQty { get; set; } = 0;
    }
}
