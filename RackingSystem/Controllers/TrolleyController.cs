using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RackingSystem.Data;
using RackingSystem.Models;
using RackingSystem.Models.Trolley;
using RackingSystem.Models.User;
using RackingSystem.Services.TrolleyServices;
using System.Security.Policy;

namespace RackingSystem.Controllers
{
    public class TrolleyController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ITrolleyService _trolleyService;

        public TrolleyController(AppDbContext context, ITrolleyService trolleyService)
        {
            _context = context;
            _trolleyService = trolleyService;
        }

        public IActionResult TrolleyList()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpMM";
            ViewData["ActiveTab"] = "TrolleyList";
            ViewData["Title"] = "Trolley List";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<TrolleyListDTO>>> GetTrolleyList()
        {
            ServiceResponseModel<List<TrolleyListDTO>> result = await _trolleyService.GetTrolleyList();
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> SaveTrolley([FromBody] TrolleyListDTO trolley)
        {
            ServiceResponseModel<TrolleyListDTO> result = await _trolleyService.SaveTrolley(trolley);
            return new JsonResult(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTrolley([FromBody] TrolleyListDTO trolley)
        {
            ServiceResponseModel<TrolleyListDTO> result = await _trolleyService.DeleteTrolley(trolley);
            return new JsonResult(result);
        }

        //Trolley slot
        public IActionResult TrolleySlot()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpMM";
            ViewData["ActiveTab"] = "TrolleySlot";
            ViewData["Title"] = "Trolley Slot";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<TrolleySlotDTO>>> GetTrolleySlotList()
        {
            ServiceResponseModel<List<TrolleySlotDTO>> result = await _trolleyService.GetTrolleySlotList();
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> SaveTrolleySlot([FromBody] TrolleySlotDTO trolleySlot)
        {
            ServiceResponseModel<TrolleySlotDTO> result = await _trolleyService.SaveTrolleySlot(trolleySlot);
            return new JsonResult(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTrolleySlot([FromBody] TrolleySlotDTO trolleySlot)
        {
            ServiceResponseModel<TrolleySlotDTO> result = await _trolleyService.DeleteTrolleySlot(trolleySlot);
            return new JsonResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> SaveRangeOfTrolleySlot([FromBody] TrolleySlotRangeDTO tsRanges)
        {
            ServiceResponseModel<TrolleySlotRangeDTO> result = await _trolleyService.SaveRangeOfTrolleySlot(tsRanges);
            return new JsonResult(result);
        }

        public async Task<IActionResult> SaveExcelTrolleySlot([FromBody] List<TrolleySlotDTO> trolleySlot)
        {
            ServiceResponseModel<List<TrolleySlotDTO>> result = await _trolleyService.SaveExcelTrolleySlot(trolleySlot);
            return new JsonResult(result);
        }

        public async Task<IActionResult> UpdateExcelTSPulses([FromBody] List<TrolleySlotDTO> tsPulses)
        {
            ServiceResponseModel<List<TrolleySlotDTO>> result = await _trolleyService.UpdateExcelTSPulses(tsPulses);
            return new JsonResult(result);
        }
    }
}

