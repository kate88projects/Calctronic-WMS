using Microsoft.EntityFrameworkCore;
using RackingSystem.Models.Slot;
using RackingSystem.Models;
using AutoMapper;
using RackingSystem.Data;
using RackingSystem.Models.GRN;
using System.Drawing.Printing;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Query.Internal;
using RackingSystem.Data.Maintenances;
using RackingSystem.Models.Item;
using RackingSystem.Data.GRN;
using RackingSystem.Helpers;
using RackingSystem.General;
using RackingSystem.Models.Loader;
using System.Drawing;

namespace RackingSystem.Services.GRNServices
{
    public class GRNService : IGRNService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public GRNService(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<ServiceResponseModel<GRNDtlDTO>> SaveGRNDtl(GRNDtlReqDTO req)
        {
            ServiceResponseModel<GRNDtlDTO> result = new ServiceResponseModel<GRNDtlDTO>();

            try
            {
                // 1. checking Data
                if (req == null)
                {
                    result.errMessage = "Please select Item.";
                    return result;
                }
                if (req.Item_Id == 0)
                {
                    result.errMessage = "Please select Item.";
                    return result;
                }
                if (req.Qty == 0)
                {
                    result.errMessage = "Please insert Qty.";
                    return result;
                }
                if (req.ExpiryDate == null)
                {
                    result.errMessage = "Please set Expiry Date.";
                    return result;
                }

                var item = _dbContext.Item.Where(x => x.Item_Id == req.Item_Id).FirstOrDefault();
                if (item == null)
                {
                    result.errMessage = "Please reselect Item.";
                    return result;
                }

                string reelId = "";
                string reelCode = "";
                // 2. get ReelId

                // 3. save Data
                if (req.GRNDetail_Id == null)
                {
                    var rReel = await DocFormatHelper.Instance.get_NextDocumentNo(_dbContext, General.EnumConfiguration.DocFormat_Reel, DateTime.Now, true);
                    if (rReel.success == false)
                    {
                        result.errMessage = rReel.errMessage;
                        return result;
                    }
                    reelCode = rReel.data;
                    var reel = new Reel();
                    reel.Reel_Id = new Guid();
                    reel.ReelCode = reelCode;
                    reel.Item_Id = req.Item_Id;
                    reel.ItemCode = item.ItemCode;
                    reel.Qty = req.Qty;
                    reel.ExpiryDate = req.ExpiryDate ?? DateTime.Now;
                    reel.IsReady = true;
                    reel.StatusIdx = (int)EnumReelStatus.WaitingLoader;
                    reel.Status = EnumReelStatus.WaitingLoader.ToString();
                    reel.ActualHeight = 0;
                    reel.CreatedDate = DateTime.Now;
                    _dbContext.Reel.Add(reel);
                    await _dbContext.SaveChangesAsync();
                    reelId = reel.Reel_Id.ToString();

                    GRNDetail _grnDtl = new GRNDetail()
                    {
                        GRNDetail_Id = new Guid(),
                        GRNBatchNo = req.GRNBatchNo,
                        SupplierName = req.SupplierName,
                        SupplierRefNo = req.SupplierRefNo,
                        Item_Id = req.Item_Id,
                        Qty = req.Qty,
                        ExpiryDate = req.ExpiryDate ?? DateTime.Now,
                        Reel_Id = reel.Reel_Id.ToString(),
                        ReelCode = reelCode,
                        Remark = req.Remark ?? "",
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                    };
                    _dbContext.GRNDetail.Add(_grnDtl);
                    req.GRNDetail_Id = _grnDtl.GRNDetail_Id;
                }
                else
                {
                    Reel? _reel = _dbContext.Reel.Where(x => x.ReelCode == req.ReelCode).FirstOrDefault();
                    if (_reel == null)
                    {
                        result.errMessage = "Cannot find this Reel, please refresh the list.";
                        return result;
                    }
                    reelCode = _reel.ReelCode;
                    _reel.Item_Id = req.Item_Id;
                    _reel.ItemCode = item.ItemCode;
                    _reel.Qty = req.Qty;
                    _reel.ExpiryDate = req.ExpiryDate ?? DateTime.Now;
                    _dbContext.Reel.Update(_reel);
                    await _dbContext.SaveChangesAsync();
                    reelId = _reel.Reel_Id.ToString();

                    GRNDetail ? _grnDtl = _dbContext.GRNDetail.Find(req.GRNDetail_Id);
                    if (_grnDtl == null)
                    {
                        result.errMessage = "Cannot find this GRN Detail, please refresh the list.";
                        return result;
                    }
                    _grnDtl.Item_Id = req.Item_Id;
                    _grnDtl.Qty = req.Qty;
                    _grnDtl.ExpiryDate = req.ExpiryDate ?? DateTime.Now;
                    _grnDtl.Remark = req.Remark ?? "";
                    _grnDtl.SupplierName = req.SupplierName;
                    _grnDtl.SupplierRefNo = req.SupplierRefNo;
                    _dbContext.GRNDetail.Update(_grnDtl);
                }
                await _dbContext.SaveChangesAsync();

                GRNDtlDTO data = new GRNDtlDTO();
                data.GRNDetail_Id = req.GRNDetail_Id ?? new Guid();
                data.GRNBatchNo = req.GRNBatchNo;
                data.SupplierName = req.SupplierName ?? "";
                data.SupplierRefNo = req.SupplierRefNo ?? "";
                data.Remark = req.Remark ?? "";
                data.Item_Id = req.Item_Id;
                data.ItemCode = item.ItemCode;
                data.ItemDesc = item.Description;
                data.Qty = req.Qty;
                data.Reel_Id = reelId;
                data.ReelCode = reelCode;
                data.Remark = req.Remark ?? "";
                data.ExpiryDate = req.ExpiryDate ?? DateTime.Now;
                data.CreatedDateDisplay = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                data.UpdatedDateDisplay = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                result.success = true;
                result.data = data;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<GRNDtlDTO>> DeleteGRNDtl(GRNDtlDTO req)
        {
            ServiceResponseModel<GRNDtlDTO> result = new ServiceResponseModel<GRNDtlDTO>();

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
                Reel? _reel = _dbContext.Reel.Where(x => x.ReelCode == req.ReelCode).FirstOrDefault();
                if (_reel == null)
                {
                    result.errMessage = "Cannot find this Reel, please refresh the list.";
                    return result;
                }
                GRNDetail? _grnDtl = _dbContext.GRNDetail.Find(req.GRNDetail_Id);
                if (_grnDtl == null)
                {
                    result.errMessage = "Cannot find this GRN Detail, please refresh the list.";
                    return result;
                }

                _dbContext.Reel.Remove(_reel);
                await _dbContext.SaveChangesAsync();

                _dbContext.GRNDetail.Remove(_grnDtl);
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

        public async Task<ServiceResponseModel<int>>GetGRNDetailTotalCount(GRNSearchReqDTO req)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();

            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@GetTotal", "1"),
                    new SqlParameter("@DateType", req.DateType),
                    new SqlParameter("@DateFrom", req.DateFrom.ToString("yyyy-MM-dd")),
                    new SqlParameter("@DateTo", req.DateTo.ToString("yyyy-MM-dd")),
                    new SqlParameter("@GRNBatchNo", req.GRNBatchNo),
                    new SqlParameter("@ItemCode", req.ItemCode),
                    new SqlParameter("@ReelCode", req.ReelCode),
                    new SqlParameter("@Remark", req.Remark),
                    new SqlParameter("@pageSize", req.pageSize),
                    new SqlParameter("@page", req.page)
                };

                string sql = "EXECUTE dbo.GRN_GET_SEARCHDTLLIST @GetTotal,@DateType,@DateFrom,@DateTo,@GRNBatchNo,@ItemCode,@ReelCode,@Remark,@pageSize,@page";
                var grndtlListDTO = await _dbContext.SP_GRNDTLSearchList.FromSqlRaw(sql, parameters).ToListAsync();

                int totalCount = 0;
                if (grndtlListDTO != null)
                {
                    totalCount = grndtlListDTO.First().totalRecord;
                }

                result.success = true;
                result.data = totalCount;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<List<GRNDtlListDTO>>> GetGRNDetailList(GRNSearchReqDTO req)
        {
            ServiceResponseModel<List<GRNDtlListDTO>> result = new ServiceResponseModel<List<GRNDtlListDTO>>();

            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@GetTotal", "0"),
                    new SqlParameter("@DateType", req.DateType),
                    new SqlParameter("@DateFrom", req.DateFrom.ToString("yyyy-MM-dd")),
                    new SqlParameter("@DateTo", req.DateTo.ToString("yyyy-MM-dd")),
                    new SqlParameter("@GRNBatchNo", req.GRNBatchNo),
                    new SqlParameter("@ItemCode", req.ItemCode),
                    new SqlParameter("@ReelCode", req.ReelCode),
                    new SqlParameter("@Remark", req.Remark),
                    new SqlParameter("@pageSize", req.pageSize),
                    new SqlParameter("@page", req.page)
                };

                string sql = "EXECUTE dbo.GRN_GET_SEARCHDTLLIST @GetTotal,@DateType,@DateFrom,@DateTo,@GRNBatchNo,@ItemCode,@ReelCode,@Remark,@pageSize,@page";
                var grndtlListDTO = await _dbContext.SP_GRNDTLSearchList.FromSqlRaw(sql, parameters).ToListAsync();

                result.success = true;
                result.data = grndtlListDTO;
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
