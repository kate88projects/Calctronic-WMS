using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RackingSystem.Data;
using RackingSystem.Models;
using RackingSystem.Models.Loader;
using RackingSystem.Services.LoaderServices;

namespace RackingSystem.Controllers.API
{
    [Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme}")]
    [Route("api/[controller]")]
    [ApiController]
    public class LoaderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILoaderService _loaderService;

        public LoaderController(AppDbContext context, ILoaderService loaderService)
        {
            _context = context;
            _loaderService = loaderService;
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
            ServiceResponseModel<LoaderDTO> result = await _loaderService.GetLoaderInfo(req, true, General.EnumLoaderStatus.ReadyToLoad, General.EnumLoaderStatus.Loaded);
            return result;
        }

    }
}
