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
using Microsoft.Data.SqlClient;
using RackingSystem.Models.Trolley;

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

                LoaderReel? binExist1 = _dbContext.LoaderReel.FirstOrDefault(x => x.Loader_Id == req.Loader_Id);
                if (binExist1 != null)
                {
                    result.errMessage = "This Loader has been used.";
                    return result;
                }

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
                var loaderList = await _dbContext.Loader.Where(x => x.IsActive == true && (x.Status == EnumLoaderStatus.ReadyToLoad.ToString() || x.Status == EnumLoaderStatus.Loaded.ToString()))
                    .OrderBy(x => x.LoaderCode).ToListAsync();
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

        public async Task<ServiceResponseModel<LoaderDTO>> GetLoaderInfo(string req, bool checkStatus, EnumLoaderStatus loaderStatus1, EnumLoaderStatus? loaderStatus2)
        {
            ServiceResponseModel<LoaderDTO> result = new ServiceResponseModel<LoaderDTO>();

            try
            {
                var loaderInfo = await _dbContext.Loader.Where(x => x.LoaderCode == req).FirstOrDefaultAsync();

                if (loaderInfo == null)
                {
                    result.errMessage = "Cannot find this Auto Loader [" + req + "].";
                    return result;
                }

                if (checkStatus)
                {
                    if (loaderStatus2 != null)
                    {
                        if (loaderInfo.Status != loaderStatus1.ToString() && loaderInfo.Status != loaderStatus2.ToString())
                        {
                            result.errMessage = "This Auto Loader is in [" + loaderInfo.Status + "].";
                            return result;
                        }
                    }
                    else
                    {
                        if (loaderInfo.Status != loaderStatus1.ToString())
                        {
                            result.errMessage = "This Auto Loader is not [" + loaderStatus1.ToString() + "].";
                            return result;
                        }
                    }
                }

                var loaderDTO = _mapper.Map<LoaderDTO>(loaderInfo);

                for (int iCol = 1; iCol <= 4; iCol++)
                {
                    var col = _dbContext.LoaderColumn.Where(x => x.Loader_Id == loaderDTO.Loader_Id && x.ColNo == iCol).FirstOrDefault();
                    if (col != null)
                    {
                        if (iCol == 1) { loaderDTO.Col1UsedHeight = loaderDTO.ColHeight - col.BalanceHeight; }
                        if (iCol == 2) { loaderDTO.Col2UsedHeight = loaderDTO.ColHeight - col.BalanceHeight; }
                        if (iCol == 3) { loaderDTO.Col3UsedHeight = loaderDTO.ColHeight - col.BalanceHeight; }
                        if (iCol == 4) { loaderDTO.Col4UsedHeight = loaderDTO.ColHeight - col.BalanceHeight; }

                        if (iCol == 1) { loaderDTO.Col1UsedPercentage = (loaderDTO.Col1UsedHeight * 100) / loaderDTO.ColHeight; }
                        if (iCol == 2) { loaderDTO.Col2UsedPercentage = (loaderDTO.Col2UsedHeight * 100) / loaderDTO.ColHeight; }
                        if (iCol == 3) { loaderDTO.Col3UsedPercentage = (loaderDTO.Col3UsedHeight * 100) / loaderDTO.ColHeight; }
                        if (iCol == 4) { loaderDTO.Col4UsedPercentage = (loaderDTO.Col4UsedHeight * 100) / loaderDTO.ColHeight; }

                    }
                    var ttl = _dbContext.LoaderReel.Where(x => x.Loader_Id == loaderDTO.Loader_Id && x.ColNo == iCol).Count();
                    if (iCol == 1) { loaderDTO.Col1TotalReels = ttl; }
                    if (iCol == 2) { loaderDTO.Col2TotalReels = ttl; }
                    if (iCol == 3) { loaderDTO.Col3TotalReels = ttl; }
                    if (iCol == 4) { loaderDTO.Col4TotalReels = ttl; }
                }


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

        public async Task<ServiceResponseModel<List<LoaderListDTO>>> GetLoaderList_PendingToUnLoad()
        {
            ServiceResponseModel<List<LoaderListDTO>> result = new ServiceResponseModel<List<LoaderListDTO>>();

            try
            {
                var loaderList = await _dbContext.Loader.Where(x => x.IsActive == true && x.Status == EnumLoaderStatus.Loaded.ToString()).OrderBy(x => x.LoaderCode).ToListAsync();
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

        public async Task<ServiceResponseModel<List<LoaderColumnDTO>>> GetLoaderColumn(int loaderId)
        {
            ServiceResponseModel<List<LoaderColumnDTO>> result = new ServiceResponseModel<List<LoaderColumnDTO>>();
            try
            {
                //var loaderColList = await _dbContext.LoaderColumn.Where(x => x.Loader_Id == loaderId).OrderBy(x => x.ColNo).ToListAsync();
                //var loaderColListDTO = _mapper.Map<List<LoaderColumnDTO>>(loaderColList).ToList();
                var loaderColList = await (from lc in _dbContext.LoaderColumn
                                           join l in _dbContext.Loader on lc.Loader_Id equals l.Loader_Id
                                           where l.Loader_Id == loaderId
                                           orderby lc.ColNo
                                           select new
                                           {
                                               lc,
                                               l.ColHeight
                                           }).ToListAsync();

                var jobReelQty = await _dbContext.RackJob.Where(r => r.Loader_Id == loaderId).OrderByDescending(r => r.StartDate).FirstOrDefaultAsync();
                var reelInLoaderQty = 0;

                var loaderColListDTO = loaderColList.Select(x =>
                {
                    var dto = _mapper.Map<LoaderColumnDTO>(x.lc);
                    dto.ColHeight = x.ColHeight;
                    double usage = ((x.ColHeight - dto.BalanceHeight) / (double)x.ColHeight);
                    dto.UsagePercentage = (int)(usage * 100);
                    dto.ReelQty = _dbContext.LoaderReel.Count(r => r.Loader_Id == loaderId && r.ColNo == x.lc.ColNo);
                    reelInLoaderQty += dto.ReelQty;
                    return dto;
                }).ToList();

                var progressPercentage = jobReelQty.TotalCount == 0 ? 0 : (jobReelQty.TotalCount - (double)reelInLoaderQty) / jobReelQty.TotalCount * 100;
                bool hasRunningTask = loaderColListDTO.Count > 0;

                if (hasRunningTask)
                {
                    var progressPercentage = (jobReelQty.TotalCount - (double)reelInLoaderQty) / jobReelQty.TotalCount * 100;
                    loaderColListDTO[0].totalProgressPercentage = (int)progressPercentage;
                }

                result.success = hasRunningTask;
                result.data = loaderColListDTO;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<List<LoaderReelDtlDTO>>> GetLoaderReelDtlList(long id)
        {
            ServiceResponseModel<List<LoaderReelDtlDTO>> result = new ServiceResponseModel<List<LoaderReelDtlDTO>>();

            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@Loader_Id", id),
                };

                string sql = "EXECUTE dbo.Loader_GET_REELDTLLIST @Loader_Id ";
                var listDTO = await _dbContext.SP_LoaderReelDtlList.FromSqlRaw(sql, parameters).ToListAsync();

                result.success = true;
                result.data = listDTO;
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
