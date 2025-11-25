using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using RackingSystem.Data.Maintenances;
using RackingSystem.Models.User;
using RackingSystem.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Models.Setting;
using RackingSystem.Data;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Components.Web;
using System;
using RackingSystem.General;
using RackingSystem.Data.GRN;

namespace RackingSystem.Services.AccountServices
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;

        public AccountService(AppDbContext dbContext, UserManager<User> userManager, IConfiguration config)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _config = config;
        }

        public async Task<ServiceResponseModel<UserSessionDTO>> Login([FromBody] LoginDTO model)
        {
            ServiceResponseModel<UserSessionDTO> response = new ServiceResponseModel<UserSessionDTO>();
            if (string.IsNullOrEmpty(model.Username))
            {
                response.errMessage = "User cannot empty.";
                return response;
            }
            if (string.IsNullOrEmpty(model.Password))
            {
                response.errMessage = "Password cannot empty.";
                return response;
            }

            User rUser;
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                if (user.IsActive == false)
                {
                    response.errMessage = "User is inactive.";
                    return response;
                }
                rUser = user;
            }
            else
            {
                var user2 = await _userManager.FindByEmailAsync(model.Username);
                if (user2 != null && await _userManager.CheckPasswordAsync(user2, model.Password))
                {
                    if (user2.IsActive == false)
                    {
                        response.errMessage = "User is inactive.";
                        return response;
                    }
                    rUser = user2;
                }
                else
                {
                    response.data = new UserSessionDTO();
                    response.errMessage = "Username or Password wrong.";
                    return response;
                }
            }

            var uacIdList = _dbContext.UserAccessRight.Where(x => x.User_Id == rUser.Id).Select(x => x.UAC_Id).ToList();

            var authClaims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, rUser.UserName!),
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
                Fullname = rUser.FullName,
                Username = rUser.UserName ?? "",
                Token = tokenR,
                UACIdList = uacIdList,
                DeviceId = Guid.NewGuid().ToString(),
            };
            return response;

        }


        public async Task<ServiceResponseModel<List<UserListDTO>>> GetUserList()
        {
            ServiceResponseModel<List<UserListDTO>> result = new ServiceResponseModel<List<UserListDTO>>();

            try
            {
                var users = await _userManager.Users.ToListAsync();
                List<UserListDTO> listDTO = new List<UserListDTO>();   
                foreach (var user in users)
                {
                    if (user.UserName == "calctronic@gmail.com") { continue; }
                    listDTO.Add(new UserListDTO
                    {
                        Id = user.Id,   
                        Username = user.UserName ?? "",
                        Fullname = user.FullName,
                        Email = user.Email ?? "",
                        IsActive = user.IsActive,   
                    });
                }
                result.success = true;
                result.data = listDTO;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<UserDTO>> SaveUser(UserDTO req)
        {
            ServiceResponseModel<UserDTO> result = new ServiceResponseModel<UserDTO>();

            try
            {
                // 1. checking Data
                if (req == null)
                {
                    result.errMessage = "Please insert Username.";
                    return result;
                }
                if (req.Username == "")
                {
                    result.errMessage = "Please insert Username.";
                    return result;
                }
                if (req.Fullname == "")
                {
                    result.errMessage = "Please insert Fullname.";
                    return result;
                }
                if (req.Email == "")
                {
                    result.errMessage = "Please insert Email.";
                    return result;
                }
                if (req.Id == "")
                {
                    if (req.Password == "")
                    {
                        result.errMessage = "Please insert Password.";
                        return result;
                    }
                    if (req.ConfirmPassword == "")
                    {
                        result.errMessage = "Please insert Confirm Password.";
                        return result;
                    }
                    if (req.Password != req.ConfirmPassword)
                    {
                        result.errMessage = "Password and Confirm Password are not match.";
                        return result;
                    }
                    User? rExist = _dbContext.Users.FirstOrDefault(x => x.UserName == req.Username);
                    if (rExist != null)
                    {
                        result.errMessage = "This Username has exist.";
                        return result;
                    }
                    rExist = _dbContext.Users.FirstOrDefault(x => x.Email == req.Email);
                    if (rExist != null)
                    {
                        result.errMessage = "This Email has exist.";
                        return result;
                    }
                }
                else
                {
                    User? rExist = _dbContext.Users.FirstOrDefault(x => x.UserName == req.Username && x.Id != req.Id);
                    if (rExist != null)
                    {
                        result.errMessage = "This Username has exist.";
                        return result;
                    }
                    rExist = _dbContext.Users.FirstOrDefault(x => x.Email == req.Email && x.Id != req.Id);
                    if (rExist != null)
                    {
                        result.errMessage = "This Email has exist.";
                        return result;
                    }
                }

                // 2. save Data
                if (req.Id == "")
                {
                    var user = new User { 
                        UserName = req.Username, 
                        FullName = req.Fullname,
                        Email = req.Email,
                        IsActive = req.IsActive,
                    };
                    var r = await _userManager.CreateAsync(user, req.Password);
                    if (r.Succeeded)
                    {
                        result.success = true;
                    }
                    else
                    {
                        foreach (var error in r.Errors)
                        {
                            result.errMessage = result.errMessage + error.Description;
                        }
                    }
                }
                else
                {
                    User? user = _dbContext.Users.Find(req.Id);
                    if (user == null)
                    {
                        result.errMessage = "Cannot find this user, please refresh the list.";
                        return result;
                    }
                    user.UserName = req.Username;
                    user.FullName = req.Fullname;
                    user.Email = req.Email;
                    user.IsActive = req.IsActive;
                    var r = await _userManager.UpdateAsync(user);
                    if (r.Succeeded)
                    {
                        result.success = true;
                    }
                    else
                    {
                        foreach (var error in r.Errors)
                        {
                            result.errMessage = result.errMessage + error.Description;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<UserDTO>> DeleteUser(UserDTO req)
        {
            ServiceResponseModel<UserDTO> result = new ServiceResponseModel<UserDTO>();

            try
            {
                // 1. checking Data
                if (req == null)
                {
                    result.errMessage = "Please refresh the list.";
                    return result;
                }

                GRNDetail? binExist1 = _dbContext.GRNDetail.FirstOrDefault(x => x.CreatedBy == req.Id);
                if (binExist1 != null)
                {
                    result.errMessage = "This User No has been used, cannot delete.";
                    return result;
                }

                // 2. save Data
                var user = await _userManager.FindByIdAsync(req.Id); // Or FindByNameAsync(username)
                if (user == null)
                {
                    result.errMessage = "Cannot find this user, please refresh the list.";
                    return result;
                }
                var rDelete = await _userManager.DeleteAsync(user);
                if (rDelete.Succeeded)
                {
                    result.success = true;
                }
                else
                {
                    foreach (var error in rDelete.Errors)
                    {
                        result.errMessage = result.errMessage + error.Description;
                    }
                }
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<UserDTO>> ResetUserPassword(UserDTO req)
        {
            ServiceResponseModel<UserDTO> result = new ServiceResponseModel<UserDTO>();

            try
            {
                // 1. checking Data
                if (req == null)
                {
                    result.errMessage = "Please insert Username.";
                    return result;
                }
                if (req.Password != req.ConfirmPassword)
                {
                    result.errMessage = "Password and Confirm Password are not match.";
                    return result;
                }

                // 2. save Data
                var user = await _userManager.FindByNameAsync(req.Username);
                if (user == null)
                {
                    result.errMessage = "Cannot find this user, please refresh the list.";
                    return result;
                }
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var param = new Dictionary<string, string>{
                    { "token", token },
                    { "username", req.Username}
                };
                var r = await _userManager.ResetPasswordAsync(user, token, req.Password);
                if (r.Succeeded)
                {
                    result.success = true;
                }
                else
                {
                    foreach (var error in r.Errors)
                    {
                        result.errMessage = result.errMessage + error.Description;
                    }
                }

            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<List<UserListDTO>>> GetUserAccessRightList()
        {
            ServiceResponseModel<List<UserListDTO>> result = new ServiceResponseModel<List<UserListDTO>>();

            try
            {
                var users = await _userManager.Users.ToListAsync();
                List<UserListDTO> listDTO = new List<UserListDTO>();
                foreach (var user in users)
                {
                    if (user.UserName == "calctronic@gmail.com") { continue; }

                    var uacIdList = _dbContext.UserAccessRight.Where(x => x.User_Id == user.Id).Select(x => x.UAC_Id).ToList();

                    listDTO.Add(new UserListDTO
                    {
                        Id = user.Id,
                        Username = user.UserName ?? "",
                        Fullname = user.FullName,
                        Email = user.Email ?? "",
                        IsActive = user.IsActive,
                        UACIdList = uacIdList,
                    });
                }
                result.success = true;
                result.data = listDTO;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<UserAccessRightReqDTO>> SaveUserAccessRight(UserAccessRightReqDTO req)
        {
            ServiceResponseModel<UserAccessRightReqDTO> result = new ServiceResponseModel<UserAccessRightReqDTO>();

            try
            {
                // 1. checking Data
                if (req == null)
                {
                    result.errMessage = "Please refresh list.";
                    return result;
                }

                // 2. save Data
                User? user = _dbContext.Users.Find(req.User_Id);
                if (user == null)
                {
                    result.errMessage = "Cannot find this user, please refresh the list.";
                    return result;
                }

                var existList = _dbContext.UserAccessRight.Where(x => x.User_Id == req.User_Id).ToList();

                // remove uac
                for (int i = existList.Count - 1; i >= 0; i--)
                {
                    var e = existList[i];
                    if (req.UACIdList.Contains(e.UAC_Id) == false)
                    {
                        _dbContext.UserAccessRight.Remove(e);
                    }
                }

                // add new uac
                foreach (int uacId in req.UACIdList)
                {
                    if (existList.Where(x => x.UAC_Id == uacId).Any() == false)
                    {
                        string enumName = Enum.GetName(typeof(EnumUAC), uacId) ?? "";
                        var uac = new UserAccessRight()
                        {
                            UAC = enumName,
                            UAC_Id = uacId,
                            User_Id = req.User_Id,
                        };
                        _dbContext.UserAccessRight.Add(uac);
                    }
                }
                await _dbContext.SaveChangesAsync();

                result.success = true;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

    }
}
