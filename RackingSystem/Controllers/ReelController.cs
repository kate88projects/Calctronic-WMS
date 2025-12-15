using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.Models;
using RackingSystem.Services.ReelServices;
using RackingSystem.Models.Reel;
using Newtonsoft.Json;
using RackingSystem.Models.User;
using RackingSystem.Models.GRN;

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

        [HttpPost]
        public async Task<ServiceResponseModel<List<ReelAvailableListDTO>>> GetAvailableReelList([FromBody] ReelAvailableSearchReqDTO req)
        {
            if (req == null)
            {
                ServiceResponseModel<List<ReelAvailableListDTO>> rErr = new ServiceResponseModel<List<ReelAvailableListDTO>>();
                rErr.errMessage = "Empty parameter.";
                return rErr;
            }
            int ttl = -1;
            int ttlW = 0;
            int ttlL = 0;
            int ttlSRMS = 0;
            int ttlT = 0;
            if (req.page == 1)
            {
                ServiceResponseModel<ReelAvailableListDTO> rTotal = await _reelService.GetAvailableReelTotalCount(req);
                if (rTotal.success)
                {
                    ttl = rTotal.data.totalRecord;
                    ttlW = rTotal.data.TotalWaiting;
                    ttlL = rTotal.data.TotalInLoader;
                    ttlSRMS = rTotal.data.TotalSRMS;
                    ttlT = rTotal.data.TotalInTrolley;
                }
                else
                {
                    ServiceResponseModel<List<ReelAvailableListDTO>> rErr = new ServiceResponseModel<List<ReelAvailableListDTO>>();
                    rErr.errMessage = rTotal.errMessage;
                    rErr.errStackTrace = rTotal.errStackTrace;
                    return rErr;
                }
            }
            ServiceResponseModel<List<ReelAvailableListDTO>> result = await _reelService.GetAvailableReelList(req);
            result.totalRecords = ttl;
            result.data[0].TotalWaiting = ttlW;
            result.data[0].TotalInLoader = ttlL;
            result.data[0].TotalSRMS = ttlSRMS;
            result.data[0].TotalInTrolley = ttlT;
            return result;
        }

    }
}
