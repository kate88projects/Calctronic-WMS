using Microsoft.AspNetCore.Mvc;
using RackingSystem.Models.Item;
using RackingSystem.Models;
using RackingSystem.Data;
using RackingSystem.Services.SettingServices;
using RackingSystem.Models.Setting;
using RackingSystem.Services.ItemServices;
using RackingSystem.Data.Maintenances;
using Newtonsoft.Json;
using RackingSystem.Models.User;

namespace RackingSystem.Controllers
{
    public class SettingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ISettingService _setService;

        public SettingController(AppDbContext context, ISettingService setService)
        {
            _context = context;
            _setService = setService;
        }

        public IActionResult ReelDimensionList()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpSETTINGS";
            ViewData["ActiveTab"] = "ReelDimensionList";
            ViewData["Title"] = "Reel Dimension";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<ReelDimensionListDTO>>> GetReelDimensionList()
        {
            ServiceResponseModel<List<ReelDimensionListDTO>> result = await _setService.GetReelDimensionList();
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> SaveReelDimension([FromBody] ReelDimensionDTO itemReq)
        {
            ServiceResponseModel<ReelDimensionDTO> result = await _setService.SaveReelDimension(itemReq);
            return new JsonResult(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteReelDimension([FromBody] ReelDimensionDTO itemReq)
        {
            ServiceResponseModel<ReelDimensionDTO> result = await _setService.DeleteReelDimension(itemReq);
            return new JsonResult(result);
        }
        
        [HttpGet]
        public async Task<ServiceResponseModel<List<ReelDimensionListDTO>>> GetReelDimensionList_DDL()
        {
            ServiceResponseModel<List<ReelDimensionListDTO>> result = await _setService.GetReelDimensionList_DDL();
            return result;
        }

        public IActionResult SlotCalculationList()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpSETTINGS";
            ViewData["ActiveTab"] = "SlotCalculationList";
            ViewData["Title"] = "Slot Calculation";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<SlotCalculationListDTO>>> GetSlotCalculationList()
        {
            ServiceResponseModel<List<SlotCalculationListDTO>> result = await _setService.GetSlotCalculationList();
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> SaveSlotCalculation([FromBody] SlotCalculationDTO itemReq)
        {
            ServiceResponseModel<SlotCalculationDTO> result = await _setService.SaveSlotCalculation(itemReq);
            return new JsonResult(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSlotCalculation([FromBody] SlotCalculationDTO itemReq)
        {
            ServiceResponseModel<SlotCalculationDTO> result = await _setService.DeleteSlotCalculation(itemReq);
            return new JsonResult(result);
        }

        public IActionResult SlotColumnSetting()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpSETTINGS";
            ViewData["ActiveTab"] = "SlotColumnSetting";
            ViewData["Title"] = "Slot Column Setting";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<SlotColumnSettingDTO>>> GetSlotColumnSetting()
        {
            ServiceResponseModel<List<SlotColumnSettingDTO>> result = await _setService.GetSlotColumnSetting();
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> SaveSlotColumnSetting([FromBody] List<SlotColumnSettingDTO> req)
        {
            ServiceResponseModel<SlotColumnSettingDTO> result = await _setService.SaveSlotColumnSetting(req);
            return new JsonResult(result);
        }

        public IActionResult GlobalSetting()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpSETTINGS";
            ViewData["ActiveTab"] = "GlobalSetting";
            ViewData["Title"] = "Global Setting";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<GlobalSettingDTO>>> GetGlobalSettingList()
        {
            ServiceResponseModel<List<GlobalSettingDTO>> result = await _setService.GetGlobalSettingList();
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> SaveGlobalSetting([FromBody] GlobalSettingDTO req)
        {
            ServiceResponseModel<GlobalSettingDTO> result = await _setService.SaveGlobalSetting(req);
            return new JsonResult(result);
        }

    }
}
