using AutoMapper;
using RackingSystem.Data;
using RackingSystem.Models.Slot;
using RackingSystem.Models;
using RackingSystem.Models.Loader;
using Microsoft.EntityFrameworkCore;
using RackingSystem.General;

namespace RackingSystem.Services.LoaderServices
{
    public class LoaderService : ILoaderService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public LoaderService(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<ServiceResponseModel<List<LoaderListDTO>>> GetLoaderList()
        {
            ServiceResponseModel<List<LoaderListDTO>> result = new ServiceResponseModel<List<LoaderListDTO>>();

            try
            {
                var loaderList = await _dbContext.Loader.OrderBy(x => x.LoaderCode).ToListAsync();
                var loaderListDTO = _mapper.Map<List<LoaderListDTO>>(loaderList).ToList();
                result.success = true;
                result.data = loaderListDTO;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<List<LoaderListDTO>>> GetLoaderList_ReadyToLoad()
        {
            ServiceResponseModel<List<LoaderListDTO>> result = new ServiceResponseModel<List<LoaderListDTO>>();

            try
            {
                var loaderList = await _dbContext.Loader.Where(x => x.IsActive == true && x.Status == EnumLoaderStatus.ReadyToLoad.ToString()).OrderBy(x => x.LoaderCode).ToListAsync();
                var loaderListDTO = _mapper.Map<List<LoaderListDTO>>(loaderList).ToList();
                result.success = true;
                result.data = loaderListDTO;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<LoaderDTO>> GetLoaderInfo(string req, bool checkStatus, EnumLoaderStatus loaderStatus)
        {
            ServiceResponseModel<LoaderDTO> result = new ServiceResponseModel<LoaderDTO>();

            try
            {
                var loaderInfo = await _dbContext.Loader.Where(x => x.LoaderCode == req).FirstOrDefaultAsync();

                if (loaderInfo == null)
                {
                    result.errMessage = "Cannot find this laoder [" + req + "].";
                    return result;
                }

                if (checkStatus)
                {
                    if (loaderInfo.Status != loaderStatus.ToString())
                    {
                        result.errMessage = "This laoder is not [" + loaderStatus.ToString() + "].";
                        return result;
                    }
                }

                var loaderDTO = _mapper.Map<LoaderDTO>(loaderInfo);
                result.success = true;
                result.data = loaderDTO;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

    }
}
