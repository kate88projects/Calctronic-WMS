using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Models.Trolley
{

    public class TrolleyListDTO : PageModel
    {
        public long Trolley_Id { get; set; }

        public string TrolleyCode { get; set; } = "";
        public string IPAddress { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public string Status { get; set; } = "";

        public string Remark { get; set; } = "";

        public int TotalCol { get; set; } = 0;

        public int TotalRow { get; set; } = 0;

        public Side Side { get; set; }

    }

    public enum Side
    {
        A = 0,
        B = 1
    }

}
