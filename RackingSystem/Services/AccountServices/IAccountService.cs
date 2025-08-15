using RackingSystem.Models.User;
using RackingSystem.Models;

namespace RackingSystem.Services.AccountServices
{
    public interface IAccountService
    {
        public Task<ServiceResponseModel<UserSessionDTO>> Login(LoginDTO model);
        
        public Task<ServiceResponseModel<List<UserListDTO>>> GetUserList();
        public Task<ServiceResponseModel<UserDTO>> SaveUser(UserDTO req);
        public Task<ServiceResponseModel<UserDTO>> DeleteUser(UserDTO req);
        public Task<ServiceResponseModel<UserDTO>> ResetUserPassword(UserDTO req);

        public Task<ServiceResponseModel<List<UserListDTO>>> GetUserAccessRightList();
        public Task<ServiceResponseModel<UserAccessRightReqDTO>> SaveUserAccessRight(UserAccessRightReqDTO req);

    }
}
