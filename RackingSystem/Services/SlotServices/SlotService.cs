using AutoMapper;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.Data.Maintenances;
using RackingSystem.Models;
using RackingSystem.Models.Slot;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;

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
                if (slotReq.XPulse <= 0)
                {
                    result.errMessage = "Please insert X Pulse and negative value is not accepted";
                    return result;
                }
                if (slotReq.YPulse <= 0)
                {
                    result.errMessage = "Please insert Y Pulse.";
                    return result;
                }
                if (slotReq.QRXPulse <= 0)
                {
                    result.errMessage = "Please insert Qr-X Pulse.";
                    return result;
                }
                if (slotReq.QRYPulse <= 0)
                {
                    result.errMessage = "Please insert Qr-Y Pulse.";
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
                        RowNo = slotReq.RowNo,
                        IsActive = slotReq.IsActive,
                        HasEmptyDrawer = slotReq.HasEmptyDrawer,
                        HasReel = slotReq.HasReel,
                        XPulse = slotReq.XPulse,
                        YPulse = slotReq.YPulse,
                        QRXPulse = slotReq.QRXPulse,
                        QRYPulse = slotReq.QRYPulse,
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
                    _slot.XPulse = slotReq.XPulse;
                    _slot.YPulse = slotReq.YPulse;
                    _slot.QRXPulse = slotReq.QRXPulse;
                    _slot.QRYPulse = slotReq.QRYPulse;
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

        public async Task<ServiceResponseModel<List<SlotListDTO>>> SaveExcelSlot(List<SlotListDTO> slots)
        {
            ServiceResponseModel<List<SlotListDTO>> result = new ServiceResponseModel<List<SlotListDTO>>();
            List<SlotListDTO> errorsLine = new List<SlotListDTO>(); // Change List<object> to List<SlotListDTO>

            try
            {
                if (slots != null)
                {
                    foreach (var slot in slots)
                    {
                        bool isError = false;

                        Slot? slotExist = _dbContext.Slot.FirstOrDefault(x => x.SlotCode == slot.SlotCode);
                        Slot? slotExist2 = _dbContext.Slot.FirstOrDefault(x => x.ColNo == slot.ColNo && x.RowNo == slot.RowNo && x.Slot_Id != 0);

                        if (slotExist != null)
                        {
                            result.errMessage = $"Slot code: {slot.SlotCode} has exist.";
                            slot.ErrorMsg = result.errMessage;
                            isError = true;
                        }
                        else if (slotExist2 != null)
                        {
                            result.errMessage = $"Slot Code: {slot.SlotCode} Column No {slot.ColNo} and Row No {slot.RowNo} has been used.";
                            slot.ErrorMsg = result.errMessage;
                            isError = true;
                        }

                        if (!isError)
                        {
                            Slot _slot = new Slot()
                            {
                                SlotCode = slot.SlotCode,
                                ColNo = slot.ColNo,
                                RowNo = slot.RowNo,
                                IsActive = slot.IsActive,
                                HasEmptyDrawer = slot.HasEmptyDrawer,
                                HasReel = slot.HasReel,
                                XPulse = slot.XPulse,
                                YPulse = slot.YPulse,
                                QRXPulse = slot.QRXPulse,
                                QRYPulse = slot.QRYPulse,
                            };
                            _dbContext.Slot.Add(_slot);
                        }
                        else
                        {
                            errorsLine.Add(slot);
                        }
                    }

                    if (errorsLine.Any())
                    {
                        result.success = false;
                        result.errMessage = "Some rows failed validation.";
                        result.data = errorsLine; 
                    }
                    else
                    {
                        await _dbContext.SaveChangesAsync();
                        result.success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<SlotRangeDTO>> SaveRangeOfSlot(SlotRangeDTO slotRanges)
        {
            ServiceResponseModel<SlotRangeDTO> result = new ServiceResponseModel<SlotRangeDTO>();

            try
            {
                string codeFormat = slotRanges.SlotFormat;
                int currentXPulse = slotRanges.XPulse;
                int currentYPulse = slotRanges.YPulse;
                int currentQrXPulse = slotRanges.QRXPulse;
                int currentQrYPulse = slotRanges.QRYPulse;

                for (int i = slotRanges.StartCol; i < slotRanges.StartCol + slotRanges.TotalCols; i++)
                {
                    for (int j = slotRanges.StartRow; j < slotRanges.StartRow + slotRanges.TotalRows; j++)
                    {
                        string slotFormat = slotRanges.SlotFormat;
                        Regex regex = new Regex("<(\\d+)>");
                        slotFormat = regex.Replace(slotFormat, "<col:$1>", 1);  
                        slotFormat = regex.Replace(slotFormat, "<row:$1>", 1);

                        string slotcode = GenerateSlotCode(slotFormat, i, j);

                        Slot? slotExist = _dbContext.Slot.FirstOrDefault(x => x.SlotCode == slotcode);
                        if (slotExist != null)
                        {
                            result.errMessage = $"Slot code: {slotcode} has exist.";
                            return result;
                        }

                        Slot? slotExist2 = _dbContext.Slot.FirstOrDefault(x => x.ColNo == i && x.RowNo == j && x.Slot_Id != 0);
                        if (slotExist2 != null)
                        {
                            result.errMessage = $"Slot Code: {slotcode} Column No {i} and Row No {j} has been used.";
                            return result;
                        }

                        Slot _slot = new Slot()
                        {
                            SlotCode = slotcode,
                            ColNo = i,
                            RowNo = j,
                            IsActive = slotRanges.IsActive,
                            HasEmptyDrawer = slotRanges.HasEmptyDrawer,
                            HasReel = slotRanges.HasReel,
                            XPulse = currentXPulse,
                            YPulse = currentYPulse,
                            QRXPulse = currentQrXPulse,
                            QRYPulse = currentQrYPulse,
                        };
                        _dbContext.Slot.Add(_slot);

                        currentXPulse += slotRanges.XPulseIncrement;
                        currentYPulse += slotRanges.YPulseIncrement;
                        currentQrXPulse += slotRanges.QRXPulseIncrement;
                        currentQrYPulse += slotRanges.QRYPulseIncrement;
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

        private string GenerateSlotCode(string format, int col, int row)
        {
            string slotcode = format;

            slotcode = Regex.Replace(slotcode, @"<(\w+):(\d+)>", match =>
            {
                int value = match.Groups[1].Value switch
                {
                    "col" => col,
                    "row" => row,
                    _ => throw new ArgumentException("Invalid placeholder type.")
                };

                int width = match.Groups[2].Value.Length;
                string formattedNumber = string.Format($"{{0:D{width}}}", value);

                return formattedNumber;
            });

            return slotcode;
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

        public async Task<ServiceResponseModel<SlotFreeDTO>> GetFreeSlot_ByColumn_ASC(SlotFreeReqDTO req)
        {
            ServiceResponseModel<SlotFreeDTO> result = new ServiceResponseModel<SlotFreeDTO>();

            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@TotalSlot", req.TotalSlot),
                    new SqlParameter("@ColNo", req.ColNo),
                };

                string sql = "EXECUTE dbo.Slot_GET_FREESLOTBYCOL_ASC @TotalSlot,@ColNo ";
                var listDTO = await _dbContext.SP_SlotGetFreeSlotByCol_ASC.FromSqlRaw(sql, parameters).ToListAsync();

                result.success = true;
                result.data = listDTO.First();
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<SlotDTO>> UpdateSlotStatus(SlotStatusReqDTO slotReq)
        {
            ServiceResponseModel<SlotDTO> result = new ServiceResponseModel<SlotDTO>();

            try
            {
                // 1. checking Data
                if (slotReq == null)
                {
                    result.errMessage = "Please select Slot.";
                    return result;
                }

                Slot? _slot = _dbContext.Slot.Find(slotReq.Slot_Id);
                if (_slot == null)
                {
                    result.errMessage = "Cannot find this slot, please refresh the list.";
                    return result;
                }
                _slot.IsActive = slotReq.IsActive;
                _slot.ForEmptyTray = slotReq.ForEmptyTray;
                _slot.HasEmptyTray = slotReq.HasEmptyTray;
                _slot.HasReel = slotReq.HasReel;
                _slot.ReelNo = slotReq.ReelNo;
                _dbContext.Slot.Update(_slot);
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
