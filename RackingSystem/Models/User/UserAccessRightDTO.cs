using System.ComponentModel.DataAnnotations;

namespace RackingSystem.Models.User
{
    public class UserAccessRightDTO
    {
        public long UserAccessRight_Id { get; set; }

        public string UAC { get; set; } = "";

        public int UAC_Id { get; set; } = 0;

        public string User_Id { get; set; } = "";
    }
}
