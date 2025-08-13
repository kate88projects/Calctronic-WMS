using AutoMapper;
using RackingSystem.Data;
using RackingSystem.Models.Slot;
using RackingSystem.Models;
using RackingSystem.Models.Loader;
using Microsoft.EntityFrameworkCore;
using RackingSystem.General;
using RackingSystem.Data.Maintenances;
using RackingSystem.Models.Item;

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

        public async Task<ServiceResponseModel<LoaderDTO>> SaveLoader(LoaderDTO req)
        {
            ServiceResponseModel<LoaderDTO> result = new ServiceResponseModel<LoaderDTO>();

            try
            {
                // 1. checking Data
                if (req == null)
                {
                    result.errMessage = "Please insert Loader Code.";
                    return result;
                }
                if (string.IsNullOrEmpty(req.LoaderCode))
                {
                    result.errMessage = "Please insert Loader Code.";
                    return result;
                }
                if (req.Loader_Id == 0)
                {
                    Loader? iExist = _dbContext.Loader.FirstOrDefault(x => x.LoaderCode == req.LoaderCode);
                    if (iExist != null)
                    {
                        result.errMessage = "This loader code has exist.";
                        return result;
                    }
                }
                else
                {
                    Loader? iExist = _dbContext.Loader.FirstOrDefault(x => x.LoaderCode == req.LoaderCode && x.Loader_Id != req.Loader_Id);
                    if (iExist != null)
                    {
                        result.errMessage = "This loader code has been used.";
                        return result;
                    }
                }

                // 2. save Data
                if (req.Loader_Id == 0)
                {
                    Loader _loader = new Loader()
                    {
                        LoaderCode = req.LoaderCode,
                        Description = req.Description ?? "",
                        IsActive = req.IsActive,
                        Status = req.Status ?? EnumLoaderStatus.ReadyToLoad.ToString(),
                        Remark = req.Remark ?? "",
                        TotalCol = req.TotalCol,
                        ColHeight = req.ColHeight
                    };
                    _dbContext.Loader.Add(_loader);
                }
                else
                {
                    Loader? _loader = _dbContext.Loader.Find(req.Loader_Id);
                    if (_loader == null)
                    {
                        result.errMessage = "Cannot find this loader, please refresh the list.";
                        return result;
                    }
                    _loader.LoaderCode = req.LoaderCode;
                    _loader.Description = req.Description;
                    _loader.IsActive = req.IsActive;
                    _loader.TotalCol = req.TotalCol;
                    _loader.ColHeight = req.ColHeight;
                    _dbContext.Loader.Update(_loader);
                }
                await _dbContext.SaveChangesAsync();

                result.success = true;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<LoaderDTO>> DeleteLoader(LoaderDTO req)
        {
            ServiceResponseModel<LoaderDTO> result = new ServiceResponseModel<LoaderDTO>();

            try
            {
                // 1. checking Data
                if (req == null)
                {
                    result.errMessage = "Please refresh the list.";
                    return result;
                }
                //Bin? binExist2 = _dbContext.Bin.FirstOrDefault(x => x.ColNo == binReq.ColNo && x.RowNo != binReq.RowNo && x.Bin_Id != binReq.Bin_Id);
                //if (binExist2 != null)
                //{
                //    result.errMessage = "This Column No and Row No has been used.";
                //    return result;
                //}

                // 2. save Data
                Loader? _item = _dbContext.Loader.Find(req.Loader_Id);
                if (_item == null)
                {
                    result.errMessage = "Cannot find this loader, please refresh the list.";
                    return result;
                }
                _dbContext.Loader.Remove(_item);
                await _dbContext.SaveChangesAsync();

                result.success = true;
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
