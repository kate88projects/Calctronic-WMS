using AutoMapper;
using RackingSystem.Data;
using RackingSystem.Models.Slot;
using RackingSystem.Models;
using RackingSystem.Models.Loader;
using Microsoft.EntityFrameworkCore;
using RackingSystem.General;
using RackingSystem.Data.Maintenances;
using RackingSystem.Models.Item;
using System.Drawing;

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
                int minHeight = 0;
                var configMinHeight = await _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.Loader_ColMinReserve.ToString()).FirstOrDefaultAsync();
                if (configMinHeight != null)
                {
                    minHeight = Convert.ToInt16(configMinHeight.ConfigValue);
                }

                var loaderList = await _dbContext.Loader.OrderBy(x => x.LoaderCode).ToListAsync();
                var loaderListDTO = _mapper.Map<List<LoaderListDTO>>(loaderList).ToList();
                foreach (var loader in loaderListDTO)
                {
                    var colList = await _dbContext.LoaderColumn.Where(x => x.Loader_Id == loader.Loader_Id).OrderBy(x => x.ColNo).ToListAsync();
                    var colListDTO = _mapper.Map<List<LoaderColumnDTO>>(colList).ToList();
                    foreach( var col in colListDTO)
                    {
                        if (col.BalanceHeight <= minHeight && minHeight != 0)
                        {
                            col.BalancePercentage = 0;
                            col.UsagePercentage = 100;
                        }
                        else
                        {
                            decimal point = Convert.ToDecimal(col.BalanceHeight) / Convert.ToDecimal(loader.ColHeight) * 100;
                            col.BalancePercentage = Convert.ToInt16(point);
                            col.UsagePercentage = 100 - col.BalancePercentage;
                        }

                        loader.BalanceHeight = loader.BalanceHeight + col.BalanceHeight;
                        loader.BalancePercentage = loader.BalancePercentage + col.BalancePercentage;
                        loader.UsagePercentage = loader.UsagePercentage + col.UsagePercentage;

                        loader.ColBalList.Add(col.BalanceHeight);
                    }
                    loader.BalancePercentage = loader.BalancePercentage / loader.TotalCol;
                    loader.UsagePercentage = loader.UsagePercentage / loader.TotalCol;
                    loader.ColList = colListDTO;
                }
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
                    if (string.IsNullOrEmpty(req.Status))
                    {
                        req.Status = EnumLoaderStatus.ReadyToLoad.ToString();
                    }
                    Loader _loader = new Loader()
                    {
                        LoaderCode = req.LoaderCode,
                        Description = req.Description ?? "",
                        IsActive = req.IsActive,
                        Status = req.Status ?? EnumLoaderStatus.ReadyToLoad.ToString(),
                        Remark = req.Remark ?? "",
                        TotalCol = req.TotalCol,
                        ColHeight = req.ColHeight,
                        IPAddr = req.IPAddr,
                    };
                    _dbContext.Loader.Add(_loader);
                    await _dbContext.SaveChangesAsync();

                    for (var iCol = 1; iCol <= req.TotalCol; iCol++)
                    {
                        LoaderColumn _loaderCol = new LoaderColumn()
                        {
                            Loader_Id = _loader.Loader_Id,
                            ColNo = iCol,
                            BalanceHeight = req.ColHeight,
                        };
                        _dbContext.LoaderColumn.Add(_loaderCol);
                    }
                }
                else
                {
                    Loader? _loader = _dbContext.Loader.Find(req.Loader_Id);
                    if (_loader == null)
                    {
                        result.errMessage = "Cannot find this loader, please refresh the list.";
                        return result;
                    }
                    var oldColH = _loader.ColHeight;
                    var oldTtlCol = _loader.TotalCol;
                    if (oldTtlCol > req.TotalCol)
                    {
                        var loadColUsed = _dbContext.LoaderColumn.Where(x => x.Loader_Id == _loader.Loader_Id && x.BalanceHeight != oldColH).ToList();
                        if (loadColUsed.Count > 0)
                        {
                            result.errMessage = "Loader is on service, cannot change Total Column.";
                            return result;
                        }
                    }
                    _loader.LoaderCode = req.LoaderCode;
                    _loader.Description = req.Description;
                    _loader.IsActive = req.IsActive;
                    _loader.TotalCol = req.TotalCol;
                    _loader.ColHeight = req.ColHeight;
                    _loader.IPAddr = req.IPAddr;
                    _dbContext.Loader.Update(_loader);
                    await _dbContext.SaveChangesAsync();

                    if (oldTtlCol > req.TotalCol)
                    {
                        var loadColExist = _dbContext.LoaderColumn.Where(x => x.Loader_Id == _loader.Loader_Id).ToList();

                        for (var iCol = loadColExist.Count - 1; iCol >= 0; iCol--)
                        {
                            if (loadColExist[iCol].ColNo > req.TotalCol)
                            {
                                _dbContext.LoaderColumn.Remove(loadColExist[iCol]);
                            }
                        }
                    }
                    else if (oldTtlCol < req.TotalCol)
                    {
                        for (var iCol = 1; iCol <= req.TotalCol; iCol++)
                        {
                            var loadColExist = _dbContext.LoaderColumn.Where(x => x.Loader_Id == _loader.Loader_Id && x.ColNo == iCol).ToList();
                            if (loadColExist.Count == 0)
                            {
                                LoaderColumn _loaderCol = new LoaderColumn()
                                {
                                    Loader_Id = _loader.Loader_Id,
                                    ColNo = iCol,
                                    BalanceHeight = req.ColHeight,
                                };
                                _dbContext.LoaderColumn.Add(_loaderCol);
                            }
                        }
                    }
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

                var loadColExist = _dbContext.LoaderColumn.Where(x => x.Loader_Id == _item.Loader_Id).ToList();
                for (var iCol = loadColExist.Count - 1; iCol >= 0; iCol--)
                {
                    _dbContext.LoaderColumn.Remove(loadColExist[iCol]);
                }
                await _dbContext.SaveChangesAsync();

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
