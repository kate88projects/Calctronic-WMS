using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RackingSystem.Controllers
{
    [Authorize(AuthenticationSchemes = "MyAuthCookie")]
    public class DashboardController : Controller
    {
        public IActionResult Dashboard()
        {
            ViewBag.PermissionList = new List<int>();
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var uacClaim = User.FindFirst("UACIdList")?.Value;
                if (uacClaim != null)
                {
                    List<int> uacIdList = uacClaim.Split(',').Select(int.Parse).ToList();
                    ViewBag.PermissionList = uacIdList;
                }
            }

            ViewData["ActiveGroup"] = "Dashboard";
            ViewData["ActiveTab"] = "Dashboard";
            ViewData["Title"] = "Dashboard";
            return View();
        }
    }
}
