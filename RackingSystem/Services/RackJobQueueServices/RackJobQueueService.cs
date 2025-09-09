using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.Models.GRN;
using RackingSystem.Models;
using RackingSystem.Models.RackJobQueue;
using RackingSystem.Data.Maintenances;
using RackingSystem.Models.Item;
using RackingSystem.General;
using Microsoft.CodeAnalysis.Elfie.Model.Tree;
using RackingSystem.Data.RackJobQueue;
using RackingSystem.Data.JO;

namespace RackingSystem.Services.RackJobQueueServices
{
    public class RackJobQueueService : IRackJobQueueService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public RackJobQueueService(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<ServiceResponseModel<List<QListDTO>>> GetQueueList()
        {
            ServiceResponseModel<List<QListDTO>> result = new ServiceResponseModel<List<QListDTO>>();

            try
            {
                string sql = "EXECUTE dbo.Queue_GET_LIST ";
                var qListDTO = await _dbContext.SP_QueueList.FromSqlRaw(sql).ToListAsync();

                result.success = true;
                result.data = qListDTO;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<QReqDTO>> AddQueue(QReqDTO req)
        {
            ServiceResponseModel<QReqDTO> result = new ServiceResponseModel<QReqDTO>();

            try
            {
                // 1. checking Data
                if (req == null)
                {
                    result.errMessage = "Please select Task Type.";
                    return result;
                }
                RackJobQueue? iExist = _dbContext.RackJobQueue.FirstOrDefault(x => x.Doc_Id == req.Doc_Id && x.DocType == req.DocType);
                if (iExist != null)
                {
                    result.errMessage = "This Task has been created.";
                    return result;
                }
                if (req.DocType == EnumQueueDocType.Loader.ToString())
                {
                    Loader? loader = _dbContext.Loader.FirstOrDefault(x => x.Loader_Id == req.Doc_Id);
                    if (loader == null)
                    {
                        result.errMessage = "Cannot find this Auto Loader.";
                        return result;
                    }
                    LoaderReel? loaderReelList = _dbContext.LoaderReel.FirstOrDefault(x => x.Loader_Id == req.Doc_Id);
                    if (loaderReelList == null)
                    {
                        result.errMessage = "This Auto Loader don't have any Reels to unload.";
                        return result;
                    }
                }
                else if (req.DocType == EnumQueueDocType.JOE.ToString())
                {

                }
                else
                {
                    JobOrder? jobOrder = _dbContext.JobOrder.FirstOrDefault(x => x.JobOrder_Id == req.Doc_Id);
                    if (jobOrder == null)
                    {
                        result.errMessage = "Cannot find this Job Order.";
                        return result;
                    }
                    if (jobOrder.Status != EnumJobOrderStatus.Draft.ToString())
                    {
                        result.errMessage = "This Job Order is in [" + jobOrder.Status + "] mode.";
                        return result;
                    }
                    JobOrderDetail? jobOrderDtlList = _dbContext.JobOrderDetail.FirstOrDefault(x => x.JobOrder_Id == req.Doc_Id);
                    if (jobOrderDtlList == null)
                    {
                        result.errMessage = "This Job Orderdon't have any Product need pick up.";
                        return result;
                    }
                }

                // 2. save Data
                RackJobQueue? lastIdx = _dbContext.RackJobQueue.OrderByDescending(x => x.Idx).FirstOrDefault();
                RackJobQueue _queue = new RackJobQueue()
                {
                    Doc_Id = req.Doc_Id,
                    DocType = req.DocType,
                    Idx = lastIdx == null ? 1 : (lastIdx.Idx + 1),
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                };
                _dbContext.RackJobQueue.Add(_queue);

                if (req.DocType == EnumQueueDocType.JO.ToString())
                {
                    List<BOMDetail> bdList = new List<BOMDetail>();
                    List<JobOrderDetail> jodtlList = _dbContext.JobOrderDetail.Where(x => x.JobOrder_Id == req.Doc_Id).ToList();
                    foreach (var jodtl in jodtlList)
                    {
                        bdList = _dbContext.BOMDetail.Where(x => x.BOM_Id == jodtl.BOM_Id).ToList();
                        foreach (var bd in bdList)
                        {
                            JobOrderRaws _raw = new JobOrderRaws()
                            {
                                JobOrderRaws_Id = new Guid(),
                                JobOrderDetail_Id = jodtl.JobOrderDetail_Id,
                                JobOrder_Id = jodtl.JobOrder_Id,
                                BOM_Id = jodtl.BOM_Id,
                                Item_Id = bd.Item_Id,
                                BaseQty = bd.Qty,
                                Qty = bd.Qty * jodtl.Qty,
                                BalQty = bd.Qty * jodtl.Qty,
                            };
                            _dbContext.JobOrderRaws.Add(_raw);

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

        public async Task<ServiceResponseModel<QReqDTO>> DeleteQueue(QReqDTO req)
        {
            ServiceResponseModel<QReqDTO> result = new ServiceResponseModel<QReqDTO>();

            try
            {
                // 1. checking Data
                if (req == null)
                {
                    result.errMessage = "Please select Task Type.";
                    return result;
                }
                RackJobQueue? iExist = _dbContext.RackJobQueue.FirstOrDefault(x => x.Doc_Id == req.Doc_Id && x.DocType == req.DocType);
                if (iExist == null)
                {
                    result.errMessage = "This Task not exist.";
                    return result;
                }
                if (req.DocType == EnumQueueDocType.Loader.ToString())
                {

                }
                else if (req.DocType == EnumQueueDocType.JOE.ToString())
                {

                }
                else
                {
                    var existingDetails = _dbContext.JobOrderRaws.Where(d => d.JobOrder_Id == req.Doc_Id && d.BalQty < d.Qty).FirstOrDefault();
                    if (iExist != null)
                    {
                        result.errMessage = "This Task has loaded cannot remove.";
                        return result;
                    }
                }

                // 2. save Data
                RackJobQueue? _q = _dbContext.RackJobQueue.Where(x => x.Doc_Id == req.Doc_Id && x.DocType == req.DocType).FirstOrDefault();
                if (_q == null)
                {
                    result.errMessage = "Cannot find this task, please refresh the list.";
                    return result;
                }
                _dbContext.RackJobQueue.Remove(_q);
                await _dbContext.SaveChangesAsync();

                if (req.DocType == EnumQueueDocType.JO.ToString())
                {
                    var existingDetails = _dbContext.JobOrderRaws.Where(d => d.JobOrder_Id == req.Doc_Id).ToList();
                    foreach (var dtl in existingDetails)
                    {
                        _dbContext.JobOrderRaws.Remove(dtl);
                    }
                    await _dbContext.SaveChangesAsync();
                }

                result.success = true;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<QReqDTO>> ChangeQueue(QReqDTO req)
        {
            ServiceResponseModel<QReqDTO> result = new ServiceResponseModel<QReqDTO>();

            try
            {
                // 1. checking Data
                if (req == null)
                {
                    result.errMessage = "Please select Task Type.";
                    return result;
                }
                if (req.NewIdx == 1)
                {
                    result.errMessage = "Only Task 2 and above can change queue.";
                    return result;
                }
                RackJobQueue? curQ = _dbContext.RackJobQueue.FirstOrDefault(x => x.Doc_Id == req.Doc_Id && x.DocType == req.DocType);
                if (curQ == null)
                {
                    result.errMessage = "This Task not exist.";
                    return result;
                }
                if (curQ.Idx == req.NewIdx)
                {
                    result.errMessage = "This take is in queue [" + req.NewIdx + "].";
                    return result;
                }
                RackJobQueue? otherQ = _dbContext.RackJobQueue.FirstOrDefault(x => x.Idx == req.NewIdx);
                if (otherQ == null)
                {
                    result.errMessage = "This Task [" + req.NewIdx + "] is not exist.";
                    return result;
                }

                // 2. save Data
                otherQ.Idx = curQ.Idx;
                await _dbContext.SaveChangesAsync();

                curQ.Idx = req.NewIdx;
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

        public async Task<ServiceResponseModel<List<JobOrder>>> GetJOList_PendingToUnLoad()
        {
            ServiceResponseModel<List<JobOrder>> result = new ServiceResponseModel<List<JobOrder>>();

            try
            {
                var joList = await _dbContext.JobOrder.Where(x => x.Status == EnumJobOrderStatus.Draft.ToString()).ToListAsync();

                result.success = true;
                result.data = joList;
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
