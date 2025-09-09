using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Models.Loader
{
    public class LoaderListDTO
    {
        public long Loader_Id { get; set; }

        public string LoaderCode { get; set; } = "";

        public string Description { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public string Status { get; set; } = "";

        public string Remark { get; set; } = "";

        public int TotalCol { get; set; } = 0;

        public int ColHeight { get; set; } = 0;

        public string IPAddr { get; set; } = "";

        public List<LoaderColumnDTO> ColList = new List<LoaderColumnDTO>();
        public List<int> ColBalList = new List<int>();

        public int BalanceHeight { get; set; } = 0;

        public int BalancePercentage { get; set; } = 0;
        public int UsagePercentage { get; set; } = 0;

    }
}
