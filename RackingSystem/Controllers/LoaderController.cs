using Microsoft.AspNetCore.Mvc;
using RackingSystem.Data;
using RackingSystem.Models;
using RackingSystem.Services.LoaderServices;
using RackingSystem.Models.Loader;
using RackingSystem.Models.Item;
using Newtonsoft.Json;
using RackingSystem.Models.User;
using Microsoft.AspNetCore.Http.HttpResults;

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
             ServiceResponseModel<LoaderDTO> result = await _loaderService.GetLoaderInfo(req, true, General.EnumLoaderStatus.ReadyToLoad);
            return result;
        }

    }
}
