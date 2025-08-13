using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Data.Maintenances
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = "";

        [Required]
        public bool IsActive { get; set; } = true;
    }
}
