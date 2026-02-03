using System.Security.Claims;

namespace RackingSystem.Models.User
{
    public class UserSessionDTO
    {
        public string Fullname { get; set; } = "";
        public string Username { get; set; } = "";
        public string Token { get; set; } = "";
        public List<int> UACIdList { get; set; } = new List<int>();
        public string DeviceId { get; set; } = "";
        public List<Claim> authClaims { get; set; } = new List<Claim>();
    }
}
