using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RackingSystem.Data;
using RackingSystem.Models.GRN;
using RackingSystem.Models;
using RackingSystem.Models.User;
using RackingSystem.Services.GRNServices;
using RackingSystem.Services.RackJobQueueServices;
using RackingSystem.Models.Item;
using RackingSystem.Models.RackJobQueue;
using RackingSystem.Models.Loader;
using RackingSystem.Data.JO;

namespace RackingSystem.Controllers
{
    public class RackJobQueueController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IRackJobQueueService _qService;

        public RackJobQueueController(AppDbContext context, IRackJobQueueService qService)
        {
            _context = context;
            _qService = qService;
        }

        public IActionResult RackJobQueueList()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpQ";
            ViewData["ActiveTab"] = "RackJobQueueList";
            ViewData["Title"] = "Queue List";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<QListDTO>>> GetQueueList()
        {
            ServiceResponseModel<List<QListDTO>> result = await _qService.GetQueueList();
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> AddQueue([FromBody] QReqDTO req)
        {
            ServiceResponseModel<QReqDTO> result = await _qService.AddQueue(req);
            return new JsonResult(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteQueue([FromBody] QReqDTO req)
        {
            ServiceResponseModel<QReqDTO> result = await _qService.DeleteQueue(req);
            return new JsonResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeQueue([FromBody] QReqDTO req)
        {
            ServiceResponseModel<QReqDTO> result = await _qService.ChangeQueue(req);
            return new JsonResult(result);
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<JobOrder>>> GetJOList_PendingToQueue()
        {
            ServiceResponseModel<List<JobOrder>> result = await _qService.GetJOList_PendingToQueue();
            return result;
        }

        public IActionResult RackJobCounter()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpQ";
            ViewData["ActiveTab"] = "RackJobCounter";
            ViewData["Title"] = "SRMS Counter";
            return View();
        }

        public IActionResult RackJobViewer()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpQ";
            ViewData["ActiveTab"] = "RackJobViewer";
            ViewData["Title"] = "SRMS Viewer";
            return View();
        }


    }
}
