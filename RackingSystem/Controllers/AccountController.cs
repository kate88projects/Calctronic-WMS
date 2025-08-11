using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using RackingSystem.Models.User;
using Microsoft.AspNetCore.Identity;
using RackingSystem.Data.Maintenances;
using RackingSystem.Services.AccountServices;

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
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //var result2 = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
            var result = await _service.Login(model);
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

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Wrong username or password.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
