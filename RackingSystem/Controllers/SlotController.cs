using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RackingSystem.Data;
using RackingSystem.Models;
using RackingSystem.Models.Slot;
using RackingSystem.Models.User;
using RackingSystem.Services.SlotServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RackingSystem.Controllers
{
    [Authorize(AuthenticationSchemes = "MyAuthCookie")]
    public class SlotController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ISlotService _slotService;

        public SlotController(AppDbContext context, ISlotService slotService)
        {
            _context = context;
            _slotService = slotService;
        }

        public IActionResult SlotList()
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

            ViewData["ActiveGroup"] = "grpMM";
            ViewData["ActiveTab"] = "SlotList";
            ViewData["Title"] = "Slot Module List";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<SlotDTO>> GetSlot(long id)
        {
            ServiceResponseModel<SlotDTO> result = await _slotService.GetSlot(id);
            return result;
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<SlotListDTO>>> GetSlotList()
        {
            ServiceResponseModel<List<SlotListDTO>> result = await _slotService.GetSlotList();
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> SaveSlot([FromBody] SlotDTO slotReq)
        {
            ServiceResponseModel<SlotDTO> result = await _slotService.SaveSlot(slotReq);
            return new JsonResult(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSlot([FromBody] SlotDTO slotReq)
        {
            ServiceResponseModel<SlotDTO> result = await _slotService.DeleteSlot(slotReq);
            return new JsonResult(result);
        }

        public IActionResult SlotStatus()
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
            ViewData["ActiveTab"] = "SlotStatus";
            ViewData["Title"] = "Slot Status";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<SlotListDTO>>> GetSlotStatus_ByColumn(string req)
        {
            if (req == null)
            {
                ServiceResponseModel<List<SlotListDTO>> rErr = new ServiceResponseModel<List<SlotListDTO>>();
                rErr.errMessage = "Please insert column.";
                return rErr;
            }
            int colNo = 0;
            bool isNum = int.TryParse(req, out colNo);
            ServiceResponseModel<List<SlotListDTO>> result = await _slotService.GetSlotStatus_ByColumn(colNo);

            int ttlIn = _context.Slot.Where(x => x.ColNo == colNo && x.IsActive == false).Count();
            int ttlC = _context.Slot.Where(x => x.ColNo == colNo && x.NeedCheck == true).Count();
            int ttlET = _context.Slot.Where(x => x.ColNo == colNo && x.HasEmptyTray == true).Count();
            int ttlTR = _context.Slot.Where(x => x.ColNo == colNo && x.HasReel && x.ReelNo == "0").Count();
            int ttlTRN = _context.Slot.Where(x => x.ColNo == colNo && x.HasReel && x.ReelNo != "0").Count();
            int ttlA = _context.Slot.Where(x => x.ColNo == colNo && x.HasReel == false && x.HasEmptyTray == false && x.IsActive == true && x.NeedCheck == false).Count();

            result.errMessage = ttlIn + "," + ttlC + "," + ttlET + "," + ttlTR + "," + ttlTRN + "," + ttlA;

            return result;
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> SaveExcelSlot([FromBody] List<SlotListDTO> slots)
        {
            ServiceResponseModel<List<SlotListDTO>> result = await _slotService.SaveExcelSlot(slots);

            return new JsonResult(result);

        }

        [HttpPost]
        public async Task<IActionResult> SaveRangeOfSlot([FromBody] SlotRangeDTO slotRanges)
        {
            ServiceResponseModel<SlotRangeDTO> result = await _slotService.SaveRangeOfSlot(slotRanges);
            return new JsonResult(result);
        }

        public IActionResult SlotSimulation()
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
            ViewData["ActiveTab"] = "SlotSimulation";
            ViewData["Title"] = "Slot Simulation";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetFreeSlot_ByColumn_ASC([FromBody] SlotFreeReqDTO slotReq)
        {
            ServiceResponseModel<SlotFreeDTO> result = await _slotService.GetFreeSlot_ByColumn_ASC(slotReq);
            return new JsonResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetFreeSlot_ByColumn_DESC([FromBody] SlotFreeReqDTO slotReq)
        {
            ServiceResponseModel<SlotFreeDTO> result = await _slotService.GetFreeSlot_ByColumn_DESC(slotReq);
            return new JsonResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSlotStatus([FromBody] SlotStatusReqDTO slotReq)
        {
            ServiceResponseModel<SlotDTO> result = await _slotService.UpdateSlotStatus(slotReq);
            return new JsonResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateExcelPulses([FromBody] List<SlotListDTO> slotPulses)
        {
            ServiceResponseModel<List<SlotListDTO>> result = await _slotService.UpdateExcelPulses(slotPulses);
            return new JsonResult(result);
        }

        public IActionResult SlotDrawer()
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

            ViewData["ActiveGroup"] = "grpMM";
            ViewData["ActiveTab"] = "SlotDrawer";
            ViewData["Title"] = "Empty Slot Drawer";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetFreeSlot_Drawer_ByColumn()
        {
            ServiceResponseModel<List<Slot_DrawerFreeDTO>> result = await _slotService.GetFreeSlot_Drawer_ByColumn();
            return new JsonResult(result);
        }

        public IActionResult SlotUpdatePulse()
        {
            ViewBag.xToken = "";
            ViewBag.PermissionList = new List<int>();
            //string s = HttpContext.Session.GetString("xSession") ?? "";
            //if (s != "")
            //{
            //    UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
            //    ViewBag.PermissionList = data.UACIdList;
            //    ViewBag.xToken = data.Token;
            //}
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var uacClaim = User.FindFirst("UACIdList")?.Value;
                var token = User.FindFirst("Token")?.Value;

                if (uacClaim != null)
                {
                    List<int> uacIdList = uacClaim.Split(',').Select(int.Parse).ToList();
                    ViewBag.PermissionList = uacIdList;
                    ViewBag.xToken = token;
                }
            }

            ViewData["ActiveGroup"] = "grpMM";
            ViewData["ActiveTab"] = "SlotUpdatePulse";
            ViewData["Title"] = "Slot Update Pulses";
            return View();
        }

        public IActionResult SlotColumnStatus()
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
            ViewData["ActiveTab"] = "SlotColumnStatus";
            ViewData["Title"] = "Slot Column Status";
            return View();
        }

        public IActionResult SlotDetailList()
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
            ViewData["ActiveTab"] = "SlotDetailList";
            ViewData["Title"] = "Slot Detail List";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<SlotUsageDTO>>> GetSlotUsage()
        {
            ServiceResponseModel<List<SlotUsageDTO>> result = await _slotService.GetSlotUsage();
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateWarningSlot([FromBody] SlotStatusReqDTO slotReq)
        {
            ServiceResponseModel<SlotStatusReqDTO> result = await _slotService.UpdateWarningSlot(slotReq);
            return new JsonResult(result);
        }

    }
}
