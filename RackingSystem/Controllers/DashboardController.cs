using Microsoft.AspNetCore.Mvc;

namespace RackingSystem.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Dashboard1()
        {
            ViewData["ActiveTab"] = "Dashboard1";
            ViewData["Title"] = "Dashboard";
            return View();
        }
    }
}
