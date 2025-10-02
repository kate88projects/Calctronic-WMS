using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RackingSystem.Data;
using RackingSystem.Models.User;
using RackingSystem.Services.GRNServices;
using RackingSystem.Services.RackServices;

namespace RackingSystem.Controllers
{
    public class RackController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IRackService _rackService;

        public RackController(AppDbContext context, IRackService rackService)
        {
            _context = context;
            _rackService = rackService;
        }

        public IActionResult RackJob()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpRACKING";
            ViewData["ActiveTab"] = "RackJob";
            ViewData["Title"] = "Rack Job";
            return View();
        }


        public IActionResult NewTransferHubInTask()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpRACKING";
            ViewData["ActiveTab"] = "NewTransferHubInTask";
            ViewData["Title"] = "New Hub In Task";
            return View();
        }
    }
}
