using Microsoft.Identity.Client;

namespace RackingSystem.Models.BOM
{
    public class BOMDtlReqDTO
    {
        public long BOM_Id { get; set; } 
        public long Item_Id { get; set; }
        public string Description { get; set; } = "";
        public bool IsActive { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = "";
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public string UpdatedBy { get; set; } = "";
        public List<BOMDtlDTO> SubItems { get; set; }

    }
}
