using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.Models;
using RackingSystem.Services.ReelServices;
using RackingSystem.Models.Reel;

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

        public IActionResult ReelList()
        {
            ViewData["Title"] = "Reel List";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<ReelListDTO>>> GetReelist()
        {
            ServiceResponseModel<List<ReelListDTO>> result = await _reelService.GetReelList();
            return result;
        }

    }
}
