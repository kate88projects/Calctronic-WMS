namespace RackingSystem.Models.User
{
    public class UserSessionDTO
    {
        public string Fullname { get; set; } = "";
        public string Username { get; set; } = "";
        public string Token { get; set; } = "";
        public List<int> UACIdList { get; set; } = new List<int>();
    }
}
