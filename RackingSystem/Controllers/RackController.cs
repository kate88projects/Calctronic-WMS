using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RackingSystem.Data;
using RackingSystem.Data.RackJob;
using RackingSystem.General;
using RackingSystem.Helpers;
using RackingSystem.Models;
using RackingSystem.Models.RackJob;
using RackingSystem.Services.RackServices;

namespace RackingSystem.Controllers
{
    [Authorize(AuthenticationSchemes = "MyAuthCookie")]
    public class RackController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IRackService _rackService;

        public RackController(AppDbContext context, IRackService rackService)
        {
            _context = context;
            _rackService = rackService;
        }

        public IActionResult RackJobHubIn(long qId)
        {
            ViewBag.QContinuos = "0";
            ViewBag.QId = qId;
            ViewBag.QNo = "";
            ViewBag.xToken = "";
            ViewBag.DeviceId = "";
            ViewBag.PermissionList = new List<int>();

            if (User.Identity?.IsAuthenticated ?? false)
            {
                var uacClaim = User.FindFirst("UACIdList")?.Value;
                if (uacClaim != null)
                {
                    List<int> uacIdList = uacClaim.Split(',').Select(int.Parse).ToList();
                    ViewBag.PermissionList = uacIdList;
                    ViewBag.xToken = User.FindFirst("Token")?.Value;
                    ViewBag.DeviceId = User.FindFirst("DeviceId")?.Value;

                    var q = _context.RackJobQueue.Where(x => x.RackJobQueue_Id == qId).FirstOrDefault();
                    if (q != null)
                    {
                        if (q.DocType == EnumQueueDocType.Loader.ToString())
                        {
                            var doc = _context.Loader.Where(x => x.Loader_Id == q.Doc_Id).FirstOrDefault();
                            if (doc != null)
                            {
                                ViewBag.QNo = "HubIn - " + doc.Description;
                            }
                        }
                        else if (q.DocType == EnumQueueDocType.JO.ToString())
                        {
                            var doc = _context.JobOrder.Where(x => x.JobOrder_Id == q.Doc_Id).FirstOrDefault();
                            if (doc != null)
                            {
                                ViewBag.QNo = "HubOut - " + doc.DocNo;
                            }
                        }
                        else
                        {
                            var doc = _context.JobOrderEmergency.Where(x => x.JobOrderEmergency_Id == q.Doc_Id).FirstOrDefault();
                            if (doc != null)
                            {
                                ViewBag.QNo = "HubOut - " + doc.DocNo;
                            }
                        }
                    }

                    var srms = _context.RackJob.FirstOrDefault();
                    if (srms != null)
                    {
                        bool isCont = false;

                        if (srms.RackJobQueue_Id != 0 && srms.LoginIP != ViewBag.DeviceId)
                        {
                            //return View("RackJobHubInView");
                        }
                        if (srms.RackJobQueue_Id == qId && srms.LoginIP == ViewBag.DeviceId && srms.Json != "")
                        {
                            RackJobHubInDTO json = JsonConvert.DeserializeObject<RackJobHubInDTO>(srms.Json) ?? new RackJobHubInDTO();
                            if (json.LoaderCode != "")
                            {
                                ViewBag.QContinuos = "1";
                                isCont = true;
                            }
                        }

                        if (!isCont)
                        {
                            srms.Loader_Id = 0;
                            srms.RackJobQueue_Id = 0;
                            srms.Json = "";
                            _context.SaveChangesAsync();
                        }
                    }
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["ActiveGroup"] = "grpRACKING";
            ViewData["ActiveTab"] = "RackJob";
            ViewData["Title"] = "Rack Job Hub In";
            return View();
        }

        public IActionResult RackJobHubOut(long qId)
        {
            ViewBag.QContinuos = "0";
            ViewBag.QId = qId;
            ViewBag.QNo = "";
            ViewBag.xToken = "";
            ViewBag.DeviceId = "";
            ViewBag.PermissionList = new List<int>();
            //string s = HttpContext.Session.GetString("xSession") ?? "";
            //if (s != "")
            //{
            //    UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
            //    ViewBag.PermissionList = data.UACIdList;
            //    ViewBag.xToken = data.Token;
            //    ViewBag.DeviceId = data.DeviceId;
            //}

            if (User.Identity?.IsAuthenticated ?? false)
            {
                var uacClaim = User.FindFirst("UACIdList")?.Value;
                if (uacClaim != null)
                {
                    List<int> uacIdList = uacClaim.Split(',').Select(int.Parse).ToList();
                    ViewBag.PermissionList = uacIdList;
                    ViewBag.xToken = User.FindFirst("Token")?.Value;
                    ViewBag.DeviceId = User.FindFirst("DeviceId")?.Value;

                    var q = _context.RackJobQueue.Where(x => x.RackJobQueue_Id == qId).FirstOrDefault();
                    if (q != null)
                    {
                        if (q.DocType == EnumQueueDocType.Loader.ToString())
                        {
                            var doc = _context.Loader.Where(x => x.Loader_Id == q.Doc_Id).FirstOrDefault();
                            if (doc != null)
                            {
                                ViewBag.QNo = "HubIn - " + doc.Description;
                            }
                        }
                        else if (q.DocType == EnumQueueDocType.JO.ToString())
                        {
                            var doc = _context.JobOrder.Where(x => x.JobOrder_Id == q.Doc_Id).FirstOrDefault();
                            if (doc != null)
                            {
                                ViewBag.QNo = "HubOut - " + doc.DocNo;
                            }
                        }
                        else
                        {
                            var doc = _context.JobOrderEmergency.Where(x => x.JobOrderEmergency_Id == q.Doc_Id).FirstOrDefault();
                            if (doc != null)
                            {
                                ViewBag.QNo = "HubOut - " + doc.DocNo;
                            }
                        }
                    }

                    var srms = _context.RackJob.FirstOrDefault();
                    if (srms != null)
                    {
                        bool isCont = false;

                        if (srms.RackJobQueue_Id != 0 && srms.LoginIP != ViewBag.DeviceId)
                        {
                            //return View("RackJobHubOutView");
                        }
                        if (srms.RackJobQueue_Id == qId && srms.LoginIP == ViewBag.DeviceId && srms.Json != "")
                        {
                            ViewBag.QContinuos = "1";
                            isCont = true;
                        }

                        if (!isCont)
                        {
                            srms.Loader_Id = 0;
                            srms.RackJobQueue_Id = 0;
                            srms.Json = "";
                            _context.SaveChangesAsync();
                        }
                    }
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["ActiveGroup"] = "grpRACKING";
            ViewData["ActiveTab"] = "RackJob";
            ViewData["Title"] = "Rack Job Hub Out";
            return View();
        }

        //public IActionResult NewTransferHubInTask()
        //{
        //    ViewBag.xToken = "";
        //    ViewBag.PermissionList = new List<int>();
        //    //string s = HttpContext.Session.GetString("xSession") ?? "";
        //    //if (s != "")
        //    //{
        //    //    UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
        //    //    ViewBag.PermissionList = data.UACIdList;
        //    //    ViewBag.xToken = data.Token;
        //    //}
        //    if (User.Identity?.IsAuthenticated ?? false)
        //    {
        //        var uacClaim = User.FindFirst("UACIdList")?.Value;
        //        if (uacClaim != null)
        //        {
        //            List<int> uacIdList = uacClaim.Split(',').Select(int.Parse).ToList();
        //            ViewBag.PermissionList = uacIdList;
        //            ViewBag.xToken = User.FindFirst("Token")?.Value;
        //        }
        //    }
        //    else
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    ViewData["ActiveGroup"] = "grpRACKING";
        //    ViewData["ActiveTab"] = "NewTransferHubInTask";
        //    ViewData["Title"] = "New Hub In Task";
        //    return View();
        //}

        public IActionResult ManualHubInTask()
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

            ViewData["ActiveGroup"] = "grpRACKING";
            ViewData["ActiveTab"] = "ManualHubInTask";
            ViewData["Title"] = "Manual Hub In Task";
            return View();
        }

        public IActionResult TrolleyReel()
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

            ViewData["ActiveGroup"] = "grpRACKING";
            ViewData["ActiveTab"] = "TrolleyReel";
            ViewData["Title"] = "Trolley Reel";
            return View();
        }

        public IActionResult RackDrawerIn(long qId)
        {
            ViewBag.QContinuos = "0";
            ViewBag.QId = qId;
            ViewBag.QNo = "";
            ViewBag.xToken = "";
            ViewBag.DeviceId = "";
            ViewBag.PermissionList = new List<int>();
            //string s = HttpContext.Session.GetString("xSession") ?? "";
            //if (s != "")
            //{
            //    UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
            //    ViewBag.PermissionList = data.UACIdList;
            //    ViewBag.xToken = data.Token;
            //    ViewBag.DeviceId = data.DeviceId;
            //}

            if (User.Identity?.IsAuthenticated ?? false)
            {
                var uacClaim = User.FindFirst("UACIdList")?.Value;
                if (uacClaim != null)
                {
                    List<int> uacIdList = uacClaim.Split(',').Select(int.Parse).ToList();
                    ViewBag.PermissionList = uacIdList;
                    ViewBag.xToken = User.FindFirst("Token")?.Value;
                    ViewBag.DeviceId = User.FindFirst("DeviceId")?.Value;

                    var q = _context.RackJobQueue.Where(x => x.RackJobQueue_Id == qId).FirstOrDefault();
                    if (q != null)
                    {
                        var doc = _context.Trolley.Where(x => x.Trolley_Id == q.Doc_Id).FirstOrDefault();
                        if (doc != null)
                        {
                            ViewBag.QNo = "Drawer - " + doc.TrolleyCode;
                        }
                    }

                    var srms = _context.RackJob.FirstOrDefault();
                    if (srms != null)
                    {
                        bool isCont = false;

                        if (srms.RackJobQueue_Id != 0 && srms.LoginIP != ViewBag.DeviceId)
                        {
                            //return View("RackDrawerInView");
                        }
                        if (srms.RackJobQueue_Id == qId && srms.LoginIP == ViewBag.DeviceId)
                        {
                            ViewBag.QContinuos = "1";
                            isCont = true;
                        }

                        if (!isCont)
                        {
                            srms.Loader_Id = 0;
                            srms.RackJobQueue_Id = 0;
                            srms.Json = "";
                            _context.SaveChangesAsync();
                        }
                    }
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["ActiveGroup"] = "grpRACKING";
            ViewData["ActiveTab"] = "RackJob";
            ViewData["Title"] = "Rack Job Drawer In";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<RackJobDTO>> GetRackJob()
        {
            ServiceResponseModel<RackJobDTO> result = await _rackService.GetRackJob();
            return result;
        }

        [HttpGet]
        public async Task<ServiceResponseModel<RackJobLog>> GetEndRackJob(long jobQId)
        {
            ServiceResponseModel<RackJobLog> result = await _rackService.GetEndRackJob(jobQId);
            return result;
        }
    }
}
