namespace RackingSystem.Models.User
{
    public class UserListDTO
    {
        public string Username { get; set; } = "";
        public string Fullname { get; set; } = "";
        public string Email { get; set; } = "";
        public string Id { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }
}
