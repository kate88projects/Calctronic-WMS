namespace RackingSystem.Models.User
{
    public class UserListDTO
    {
        public string Username { get; set; } = "";
        public string Fullname { get; set; } = "";
        public string Email { get; set; } = "";
        public string Id { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public List<int> UACIdList { get; set; } = new List<int>();
    }
}
