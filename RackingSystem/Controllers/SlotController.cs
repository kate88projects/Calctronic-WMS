using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.Models.Slot;
using RackingSystem.Models;
using RackingSystem.Services.SlotServices;
using Newtonsoft.Json;
using RackingSystem.Models.User;

namespace RackingSystem.Controllers
{
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
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpMM";
            ViewData["ActiveTab"] = "SlotList";
            ViewData["Title"] = "Slot List";
            return View();
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
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
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
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
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
    }
}
