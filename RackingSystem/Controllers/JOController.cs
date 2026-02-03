using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RackingSystem.Data;
using RackingSystem.Models;
using RackingSystem.Models.JO;
using RackingSystem.Services.JOServices;

namespace RackingSystem.Controllers
{
    [Authorize(AuthenticationSchemes = "MyAuthCookie")]
    public class JOController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IJOService _joService;

        public JOController(AppDbContext context, IJOService joService)
        {
            _context = context;
            _joService = joService;
        }

        public IActionResult JOList()
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

            ViewData["ActiveGroup"] = "grpJO";
            ViewData["ActiveTab"] = "JOList";
            ViewData["Title"] = "JO List";
            return View();
        }
        public IActionResult JODetails(int id, string mode)
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

            ViewData["ActiveGroup"] = "grpJO";
            ViewData["ActiveTab"] = "JODetails";
            ViewData["Title"] = "JO Details";
            ViewBag.Mode = mode;

            JOListDTO joData = new JOListDTO();

            if (id != 0)
            {
                var jo = GetJO(id);
                jo.Wait();
                var joHeader = jo.Result;
                joData = joHeader.data ?? new JOListDTO();
            }

            return View(joData);
        }

        //[HttpPost]
        //public async Task<ServiceResponseModel<List<JOListDTO>>> GetGRNDetailList([FromBody] JOSearchReqDTO req)
        //{
        //    if (req == null)
        //    {
        //        ServiceResponseModel<List<JOListDTO>> rErr = new ServiceResponseModel<List<JOListDTO>>();
        //        rErr.errMessage = "Empty parameter.";
        //        return rErr;
        //    }
        //    int ttl = -1;
        //    if (req.page == 1)
        //    {
        //        ServiceResponseModel<int> rTotal = await _joService.GetGRNDetailTotalCount(req);
        //        if (rTotal.success)
        //        {
        //            ttl = rTotal.data;
        //        }
        //        else
        //        {
        //            ServiceResponseModel<List<JOListDTO>> rErr = new ServiceResponseModel<List<JOListDTO>>();
        //            rErr.errMessage = rTotal.errMessage;
        //            rErr.errStackTrace = rTotal.errStackTrace;
        //            return rErr;
        //        }
        //    }
        //    ServiceResponseModel<List<GRNDtlListDTO>> result = await _joService.GetGRNDetailList(req);
        //    result.totalRecords = ttl;
        //    return result;
        //}

        [HttpGet]
        public async Task<ServiceResponseModel<JOListDTO>> GetJO(long Id)
        {
            ServiceResponseModel<JOListDTO> result = await _joService.GetJO(Id);
            return result;
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<JOListDTO>>> GetJOList() //[FromBody] JOSearchReqDTO req
        {
            ServiceResponseModel<List<JOListDTO>> result = await _joService.GetJOList();
            return result;
        }

        [HttpPost]
        public async Task<ServiceResponseModel<List<JODetailReqDTO>>> GetJODetail([FromBody] long jobId)
        {
            ServiceResponseModel<List<JODetailReqDTO>> result = await _joService.GetJODetail(jobId);
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> SaveJob([FromBody] JOReqDTO job)
        {
            ServiceResponseModel<JOReqDTO> result = await _joService.SaveJob(job);
            return new JsonResult(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteJob([FromBody] JOListDTO job)
        {
            ServiceResponseModel<JOListDTO> result = await _joService.DeleteJob(job);
            return new JsonResult(result);
        }


        public IActionResult JOEmergencyList()
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

            ViewData["ActiveGroup"] = "grpJO";
            ViewData["ActiveTab"] = "JOEmergencyList";
            ViewData["Title"] = "JO Emergency List";
            return View();
        }
        public IActionResult JOEmergencyDetails(int id, string mode)
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

            ViewData["ActiveGroup"] = "grpJO";
            ViewData["ActiveTab"] = "JOEmergencyDetails";
            ViewData["Title"] = "JO Emergency Details";
            ViewBag.Mode = mode;

            JOEmergencyReqDTO joEData = new JOEmergencyReqDTO();

            if (id != 0)
            {
                var joE = GetJOEmergency(id);
                joE.Wait();
                var joEHeader = joE.Result;
                joEData = joEHeader.data ?? new JOEmergencyReqDTO();
            }

            return View(joEData);
        }

        [HttpGet]
        public async Task<ServiceResponseModel<JOEmergencyReqDTO>> GetJOEmergency(long Id)
        {
            ServiceResponseModel<JOEmergencyReqDTO> result = await _joService.GetJOEmergency(Id);
            return result;
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<JOEmergencyReqDTO>>> GetJOEmergencyList()
        {
            ServiceResponseModel<List<JOEmergencyReqDTO>> result = await _joService.GetJOEmergencyList();
            return result;
        }

        [HttpPost]
        public async Task<ServiceResponseModel<List<JOEmergencyDetailReqDTO>>> GetJOEmergencyDetail([FromBody] long joEId)
        {
            ServiceResponseModel<List<JOEmergencyDetailReqDTO>> result = await _joService.GetJOEmergencyDetail(joEId);
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> SaveEmergency([FromBody] JOEmergencyReqDTO jobEmergency)
        {
            ServiceResponseModel<JOEmergencyReqDTO> result = await _joService.SaveEmergency(jobEmergency);
            return new JsonResult(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteEmergency([FromBody] JOEmergencyReqDTO jobEmergency)
        {
            ServiceResponseModel<JOEmergencyReqDTO> result = await _joService.DeleteEmergency(jobEmergency);
            return new JsonResult(result);
        }

        
    }
}
