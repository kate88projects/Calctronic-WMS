namespace RackingSystem.Models.User
{
    public class UserAccessRightReqDTO
    {
        public string User_Id { get; set; } = "";
        public List<int> UACIdList { get; set; } = new List<int>();
    }
}
