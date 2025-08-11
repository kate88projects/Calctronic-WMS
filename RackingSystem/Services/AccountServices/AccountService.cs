using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using RackingSystem.Data.Maintenances;
using RackingSystem.Models.User;
using RackingSystem.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RackingSystem.Services.AccountServices
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;

        public AccountService(UserManager<User> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        public async Task<ServiceResponseModel<UserSessionDTO>> Login(LoginDTO model)
        {
            ServiceResponseModel<UserSessionDTO> response = new ServiceResponseModel<UserSessionDTO>();
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var authClaims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    expires: DateTime.Now.AddMinutes(double.Parse(_config["Jwt:ExpiryMinutes"]!)),
                    //audience: _audience,
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)), SecurityAlgorithms.HmacSha256)
                );
                string tokenR = new JwtSecurityTokenHandler().WriteToken(token);
                response.success = true;
                response.data = new UserSessionDTO()
                {
                    Fullname = user.FullName,
                    Username = user.UserName ?? "",
                    Token = tokenR
                };
                return response;
            }
            response.data = new UserSessionDTO();
            return response;
        }
    }
}
