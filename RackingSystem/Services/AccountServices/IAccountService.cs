using RackingSystem.Models.User;
using RackingSystem.Models;

namespace RackingSystem.Services.AccountServices
{
    public interface IAccountService
    {
        public Task<ServiceResponseModel<UserSessionDTO>> Login(LoginDTO model);
    }
}
