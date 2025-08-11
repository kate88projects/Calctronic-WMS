using Microsoft.AspNetCore.Identity;

namespace RackingSystem.Data.Maintenances
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = "";
    }
}
