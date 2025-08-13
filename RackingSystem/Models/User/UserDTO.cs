namespace RackingSystem.Models.User
{
    public class UserDTO
    {
        public string Username { get; set; } = "";

        public string Password { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";

        public string Fullname { get; set; } = "";
        public string Email { get; set; } = "";
        public string Id { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }
}
