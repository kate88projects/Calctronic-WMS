using Microsoft.AspNetCore.Mvc;
using RackingSystem.Data;
using RackingSystem.Models;
using RackingSystem.Services.LoaderServices;
using RackingSystem.Models.Loader;
using RackingSystem.Models.Item;

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
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<LoaderListDTO>>> GetLoaderList()
        {
            ServiceResponseModel<List<LoaderListDTO>> result = await _loaderService.GetLoaderList();
            return result;
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
