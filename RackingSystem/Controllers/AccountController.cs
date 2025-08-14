using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using RackingSystem.Models.User;
using Microsoft.AspNetCore.Identity;
using RackingSystem.Data.Maintenances;
using RackingSystem.Services.AccountServices;
using RackingSystem.Models.Setting;
using RackingSystem.Models;
using RackingSystem.Models.Loader;

namespace RackingSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _service;

        public AccountController(IAccountService service)
        {
            _service = service;
        }

        [HttpGet]
        public string getSession()
        {
            string s = HttpContext.Session.GetString("xSession") ?? "";
            return s;
        }

        public IActionResult Login()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginAccount([FromBody] LoginDTO req)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View(model);
            //}

            //var result2 = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
            var result = await _service.Login(req);
            if (result.success)
            {
                string json = JsonConvert.SerializeObject(result.data);
                HttpContext.Session.SetString("xSession", json);

                //var claims = new List<Claim>
                //    {
                //        new Claim(ClaimTypes.Name, result.data.Fullname)
                //    };
                //var claimsIdn = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                //await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdn));
                //HttpContext.User = new ClaimsPrincipal(claimsIdn);
            }

            return new JsonResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        public IActionResult UserList()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpSETTINGS";
            ViewData["ActiveTab"] = "UserList";
            ViewData["Title"] = "User List";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<UserListDTO>>> GetUserList()
        {
            ServiceResponseModel<List<UserListDTO>> result = await _service.GetUserList();
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> SaveUser([FromBody] UserDTO itemReq)
        {
            ServiceResponseModel<UserDTO> result = await _service.SaveUser(itemReq);
            return new JsonResult(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser([FromBody] UserDTO itemReq)
        {
            ServiceResponseModel<UserDTO> result = await _service.DeleteUser(itemReq);
            return new JsonResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> ResetUserPassword([FromBody] UserDTO itemReq)
        {
            ServiceResponseModel<UserDTO> result = await _service.ResetUserPassword(itemReq);
            return new JsonResult(result);
        }

        public IActionResult UserAccessRightList()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpSETTINGS";
            ViewData["ActiveTab"] = "UserAccessRightList";
            ViewData["Title"] = "User Access Right List";
            return View();
        }


    }
}
