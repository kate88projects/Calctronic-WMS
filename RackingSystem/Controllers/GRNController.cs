using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RackingSystem.Data;
using RackingSystem.Models;
using RackingSystem.Models.GRN;
using RackingSystem.Services.GRNServices;

namespace RackingSystem.Controllers
{
    [Authorize(AuthenticationSchemes = "MyAuthCookie")]
    public class GRNController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IGRNService _grnService;

        public GRNController(AppDbContext context, IGRNService grnService)
        {
            _context = context;
            _grnService = grnService;
        }

        public IActionResult NewGRN()
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

            ViewData["ActiveGroup"] = "grpGRN";
            ViewData["ActiveTab"] = "NewGRN";
            ViewData["Title"] = "New GRN";
            return View();
        }

        [HttpPost]
        public async Task<ServiceResponseModel<GRNDtlDTO>> SaveGRNDtl([FromBody] GRNDtlReqDTO req)
        {
            if (req == null)
            {
                ServiceResponseModel<GRNDtlDTO> rErr = new ServiceResponseModel<GRNDtlDTO>();
                rErr.errMessage = "Please select item.";
                return rErr;
            }
            ServiceResponseModel<GRNDtlDTO> result = await _grnService.SaveGRNDtl(req);
            return result;
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteGRNDtl([FromBody] GRNDtlDTO req)
        {
            ServiceResponseModel<GRNDtlDTO> result = await _grnService.DeleteGRNDtl(req);
            return new JsonResult(result);
        }

        public IActionResult GRNDetailList()
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

            ViewData["ActiveGroup"] = "grpGRN";
            ViewData["ActiveTab"] = "GRNDetailList";
            ViewData["Title"] = "GRN List";
            return View();
        }

        [HttpPost]
        public async Task<ServiceResponseModel<List<GRNDtlListDTO>>> GetGRNDetailList([FromBody] GRNSearchReqDTO req)
        {
            if (req == null)
            {
                ServiceResponseModel<List<GRNDtlListDTO>> rErr = new ServiceResponseModel<List<GRNDtlListDTO>>();
                rErr.errMessage = "Empty parameter.";
                return rErr;
            }
            int ttl = -1;
            if (req.page == 1)
            {
                ServiceResponseModel<int> rTotal = await _grnService.GetGRNDetailTotalCount(req);
                if (rTotal.success)
                {
                    ttl = rTotal.data;
                }
                else
                {
                    ServiceResponseModel<List<GRNDtlListDTO>> rErr = new ServiceResponseModel<List<GRNDtlListDTO>>();
                    rErr.errMessage = rTotal.errMessage;
                    rErr.errStackTrace = rTotal.errStackTrace;
                    return rErr;
                }
            }
            ServiceResponseModel<List<GRNDtlListDTO>> result = await _grnService.GetGRNDetailList(req);
            result.totalRecords = ttl;
            return result;
        }

        public IActionResult NewAutoLoaderTask()
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
                if (uacClaim != null)
                {
                    List<int> uacIdList = uacClaim.Split(',').Select(int.Parse).ToList();
                    ViewBag.PermissionList = uacIdList;
                    ViewBag.xToken = User.FindFirst("Token")?.Value;
                }
            }

            ViewData["ActiveGroup"] = "grpGRN";
            ViewData["ActiveTab"] = "NewAutoLoaderTask";
            ViewData["Title"] = "New Auto Loader Task";
            return View();
        }

        //public IActionResult NewAutoLoaderTask2()
        //{
        //    ViewBag.PermissionList = new List<int>();
        //    string s = HttpContext.Session.GetString("xSession") ?? "";
        //    if (s != "")
        //    {
        //        UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
        //        ViewBag.PermissionList = data.UACIdList;
        //    }

        //    ViewData["ActiveGroup"] = "grpGRN";
        //    ViewData["ActiveTab"] = "NewAutoLoaderTask";
        //    ViewData["Title"] = "New Auto Loader Task";
        //    return View();
        //}

        [HttpPost]
        public async Task<ServiceResponseModel<GRNDtlDTO>> GetLatestGRNDetail([FromBody] Guid detailId)
        {
            ServiceResponseModel<GRNDtlDTO> result = await _grnService.GetLatestGRNDetail(detailId);
            return result;
        }

        public IActionResult LoadInTaskManual()
        {
            ViewBag.PermisssionList = new List<string>();
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

            ViewData["ActiveGroup"] = "grpGRN";
            ViewData["ActiveTab"] = "LoadInTaskManual";
            ViewData["Title"] = "Load In Task";
            return View();
        }
    }
}
