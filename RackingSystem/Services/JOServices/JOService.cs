using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using RackingSystem.Data;
using RackingSystem.Data.GRN;
using RackingSystem.Data.JO;
using RackingSystem.Data.Maintenances;
using RackingSystem.General;
using RackingSystem.Helpers;
using RackingSystem.Models;
using RackingSystem.Models.BOM;
using RackingSystem.Models.GRN;
using RackingSystem.Models.JO;
using System.CodeDom;

namespace RackingSystem.Services.JOServices
{
    public class JOService : IJOService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public JOService(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        //public async Task<ServiceResponseModel<int>> GetJOTotalCount(JOSearchReqDTO req)
        //{
        //    ServiceResponseModel<int> result = new ServiceResponseModel<int>();

        //    try
        //    {
        //        var parameters = new[]
        //        {
        //            new SqlParameter("@GetTotal", "1"),
        //            new SqlParameter("@DateType", req.DateType),
        //            new SqlParameter("@DateFrom", req.DateFrom.ToString("yyyy-MM-dd")),
        //            new SqlParameter("@DateTo", req.DateTo.ToString("yyyy-MM-dd")),
        //            new SqlParameter("@GRNBatchNo", req.GRNBatchNo),
        //            new SqlParameter("@ItemCode", req.ItemCode),
        //            new SqlParameter("@ReelCode", req.ReelCode),
        //            new SqlParameter("@Remark", req.Remark),
        //            new SqlParameter("@pageSize", req.pageSize),
        //            new SqlParameter("@page", req.page)
        //        };

        //        string sql = "EXECUTE dbo.GRN_GET_SEARCHDTLLIST @GetTotal,@DateType,@DateFrom,@DateTo,@GRNBatchNo,@ItemCode,@ReelCode,@Remark,@pageSize,@page";
        //        var grndtlListDTO = await _dbContext.SP_GRNDTLSearchList.FromSqlRaw(sql, parameters).ToListAsync();

        //        int totalCount = 0;
        //        if (grndtlListDTO != null)
        //        {
        //            totalCount = grndtlListDTO.First().totalRecord;
        //        }

        //        result.success = true;
        //        result.data = totalCount;
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        result.errMessage = ex.Message;
        //        result.errStackTrace = ex.StackTrace ?? "";
        //    }

        //    return result;
        //}

        public async Task<ServiceResponseModel<JOListDTO>> GetJO(long id)
        {
            ServiceResponseModel<JOListDTO> result = new ServiceResponseModel<JOListDTO>();

            try
            {
                var joList = await _dbContext.JobOrder.Where(x => x.JobOrder_Id == id).FirstOrDefaultAsync();
                var joListDTO = _mapper.Map<JOListDTO>(joList);
                result.success = true;
                result.data = joListDTO;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            return result;
        }

        public async Task<ServiceResponseModel<List<JOListDTO>>> GetJOList() //JOSearchReqDTO req
        {
            ServiceResponseModel<List<JOListDTO>> result = new ServiceResponseModel<List<JOListDTO>>();

            try
            {
                //var parameters = new[]
                //{
                //    new SqlParameter("@GetTotal", "0"),
                //    new SqlParameter("@DateType", req.DateType),
                //    new SqlParameter("@DateFrom", req.DateFrom.ToString("yyyy-MM-dd")),
                //    new SqlParameter("@DateTo", req.DateTo.ToString("yyyy-MM-dd")),
                //    new SqlParameter("@GRNBatchNo", req.GRNBatchNo),
                //    new SqlParameter("@ItemCode", req.ItemCode),
                //    new SqlParameter("@ReelCode", req.ReelCode),
                //    new SqlParameter("@Remark", req.Remark),
                //    new SqlParameter("@pageSize", req.pageSize),
                //    new SqlParameter("@page", req.page)
                //};

                //string sql = "EXECUTE dbo.GRN_GET_SEARCHDTLLIST @GetTotal,@DateType,@DateFrom,@DateTo,@GRNBatchNo,@ItemCode,@ReelCode,@Remark,@pageSize,@page";
                //var grndtlListDTO = await _dbContext.SP_GRNDTLSearchList.FromSqlRaw(sql, parameters).ToListAsync();

                //result.success = true;
                //result.data = grndtlListDTO;

                var jomList = await _dbContext.JobOrder.OrderBy(x => x.JobOrder_Id).ToListAsync();
                var jomListDTO = _mapper.Map<List<JOListDTO>>(jomList);
                result.success = true;
                result.data = jomListDTO;

                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<List<JODetailReqDTO>>> GetJODetail(long jobId)
        {
            ServiceResponseModel<List<JODetailReqDTO>> result = new ServiceResponseModel<List<JODetailReqDTO>>();

            try
            {
                var jobDtl = _dbContext.JobOrderDetail.Where(d => d.JobOrder_Id == jobId).ToList();
                var jobDtlDTO = _mapper.Map<List<JODetailReqDTO>>(jobDtl);
                result.success = true;
                result.data = jobDtlDTO;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            return result;
        }

        public async Task<ServiceResponseModel<JOReqDTO>> SaveJob(JOReqDTO job)
        {
            ServiceResponseModel<JOReqDTO> result = new ServiceResponseModel<JOReqDTO>();

            try
            {
                //if (string.IsNullOrEmpty(job.DocNo))
                //{
                //    result.errMessage = "Please enter document no.";
                //    return result;
                //}

                int index = 1;
                foreach (var dtl in job.Details)
                {
                    if (dtl.Item_Id == 0)
                    {
                        result.errMessage = $"Item {index}: No product selected. Please choose a product.";
                        return result;
                    }
                    else if (dtl.Qty == 0)
                    {
                        result.errMessage = $"Item {index}: Quantity is missing. Please enter a quantity.";
                        return result;
                    }
                    index++;
                }

                if (job.JobOrder_Id == 0)
                {
                    var jobOrder = await DocFormatHelper.Instance.get_NextDocumentNo(_dbContext, General.EnumConfiguration.DocFormat_JO, DateTime.Now, true);
                    if (jobOrder.success == false)
                    {
                        result.errMessage = jobOrder.errMessage;
                        return result;
                    }

                    JobOrder _job = new JobOrder()
                    {
                        DocNo = jobOrder.data,
                        Description = string.IsNullOrEmpty(job.Description) ? "" : job.Description,
                        Status = "",
                        DocDate = DateTime.Now,
                        CreatedBy = job.CreatedBy,
                        CreatedDate = DateTime.Now,
                    };
                    _dbContext.JobOrder.Add(_job);
                    await _dbContext.SaveChangesAsync();

                    foreach (var dtl in job.Details)
                    {
                        JobOrderDetail _jobDtl = new JobOrderDetail()
                        {
                            JobOrder_Id = _job.JobOrder_Id,
                            Item_Id = dtl.Item_Id,
                            Qty = dtl.Qty,
                        };
                        _dbContext.JobOrderDetail.Add(_jobDtl);
                    }
                    await _dbContext.SaveChangesAsync();
                    result.success = true;
                }
                else
                {
                    JobOrder? _job = _dbContext.JobOrder.Find(job.JobOrder_Id);
                    if (_job == null)
                    {
                        result.errMessage = "Cannot find this specified Job. Please refersh the list and try again.";
                        return result;
                    }

                    var existingDetails = _dbContext.JobOrderDetail.Where(d => d.JobOrder_Id == _job.JobOrder_Id).ToList();
                    if (existingDetails.Count == 0)
                    {
                        result.errMessage = "Cannot find this specified Job Detail. Please refersh the list and try again.";
                        return result;
                    }

                    //_job.DocNo = job.DocNo;
                    _job.Description = string.IsNullOrEmpty(job.Description) ? "" : job.Description;
                    //_job.UpdatedBy = job.CreatedBy;
                    _job.CreatedBy = job.CreatedBy;
                    _job.UpdatedDate = DateTime.Now;
                    _dbContext.JobOrder.Update(_job);

                    //remove the previous if not found on the latest detail
                    var detailsToRemove = new List<JobOrderDetail>();
                    foreach (var exist in existingDetails)
                    {
                        var updated = job.Details.FirstOrDefault(d => d.JobOrderDetail_Id == exist.JobOrderDetail_Id);
                        if (updated == null)
                        {
                            detailsToRemove.Add(exist);
                        }
                    }

                    //update the latest detail
                    foreach (var dtl in job.Details)
                    {
                        if (dtl.JobOrderDetail_Id == 0)
                        {
                            JobOrderDetail _jobDtl = new JobOrderDetail()
                            {
                                JobOrder_Id = _job.JobOrder_Id,
                                Item_Id = dtl.Item_Id,
                                Qty = dtl.Qty,
                            };
                            _dbContext.JobOrderDetail.Add(_jobDtl);
                        }
                        else
                        {
                            JobOrderDetail? _jobDtl = existingDetails.FirstOrDefault(d => d.JobOrder_Id == dtl.JobOrder_Id && d.JobOrderDetail_Id == dtl.JobOrderDetail_Id);
                            if (_jobDtl == null)
                            {
                                result.errMessage = "Cannot find the specific Job Detail for update.";
                                return result;
                            }

                            _jobDtl.Item_Id = dtl.Item_Id;
                            _jobDtl.Qty = dtl.Qty;
                            _dbContext.JobOrderDetail.Update(_jobDtl);
                        }   
                    }

                    if (detailsToRemove.Any())
                    {
                        _dbContext.JobOrderDetail.RemoveRange(detailsToRemove);
                    }

                    await _dbContext.SaveChangesAsync();
                    result.success = true;
                }
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            return result;
        }

        public async Task<ServiceResponseModel<JOListDTO>> DeleteJob(JOListDTO job)
        {
            ServiceResponseModel<JOListDTO> result = new ServiceResponseModel<JOListDTO>();

            try
            {
                if (job == null)
                {
                    result.errMessage = "Something wrong. please refresh ths list.";
                    return result;
                }

                JobOrder? _job = _dbContext.JobOrder.Find(job.JobOrder_Id);
                if (_job == null)
                {
                    result.errMessage = "Cannot find this Job, please refresh the list.";
                    return result;
                }
                _dbContext.JobOrder.Remove(_job);

                var existingDetails = _dbContext.JobOrderDetail.Where(d => d.JobOrder_Id == _job.JobOrder_Id).ToList();
                foreach (var dtl in existingDetails)
                {
                    _dbContext.JobOrderDetail.Remove(dtl);
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

        
    }
}
