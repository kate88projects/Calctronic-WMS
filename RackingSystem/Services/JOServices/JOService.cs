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
                        Status = EnumJobOrderStatus.Draft.ToString(),
                        DocDate = DateTime.Now,
                        CreatedBy = job.CreatedBy,
                        CreatedDate = DateTime.Now,
                        Backorder = job.Backorder,
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
                    _job.Backorder = job.Backorder;
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

        public async Task<ServiceResponseModel<JOEmergencyReqDTO>> GetJOEmergency(long id)
        {
            ServiceResponseModel<JOEmergencyReqDTO> result = new ServiceResponseModel<JOEmergencyReqDTO>();

            try
            {
                var jomEmergency = await _dbContext.JobOrderEmergency.Where(x => x.JobOrderEmergency_Id == id).FirstOrDefaultAsync();
                var jomEmergencyDTO = _mapper.Map<JOEmergencyReqDTO>(jomEmergency);
                result.success = true;
                result.data = jomEmergencyDTO;

            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            return result;
        }  

        public async Task<ServiceResponseModel<List<JOEmergencyReqDTO>>> GetJOEmergencyList()
        {
            ServiceResponseModel<List<JOEmergencyReqDTO>> result = new ServiceResponseModel<List<JOEmergencyReqDTO>>();

            try
            {
                var jomListEmergency = await _dbContext.JobOrderEmergency.OrderBy(x => x.JobOrderEmergency_Id).ToListAsync();
                var jomListEmergencyDTO = _mapper.Map<List<JOEmergencyReqDTO>>(jomListEmergency);
                result.success = true;
                result.data = jomListEmergencyDTO;

            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            return result;
        }

        public async Task<ServiceResponseModel<List<JOEmergencyDetailReqDTO>>> GetJOEmergencyDetail(long jobEId)
        {
            ServiceResponseModel<List<JOEmergencyDetailReqDTO>> result = new ServiceResponseModel<List<JOEmergencyDetailReqDTO>>();

            try
            {
                var jobEDtl = _dbContext.JobOrderEmergencyDetail.Where(d => d.JobOrderEmergency_Id == jobEId).ToList();
                var jobEDtlDTO = _mapper.Map<List<JOEmergencyDetailReqDTO>>(jobEDtl);
                result.success = true;
                result.data = jobEDtlDTO;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            return result;
        }

        public async Task<ServiceResponseModel<JOEmergencyReqDTO>> SaveEmergency(JOEmergencyReqDTO jobEmergency)
        {
            ServiceResponseModel<JOEmergencyReqDTO> result = new ServiceResponseModel<JOEmergencyReqDTO>();

            try
            {
                int index = 1;
                foreach (var dtl in jobEmergency.EmergencyDetails)
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

                var aggregatedSubItems = jobEmergency.EmergencyDetails
                    .GroupBy(x => x.Item_Id)
                    .Select(g => new
                    {
                        Item_Id = g.Key,
                        Qty = g.Sum(x => x.Qty),
                        JobOrderEmergencyDetail_Id = g.First().JobOrderEmergencyDetail_Id,
                        JobOrderEmergency_Id = g.First().JobOrderEmergency_Id,
                    }).ToList();

                if (jobEmergency.JobOrderEmergency_Id == 0)
                {
                    var jobOrderEmergency = await DocFormatHelper.Instance.get_NextDocumentNo(_dbContext, General.EnumConfiguration.DocFormat_EmergencyJO, DateTime.Now, true);
                    if (jobOrderEmergency.success == false)
                    {
                        result.errMessage = jobOrderEmergency.errMessage;
                        return result;
                    }

                    JobOrderEmergency _jobE = new JobOrderEmergency()
                    {
                        DocNo = jobOrderEmergency.data,
                        Description = string.IsNullOrEmpty(jobEmergency.Description) ? "" : jobEmergency.Description,
                        Status = EnumJobOrderStatus.Draft.ToString(),
                        DocDate = DateTime.Now,
                        CreatedBy = jobEmergency.CreatedBy,
                        CreatedDate = DateTime.Now,
                    };
                    _dbContext.JobOrderEmergency.Add(_jobE);
                    await _dbContext.SaveChangesAsync();

                    foreach (var dtl in aggregatedSubItems)
                    {
                        JobOrderEmergencyDetail _jobEDtl = new JobOrderEmergencyDetail()
                        {
                            JobOrderEmergency_Id = _jobE.JobOrderEmergency_Id,
                            Item_Id = dtl.Item_Id,
                            Qty = dtl.Qty,
                            BalQty = dtl.Qty,
                        };
                        _dbContext.JobOrderEmergencyDetail.Add(_jobEDtl);
                    }
                    await _dbContext.SaveChangesAsync();
                    result.success = true;
                }
                else
                {
                    JobOrderEmergency? _jobE = _dbContext.JobOrderEmergency.Find(jobEmergency.JobOrderEmergency_Id);
                    if (_jobE == null)
                    {
                        result.errMessage = "Cannot find this specified Job. Please refersh the list and try again.";
                        return result;
                    }

                    var existingDetails = _dbContext.JobOrderEmergencyDetail.Where(d => d.JobOrderEmergency_Id == _jobE.JobOrderEmergency_Id).ToList();
                    if (existingDetails.Count == 0)
                    {
                        result.errMessage = "Cannot find this specified Job Detail. Please refersh the list and try again.";
                        return result;
                    }

                    _jobE.Description = string.IsNullOrEmpty(jobEmergency.Description) ? "" : jobEmergency.Description;
                    _jobE.CreatedBy = jobEmergency.CreatedBy;
                    _dbContext.JobOrderEmergency.Update(_jobE);

                    //remove the previous if not found on the latest detail
                    var detailsToRemove = new List<JobOrderEmergencyDetail>();
                    foreach (var exist in existingDetails)
                    {
                        var updated = jobEmergency.EmergencyDetails.FirstOrDefault(d => d.JobOrderEmergencyDetail_Id == exist.JobOrderEmergencyDetail_Id);
                        if (updated == null)
                        {
                            detailsToRemove.Add(exist);
                        }
                    }

                    //update the latest detail
                    foreach (var dtl in aggregatedSubItems)
                    {
                        if (dtl.JobOrderEmergencyDetail_Id == 0)
                        {
                            JobOrderEmergencyDetail _jobEDtl = new JobOrderEmergencyDetail()
                            {
                                JobOrderEmergency_Id = _jobE.JobOrderEmergency_Id,
                                Item_Id = dtl.Item_Id,
                                Qty = dtl.Qty,
                                BalQty = dtl.Qty,
                            };
                            _dbContext.JobOrderEmergencyDetail.Add(_jobEDtl);
                        }
                        else
                        {
                            JobOrderEmergencyDetail? _jobEDtl = existingDetails.FirstOrDefault(d => d.JobOrderEmergency_Id == dtl.JobOrderEmergency_Id && d.JobOrderEmergencyDetail_Id == dtl.JobOrderEmergencyDetail_Id);
                            if (_jobEDtl == null)
                            {
                                result.errMessage = "Cannot find the specific Job Detail for update.";
                                return result;
                            }

                            _jobEDtl.Item_Id = dtl.Item_Id;
                            _jobEDtl.Qty = dtl.Qty;
                            _jobEDtl.BalQty = dtl.Qty;
                            _dbContext.JobOrderEmergencyDetail.Update(_jobEDtl);
                        }
                    }

                    if (detailsToRemove.Any())
                    {
                        _dbContext.JobOrderEmergencyDetail.RemoveRange(detailsToRemove);
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

        public async Task<ServiceResponseModel<JOEmergencyReqDTO>> DeleteEmergency(JOEmergencyReqDTO jobEmergency)
        {
            ServiceResponseModel<JOEmergencyReqDTO> result = new ServiceResponseModel<JOEmergencyReqDTO>();

            try
            {
                if (jobEmergency == null)
                {
                    result.errMessage = "Something wrong. please refresh ths list.";
                    return result;
                }

                JobOrderEmergency? _jobE = _dbContext.JobOrderEmergency.Find(jobEmergency.JobOrderEmergency_Id);
                if (_jobE == null)
                {
                    result.errMessage = "Cannot find this Job, please refresh the list.";
                    return result;
                }
                _dbContext.JobOrderEmergency.Remove(_jobE);

                var existingDetails = _dbContext.JobOrderEmergencyDetail.Where(d => d.JobOrderEmergency_Id == _jobE.JobOrderEmergency_Id).ToList();
                foreach (var dtl in existingDetails)
                {
                    _dbContext.JobOrderEmergencyDetail.Remove(dtl);
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

        public async Task<ServiceResponseModel<List<JORawMaterialStockCheckDTO>>> CheckJORawMaterialStock(long jobOrderId, bool includeQueue = true, bool includeEmergency = true, bool includeLoader = true)
        {
            ServiceResponseModel<List<JORawMaterialStockCheckDTO>> result = new ServiceResponseModel<List<JORawMaterialStockCheckDTO>>();

            try
            {
                // Get all job details for this job order
                var jobDetails = await _dbContext.JobOrderDetail
                    .Where(x => x.JobOrder_Id == jobOrderId)
                    .ToListAsync();

                if (!jobDetails.Any())
                {
                    result.success = true;
                    result.data = new List<JORawMaterialStockCheckDTO>();
                    return result;
                }

                var rawMaterialCheckList = new List<JORawMaterialStockCheckDTO>();

                // Build a dictionary to track reserved reels by item
                var (reservedReels, incomingLoaderReels) = await CalculateReservedReels(includeQueue, includeEmergency, includeLoader, jobOrderId, 0);

                foreach (var detail in jobDetails)
                {
                    // Get the finish good item
                    var finishGoodItem = await _dbContext.Item.FirstOrDefaultAsync(x => x.Item_Id == detail.Item_Id);
                    if (finishGoodItem == null) continue;

                    // Find the BOM for this finish good item
                    var bom = await _dbContext.BOM
                        .FirstOrDefaultAsync(x => x.Item_Id == detail.Item_Id && x.IsActive == true);

                    if (bom == null)
                    {
                        // No BOM found for this finish good, consider it unfulfillable
                        rawMaterialCheckList.Add(new JORawMaterialStockCheckDTO
                        {
                            JobOrderDetail_Id = detail.JobOrderDetail_Id,
                            FinishGoodItemCode = finishGoodItem.ItemCode,
                            FinishGoodItemDescription = finishGoodItem.Description,
                            FinishGoodQty = detail.Qty,
                            IsFulfillable = false,
                            RawMaterials = new List<RawMaterialStockDTO>(),
                            Shortages = new List<RawMaterialShortageDTO>()
                        });
                        continue;
                    }

                    // Get all BOM details (raw materials) for this BOM
                    var bomDetails = await _dbContext.BOMDetail
                        .Where(x => x.BOM_Id == bom.BOM_Id)
                        .ToListAsync();

                    var rawMaterials = new List<RawMaterialStockDTO>();
                    var shortages = new List<RawMaterialShortageDTO>();
                    bool isFulfillable = true;

                    // Track which reels are used within this BOM (for this job detail)
                    var usedReelsInThisBOM = new HashSet<Guid>();

                    // For each raw material in the BOM, check stock availability
                    foreach (var bomDetail in bomDetails)
                    {
                        var rawMaterialItem = await _dbContext.Item
                            .FirstOrDefaultAsync(x => x.Item_Id == bomDetail.Item_Id);

                        if (rawMaterialItem == null) continue;

                        int requiredQty = bomDetail.Qty * detail.Qty; // Required qty * finish good qty

                        // Get all available reels (ready status) for this raw material
                        var availableReels = await _dbContext.Reel
                            .Where(x => x.Item_Id == bomDetail.Item_Id &&
                                       x.NeedCheck == false &&
                                       x.IsReady == true &&
                                       x.Status == EnumReelStatus.IsReady.ToString() &&
                                       x.ExpiryDate.Date >= DateTime.Now.Date)
                            .OrderBy(x => x.ExpiryDate)
                            .ToListAsync();

                        // Reels currently being Hub In'd by a Loader queue entry ahead of this job
                        // will already be on the rack by the time this job's turn comes.
                        availableReels.AddRange(incomingLoaderReels.Where(x => x.Item_Id == bomDetail.Item_Id));
                        availableReels = availableReels.OrderBy(x => x.ExpiryDate).ToList();

                        Reel? allocatedReel = null;

                        foreach (var reel in availableReels)
                        {
                            // Check if this reel is already reserved for another raw material (from queue/emergency jobs)
                            if (reservedReels.ContainsKey(reel.Reel_Id))
                                continue; // Skip this reel, it's reserved by queue/emergency

                            // Check if this reel is already used for another raw material in THIS BOM
                            if (usedReelsInThisBOM.Contains(reel.Reel_Id))
                                continue; // Skip this reel, it's already allocated to another raw material

                            // Only allocate if reel qty meets the requirement
                            if (reel.Qty >= requiredQty)
                            {
                                allocatedReel = reel; // Collect reel code
                                usedReelsInThisBOM.Add(reel.Reel_Id); // Mark as used in this BOM
                                reservedReels[reel.Reel_Id] = reel.Item_Id; // Reserve this reel globally
                                break; // Reel fully satisfies requirement
                            }
                        }

                        bool hasSufficientStock = allocatedReel == null ? false : (allocatedReel?.Qty >= requiredQty);

                        rawMaterials.Add(new RawMaterialStockDTO
                        {
                            RawMaterial_Id = rawMaterialItem.Item_Id,
                            RawMaterialItemCode = rawMaterialItem.ItemCode,
                            RawMaterialItemDescription = rawMaterialItem.Description,
                            RequiredQty = requiredQty,
                            AvailableQty = allocatedReel?.Qty ?? 0,
                            HasSufficientStock = hasSufficientStock,
                            AllocatedReelCodes = allocatedReel?.ReelCode ?? "", // Join reel codes with comma
                            AllocatedReelCount = 1
                        });

                        // Track shortages
                        if (!hasSufficientStock)
                        {
                            isFulfillable = false;
                            shortages.Add(new RawMaterialShortageDTO
                            {
                                RawMaterial_Id = rawMaterialItem.Item_Id,
                                RawMaterialItemCode = rawMaterialItem.ItemCode,
                                RawMaterialItemDescription = rawMaterialItem.Description,
                                RequiredQty = requiredQty,
                                AvailableQty = 0,
                                ShortageQty = requiredQty
                            });
                        }
                    }

                    rawMaterialCheckList.Add(new JORawMaterialStockCheckDTO
                    {
                        JobOrderDetail_Id = detail.JobOrderDetail_Id,
                        FinishGoodItemCode = finishGoodItem.ItemCode,
                        FinishGoodItemDescription = finishGoodItem.Description,
                        FinishGoodQty = detail.Qty,
                        IsFulfillable = isFulfillable,
                        RawMaterials = rawMaterials,
                        Shortages = shortages
                    });
                }

                result.success = true;
                result.data = rawMaterialCheckList;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<List<JORawMaterialStockCheckDTO>>> CheckJOERawMaterialStock(long jobOrderEId, bool includeQueue = true, bool includeEmergency = true, bool includeLoader = true)
        {
            ServiceResponseModel<List<JORawMaterialStockCheckDTO>> result = new ServiceResponseModel<List<JORawMaterialStockCheckDTO>>();

            try
            {
                // Get all job details for this job order
                var jobDetails = await _dbContext.JobOrderEmergencyDetail
                    .Where(x => x.JobOrderEmergency_Id == jobOrderEId)
                    .ToListAsync();

                if (!jobDetails.Any())
                {
                    result.success = true;
                    result.data = new List<JORawMaterialStockCheckDTO>();
                    return result;
                }

                var rawMaterialCheckList = new List<JORawMaterialStockCheckDTO>();

                // Build a dictionary to track reserved reels by item
                var (reservedReels, incomingLoaderReels) = await CalculateReservedReels(includeQueue, includeEmergency, includeLoader, 0, jobOrderEId);

                foreach (var detail in jobDetails)
                {
                    var rawMaterials = new List<RawMaterialStockDTO>();
                    var shortages = new List<RawMaterialShortageDTO>();

                    var rawMaterialItem = await _dbContext.Item
                        .FirstOrDefaultAsync(x => x.Item_Id == detail.Item_Id);

                    if (rawMaterialItem == null) continue;

                    int requiredQty = detail.Qty; // Required qty 

                    // Get all available reels (ready status) for this raw material
                    var availableReels = await _dbContext.Reel
                        .Where(x => x.Item_Id == detail.Item_Id &&
                                   x.NeedCheck == false &&
                                   x.IsReady == true &&
                                   x.Status == EnumReelStatus.IsReady.ToString() &&
                                   x.ExpiryDate.Date >= DateTime.Now.Date)
                        .OrderBy(x => x.ExpiryDate)
                        .ToListAsync();

                    // Reels currently being Hub In'd by a Loader queue entry ahead of this job
                    // will already be on the rack by the time this job's turn comes.
                    availableReels.AddRange(incomingLoaderReels.Where(x => x.Item_Id == detail.Item_Id));
                    availableReels = availableReels.OrderBy(x => x.ExpiryDate).ToList();

                    Reel? allocatedReel = null;

                    foreach (var reel in availableReels)
                    {
                        // Check if this reel is already reserved for another raw material (from queue/emergency jobs)
                        if (reservedReels.ContainsKey(reel.Reel_Id))
                            continue; // Skip this reel, it's reserved by queue/emergency

                        // Only allocate if reel qty meets the requirement
                        if (reel.Qty >= requiredQty)
                        {
                            allocatedReel = reel; // Collect reel code
                            reservedReels[reel.Reel_Id] = reel.Item_Id; // Reserve this reel globally
                            break; // Reel fully satisfies requirement
                        }
                    }

                    bool hasSufficientStock = allocatedReel == null ? false : (allocatedReel?.Qty >= requiredQty);

                    rawMaterials.Add(new RawMaterialStockDTO
                    {
                        RawMaterial_Id = rawMaterialItem.Item_Id,
                        RawMaterialItemCode = rawMaterialItem.ItemCode,
                        RawMaterialItemDescription = rawMaterialItem.Description,
                        RequiredQty = requiredQty,
                        AvailableQty = allocatedReel?.Qty ?? 0,
                        HasSufficientStock = hasSufficientStock,
                        AllocatedReelCodes = allocatedReel?.ReelCode ?? "", // Join reel codes with comma
                        AllocatedReelCount = 1
                    });

                    // Track shortages
                    if (!hasSufficientStock)
                    {
                        shortages.Add(new RawMaterialShortageDTO
                        {
                            RawMaterial_Id = rawMaterialItem.Item_Id,
                            RawMaterialItemCode = rawMaterialItem.ItemCode,
                            RawMaterialItemDescription = rawMaterialItem.Description,
                            RequiredQty = requiredQty,
                            AvailableQty = 0,
                            ShortageQty = requiredQty
                        });
                    }

                    // Emergency JO details request a raw material directly (no BOM/finish good),
                    // so the detail itself is shown as the group row with its single raw material.
                    rawMaterialCheckList.Add(new JORawMaterialStockCheckDTO
                    {
                        JobOrderDetail_Id = detail.JobOrderEmergencyDetail_Id,
                        FinishGoodItemCode = rawMaterialItem.ItemCode,
                        FinishGoodItemDescription = rawMaterialItem.Description,
                        FinishGoodQty = detail.Qty,
                        IsFulfillable = hasSufficientStock,
                        RawMaterials = rawMaterials,
                        Shortages = shortages
                    });
                }

                result.success = true;
                result.data = rawMaterialCheckList;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        private async Task<(Dictionary<Guid, long> ReservedReels, List<Reel> IncomingLoaderReels)> CalculateReservedReels(bool includeQueue, bool includeEmergency, bool includeLoader, long skipJobId, long skipJobEId)
        {
            var reservedReels = new Dictionary<Guid, long>(); // Reel_Id -> Item_Id (which item type reserved it)
            var incomingLoaderReels = new List<Reel>(); // Reels being Hub In'd by a Loader queue entry ahead of the job being checked

            try
            {
                // Determine the queue sequence (Idx) of the job being simulated, so only
                // higher-priority (earlier-sequence) queue entries reserve reels ahead of it.
                // A job not currently in the queue is treated as lowest priority (reserve everything).
                int currentIdx = int.MaxValue;
                if (skipJobId != 0)
                {
                    var currentQ = await _dbContext.RackJobQueue
                        .Where(x => x.DocType == EnumQueueDocType.JO.ToString() && x.Doc_Id == skipJobId)
                        .FirstOrDefaultAsync();
                    if (currentQ != null) currentIdx = currentQ.Idx;
                }
                else if (skipJobEId != 0)
                {
                    var currentQ = await _dbContext.RackJobQueue
                        .Where(x => x.DocType == EnumQueueDocType.JOE.ToString() && x.Doc_Id == skipJobEId)
                        .FirstOrDefaultAsync();
                    if (currentQ != null) currentIdx = currentQ.Idx;
                }

                if (includeQueue)
                {
                    // Get all pending job orders and allocate their reels
                    var pendingJobOrders = await _dbContext.RackJobQueue
                        .Where(x => x.DocType == EnumQueueDocType.JO.ToString())
                        .Select(x => new { x.Doc_Id, x.Idx })
                        .ToListAsync();

                    foreach (var q in pendingJobOrders)
                    {
                        if (q.Doc_Id == skipJobId)
                            continue;

                        // Only reserve for queue entries ahead of (earlier sequence than) the job being checked
                        if (q.Idx >= currentIdx)
                            continue;

                        var jobId = q.Doc_Id;

                        // when add in queue will auto create JobOrderRaws list from BOM
                        var jobRaws = await _dbContext.JobOrderRaws
                            .Where(x => x.JobOrder_Id == jobId)
                            .ToListAsync();

                        // For each job raw, track used reels in its BOM (single-use allocation)
                        foreach (var raw in jobRaws)
                        {
                            // Track reels used within this BOM (same as current job logic)
                            var usedReelsInThisBOM = new HashSet<Guid>();

                            int requiredQty = raw.BalQty;
                            var availableReels = await _dbContext.Reel
                                .Where(x => x.Item_Id == raw.Item_Id &&
                                           x.NeedCheck == false &&
                                           x.IsReady == true &&
                                           x.Status == EnumReelStatus.IsReady.ToString() &&
                                           x.ExpiryDate.Date >= DateTime.Now.Date)
                                //!reservedReels.ContainsKey(x.Reel_Id) && // Not reserved by earlier queue/emergency jobs
                                //!usedReelsInThisBOM.Contains(x.Reel_Id)) // Not used by earlier raw material in this BOM
                                .OrderBy(x => x.ExpiryDate)
                                .ToListAsync();

                            int allocatedQty = 0;
                            foreach (var reel in availableReels)
                            {
                                // Double-check not already reserved/used
                                if (!reservedReels.ContainsKey(reel.Reel_Id) && !usedReelsInThisBOM.Contains(reel.Reel_Id))
                                {
                                    // Only allocate if reel qty meets the requirement
                                    if (reel.Qty >= requiredQty)
                                    {
                                        reservedReels[reel.Reel_Id] = raw.Item_Id; // Mark reel as reserved
                                        usedReelsInThisBOM.Add(reel.Reel_Id); // Mark as used in this BOM
                                        allocatedQty = reel.Qty;
                                        break; // Reel fully satisfies requirement
                                    }
                                }
                            }
                        }
                    }
                }

                if (includeEmergency)
                {
                    // Get all pending emergency job orders and allocate their reels
                    var pendingEmergencies = await _dbContext.RackJobQueue
                        .Where(x => x.DocType == EnumQueueDocType.JOE.ToString())
                        .Select(x => new { x.Doc_Id, x.Idx })
                        .ToListAsync();

                    foreach (var q in pendingEmergencies)
                    {
                        if (q.Doc_Id == skipJobEId)
                            continue;

                        // Only reserve for queue entries ahead of (earlier sequence than) the job being checked
                        if (q.Idx >= currentIdx)
                            continue;

                        var emergId = q.Doc_Id;

                        var emergDetails = await _dbContext.JobOrderEmergencyDetail
                            .Where(x => x.JobOrderEmergency_Id == emergId)
                            .ToListAsync();

                        // For each emergency detail, track used reels in its BOM (single-use allocation)
                        foreach (var detail in emergDetails)
                        {
                            // Track reels used within this BOM (same as current job logic)
                            var usedReelsInThisBOM = new HashSet<Guid>();

                            int requiredQty = detail.Qty;
                            var availableReels = await _dbContext.Reel
                                .Where(x => x.Item_Id == detail.Item_Id &&
                                           x.NeedCheck == false &&
                                           x.IsReady == true &&
                                           x.Status == EnumReelStatus.IsReady.ToString() &&
                                           x.ExpiryDate.Date >= DateTime.Now.Date)
                                //!reservedReels.ContainsKey(x.Reel_Id) && // Not reserved by earlier queue/emergency jobs
                                //!usedReelsInThisBOM.Contains(x.Reel_Id)) // Not used by earlier raw material in this BOM
                                .OrderBy(x => x.CreatedDate)
                                .ToListAsync();

                            int allocatedQty = 0;
                            foreach (var reel in availableReels)
                            {
                                // Double-check not already reserved/used
                                if (!reservedReels.ContainsKey(reel.Reel_Id) && !usedReelsInThisBOM.Contains(reel.Reel_Id))
                                {
                                    // Only allocate if reel qty meets the requirement
                                    if (reel.Qty >= requiredQty)
                                    {
                                        reservedReels[reel.Reel_Id] = detail.Item_Id; // Mark reel as reserved
                                        usedReelsInThisBOM.Add(reel.Reel_Id); // Mark as used in this BOM
                                        allocatedQty = reel.Qty;
                                        break; // Reel fully satisfies requirement
                                    }
                                }
                            }
                        }
                    }
                }

                if (includeLoader)
                {
                    // Get all pending Hub In (Loader) queue entries ahead of the job being checked.
                    // Their reels are still WaitingLoader/InLoader (not yet IsReady), but will already
                    // be on the rack by the time this job's turn comes, so count them as available too.
                    var pendingLoaders = await _dbContext.RackJobQueue
                        .Where(x => x.DocType == EnumQueueDocType.Loader.ToString())
                        .Select(x => new { x.Doc_Id, x.Idx })
                        .ToListAsync();

                    foreach (var q in pendingLoaders)
                    {
                        if (q.Idx >= currentIdx)
                            continue;

                        var pendingReelIds = await _dbContext.LoaderReel
                            .Where(x => x.Loader_Id == q.Doc_Id)
                            .Select(x => x.Reel_Id)
                            .ToListAsync();

                        if (!pendingReelIds.Any())
                            continue;

                        var pendingReels = await _dbContext.Reel
                            .Where(x => pendingReelIds.Contains(x.Reel_Id) &&
                                       x.NeedCheck == false &&
                                       x.ExpiryDate.Date >= DateTime.Now.Date)
                            .ToListAsync();

                        incomingLoaderReels.AddRange(pendingReels);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw - return partial reserved dict
            }

            return (reservedReels, incomingLoaderReels);
        }
    }
}
