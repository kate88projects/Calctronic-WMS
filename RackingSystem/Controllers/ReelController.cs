using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.Models;
using RackingSystem.Services.ReelServices;
using RackingSystem.Models.Reel;
using Newtonsoft.Json;
using RackingSystem.Models.User;

namespace RackingSystem.Controllers
{
    public class ReelController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IReelService _reelService;

        public ReelController(AppDbContext context, IReelService reelService)
        {
            _context = context;
            _reelService = reelService;
        }

        public IActionResult AvailableReelList()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpRACKING";
            ViewData["ActiveTab"] = "AvailableReelList";
            ViewData["Title"] = "Available Reel List";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<ReelListDTO>>> GetAvailableReelList()
        {
            ServiceResponseModel<List<ReelListDTO>> result = await _reelService.GetAvailableReelList();
            return result;
        }

    }
}
