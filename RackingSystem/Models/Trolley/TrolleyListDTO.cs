using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Models.Trolley
{

    public class TrolleyListDTO : PageModel
    {
        public long Trolley_Id { get; set; }
        public string TrolleyCode { get; set; } = "";
        public string IPAdd1 { get; set; } = "";
        public string IPAdd2 { get; set; } = "";
        public string IPAdd3 { get; set; } = "";
        public int IPAdd1Col1 { get; set; } = 0;
        public int IPAdd1Col2 { get; set; } = 0;
        public int IPAdd2Col1 { get; set; } = 0;
        public int IPAdd2Col2 { get; set; } = 0;
        public int IPAdd3Col1 { get; set; } = 0;
        public int IPAdd3Col2 { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public string Status { get; set; } = "";
        public string Remark { get; set; } = "";
        public int TotalRow { get; set; } = 0;

        //public Side Side { get; set; }

    }

    public enum Side
    {
        A = 0,
        B = 1
    }

}
