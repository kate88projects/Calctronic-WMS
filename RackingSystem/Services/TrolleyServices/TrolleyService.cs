using AspNetCoreGeneratedDocument;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.Data.Maintenances;
using RackingSystem.General;
using RackingSystem.Models;
using RackingSystem.Models.Loader;
using RackingSystem.Models.Slot;
using RackingSystem.Models.Trolley;
using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;
using static System.Reflection.Metadata.BlobBuilder;

namespace RackingSystem.Services.TrolleyServices
{
    [Authorize(AuthenticationSchemes = "MyAuthCookie")]
    public class TrolleyService : ITrolleyService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public TrolleyService(AppDbContext context, IMapper mapper)
        {
            _dbContext = context;
            _mapper = mapper;
        }
        public async Task<ServiceResponseModel<List<TrolleyListDTO>>> GetTrolleyList()
        {
            ServiceResponseModel<List<TrolleyListDTO>> result = new ServiceResponseModel<List<TrolleyListDTO>>();

            try
            {
                var trolleyList = await _dbContext.Trolley.OrderBy(x => x.TrolleyCode).ToListAsync();
                var trolleyListDTO = _mapper.Map<List<TrolleyListDTO>>(trolleyList).ToList();
                result.success = true;
                result.data = trolleyListDTO;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<List<TrolleyListDTO>>> GetActiveTrolleyList()
        {
            ServiceResponseModel<List<TrolleyListDTO>> result = new ServiceResponseModel<List<TrolleyListDTO>>();

            try
            {
                var trolleyList = await _dbContext.Trolley.Where(x => x.IsActive == true).OrderBy(x => x.TrolleyCode).ToListAsync();
                var trolleyListDTO = _mapper.Map<List<TrolleyListDTO>>(trolleyList).ToList();
                result.success = true;
                result.data = trolleyListDTO;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<TrolleyListDTO>> SaveTrolley(TrolleyListDTO trolley)
        {
            ServiceResponseModel<TrolleyListDTO> result = new ServiceResponseModel<TrolleyListDTO>();

            try
            {
                //validate
                if (string.IsNullOrEmpty(trolley.TrolleyCode))
                {
                    result.errMessage = "Please insert Trolley Code.";
                    return result;
                }
                //if (trolley.TotalCol <= 0)
                //{
                //    result.errMessage = "Please insert Total Column.";
                //    return result;
                //}
                if (trolley.IPAdd1RowNo <= 0)
                {
                    result.errMessage = "Please insert IP Address 1 Row No";
                    return result;
                }
                if (trolley.IPAdd2RowNo <= 0)
                {
                    result.errMessage = "Please insert IP Address 2 Row No";
                    return result;
                }
                if (trolley.IPAdd3RowNo <= 0)
                {
                    result.errMessage = "Please insert IP Address 3 Row No";
                    return result;
                }
                if (string.IsNullOrEmpty(trolley.IPAdd1))
                {
                    result.errMessage = "Please insert IP Address 1";
                    return result;
                }
                if (!IPAddress.TryParse(trolley.IPAdd1, out _)) //only verify on IPv4 and IPv6
                {
                    result.errMessage = "Invalid IP Address1 format. Please insert again.";
                    return result;
                }
                if (string.IsNullOrEmpty(trolley.IPAdd2))
                {
                    result.errMessage = "Please insert IP Address 2";
                    return result;
                }
                if (!IPAddress.TryParse(trolley.IPAdd2, out _)) //only verify on IPv4 and IPv6
                {
                    result.errMessage = "Invalid IP Address2 format. Please insert again.";
                    return result;
                }
                if (string.IsNullOrEmpty(trolley.IPAdd3))
                {
                    result.errMessage = "Please insert IP Address 3";
                    return result;
                }
                if (!IPAddress.TryParse(trolley.IPAdd3, out _)) //only verify on IPv4 and IPv6
                {
                    result.errMessage = "Invalid IP Address3 format. Please insert again.";
                    return result;
                }

                if (trolley.IPAdd1AColNo == 0)
                {
                    result.errMessage = "Please insert IP Address 1 -> Side A Column No";
                    return result;
                }
                if (trolley.IPAdd1BColNo == 0)
                {
                    result.errMessage = "Please insert IP Address 1 -> Side B Column No";
                    return result;
                }
                if (trolley.IPAdd2AColNo == 0)
                {
                    result.errMessage = "Please insert IP Address 2 -> Side A Column No";
                    return result;
                }
                if (trolley.IPAdd2BColNo == 0)
                {
                    result.errMessage = "Please insert IP Address 2 -> Side B Column No";
                    return result;
                }
                if (trolley.IPAdd3AColNo == 0)
                {
                    result.errMessage = "Please insert IP Address 3 -> Side A Column No";
                    return result;
                }
                if (trolley.IPAdd3BColNo == 0)
                {
                    result.errMessage = "Please insert IP Address 3 -> Side B Column No";
                    return result;
                }


                if (trolley.Trolley_Id == 0)
                {
                    Trolley? trolleyExist = _dbContext.Trolley.FirstOrDefault(x => x.TrolleyCode == trolley.TrolleyCode);
                    if (trolleyExist != null)
                    {
                        result.errMessage = "This Trolley Code already exist.";
                        return result;
                    }
                }
                else
                {
                    Trolley? trolleyExist = _dbContext.Trolley.FirstOrDefault(x => x.TrolleyCode == trolley.TrolleyCode && x.Trolley_Id != trolley.Trolley_Id);
                    if (trolleyExist != null)
                    {
                        result.errMessage = "This Trolley Code already exist.";
                        return result;
                    }
                }

                //save
                if (trolley.Trolley_Id == 0)
                {
                    Trolley _trolley = new Trolley()
                    {
                        TrolleyCode = trolley.TrolleyCode,
                        IPAdd1 = trolley.IPAdd1,
                        IPAdd2 = trolley.IPAdd2,
                        IPAdd3 = trolley.IPAdd3,
                        IPAdd1AColNo = trolley.IPAdd1AColNo,
                        IPAdd1BColNo = trolley.IPAdd1BColNo,
                        IPAdd2AColNo = trolley.IPAdd2AColNo,
                        IPAdd2BColNo = trolley.IPAdd2BColNo,
                        IPAdd3AColNo = trolley.IPAdd3AColNo,
                        IPAdd3BColNo = trolley.IPAdd3BColNo,
                        IsActive = trolley.IsActive,
                        Remark = trolley.Remark,
                        //TotalCol = trolley.TotalCol,
                        IPAdd1RowNo = trolley.IPAdd1RowNo,
                        IPAdd2RowNo = trolley.IPAdd2RowNo,
                        IPAdd3RowNo = trolley.IPAdd3RowNo,
                        //Side = (int)trolley.Side,
                    };
                    _dbContext.Trolley.Add(_trolley);

                }
                else
                {
                    Trolley? _trolley = _dbContext.Trolley.Find(trolley.Trolley_Id);
                    if (_trolley == null)
                    {
                        result.errMessage = "Cannot find this trolley, please refresh the list.";
                        return result;
                    }
                    _trolley.TrolleyCode = trolley.TrolleyCode;
                    _trolley.IPAdd1 = trolley.IPAdd1;
                    _trolley.IPAdd2 = trolley.IPAdd2;
                    _trolley.IPAdd3 = trolley.IPAdd3;
                    _trolley.IPAdd1AColNo = trolley.IPAdd1AColNo;
                    _trolley.IPAdd1BColNo = trolley.IPAdd1BColNo;
                    _trolley.IPAdd1BColNo = trolley.IPAdd1BColNo;
                    _trolley.IPAdd2AColNo = trolley.IPAdd2AColNo;
                    _trolley.IPAdd3AColNo = trolley.IPAdd3AColNo;
                    _trolley.IPAdd3BColNo = trolley.IPAdd3BColNo;
                    _trolley.Remark = trolley.Remark;
                    //_trolley.TotalCol = trolley.TotalCol;
                    _trolley.IPAdd1RowNo = trolley.IPAdd1RowNo;
                    _trolley.IPAdd2RowNo = trolley.IPAdd2RowNo;
                    _trolley.IPAdd3RowNo = trolley.IPAdd3RowNo;
                    _trolley.IsActive = trolley.IsActive;
                    //_trolley.Side = (int)trolley.Side;
                    _dbContext.Trolley.Update(_trolley);
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

        public async Task<ServiceResponseModel<TrolleyListDTO>> DeleteTrolley(TrolleyListDTO trolley)
        {
            ServiceResponseModel<TrolleyListDTO> result = new ServiceResponseModel<TrolleyListDTO>();

            try
            {
                if (trolley == null)
                {
                    result.errMessage = "Something wrong. Please refresh the list and try again.";
                    return result;
                }

                TrolleySlot? _trolleySlot = _dbContext.TrolleySlot.FirstOrDefault(x => x.Trolley_Id == trolley.Trolley_Id && x.HasReel == true);
                if (_trolleySlot != null)
                {
                    result.errMessage = "This trolley has Reel, cannot delete.";
                    return result;
                }

                Trolley? _trolley = _dbContext.Trolley.Find(trolley.Trolley_Id);
                if(_trolley == null)
                {
                    result.errMessage = "Cannot find this trolley, please refresh the list.";
                    return result;
                }
                _dbContext.Trolley.Remove(_trolley);

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

        public async Task<ServiceResponseModel<TrolleySlotDTO>> GetTrolleySlot(long id)
        {
            ServiceResponseModel<TrolleySlotDTO> result = new ServiceResponseModel<TrolleySlotDTO>();

            try
            {
                var trolley = await _dbContext.TrolleySlot.FirstOrDefaultAsync(x => x.TrolleySlot_Id == id);
                var trolleyDTO = _mapper.Map<TrolleySlotDTO>(trolley);
                result.success = true;
                result.data = trolleyDTO;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<List<TrolleySlotDTO>>> GetTrolleySlotList()
        {
            ServiceResponseModel<List<TrolleySlotDTO>> result = new ServiceResponseModel<List<TrolleySlotDTO>>();

            try
            {
                var trolleySlotList = await _dbContext.TrolleySlot.OrderBy(x => x.TrolleySlotCode).ToListAsync();
                var trolleySlotListDTO = _mapper.Map<List<TrolleySlotDTO>>(trolleySlotList).ToList();
                foreach (var s in trolleySlotListDTO)
                {
                    var r = _dbContext.Reel.Where(x => x.Reel_Id == s.Reel_Id).FirstOrDefault();
                    if (r != null)
                    {
                        s.ReelCode = r.ReelCode;
                    }
                }
                result.success = true;
                result.data = trolleySlotListDTO;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<TrolleySlotDTO>> SaveTrolleySlot(TrolleySlotDTO trolleySlot)
        {
            ServiceResponseModel<TrolleySlotDTO> result = new ServiceResponseModel<TrolleySlotDTO>();

            try
            {
                if (string.IsNullOrEmpty(trolleySlot.TrolleySlotCode))
                {
                    result.errMessage = "Please insert Slot Code.";
                    return result;
                }

                var validatefield = new List<(int? value, string fieldName)>
                {
                    (trolleySlot.ColNo, "Column No"),
                    (trolleySlot.RowNo, "Row No"),
                    (trolleySlot.XPulse, "X Pulse"),
                    (trolleySlot.YPulse, "Y Pulse"),
                    (trolleySlot.QRXPulse, "Qr-X Pulse"),
                    (trolleySlot.QRYPulse, "Qr-Y Pulse"),
                    (trolleySlot.Add1Pulse, "Additional Pulse 1"),
                    (trolleySlot.Add2Pulse, "Additional Pulse 2"),
                    (trolleySlot.Add3Pulse, "Additional Pulse 3"),
                    (trolleySlot.Add4Pulse, "Additional Pulse 4"),
                    (trolleySlot.Add5Pulse, "Additional Pulse 5"),
                    (trolleySlot.Add6Pulse, "Additional Pulse 6"),
                };

                foreach (var (value, fieldName) in validatefield)
                {
                    if (!value.HasValue || value < 0)
                    {
                        result.errMessage = $"Please insert {fieldName}. Negative values are not allowed.";
                        return result;
                    }
                }

                if (trolleySlot.TrolleySlot_Id == 0)
                {
                    TrolleySlot? slotExist = _dbContext.TrolleySlot.FirstOrDefault(x => x.TrolleySlotCode == trolleySlot.TrolleySlotCode);
                    if (slotExist != null)
                    {
                        result.errMessage = "This Trolley Slot Code already exists.";
                        return result;
                    }
                }
                else
                {
                    TrolleySlot? slotExist = _dbContext.TrolleySlot.FirstOrDefault(x => x.TrolleySlotCode == trolleySlot.TrolleySlotCode && x.TrolleySlot_Id != trolleySlot.TrolleySlot_Id);
                    if (slotExist != null)
                    {
                        result.errMessage = "This slot code has been used.";
                        return result;
                    }
                }
                TrolleySlot? slotExist2 = _dbContext.TrolleySlot.FirstOrDefault(x => x.ColNo == trolleySlot.ColNo && x.RowNo == trolleySlot.RowNo && x.TrolleySlot_Id != trolleySlot.TrolleySlot_Id);
                if (slotExist2 != null)
                {
                    result.errMessage = "This Column No and Row No have been used.";
                    return result;
                }

                if (trolleySlot.TrolleySlot_Id == 0)
                {
                    TrolleySlot _ts = new TrolleySlot()
                    {
                        TrolleySlotCode = trolleySlot.TrolleySlotCode,
                        Trolley_Id = trolleySlot.Trolley_Id,
                        ColNo = trolleySlot.ColNo,
                        RowNo = trolleySlot.RowNo,
                        IsActive = trolleySlot.IsActive,
                        IsLeft = trolleySlot.IsLeft,
                        //IsLeft = (int)trolleySlot.IsLeft,

                        HasReel = trolleySlot.HasReel,
                        XPulse = trolleySlot.XPulse,
                        YPulse = trolleySlot.YPulse,
                        QRXPulse = trolleySlot.QRXPulse,
                        QRYPulse = trolleySlot.QRYPulse,
                        Add1Pulse = trolleySlot.Add1Pulse,
                        Add2Pulse = trolleySlot.Add2Pulse,
                        Add3Pulse = trolleySlot.Add3Pulse,
                        Add4Pulse = trolleySlot.Add4Pulse,
                        Add5Pulse = trolleySlot.Add5Pulse,
                        Add6Pulse = trolleySlot.Add6Pulse,
                    };
                    _dbContext.TrolleySlot.Add(_ts);
                }
                else
                {
                    TrolleySlot? _ts = _dbContext.TrolleySlot.Find(trolleySlot.TrolleySlot_Id);
                    if (_ts == null)
                    {
                        result.errMessage = "Cannot find this slot, please refresh the list.";
                        return result;
                    }
                    _ts.Trolley_Id = trolleySlot.Trolley_Id;
                    _ts.TrolleySlotCode = trolleySlot.TrolleySlotCode;
                    _ts.ColNo = trolleySlot.ColNo;
                    _ts.RowNo = trolleySlot.RowNo;
                    _ts.IsActive = trolleySlot.IsActive;
                    _ts.IsLeft = trolleySlot.IsLeft;
                    //_ts.Side = (int)trolleySlot.Side;

                    _ts.XPulse = trolleySlot.XPulse;
                    _ts.YPulse = trolleySlot.YPulse;
                    _ts.QRXPulse = trolleySlot.QRXPulse;
                    _ts.QRYPulse = trolleySlot.QRYPulse;
                    _ts.Add1Pulse = trolleySlot.Add1Pulse;
                    _ts.Add2Pulse = trolleySlot.Add2Pulse;
                    _ts.Add3Pulse = trolleySlot.Add3Pulse;
                    _ts.Add4Pulse = trolleySlot.Add4Pulse;
                    _ts.Add5Pulse = trolleySlot.Add5Pulse;
                    _ts.Add6Pulse = trolleySlot.Add6Pulse;
                    _dbContext.TrolleySlot.Update(_ts);
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

        public async Task<ServiceResponseModel<TrolleySlotDTO>> DeleteTrolleySlot(TrolleySlotDTO trolleySlot)
        {
            ServiceResponseModel<TrolleySlotDTO> result = new ServiceResponseModel<TrolleySlotDTO>();

            try
            {
                if (trolleySlot == null)
                {
                    result.errMessage = "Something wrong. Please refresh the list and try again.";
                    return result;
                }

                TrolleySlot? _trolleySlot = _dbContext.TrolleySlot.Find(trolleySlot.TrolleySlot_Id);
                if (_trolleySlot == null)
                {
                    result.errMessage = "Cannot find this trolley slot, please refresh the list.";
                    return result;
                }

                if (_trolleySlot.HasReel)
                {
                    result.errMessage = "This trolley Slot has Reel, cannot delete.";
                    return result;
                }


                _dbContext.TrolleySlot.Remove(_trolleySlot);

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

        public async Task<ServiceResponseModel<TrolleySlotRangeDTO>> SaveRangeOfTrolleySlot(TrolleySlotRangeDTO tsRanges)
        {
            ServiceResponseModel<TrolleySlotRangeDTO> result = new ServiceResponseModel<TrolleySlotRangeDTO>();

            try
            {
                string codeFormat = tsRanges.TrolleySlotFormat;
                int currentXPulse = tsRanges.XPulse;
                int currentYPulse = tsRanges.YPulse;
                int currentQrXPulse = tsRanges.QRXPulse;
                int currentQrYPulse = tsRanges.QRYPulse;
                int currentAdd1Pulse = tsRanges.Add1Pulse;
                int currentAdd2Pulse = tsRanges.Add2Pulse;
                int currentAdd3Pulse = tsRanges.Add3Pulse;
                int currentAdd4Pulse = tsRanges.Add4Pulse;
                int currentAdd5Pulse = tsRanges.Add5Pulse;
                int currentAdd6Pulse = tsRanges.Add6Pulse;

                for (int i = tsRanges.StartCol; i < tsRanges.StartCol + tsRanges.TotalCols; i++)
                {
                    for (int j = tsRanges.StartRow; j < tsRanges.StartRow + tsRanges.TotalRows; j++)
                    {
                        string slotFormat = tsRanges.TrolleySlotFormat;
                        Regex regex = new Regex("<(\\d+)>");
                        slotFormat = regex.Replace(slotFormat, "<col:$1>", 1);
                        slotFormat = regex.Replace(slotFormat, "<row:$1>", 1);

                        string slotcode = GenerateSlotCode(slotFormat, i, j);
                        TrolleySlot? slotExist = _dbContext.TrolleySlot.FirstOrDefault(x => x.TrolleySlotCode == slotcode);
                        if (slotExist != null)
                        {
                            result.errMessage = $"Slot code: {slotcode} has been used";
                            return result;
                        }

                        TrolleySlot? slotExist2 = _dbContext.TrolleySlot.FirstOrDefault(x => x.ColNo == i && x.RowNo == j && x.TrolleySlot_Id != 0);
                        if (slotExist2 != null)
                        {
                            result.errMessage = $"Slot Code: {slotcode} Column No {i} and Row No {j} have been used.";
                            return result;
                        }

                        Trolley? trolley = _dbContext.Trolley.FirstOrDefault(x => x.Trolley_Id == tsRanges.Trolley_Id);
                        if (trolley == null)
                        {
                            result.errMessage = $"Cannot find this Trolley, please refresh the list.";
                            return result;
                        }

                        TrolleySlot _ts = new TrolleySlot()
                        {
                            Trolley_Id = trolley.Trolley_Id,
                            TrolleySlotCode = slotcode,
                            ColNo = i,
                            RowNo = j,
                            IsActive = tsRanges.IsActive,
                            //HasEmptyDrawer = tsRanges.HasEmptyDrawer,
                            HasReel = tsRanges.HasReel,
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
                            IsLeft = tsRanges.IsLeft,
                        };
                        _dbContext.TrolleySlot.Add(_ts);

                        currentXPulse += tsRanges.XPulseIncrement;
                        currentYPulse += tsRanges.YPulseIncrement;
                        currentQrXPulse += tsRanges.QRXPulseIncrement;
                        currentQrYPulse += tsRanges.QRYPulseIncrement;
                        currentAdd1Pulse += tsRanges.Add1PulseIncrement;
                        currentAdd2Pulse += tsRanges.Add2PulseIncrement;
                        currentAdd3Pulse += tsRanges.Add3PulseIncrement;
                        currentAdd4Pulse += tsRanges.Add4PulseIncrement;
                        currentAdd5Pulse += tsRanges.Add5PulseIncrement;
                        currentAdd6Pulse += tsRanges.Add6PulseIncrement;

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

        public async Task<ServiceResponseModel<List<TrolleySlotDTO>>> SaveExcelTrolleySlot(List<TrolleySlotDTO> trolleySlot)
        {
            ServiceResponseModel<List<TrolleySlotDTO>> result = new ServiceResponseModel<List<TrolleySlotDTO>>();
            List<TrolleySlotDTO> errorsLine = new List<TrolleySlotDTO>();

            try
            {
                if (trolleySlot != null)
                {
                    foreach (var ts in trolleySlot)
                    {
                        bool isError = false;

                        TrolleySlot? slotExist = _dbContext.TrolleySlot.FirstOrDefault(x => x.TrolleySlotCode == ts.TrolleySlotCode);
                        TrolleySlot? slotExist2 = _dbContext.TrolleySlot.FirstOrDefault(x => x.ColNo == ts.ColNo && x.RowNo == ts.RowNo && x.TrolleySlot_Id != 0);

                        if (slotExist != null)
                        {
                            result.errMessage = $"Trolley Slot code: {ts.TrolleySlotCode} has been used";
                            ts.ErrorMsg = result.errMessage;
                            isError = true;
                        }
                        else if (slotExist2 != null)
                        {
                            result.errMessage = $"Trolley Slot Code: {ts.TrolleySlotCode} Column No {ts.ColNo} and Row No {ts.RowNo} have been used.";
                            ts.ErrorMsg = result.errMessage;
                            isError = true;
                        }

                        if (!isError)
                        {
                            TrolleySlot _ts = new TrolleySlot()
                            {
                                Trolley_Id = ts.Trolley_Id,
                                TrolleySlotCode = ts.TrolleySlotCode,
                                ColNo = ts.ColNo,
                                RowNo = ts.RowNo,
                                IsActive = ts.IsActive,
                                IsLeft = ts.IsLeft,
                                //HasEmptyDrawer = trolleySlot.HasEmptyDrawer,
                                //HasReel = trolleySlot.HasReel,
                                XPulse = ts.XPulse,
                                YPulse = ts.YPulse,
                                QRXPulse = ts.QRXPulse,
                                QRYPulse = ts.QRYPulse,
                                Add1Pulse = ts.Add1Pulse,
                                Add2Pulse = ts.Add2Pulse,
                                Add3Pulse = ts.Add3Pulse,
                                Add4Pulse = ts.Add4Pulse,
                                Add5Pulse = ts.Add5Pulse,
                                Add6Pulse = ts.Add6Pulse,
                            };
                            _dbContext.TrolleySlot.Add(_ts);
                            ts.ErrorMsg = "";
                            //errorsLine.Add();
                        }
                        else
                        {
                            errorsLine.Add(ts);
                        }
                        //errorsLine.Add(ts);

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

        public async Task<ServiceResponseModel<List<TrolleySlotDTO>>> UpdateExcelTSPulses(List<TrolleySlotDTO> tsPulses)
        {
            ServiceResponseModel<List<TrolleySlotDTO>> result = new ServiceResponseModel<List<TrolleySlotDTO>>();
            List<TrolleySlotDTO> errorsLine = new List<TrolleySlotDTO>();
            long slotId = 0;

            try
            {
                foreach (var ts in tsPulses)
                {
                    bool isError = false;

                    //validation
                    var validatefield = new List<(int? value, string fieldName)>
                    {
                        (ts.XPulse, "X Pulse"),
                        (ts.YPulse, "Y Pulse"),
                        (ts.QRXPulse, "Qr-X Pulse"),
                        (ts.QRYPulse, "Qr-Y Pulse"),
                        (ts.Add1Pulse, "Additional Pulse 1"),
                        (ts.Add2Pulse, "Additional Pulse 2"),
                        (ts.Add3Pulse, "Additional Pulse 3"),
                        (ts.Add4Pulse, "Additional Pulse 4"),
                        (ts.Add5Pulse, "Additional Pulse 5"),
                        (ts.Add6Pulse, "Additional Pulse 6"),
                    };

                    foreach (var (value, fieldName) in validatefield)
                    {
                        if (!value.HasValue || value < 0)
                        {
                            result.errMessage = $"Please enter a valid value for {fieldName}. Negative values are not allowed.";
                            ts.ErrorMsg = result.errMessage;
                            errorsLine?.Add(ts);
                            break;
                        }
                    }

                    TrolleySlot? slotExist = _dbContext.TrolleySlot.FirstOrDefault(x => x.TrolleySlotCode == ts.TrolleySlotCode);
                    //Slot? slotExist2 = _dbContext.Slot.FirstOrDefault(x => x.ColNo == slot.ColNo && x.RowNo == slot.RowNo && x.Slot_Id != 0);

                    if (slotExist == null)
                    {
                        result.errMessage = $"Slot code: {ts.TrolleySlotCode} did not exist.";
                        ts.ErrorMsg = result.errMessage;
                        isError = true;
                    }
                    else
                    {
                        slotId = slotExist.TrolleySlot_Id;
                    }

                    if (!isError)
                    {
                        if (errorsLine != null && errorsLine.Count != 0) break;

                        TrolleySlot? _ts = _dbContext.TrolleySlot.Find(slotId);
                        if (_ts == null)
                        {
                            result.errMessage = "This slot could not be found. Please refresh the list or check back later.";
                            return result;
                        }

                        _ts.XPulse = ts.XPulse;
                        _ts.YPulse = ts.YPulse;
                        _ts.QRXPulse = ts.QRXPulse;
                        _ts.QRYPulse = ts.QRYPulse;
                        _ts.Add1Pulse = ts.Add1Pulse;
                        _ts.Add2Pulse = ts.Add2Pulse;
                        _ts.Add3Pulse = ts.Add3Pulse;
                        _ts.Add4Pulse = ts.Add4Pulse;
                        _ts.Add5Pulse = ts.Add5Pulse;
                        _ts.Add6Pulse = ts.Add6Pulse;
                        //_slot.IsActive = slot.IsActive;
                        //_slot.IsLeft = slot.IsLeft;
                        _dbContext.TrolleySlot.Update(_ts);
                    }
                    else
                    {
                        errorsLine?.Add(ts);
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

        public async Task<ServiceResponseModel<TrolleyDTO>> GetTrolleyInfo(string req)
        {
            ServiceResponseModel<TrolleyDTO> result = new ServiceResponseModel<TrolleyDTO>();

            try
            {
                var trolleyInfo = await _dbContext.Trolley.Where(x => x.TrolleyCode == req).FirstOrDefaultAsync();

                if (trolleyInfo == null)
                {
                    result.errMessage = "Cannot find this Trolley [" + req + "].";
                    return result;
                }

                var trDTO = _mapper.Map<TrolleyDTO>(trolleyInfo);

                trDTO.Col1Balance = _dbContext.TrolleySlot.Where(x => x.Trolley_Id == trDTO.Trolley_Id && x.ColNo == 1 && x.IsLeft == true && x.HasReel == false).Count();
                trDTO.Col2Balance = _dbContext.TrolleySlot.Where(x => x.Trolley_Id == trDTO.Trolley_Id && x.ColNo == 2 && x.IsLeft == true && x.HasReel == false).Count();
                trDTO.Col3Balance = _dbContext.TrolleySlot.Where(x => x.Trolley_Id == trDTO.Trolley_Id && x.ColNo == 3 && x.IsLeft == true && x.HasReel == false).Count();
                trDTO.Col4Balance = _dbContext.TrolleySlot.Where(x => x.Trolley_Id == trDTO.Trolley_Id && x.ColNo == 1 && x.IsLeft == false && x.HasReel == false).Count();
                trDTO.Col5Balance = _dbContext.TrolleySlot.Where(x => x.Trolley_Id == trDTO.Trolley_Id && x.ColNo == 2 && x.IsLeft == false && x.HasReel == false).Count();
                trDTO.Col6Balance = _dbContext.TrolleySlot.Where(x => x.Trolley_Id == trDTO.Trolley_Id && x.ColNo == 3 && x.IsLeft == false && x.HasReel == false).Count();

                trDTO.Col1TotalUsed = 50 - trDTO.Col1Balance;
                trDTO.Col2TotalUsed = 50 - trDTO.Col2Balance;
                trDTO.Col3TotalUsed = 50 - trDTO.Col3Balance;
                trDTO.Col4TotalUsed = 50 - trDTO.Col4Balance;
                trDTO.Col5TotalUsed = 50 - trDTO.Col5Balance;
                trDTO.Col6TotalUsed = 50 - trDTO.Col6Balance;

                result.success = true;
                result.data = trDTO;
                return result;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        public async Task<ServiceResponseModel<SlotFreeDTO>> GetFreeTrolleySlot_BySlot_ASC(SlotFreeReqDTO req)
        {
            ServiceResponseModel<SlotFreeDTO> result = new ServiceResponseModel<SlotFreeDTO>();

            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@TotalSlot", req.TotalSlot),
                    new SqlParameter("@IsLeft", req.IsLeft),
                };

                string sql = "EXECUTE dbo.TrolleySlot_GET_FREESLOT_ASC @TotalSlot, @IsLeft ";
                var listDTO = await _dbContext.SP_TrolleySlot_GET_FREESLOT_ASC.FromSqlRaw(sql, parameters).ToListAsync();

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

        public async Task<ServiceResponseModel<List<TrolleyReelDtlDTO>>> GetTrolleyReelDtlList(long id)
        {
            ServiceResponseModel<List<TrolleyReelDtlDTO>> result = new ServiceResponseModel<List<TrolleyReelDtlDTO>>();

            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@Trolley_Id", id),
                };

                string sql = "EXECUTE dbo.Trolley_GET_REELDTLLIST @Trolley_Id ";
                var listDTO = await _dbContext.SP_TrolleyReelDtlList.FromSqlRaw(sql, parameters).ToListAsync();

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

        public async Task<ServiceResponseModel<TrolleySlotDTO>> UpdateTrolleySlotStatus(SlotStatusReqDTO slotReq)
        {
            ServiceResponseModel<TrolleySlotDTO> result = new ServiceResponseModel<TrolleySlotDTO>();

            try
            {
                // 1. checking Data
                if (slotReq == null)
                {
                    result.errMessage = "Please select Slot.";
                    return result;
                }

                TrolleySlot? _slot = _dbContext.TrolleySlot.Find(slotReq.Slot_Id);
                if (_slot == null)
                {
                    result.errMessage = "Cannot find this slot, please refresh the list.";
                    return result;
                }
                _slot.Priority = slotReq.Priority;
                _slot.IsActive = slotReq.IsActive;
                _slot.HasReel = slotReq.HasReel;
                _slot.ReelNo = slotReq.ReelNo;
                _slot.NeedCheck = slotReq.NeedCheck;

                if (slotReq.HasReel)
                {
                    if (slotReq.ReelCode != "")
                    {
                        var r = _dbContext.Reel.Where(x => x.ReelCode == slotReq.ReelCode).FirstOrDefault();
                        if (r != null)
                        {
                            _slot.Reel_Id = r.Reel_Id;
                        }
                    }
                    else
                    {
                        _slot.Reel_Id = Guid.Empty;
                    }
                }
                else
                {
                    _slot.Reel_Id = Guid.Empty;
                }

                _dbContext.TrolleySlot.Update(_slot);
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
