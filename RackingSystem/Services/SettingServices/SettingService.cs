using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.Models.Item;
using RackingSystem.Models;
using RackingSystem.Models.Setting;
using RackingSystem.Data.Maintenances;
using RackingSystem.Models.Slot;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RackingSystem.Services.SettingServices
{
    public class SettingService : ISettingService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public SettingService(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<ServiceResponseModel<List<ReelDimensionListDTO>>> GetReelDimensionList()
        {
            ServiceResponseModel<List<ReelDimensionListDTO>> result = new ServiceResponseModel<List<ReelDimensionListDTO>>();

            try
            {
                var itemList = await _dbContext.ReelDimension.OrderBy(x => x.Thickness).ToListAsync();
                var itemListDTO = _mapper.Map<List<ReelDimensionListDTO>>(itemList).ToList();
                result.success = true;
                result.data = itemListDTO;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<ReelDimensionDTO>> SaveReelDimension(ReelDimensionDTO req)
        {
            ServiceResponseModel<ReelDimensionDTO> result = new ServiceResponseModel<ReelDimensionDTO>();

            try
            {
                // 1. checking Data
                if (req == null)
                {
                    result.errMessage = "Please insert Thickness.";
                    return result;
                }
                if (req.Thickness == 0)
                {
                    result.errMessage = "Please insert Thickness.";
                    return result;
                }
                if (req.Width == 0)
                {
                    result.errMessage = "Please insert Width.";
                    return result;
                }
                if (req.MaxThickness == 0)
                {
                    result.errMessage = "Please insert MaxThickness.";
                    return result;
                }
                if (req.MaxThickness < req.Thickness)
                {
                    result.errMessage = "MaxThickness cannot less than Thickness.";
                    return result;
                }
                if (req.ReelDimension_Id == 0)
                {
                    ReelDimension? rExist = _dbContext.ReelDimension.FirstOrDefault(x => x.Thickness == req.Thickness);
                    if (rExist != null)
                    {
                        result.errMessage = "This Thickness has exist.";
                        return result;
                    }
                }
                else
                {
                    ReelDimension? rExist = _dbContext.ReelDimension.FirstOrDefault(x => x.Thickness == req.Thickness && x.ReelDimension_Id != req.ReelDimension_Id);
                    if (rExist != null)
                    {
                        result.errMessage = "This Thickness has exist.";
                        return result;
                    }
                }

                // 2. save Data
                if (req.ReelDimension_Id == 0)
                {
                    ReelDimension _r = new ReelDimension()
                    {
                        Thickness = req.Thickness,
                        Width = req.Width,
                        MaxThickness = req.MaxThickness,
                    };
                    _dbContext.ReelDimension.Add(_r);
                }
                else
                {
                    ReelDimension? _r = _dbContext.ReelDimension.Find(req.ReelDimension_Id);
                    if (_r == null)
                    {
                        result.errMessage = "Cannot find this dimension, please refresh the list.";
                        return result;
                    }
                    _r.Thickness = req.Thickness;
                    _r.Width = req.Width;
                    _r.MaxThickness = req.MaxThickness;
                    _dbContext.ReelDimension.Update(_r);
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

        public async Task<ServiceResponseModel<ReelDimensionDTO>> DeleteReelDimension(ReelDimensionDTO itemReq)
        {
            ServiceResponseModel<ReelDimensionDTO> result = new ServiceResponseModel<ReelDimensionDTO>();

            try
            {
                // 1. checking Data
                if (itemReq == null)
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
                ReelDimension? _item = _dbContext.ReelDimension.Find(itemReq.ReelDimension_Id);
                if (_item == null)
                {
                    result.errMessage = "Cannot find this item, please refresh the list.";
                    return result;
                }
                _dbContext.ReelDimension.Remove(_item);
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

        public async Task<ServiceResponseModel<List<ReelDimensionListDTO>>> GetReelDimensionList_DDL()
        {
            ServiceResponseModel<List<ReelDimensionListDTO>> result = new ServiceResponseModel<List<ReelDimensionListDTO>>();

            try
            {
                var itemList = await _dbContext.ReelDimension.Select(s => new ReelDimensionListDTO
                                {
                                    ReelDimension_Id = s.ReelDimension_Id,
                                    Description = s.Thickness + " mm X " + s.Width + " inch "
                                }).OrderBy(x => x.Thickness).ToListAsync();
                result.success = true;
                result.data = itemList;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<List<SlotCalculationListDTO>>> GetSlotCalculationList()
        {
            ServiceResponseModel<List<SlotCalculationListDTO>> result = new ServiceResponseModel<List<SlotCalculationListDTO>>();

            try
            {
                var itemList = await _dbContext.SlotCalculation.OrderBy(x => x.MaxThickness).ToListAsync();
                var itemListDTO = _mapper.Map<List<SlotCalculationListDTO>>(itemList).ToList();
                result.success = true;
                result.data = itemListDTO;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<SlotCalculationDTO>> SaveSlotCalculation(SlotCalculationDTO req)
        {
            ServiceResponseModel<SlotCalculationDTO> result = new ServiceResponseModel<SlotCalculationDTO>();

            try
            {
                // 1. checking Data
                if (req == null)
                {
                    result.errMessage = "Please insert MaxThickness.";
                    return result;
                }
                if (req.MaxThickness == 0)
                {
                    result.errMessage = "Please insert MaxThickness.";
                    return result;
                }
                if (req.ReserveSlot == 0)
                {
                    result.errMessage = "Please insert Reserve Slot.";
                    return result;
                }
                if (req.SlotCalculation_Id == 0)
                {
                    SlotCalculation? rExist = _dbContext.SlotCalculation.FirstOrDefault(x => x.MaxThickness == req.MaxThickness);
                    if (rExist != null)
                    {
                        result.errMessage = "This MaxThickness has exist.";
                        return result;
                    }
                }
                else
                {
                    SlotCalculation? rExist = _dbContext.SlotCalculation.FirstOrDefault(x => x.MaxThickness == req.MaxThickness && x.SlotCalculation_Id != req.SlotCalculation_Id);
                    if (rExist != null)
                    {
                        result.errMessage = "This MaxThickness has exist.";
                        return result;
                    }
                }

                // 2. save Data
                if (req.SlotCalculation_Id == 0)
                {
                    SlotCalculation _r = new SlotCalculation()
                    {
                        MaxThickness = req.MaxThickness,
                        ReserveSlot = req.ReserveSlot,
                    };
                    _dbContext.SlotCalculation.Add(_r);
                }
                else
                {
                    SlotCalculation? _r = _dbContext.SlotCalculation.Find(req.SlotCalculation_Id);
                    if (_r == null)
                    {
                        result.errMessage = "Cannot find this dimension, please refresh the list.";
                        return result;
                    }
                    _r.MaxThickness = req.MaxThickness;
                    _r.ReserveSlot = req.ReserveSlot;
                    _dbContext.SlotCalculation.Update(_r);
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

        public async Task<ServiceResponseModel<SlotCalculationDTO>> DeleteSlotCalculation(SlotCalculationDTO itemReq)
        {
            ServiceResponseModel<SlotCalculationDTO> result = new ServiceResponseModel<SlotCalculationDTO>();

            try
            {
                // 1. checking Data
                if (itemReq == null)
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
                SlotCalculation? _item = _dbContext.SlotCalculation.Find(itemReq.SlotCalculation_Id);
                if (_item == null)
                {
                    result.errMessage = "Cannot find this item, please refresh the list.";
                    return result;
                }
                _dbContext.SlotCalculation.Remove(_item);
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
