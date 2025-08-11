namespace RackingSystem.Models.Loader
{
    public class LoaderDTO
    {
        public long Loader_Id { get; set; }

        public string LoaderCode { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public string Status { get; set; } = "";

        public string Remark { get; set; } = "";

        public int TotalCol { get; set; } = 0;

        public int ColHeight { get; set; } = 0;
    }
}
