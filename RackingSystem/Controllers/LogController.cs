using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RackingSystem.Data;
using RackingSystem.Models.Item;
using RackingSystem.Models;
using RackingSystem.Models.User;
using RackingSystem.Services.ItemServices;
using RackingSystem.Data.Log;
using Microsoft.EntityFrameworkCore;

namespace RackingSystem.Controllers
{
    public class LogController : Controller
    {
        private readonly AppDbContext _context;

        public LogController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult HubInLogView()
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
            ViewData["ActiveTab"] = "HubInLogView";
            ViewData["Title"] = "Hub In Log";
            return View();
        }

        [HttpPost]
        public async Task<ServiceResponseModel<List<PLCHubInLog>>> GetHubInLog([FromBody] ItemSearchReqDTO req)
        {
            if (req == null)
            {
                ServiceResponseModel<List<PLCHubInLog>> rErr = new ServiceResponseModel<List<PLCHubInLog>>();
                rErr.errMessage = "Empty parameter.";
                return rErr;
            }
            int ttl = -1;
            if (req.page == 1)
            {
                ttl = _context.PLCHubInLog.Count();
            }
            ServiceResponseModel<List<PLCHubInLog>> result = new ServiceResponseModel<List<PLCHubInLog>>();
            result.success = true;
            result.data = await _context.PLCHubInLog.OrderByDescending(x => x.CreatedDate).Skip((req.page - 1) * req.pageSize).Take(req.pageSize).ToListAsync();
            result.totalRecords = ttl;
            return result;
        }

        public IActionResult HubOutLogView()
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
            ViewData["ActiveTab"] = "HubOutLogView";
            ViewData["Title"] = "Hub Out Log";
            return View();
        }

        [HttpPost]
        public async Task<ServiceResponseModel<List<PLCHubOutLog>>> GetHubOutLog([FromBody] ItemSearchReqDTO req)
        {
            if (req == null)
            {
                ServiceResponseModel<List<PLCHubOutLog>> rErr = new ServiceResponseModel<List<PLCHubOutLog>>();
                rErr.errMessage = "Empty parameter.";
                return rErr;
            }
            int ttl = -1;
            if (req.page == 1)
            {
                ttl = _context.PLCHubOutLog.Count();
            }
            ServiceResponseModel<List<PLCHubOutLog>> result = new ServiceResponseModel<List<PLCHubOutLog>>();
            result.success = true;
            result.data = await _context.PLCHubOutLog.OrderByDescending(x => x.CreatedDate).Skip((req.page - 1) * req.pageSize).Take(req.pageSize).ToListAsync();
            result.totalRecords = ttl;
            return result;
        }

        public IActionResult DrawerInView()
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
            ViewData["ActiveTab"] = "DrawerInView";
            ViewData["Title"] = "Drawer In Log";
            return View();
        }

        [HttpPost]
        public async Task<ServiceResponseModel<List<PLCTrolleyLog>>> GetDrawerInLog([FromBody] ItemSearchReqDTO req)
        {
            if (req == null)
            {
                ServiceResponseModel<List<PLCTrolleyLog>> rErr = new ServiceResponseModel<List<PLCTrolleyLog>>();
                rErr.errMessage = "Empty parameter.";
                return rErr;
            }
            int ttl = -1;
            if (req.page == 1)
            {
                ttl = _context.PLCTrolleyLog.Count();
            }
            ServiceResponseModel<List<PLCTrolleyLog>> result = new ServiceResponseModel<List<PLCTrolleyLog>>();
            result.success = true;
            result.data = await _context.PLCTrolleyLog.OrderByDescending(x => x.CreatedDate).Skip((req.page - 1) * req.pageSize).Take(req.pageSize).ToListAsync();
            result.totalRecords = ttl;
            return result;
        }


    }
}
