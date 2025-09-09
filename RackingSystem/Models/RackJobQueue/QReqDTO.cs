using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Models.RackJobQueue
{
    public class QReqDTO
    {
        public long Doc_Id { get; set; }
        public string DocType { get; set; } = "";
        public int NewIdx { get; set; } = 1;
    }
}
