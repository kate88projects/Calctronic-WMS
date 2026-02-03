using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RackingSystem.Data;
using RackingSystem.Models;
using RackingSystem.Models.GRN;
using RackingSystem.Models.Reel;
using RackingSystem.Models.User;
using RackingSystem.Services.ReelServices;

namespace RackingSystem.Controllers
{
    [Authorize(AuthenticationSchemes = "MyAuthCookie")]
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

            ViewData["ActiveGroup"] = "grpRACKING";
            ViewData["ActiveTab"] = "AvailableReelList";
            ViewData["Title"] = "Available Reel List";
            return View();
        }

        [HttpPost]
        public async Task<ServiceResponseModel<ReelAvailableResponseDTO>> GetAvailableReelList([FromBody] ReelAvailableSearchReqDTO req)
        {
            if (req == null)
            {
                ServiceResponseModel<ReelAvailableResponseDTO> rErr = new ServiceResponseModel<ReelAvailableResponseDTO>();
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
                    ServiceResponseModel<ReelAvailableResponseDTO> rErr = new ServiceResponseModel<ReelAvailableResponseDTO>();
                    rErr.errMessage = rTotal.errMessage;
                    rErr.errStackTrace = rTotal.errStackTrace;
                    return rErr;
                }
            }

            ServiceResponseModel<List<ReelAvailableListDTO>> r = await _reelService.GetAvailableReelList(req);
            ServiceResponseModel<ReelAvailableResponseDTO> result = new ServiceResponseModel<ReelAvailableResponseDTO>();
            result.success = true;
            result.totalRecords = ttl;
            result.data = new ReelAvailableResponseDTO();
            result.data.ReelList = r.data;
            result.data.TotalWaiting = ttlW;
            result.data.TotalInLoader = ttlL;
            result.data.TotalSRMS = ttlSRMS;
            result.data.TotalInTrolley = ttlT;

            return result;
        }

    }
}
