using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RackingSystem.Models.User;

namespace RackingSystem.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Dashboard1()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "";
            ViewData["ActiveTab"] = "Dashboard1";
            ViewData["Title"] = "Dashboard";
            return View();
        }
    }
}
