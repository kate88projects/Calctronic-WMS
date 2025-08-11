using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.Data.Maintenances;
using RackingSystem.Models;
using RackingSystem.Models.Slot;

namespace RackingSystem.Services.SlotServices
{
    public class SlotService : ISlotService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public SlotService(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<ServiceResponseModel<List<SlotListDTO>>> GetSlotList()
        {
            ServiceResponseModel<List<SlotListDTO>> result = new ServiceResponseModel<List<SlotListDTO>>();

            try
            {
                var slotList = await _dbContext.Slot.OrderBy(x => x.SlotCode).ToListAsync();
                var slotListDTO = _mapper.Map<List<SlotListDTO>>(slotList).ToList();
                result.success = true;
                result.data = slotListDTO;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<SlotDTO>> SaveSlot(SlotDTO slotReq)
        {
            ServiceResponseModel<SlotDTO> result = new ServiceResponseModel<SlotDTO>();

            try
            {
                // 1. checking Data
                if (slotReq == null)
                {
                    result.errMessage = "Please insert Slot Code.";
                    return result;
                }
                if (string.IsNullOrEmpty(slotReq.SlotCode))
                {
                    result.errMessage = "Please insert Slot Code.";
                    return result;
                }
                if (slotReq.ColNo == null)
                {
                    result.errMessage = "Please insert Column No.";
                    return result;
                }
                if (slotReq.RowNo == null)
                {
                    result.errMessage = "Please insert Row No.";
                    return result;
                }
                if (slotReq.Slot_Id == 0)
                {
                    Slot? slotExist = _dbContext.Slot.FirstOrDefault(x => x.SlotCode == slotReq.SlotCode);
                    if (slotExist != null)
                    {
                        result.errMessage = "This slot code has exist.";
                        return result;
                    }
                }
                else
                {
                    Slot? slotExist = _dbContext.Slot.FirstOrDefault(x => x.SlotCode == slotReq.SlotCode && x.Slot_Id != slotReq.Slot_Id);
                    if (slotExist != null)
                    {
                        result.errMessage = "This slot code has been used.";
                        return result;
                    }
                }
                Slot? slotExist2 = _dbContext.Slot.FirstOrDefault(x => x.ColNo == slotReq.ColNo && x.RowNo == slotReq.RowNo && x.Slot_Id != slotReq.Slot_Id);
                if (slotExist2 != null)
                {
                    result.errMessage = "This Column No and Row No has been used.";
                    return result;
                }

                // 2. save Data
                if (slotReq.Slot_Id == 0)
                {
                    Slot _slot = new Slot()
                    {
                        SlotCode = slotReq.SlotCode,
                        ColNo = slotReq.ColNo,
                        RowNo = slotReq.RowNo ,
                        IsActive = slotReq.IsActive,
                    };
                    _dbContext.Slot.Add(_slot);
                }
                else
                {
                    Slot? _slot = _dbContext.Slot.Find(slotReq.Slot_Id);
                    if (_slot == null)
                    {
                        result.errMessage = "Cannot find this slot, please refresh the list.";
                        return result;
                    }
                    _slot.SlotCode = slotReq.SlotCode;
                    _slot.ColNo = slotReq.ColNo;
                    _slot.RowNo = slotReq.RowNo;
                    _slot.IsActive = slotReq.IsActive;
                    _dbContext.Slot.Update(_slot);
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

        public async Task<ServiceResponseModel<SlotDTO>> DeleteSlot(SlotDTO slotReq)
        {
            ServiceResponseModel<SlotDTO> result = new ServiceResponseModel<SlotDTO>();

            try
            {
                // 1. checking Data
                if (slotReq == null)
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
                Slot? _slot = _dbContext.Slot.Find(slotReq.Slot_Id);
                if (_slot == null)
                {
                    result.errMessage = "Cannot find this slot, please refresh the list.";
                    return result;
                }
                _dbContext.Slot.Remove(_slot);
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

        public async Task<ServiceResponseModel<List<SlotListDTO>>> GetSlotStatus_ByColumn(int req)
        {
            ServiceResponseModel<List<SlotListDTO>> result = new ServiceResponseModel<List<SlotListDTO>>();

            try
            {
                var slotList = await _dbContext.Slot.Where(x => x.ColNo == req).OrderBy(x => x.RowNo).ToListAsync();
                var slotListDTO = _mapper.Map<List<SlotListDTO>>(slotList).ToList();
                result.success = true;
                result.data = slotListDTO;
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
