using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.General;
using RackingSystem.Models;
using RackingSystem.Models.Setting;
using RackingSystem.Services.RackJobQueueServices;
using RackingSystem.Services.SettingServices;

namespace RackingSystem.Controllers
{
    [Authorize(AuthenticationSchemes = "MyAuthCookie")]
    public class SettingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ISettingService _setService;
        private readonly IRackJobQueueService _qService;
        private readonly IConfiguration _config;

        public SettingController(AppDbContext context, ISettingService setService, IRackJobQueueService qService, IConfiguration config)
        {
            _context = context;
            _setService = setService;
            _qService = qService;
            _config = config;
        }

        public IActionResult ReelDimensionList()
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

        [HttpGet]
        public async Task<ServiceResponseModel<List<int>>> GetReelWidthList()
        {
            ServiceResponseModel<List<int>> result = await _setService.GetReelWidthList();
            return result;
        }

        public IActionResult SlotCalculationList()
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

        public IActionResult SystemTools()
        {
            ViewBag.PermissionList = new List<int>();
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var uacClaim = User.FindFirst("UACIdList")?.Value;

                if (uacClaim != null)
                {
                    List<int> uacIdList = uacClaim.Split(',').Select(int.Parse).ToList();
                    ViewBag.PermissionList = uacIdList;
                }
            }

            ViewBag.IsTestingMode = AppModeHelper.IsTesting(_config);
            ViewData["ActiveGroup"] = "grpSETTINGS";
            ViewData["ActiveTab"] = "SystemTools";
            ViewData["Title"] = "System Tools";
            return View();
        }

        private bool HasSystemToolsAccess()
        {
            var uacClaim = User.FindFirst("UACIdList")?.Value ?? "";
            return uacClaim.Split(',').Contains(((int)EnumUAC.SETTINGS_SystemTools_MENU).ToString());
        }

        [HttpPost]
        public async Task<IActionResult> InitializeTestingDB()
        {
            ServiceResponseModel<bool> result = new ServiceResponseModel<bool>();
            if (!HasSystemToolsAccess())
            {
                return new JsonResult(result);
            }
            if (!AppModeHelper.IsTesting(_config))
            {
                result.errMessage = "Initialize Testing DB is only allowed in Testing mode.";
                return new JsonResult(result);
            }

            try
            {
                await _context.Database.ExecuteSqlRawAsync($"EXEC {GeneralStatic.SP_Reset_SRMSData}");

                result.success = true;
                result.data = true;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return new JsonResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> ClearRackJobCache()
        {
            if (!HasSystemToolsAccess())
            {
                return new JsonResult(new ServiceResponseModel<bool>());
            }

            ServiceResponseModel<bool> result = await _qService.ClearRackJobInfo();
            return new JsonResult(result);
        }

    }
}
