using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.Data.Maintenances;
using RackingSystem.Models;
using RackingSystem.Models.Slot;
using System.Drawing;
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
                var validatefield = new List<(int? value, string fieldName)>
                {
                    (slotReq.ColNo, "Column No"),
                    (slotReq.RowNo, "Row No"),
                    (slotReq.XPulse, "X Pulse"),
                    (slotReq.YPulse, "Y Pulse"),
                    (slotReq.QRXPulse, "Qr-X Pulse"),
                    (slotReq.QRYPulse, "Qr-Y Pulse"),
                    (slotReq.Priority, "Priority"),
                };

                foreach (var (value, fieldName) in validatefield)
                {
                    if (!value.HasValue || value < 0)
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

                var match = Regex.Match(slotReq.SlotCode, @"^(\w+)c(\d{3})r(\d{3})$", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    int colNoInput = int.Parse(match.Groups[2].Value);
                    int rowNoInput = int.Parse(match.Groups[3].Value);

                    if (slotReq.ColNo != colNoInput || slotReq.RowNo != rowNoInput)
                    {
                        result.errMessage = "The Slot Code does not match the Column No and Row No.";
                        return result;
                    }
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
                        HasEmptyTray = true,
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
                        Priority = slotReq.Priority,
                    };
                    _dbContext.Slot.Add(_slot);

                    //check if previously have any same column no
                    SlotColumnSetting? scSetting = _dbContext.SlotColumnSetting.FirstOrDefault(x => x.ColNo == slotReq.ColNo);
                    if (scSetting == null)
                    {
                        var totalCount = (_dbContext.SlotColumnSetting.Count() == 0 ? 1 : _dbContext.SlotColumnSetting.Count());
                        var latestSlotCol = _dbContext.SlotColumnSetting.OrderByDescending(s => s.SlotColumnSetting_Id).FirstOrDefault();

                        SlotColumnSetting _slotCol = new SlotColumnSetting()
                        {
                            ColNo = slotReq.ColNo,
                            EmptyDrawer_IN_Idx = latestSlotCol == null ? totalCount : latestSlotCol.EmptyDrawer_OUT_Idx + 1,
                            EmptyDrawer_OUT_Idx = latestSlotCol == null ? totalCount : latestSlotCol.EmptyDrawer_OUT_Idx + 1,
                            Reel_IN_Idx = 0,
                            Reel_OUT_Idx = 0,
                        };
                        _dbContext.SlotColumnSetting.Add(_slotCol);
                    }
                }
                else
                {
                    Slot? _slot = _dbContext.Slot.Find(slotReq.Slot_Id);
                    if (_slot == null)
                    {
                        result.errMessage = "Cannot find this slot, please refresh the list.";
                        return result;
                    }
                    var oldColNo = _slot.ColNo;
                    var oldRowNo = _slot.RowNo;

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
                    _slot.Priority = slotReq.Priority;
                    _dbContext.Slot.Update(_slot);

                    SlotColumnSetting? _slotCol = _dbContext.SlotColumnSetting.FirstOrDefault(x => x.ColNo == oldColNo);
                    if (_slotCol != null)
                    {
                        /*
                        //check if the slotcol empty drawer is same as current update oldRowNo
                        if (_slotCol.EmptyDrawer_IN_Idx == oldRowNo)
                        {
                            //if same, find other slot having same column, get the second lowest empty drawer inside(bcuz the lowest will be same as oldRowNo)
                            var sameColSlot = _dbContext.Slot.Where(slot => slot.ColNo == oldColNo).OrderBy(slot => slot.RowNo).Skip(1).FirstOrDefault();
                            if (sameColSlot != null && sameColSlot.RowNo > oldRowNo)
                            {
                                _slotCol.EmptyDrawer_IN_Idx = sameColSlot.RowNo;
                                _slotCol.EmptyDrawer_OUT_Idx = sameColSlot.RowNo;
                            }
                            else
                            {
                                _dbContext.SlotColumnSetting.Remove(_slotCol);
                            }
                        }
                        */
                        var totalCount = _dbContext.Slot.Count(s => s.ColNo == oldColNo);
                        if (totalCount - (oldColNo == slotReq.ColNo ? 0 : 1) <= 0)
                        {
                            _dbContext.SlotColumnSetting.Remove(_slotCol);
                        }
                    }

                    //check if the same column no exist before
                    SlotColumnSetting? _scExist = _dbContext.SlotColumnSetting.FirstOrDefault(x => x.ColNo == slotReq.ColNo);
                    if (_scExist == null)
                    {
                        var latestSlotCol = _dbContext.SlotColumnSetting.OrderByDescending(s => s.SlotColumnSetting_Id).FirstOrDefault();
                        SlotColumnSetting _slotColNew = new SlotColumnSetting()
                        {
                            ColNo = slotReq.ColNo,
                            EmptyDrawer_IN_Idx = latestSlotCol == null ? _dbContext.SlotColumnSetting.Count() : latestSlotCol.EmptyDrawer_OUT_Idx + 1,
                            EmptyDrawer_OUT_Idx = latestSlotCol == null ? _dbContext.SlotColumnSetting.Count() : latestSlotCol.EmptyDrawer_OUT_Idx + 1,
                            Reel_IN_Idx = 0,
                            Reel_OUT_Idx = 0,
                        };
                        _dbContext.SlotColumnSetting.Add(_slotColNew);
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

        public async Task<ServiceResponseModel<SlotDTO>> DeleteSlot(SlotDTO slotReq)
        {
            ServiceResponseModel<SlotDTO> result = new ServiceResponseModel<SlotDTO>();

            try
            {
                if (slotReq == null)
                {
                    result.errMessage = "Please refresh the list.";
                    return result;
                }

                Slot? _slot = _dbContext.Slot.Find(slotReq.Slot_Id);
                if (_slot == null)
                {
                    result.errMessage = "Cannot find this slot, please refresh the list.";
                    return result;
                }

                if (_slot.HasEmptyTray)
                {
                    result.errMessage = "This slot has Empty Tray, cannot delete.";
                    return result;
                }
                if (_slot.HasReel)
                {
                    result.errMessage = "This slot has Reel, cannot delete.";
                    return result;
                }

                _dbContext.Slot.Remove(_slot);

                SlotColumnSetting? _slotCol = _dbContext.SlotColumnSetting.FirstOrDefault(x => x.ColNo == _slot.ColNo);
                if (_slotCol != null)
                {
                    var totalCount = _dbContext.Slot.Count(s => s.ColNo == _slot.ColNo);
                    if (totalCount - 1 <= 0)
                    {
                        _dbContext.SlotColumnSetting.Remove(_slotCol);
                    }
                    /*
                    if (_slotCol.EmptyDrawer_IN_Idx == _slot.RowNo)
                    {
                        var sameColSlot = _dbContext.Slot.Where(slot => slot.ColNo == _slot.ColNo).OrderBy(slot => slot.RowNo).Skip(1).FirstOrDefault();
                        if (sameColSlot != null && sameColSlot.RowNo > _slot.RowNo)
                        {
                            _slotCol.EmptyDrawer_IN_Idx = sameColSlot.RowNo;
                            _slotCol.EmptyDrawer_OUT_Idx = sameColSlot.RowNo;
                        }
                        else
                        {
                            _dbContext.SlotColumnSetting.Remove(_slotCol);
                        }
                    }
                    */
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

        public async Task<ServiceResponseModel<List<SlotListDTO>>> SaveExcelSlot(List<SlotListDTO> slots)
        {
            ServiceResponseModel<List<SlotListDTO>> result = new ServiceResponseModel<List<SlotListDTO>>();
            List<SlotListDTO> errorsLine = new List<SlotListDTO>(); // Change List<object> to List<SlotListDTO>
            List<SlotColumnSetting> slotColumns = new List<SlotColumnSetting>();

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

                        var match = Regex.Match(slot.SlotCode, @"^(\w+)c(\d{3})r(\d{3})$", RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            int colNoInput = int.Parse(match.Groups[2].Value);
                            int rowNoInput = int.Parse(match.Groups[3].Value);

                            if (slot.ColNo != colNoInput || slot.RowNo != rowNoInput)
                            {
                                result.errMessage = "The Slot Code does not match the Column No and Row No.";
                                slot.ErrorMsg = result.errMessage;
                                isError = true;
                            }
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
                                HasEmptyTray = false,
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
                                Priority = slot.Priority,
                            };
                            _dbContext.Slot.Add(_slot);

                            if (!slotColumns.Any(x => x.ColNo == slot.ColNo))
                            {
                                var totalCount = (_dbContext.SlotColumnSetting.Count() == 0 ? 1 : _dbContext.SlotColumnSetting.Count()) + slotColumns.Count();
                                var latestSlotCol = _dbContext.SlotColumnSetting.OrderByDescending(s => s.SlotColumnSetting_Id).FirstOrDefault();

                                SlotColumnSetting _sCol = new SlotColumnSetting()
                                {
                                    ColNo = slot.ColNo,
                                    EmptyDrawer_IN_Idx = latestSlotCol == null ? totalCount : latestSlotCol.EmptyDrawer_OUT_Idx + 1 + slotColumns.Count,
                                    EmptyDrawer_OUT_Idx = latestSlotCol == null ? totalCount : latestSlotCol.EmptyDrawer_OUT_Idx + 1 + slotColumns.Count,
                                    Reel_IN_Idx = 0,
                                    Reel_OUT_Idx = 0,
                                };
                                slotColumns.Add(_sCol);
                            }
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
                        _dbContext.SlotColumnSetting.AddRange(slotColumns);

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
            List<SlotColumnSetting> slotColumns = new List<SlotColumnSetting>();

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

                        var match = Regex.Match(slotcode, @"^(\w+)c(\d{3})r(\d{3})$", RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            int colNoInput = int.Parse(match.Groups[2].Value);
                            int rowNoInput = int.Parse(match.Groups[3].Value);

                            if (i != colNoInput || j != rowNoInput)
                            {
                                result.errMessage = "The Slot Code does not match the Column No and Row No.";
                                return result;
                            }
                        }

                        Slot _slot = new Slot()
                        {
                            SlotCode = slotcode,
                            ColNo = i,
                            RowNo = j,
                            IsActive = slotRanges.IsActive,
                            HasEmptyTray = true,
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

                        //insert into slot column setting
                        if (!slotColumns.Any(x => x.ColNo == i))
                        {
                            var totalCount = (_dbContext.SlotColumnSetting.Count() == 0 ? 1 : _dbContext.SlotColumnSetting.Count()) + slotColumns.Count();
                            var latestSlotCol = _dbContext.SlotColumnSetting.OrderByDescending(s => s.SlotColumnSetting_Id).FirstOrDefault();

                            SlotColumnSetting _sCol = new SlotColumnSetting()
                            {
                                ColNo = i,
                                EmptyDrawer_IN_Idx = latestSlotCol == null ? totalCount : latestSlotCol.EmptyDrawer_OUT_Idx + 1 + slotColumns.Count,
                                EmptyDrawer_OUT_Idx = latestSlotCol == null ? totalCount : latestSlotCol.EmptyDrawer_OUT_Idx + 1 + slotColumns.Count,
                                Reel_IN_Idx = 0,
                                Reel_OUT_Idx = 0,
                            };
                            slotColumns.Add(_sCol);
                        }
                    }
                }

                _dbContext.SlotColumnSetting.AddRange(slotColumns);

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
                var slotList = await _dbContext.Slot.Where(x => x.ColNo == req).OrderByDescending(x => x.RowNo).ToListAsync();
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

        public async Task<ServiceResponseModel<SlotFreeDTO>> GetFreeSlot_BySlot_ASC(SlotFreeReqDTO req)
        {
            ServiceResponseModel<SlotFreeDTO> result = new ServiceResponseModel<SlotFreeDTO>();

            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@TotalSlot", req.TotalSlot),
                };

                string sql = "EXECUTE dbo.Slot_GET_FREESLOT_ASC @TotalSlot ";
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
                _slot.Priority = slotReq.Priority;
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

        public async Task<ServiceResponseModel<List<Slot_DrawerFreeDTO>>> GetFreeSlot_Drawer_ByColumn()
        {
            ServiceResponseModel<List<Slot_DrawerFreeDTO>> result = new ServiceResponseModel<List<Slot_DrawerFreeDTO>>();

            try
            {
                string sql = "EXECUTE dbo.Slot_GET_FREEDRAWERBYCOL";
                var listDTO = await _dbContext.SP_SlotGetFreeSDrawerByCol.FromSqlRaw(sql).ToListAsync();
                if (listDTO.Count == 0)
                {
                    result.success = false;
                    result.errMessage = "No empty trays for this column.";
                }
                else
                {
                    result.success = true;
                    result.data = listDTO;
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
