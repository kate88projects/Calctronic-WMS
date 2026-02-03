using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RackingSystem.Data.Maintenances;
using RackingSystem.Models;
using RackingSystem.Models.User;
using RackingSystem.Services.AccountServices;
using System.Security.Claims;

namespace RackingSystem.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IAccountService _service;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(IAccountService service, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _service = service;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public string getSession()
        {
            //string s = HttpContext.Session.GetString("xSession") ?? "";
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var uacIds = User.FindFirst("UACIdList")?.Value;
                return uacIds;
            }

            return "";
        }

        public IActionResult Login()
        {
            //ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            //if (s != "")
            //{
            //    return RedirectToAction("Index", "Home");
            //}
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var uacClaim = User.FindFirst("UACIdList")?.Value;
                if (uacClaim != null)
                {
                    List<int> uacIdList = uacClaim.Split(',').Select(int.Parse).ToList();
                    ViewBag.PermissionList = uacIdList;
                }
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
            try
            {
                //var result2 = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
                var result = await _service.Login(req);
                //if (result.success)
                //{
                //    string json = JsonConvert.SerializeObject(result.data);
                //    HttpContext.Session.SetString("xSession", json);
                //}

                //return new JsonResult(result);

                if (!result.success)
                    return Json(result);

                var claims = result.data.authClaims;
                var identity = new ClaimsIdentity(
                    claims,
                    "MyAuthCookie"
                );

                await HttpContext.SignInAsync(
                    "MyAuthCookie",
                    new ClaimsPrincipal(identity)
                );
                var principal = new ClaimsPrincipal(identity);
                var uacIds = principal.FindFirst("UACIdList")?.Value;

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                ServiceResponseModel<string> r = new ServiceResponseModel<string>();
                r.errMessage = ex.Message;
                r.errStackTrace = ex.StackTrace;
                return new JsonResult(r);
            }
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        public IActionResult UserList()
        {
            ViewBag.PermissionList = new List<int>();
            //string s = HttpContext.Session.GetString("xSession") ?? "";
            //if (s != "")
            //{
            //    UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
            //    ViewBag.PermissionList = data.UACIdList;
            //}
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var uacClaim = User.FindFirst("UACIdList")?.Value;
                if (uacClaim != null)
                {
                    List<int> uacIdList = uacClaim.Split(',').Select(int.Parse).ToList();
                    ViewBag.PermissionList = uacIdList;
                }
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
            //string s = HttpContext.Session.GetString("xSession") ?? "";
            //if (s != "")
            //{
            //    UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
            //    var uacIds = User.FindFirst("UACIdList")?.Value;

            //    ViewBag.PermissionList = data.UACIdList;
            //}
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var uacClaim = User.FindFirst("UACIdList")?.Value;
                if (uacClaim != null)
                {
                    List<int> uacIdList = uacClaim.Split(',').Select(int.Parse).ToList();
                    ViewBag.PermissionList = uacIdList;
                }
            }

            ViewData["ActiveGroup"] = "grpSETTINGS";
            ViewData["ActiveTab"] = "UserAccessRightList";
            ViewData["Title"] = "User Access Right List";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<UserListDTO>>> GetUserAccessRightList()
        {
            ServiceResponseModel<List<UserListDTO>> result = await _service.GetUserAccessRightList();
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> SaveUserAccessRight([FromBody] UserAccessRightReqDTO itemReq)
        {
            ServiceResponseModel<UserAccessRightReqDTO> result = await _service.SaveUserAccessRight(itemReq);
            return new JsonResult(result);
        }


    }
}
