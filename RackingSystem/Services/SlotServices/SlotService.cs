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
using static System.Reflection.Metadata.BlobBuilder;

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
                /*
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
                */
                var validatefield = new List<(int? value, string fieldName)>
                {
                    (slotReq.ColNo, "Column No"),
                    (slotReq.RowNo, "Row No"),
                    (slotReq.XPulse, "X Pulse"),
                    (slotReq.YPulse, "Y Pulse"),
                    (slotReq.QRXPulse, "Qr-X Pulse"),
                    (slotReq.QRYPulse, "Qr-Y Pulse"),
                    //(slotReq.Add1Pulse, "Additional Pulse 1"),
                    //(slotReq.Add2Pulse, "Additional Pulse 2"),
                    //(slotReq.Add3Pulse, "Additional Pulse 3"),
                    //(slotReq.Add4Pulse, "Additional Pulse 4"),
                    //(slotReq.Add5Pulse, "Additional Pulse 5"),
                    //(slotReq.Add6Pulse, "Additional Pulse 6"),
                };

                foreach (var (value, fieldName) in validatefield)
                {
                    if (!value.HasValue || value <= 0)
                    {
                        result.errMessage = $"Please insert {fieldName}. Negative values are not allowed.";
                        return result;
                    }
                }

                if (slotReq.Slot_Id == 0)
                {
                    Slot? slotExist = _dbContext.Slot.FirstOrDefault(x => x.SlotCode == slotReq.SlotCode);
                    if (slotExist != null)
                    {
                        result.errMessage = "This slot code already exists.";
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
                    result.errMessage = "This Column No and Row No have been used.";
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
                        Add1Pulse = slotReq.Add1Pulse,
                        Add2Pulse = slotReq.Add2Pulse,
                        Add3Pulse = slotReq.Add3Pulse,
                        Add4Pulse = slotReq.Add4Pulse,
                        Add5Pulse = slotReq.Add5Pulse,
                        Add6Pulse = slotReq.Add6Pulse,
                        IsLeft = slotReq.IsLeft,
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
                    _slot.Add1Pulse = slotReq.Add1Pulse;
                    _slot.Add2Pulse = slotReq.Add2Pulse;
                    _slot.Add3Pulse = slotReq.Add3Pulse;
                    _slot.Add4Pulse = slotReq.Add4Pulse;
                    _slot.Add5Pulse = slotReq.Add5Pulse;
                    _slot.Add6Pulse = slotReq.Add6Pulse;
                    _slot.IsLeft = slotReq.IsLeft;
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
                            result.errMessage = $"Slot code: {slot.SlotCode} has been used";
                            slot.ErrorMsg = result.errMessage;
                            isError = true;
                        }
                        else if (slotExist2 != null)
                        {
                            result.errMessage = $"Slot Code: {slot.SlotCode} Column No {slot.ColNo} and Row No {slot.RowNo} have been used.";
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
                                IsLeft = slot.IsLeft,
                                HasEmptyDrawer = slot.HasEmptyDrawer,
                                HasReel = slot.HasReel,
                                XPulse = slot.XPulse,
                                YPulse = slot.YPulse,
                                QRXPulse = slot.QRXPulse,
                                QRYPulse = slot.QRYPulse,
                                Add1Pulse = slot.Add1Pulse,
                                Add2Pulse = slot.Add2Pulse,
                                Add3Pulse = slot.Add3Pulse,
                                Add4Pulse = slot.Add4Pulse,
                                Add5Pulse = slot.Add5Pulse,
                                Add6Pulse = slot.Add6Pulse,
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
                int currentAdd1Pulse = slotRanges.Add1Pulse;
                int currentAdd2Pulse = slotRanges.Add2Pulse;
                int currentAdd3Pulse = slotRanges.Add3Pulse;
                int currentAdd4Pulse = slotRanges.Add4Pulse;
                int currentAdd5Pulse = slotRanges.Add5Pulse;
                int currentAdd6Pulse = slotRanges.Add6Pulse;

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
                            result.errMessage = $"Slot code: {slotcode} has been used";
                            return result;
                        }

                        Slot? slotExist2 = _dbContext.Slot.FirstOrDefault(x => x.ColNo == i && x.RowNo == j && x.Slot_Id != 0);
                        if (slotExist2 != null)
                        {
                            result.errMessage = $"Slot Code: {slotcode} Column No {i} and Row No {j} have been used.";
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
                            Add1Pulse = currentAdd1Pulse,
                            Add2Pulse = currentAdd2Pulse,
                            Add3Pulse = currentAdd3Pulse,
                            Add4Pulse = currentAdd4Pulse,
                            Add5Pulse = currentAdd5Pulse,
                            Add6Pulse = currentAdd6Pulse,
                            IsLeft = slotRanges.IsLeft,
                        };
                        _dbContext.Slot.Add(_slot);

                        currentXPulse += slotRanges.XPulseIncrement;
                        currentYPulse += slotRanges.YPulseIncrement;
                        currentQrXPulse += slotRanges.QRXPulseIncrement;
                        currentQrYPulse += slotRanges.QRYPulseIncrement;
                        currentAdd1Pulse += slotRanges.Add1PulseIncrement;
                        currentAdd2Pulse += slotRanges.Add2PulseIncrement;
                        currentAdd3Pulse += slotRanges.Add3PulseIncrement;
                        currentAdd4Pulse += slotRanges.Add4PulseIncrement;
                        currentAdd5Pulse += slotRanges.Add5PulseIncrement;
                        currentAdd6Pulse += slotRanges.Add6PulseIncrement;

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

        public async Task<ServiceResponseModel<SlotFreeDTO>> GetFreeSlot_ByColumn_DESC(SlotFreeReqDTO req)
        {
            ServiceResponseModel<SlotFreeDTO> result = new ServiceResponseModel<SlotFreeDTO>();

            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@TotalSlot", req.TotalSlot),
                    new SqlParameter("@ColNo", req.ColNo),
                };

                string sql = "EXECUTE dbo.Slot_GET_FREESLOTBYCOL_DESC @TotalSlot,@ColNo ";
                var listDTO = await _dbContext.SP_SlotGetFreeSlotByCol_DESC.FromSqlRaw(sql, parameters).ToListAsync();

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
                _slot.ForEmptyDrawer = slotReq.ForEmptyDrawer;
                _slot.HasEmptyDrawer = slotReq.HasEmptyDrawer;
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

        public async Task<ServiceResponseModel<List<SlotListDTO>>> UpdateExcelPulses(List<SlotListDTO> slotPulses)
        {
            ServiceResponseModel<List<SlotListDTO>> result = new ServiceResponseModel<List<SlotListDTO>>();
            List<SlotListDTO> errorsLine = new List<SlotListDTO>(); // Change List<object> to List<SlotListDTO>
            long slotId = 0;

            try
            {
                foreach (var slot in slotPulses)
                {
                    bool isError = false;

                    //validation
                    var validatefield = new List<(int? value, string fieldName)>
                    {
                        (slot.XPulse, "X Pulse"),
                        (slot.YPulse, "Y Pulse"),
                        (slot.QRXPulse, "Qr-X Pulse"),
                        (slot.QRYPulse, "Qr-Y Pulse"),
                        (slot.Add1Pulse, "Additional Pulse 1"),
                        (slot.Add2Pulse, "Additional Pulse 2"),
                        (slot.Add3Pulse, "Additional Pulse 3"),
                        (slot.Add4Pulse, "Additional Pulse 4"),
                        (slot.Add5Pulse, "Additional Pulse 5"),
                        (slot.Add6Pulse, "Additional Pulse 6"),
                    };

                    foreach (var (value, fieldName) in validatefield)
                    {
                        if (!value.HasValue || value < 0)
                        {
                            result.errMessage = $"Please insert {fieldName}. Negative values are not allowed.";
                            slot.ErrorMsg = result.errMessage;
                            errorsLine?.Add(slot);
                            break;
                        }
                    }

                    Slot? slotExist = _dbContext.Slot.FirstOrDefault(x => x.SlotCode == slot.SlotCode);
                    //Slot? slotExist2 = _dbContext.Slot.FirstOrDefault(x => x.ColNo == slot.ColNo && x.RowNo == slot.RowNo && x.Slot_Id != 0);

                    if (slotExist == null)
                    {
                        result.errMessage = $"Slot code: {slot.SlotCode} did not exist.";
                        slot.ErrorMsg = result.errMessage;
                        isError = true;
                    }
                    else
                    {
                        slotId = slotExist.Slot_Id;
                    }

                    if (!isError)
                    {
                        if (errorsLine != null && errorsLine.Count != 0) break;

                        Slot? _slot = _dbContext.Slot.Find(slotId);
                        if (_slot == null)
                        {
                            result.errMessage = "Cannot find this slot, please refresh the list.";
                            return result;
                        }
                        
                        _slot.XPulse = slot.XPulse;
                        _slot.YPulse = slot.YPulse;
                        _slot.QRXPulse = slot.QRXPulse;
                        _slot.QRYPulse = slot.QRYPulse;
                        _slot.Add1Pulse = slot.Add1Pulse;
                        _slot.Add2Pulse = slot.Add2Pulse;
                        _slot.Add3Pulse = slot.Add3Pulse;
                        _slot.Add4Pulse = slot.Add4Pulse;
                        _slot.Add5Pulse = slot.Add5Pulse;
                        _slot.Add6Pulse = slot.Add6Pulse;
                        //_slot.IsActive = slot.IsActive;
                        //_slot.IsLeft = slot.IsLeft;
                        _dbContext.Slot.Update(_slot);
                    }
                    else
                    {
                        errorsLine?.Add(slot);
                    }
                }

                if (errorsLine != null && errorsLine.Count != 0)
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
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }
    }
}
