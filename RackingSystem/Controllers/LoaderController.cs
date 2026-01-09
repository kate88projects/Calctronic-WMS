using Microsoft.AspNetCore.Mvc;
using RackingSystem.Data;
using RackingSystem.Models;
using RackingSystem.Services.LoaderServices;
using RackingSystem.Models.Loader;
using RackingSystem.Models.Item;
using Newtonsoft.Json;
using RackingSystem.Models.User;
using Microsoft.AspNetCore.Http.HttpResults;
using RackingSystem.General;
using RackingSystem.Models.Trolley;

namespace RackingSystem.Controllers
{
    public class LoaderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILoaderService _loaderService;

        public LoaderController(AppDbContext context, ILoaderService loaderService)
        {
            _context = context;
            _loaderService = loaderService;
        }

        public IActionResult LoaderList()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpMM";
            ViewData["ActiveTab"] = "LoaderList";
            ViewData["Title"] = "Auto Loader List";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetLoaderList()
        {
            ServiceResponseModel<List<LoaderListDTO>> result = await _loaderService.GetLoaderList();
            return new JsonResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> SaveLoader([FromBody] LoaderDTO req)
        {
            ServiceResponseModel<LoaderDTO> result = await _loaderService.SaveLoader(req);
            return new JsonResult(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteLoader([FromBody] LoaderDTO req)
        {
            ServiceResponseModel<LoaderDTO> result = await _loaderService.DeleteLoader(req);
            return new JsonResult(result);
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<LoaderListDTO>>> GetLoaderList_ReadyToLoad()
        {
            ServiceResponseModel<List<LoaderListDTO>> result = await _loaderService.GetLoaderList_ReadyToLoad();
            return result;
        }

        [HttpGet]
        public async Task<ServiceResponseModel<LoaderDTO>> GetLoaderInfo_ReadyToLoad(string req)
        {
             ServiceResponseModel<LoaderDTO> result = await _loaderService.GetLoaderInfo(req, true, EnumLoaderStatus.ReadyToLoad, EnumLoaderStatus.Loaded);
            return result;
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<LoaderListDTO>>> GetLoaderList_PendingToUnLoad()
        {
            ServiceResponseModel<List<LoaderListDTO>> result = await _loaderService.GetLoaderList_PendingToUnLoad();
            return result;
        }

        [HttpGet]
        public async Task<ServiceResponseModel<LoaderDTO>> GetLoaderInfo_PendingToUnLoad(string req)
        {
            ServiceResponseModel<LoaderDTO> result = await _loaderService.GetLoaderInfo(req, true, EnumLoaderStatus.Loaded, null);
            return result;
        }

        [HttpPost]
        public async Task<ServiceResponseModel<List<LoaderColumnDTO>>> GetLoaderColumn([FromBody] int loaderId)
        {
            ServiceResponseModel<List<LoaderColumnDTO>> result = await _loaderService.GetLoaderColumn(loaderId);
            return result;
        }

        public IActionResult LoaderDetailList()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpRACKING";
            ViewData["ActiveTab"] = "LoaderDetailList";
            ViewData["Title"] = "Loader Detail List";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<LoaderReelDtlDTO>>> GetTrolleyDetailList(long id)
        {
            ServiceResponseModel<List<LoaderReelDtlDTO>> result = await _loaderService.GetLoaderReelDtlList(id);
            result.errMessage = "-,-,-,-,0,0,0,0,-";

            var columns = new List<Dictionary<string, object>>();

            
            if (result.success)
            {
                var loader = _context.Loader.Where(x => x.Loader_Id == id).FirstOrDefault();
                if (loader == null)
                {
                    result.success = false;
                    result.errMessage = "Cannot find this loader.";
                    return result;
                }
                //string grpInfo = "";
                //var grps = result.data.GroupBy(x => x.ItemGroupCode);
                //foreach (var igrp in grps)
                //{
                //    grpInfo += igrp.Key;
                //}
                string u1 = "-";
                string u2 = "-";
                string u3 = "-";
                string u4 = "-";
                if (result.data.Count > 0)
                {
                    int ttlU1 = loader.ColHeight - (result.data.Where(x => x.ColNo == 1).Count() > 0 ? result.data.Where(x => x.ColNo == 1).First().BalanceHeight : 0);
                    u1 = ttlU1.ToString() + " mm";
                    int ttlU2 = loader.ColHeight - (result.data.Where(x => x.ColNo == 2).Count() > 0 ? result.data.Where(x => x.ColNo == 2).First().BalanceHeight : 0);
                    u2 = ttlU2.ToString() + " mm";
                    int ttlU3 = loader.ColHeight - (result.data.Where(x => x.ColNo == 3).Count() > 0 ? result.data.Where(x => x.ColNo == 3).First().BalanceHeight : 0);
                    u3 = ttlU3.ToString() + " mm";
                    int ttlU4 = loader.ColHeight - (result.data.Where(x => x.ColNo == 4).Count() > 0 ? result.data.Where(x => x.ColNo == 4).First().BalanceHeight : 0);
                    u4 = ttlU4.ToString() + " mm";
                }

                int ttlR1 = result.data.Where(x => x.ColNo == 1).Count();
                int ttlR2 = result.data.Where(x => x.ColNo == 2).Count();
                int ttlR3 = result.data.Where(x => x.ColNo == 3).Count();
                int ttlR4 = result.data.Where(x => x.ColNo == 4).Count();

                result.errStackTrace = u1 + "," + u2 + "," + u3 + "," + u4 + "," + ttlR1 + "," + ttlR2 + "," + ttlR3 + "," + ttlR4 + "," + loader?.Status;

            }

            return result;
        }

        public IActionResult LoaderColumnList()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpMM";
            ViewData["ActiveTab"] = "LoaderColumnList";
            ViewData["Title"] = "Loader Column List";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetLoaderColumnList()
        {
            ServiceResponseModel<List<LoaderColumnDTO>> result = await _loaderService.GetLoaderColumnList();
            return new JsonResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> SaveLoaderColumn([FromBody] LoaderColumnDTO loaderCol)
        {
            ServiceResponseModel<LoaderColumnDTO> result = await _loaderService.SaveLoaderColumn(loaderCol);
            return new JsonResult(result);
        }

    }
}
