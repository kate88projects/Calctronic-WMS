using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RackingSystem.Models;

namespace RackingSystem.Controllers
{
    [Authorize(AuthenticationSchemes = "MyAuthCookie")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
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

            ViewData["ActiveGroup"] = "Home";
            ViewData["ActiveTab"] = "Index";
            ViewData["Title"] = "Index";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
