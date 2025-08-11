using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Models.Trolley
{
    public class TrolleyListDTO
    {
        public long Trolley_Id { get; set; }

        public string TrolleyCode { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public string Status { get; set; } = "";

        public string Remark { get; set; } = "";

        public int TotalCol { get; set; } = 0;

        public int TotalRow { get; set; } = 0;
    }
}
