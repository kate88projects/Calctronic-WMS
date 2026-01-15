using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RackingSystem.Data;
using RackingSystem.General;
using RackingSystem.Models.Loader;
using RackingSystem.Models.RackJob;
using RackingSystem.Models;
using RackingSystem.Services.LoaderServices;
using RackingSystem.Services.SlotServices;
using RackingSystem.Services.TrolleyServices;
using RackingSystem.Models.Trolley;
using RackingSystem.Data.RackJob;
using RackingSystem.Helpers;
using RackingSystem.Models.Slot;
using EasyModbus;
using RackingSystem.Models.API;
using RackingSystem.Data.Maintenances;
using RackingSystem.Models.Log;

namespace RackingSystem.Controllers.API
{
    [Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme}")]
    [ApiController]
    [Route("api/[controller]")]
    public class PLCHubOutController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly ILoaderService _loaderService;
        private readonly ISlotService _slotService;
        private readonly ITrolleyService _trolleyService;
        private readonly IMapper _mapper;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public PLCHubOutController(AppDbContext dbContext, ILoaderService loaderService, ISlotService slotService, ITrolleyService trolleyService, IMapper mapper, IDbContextFactory<AppDbContext> contextFactory)
        {
            _dbContext = dbContext;
            _loaderService = loaderService;
            _slotService = slotService;
            _trolleyService = trolleyService;
            _mapper = mapper;
            _contextFactory = contextFactory;
        }

        [HttpPost("UpdateRackJobJson")]
        public async Task<ServiceResponseModel<bool>> UpdateRackJobJson([FromBody] RackJobHubOutJsonDTO req)
        {
            ServiceResponseModel<bool> result = new ServiceResponseModel<bool>();
            result.data = false;
            if (req == null)
            {
                result.errMessage = "No body.";
                return result;
            }

            try
            {
                var _job = _dbContext.RackJob.FirstOrDefault();
                if (_job != null)
                {
                    _job.Json = JsonConvert.SerializeObject(req);
                    await _dbContext.SaveChangesAsync();
                    result.success = true;
                }
                else
                {
                    result.errMessage = "No RackJob found.";
                }
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
            }
            return result;
        }

        [HttpGet("GetRackJobJson")]
        public ServiceResponseModel<RackJobHubOutJsonDTO> GetRackJobJson()
        {
            ServiceResponseModel<RackJobHubOutJsonDTO> result = new ServiceResponseModel<RackJobHubOutJsonDTO>();
            result.data = new RackJobHubOutJsonDTO();
            try
            {
                var _job = _dbContext.RackJob.FirstOrDefault();
                if (_job != null)
                {
                    result.data = JsonConvert.DeserializeObject<RackJobHubOutJsonDTO>(_job.Json) ?? new RackJobHubOutJsonDTO();
                    result.success = true;
                }
                else
                {
                    result.errMessage = "No RackJob found.";
                }
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
            }
            return result;
        }

        [HttpGet("StartHubOut/{req}/{qId}")]
        public async Task<ServiceResponseModel<RackJobHubOutDTO>> StartHubOut(string req, long qId)
        {
            ServiceResponseModel<RackJobHubOutDTO> result = new ServiceResponseModel<RackJobHubOutDTO>();
            result.data = new RackJobHubOutDTO();

            try
            {
                await PLCLogHelper.Instance.DeleteLog(_dbContext);

                var claims = User.Identities.First().Claims.ToList();
                string devId = claims?.FirstOrDefault(x => x.Type.Equals("DeviceId", StringComparison.OrdinalIgnoreCase))?.Value ?? "";

                var rackJob = _dbContext.RackJobQueue.Where(x => x.RackJobQueue_Id == qId).First();
                if (rackJob == null)
                {
                    result.success = false;
                    result.errMessage = "Cannot find this Job in queue.";
                    return result;
                }
                ServiceResponseModel<TrolleyDTO> r = await _trolleyService.GetTrolleyInfo(req);
                if (r.data == null)
                {
                    result.success = false;
                    result.errMessage = "This trolley is not for hub out.";
                    return result;
                }
                if (r.data.Col1Balance > 0 || r.data.Col2Balance > 0 || r.data.Col3Balance > 0 || r.data.Col4Balance > 0 || r.data.Col5Balance > 0 || r.data.Col6Balance > 0)
                {
                    //if (rackJob.Doc_Id != r.data.Trolley_Id)
                    //{
                    //    result.success = false;
                    //    result.errMessage = "This Trolley is not for this Queue.";
                    //    return result;
                    //}

                    var fLock = GetFlexilockStatus();
                    if (fLock.success == false)
                    {
                        result.success = false;
                        result.errMessage = "Please check Flexilock.";
                        return result;
                    }

                    var srms = _dbContext.RackJob.First();

                    RackJobHubOutDTO json = JsonConvert.DeserializeObject<RackJobHubOutDTO>(srms.Json) ?? new RackJobHubOutDTO();
                    result.data = json;
                    result.data.TrolleyInfo = r.data;

                    // get outstanding
                    if (rackJob.DocType == EnumQueueDocType.JO.ToString())
                    {
                        var list = _dbContext.JobOrderRaws.Where(x => x.JobOrder_Id == rackJob.Doc_Id && x.BalQty > 0).ToList();
                        foreach (var itm in list)
                        {
                            result.data.DtlList.Add(new RackJobHubOutDtlDTO
                            {
                                Detail_Id = itm.JobOrderRaws_Id.ToString(),
                                Id = itm.JobOrder_Id,
                                Item_Id = itm.Item_Id,
                                Qty = itm.BalQty,
                            });
                        }
                    }
                    else
                    {
                        var list = _dbContext.JobOrderEmergencyDetail.Where(x => x.JobOrderEmergency_Id == rackJob.Doc_Id && x.BalQty > 0).ToList();
                        foreach (var itm in list)
                        {
                            result.data.DtlList.Add(new RackJobHubOutDtlDTO
                            {
                                Detail_Id = itm.JobOrderEmergencyDetail_Id.ToString(),
                                Id = itm.JobOrderEmergency_Id,
                                Item_Id = itm.Item_Id,
                                Qty = itm.Qty,
                            });
                        }
                    }

                    if (result.data.DtlList.Count == 0)
                    {
                        result.success = false;
                        result.errMessage = "This Job Order is done retrieve.";
                        return result;
                    }

                    // update rackjob
                    srms.StartDate = DateTime.Now;
                    srms.CurrentJobType = rackJob.DocType;
                    srms.RackJobQueue_Id = qId;
                    srms.Trolley_Id = r.data.Trolley_Id;
                    srms.LoginIP = devId;
                    srms.TotalCount = result.data.DtlList.Count;
                    _dbContext.SaveChanges();

                    result.totalRecords = result.data.DtlList.Count;
                    result.success = true;
                    return result;
                }
                else
                {
                    result.success = false;
                    result.errMessage = "Trolley is fully loaded.";
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
            }
            return result;
        }

        internal ServiceResponseModel<List<int>> GetFlexilockStatus()
        {
            ServiceResponseModel<List<int>> result = new ServiceResponseModel<List<int>>();
            result.data = new List<int>();
            string methodName = "GetFlexilockStatus";

            var config = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Gantry1.ToString()).FirstOrDefault();
            if (config == null)
            {
                result.errMessage = "Please set IP Address. ";
                result.data.Add(0);
                result.data.Add(0);
                return result;
            }

            //// *** testing
            //result.success = true;
            //result.errMessage = "Right is locked. Left is locked. ";
            //result.data.Add(2);
            //result.data.Add(2);
            //return result;
            //// *** testing

            string decimalText = "";
            string lock1 = "0";
            string lock2 = "0";

            string plcIp = config.ConfigValue;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                int startAddress = 4208;
                int numRegisters = 1;
                int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                    lock1 = registers[i].ToString();
                }

                // second addr
                lock2 = "1";
                //startAddress = 4209;
                //numRegisters = 1;
                //registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                //for (int i = 0; i < registers.Length; i++)
                //{
                //    PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                //    decimalText = getDecimalText(registers[i]);
                //    lock2 = decimalText;
                //}

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            if (string.IsNullOrEmpty(lock1)) { lock1 = "0"; }
            if (string.IsNullOrEmpty(lock2)) { lock2 = "0"; }
            result.success = lock1 == "2" && lock2 == "1";
            result.data.Add(Convert.ToInt32(lock1));
            result.data.Add(Convert.ToInt32(lock2));
            return result;
        }

        [HttpGet("GetReelSlot/{slotCode}/{qId}")]
        public async Task<ServiceResponseModel<SlotReelDTO>> GetReelSlot(string slotCode, long qId)
        {
            ServiceResponseModel<SlotReelDTO> result = new ServiceResponseModel<SlotReelDTO>();
            result.data = new SlotReelDTO();
            string methodName = "GetReelSlot";

            try
            {
                var claims = User.Identities.First().Claims.ToList();
                string devId = claims?.FirstOrDefault(x => x.Type.Equals("DeviceId", StringComparison.OrdinalIgnoreCase))?.Value ?? "";

                var rackJob = _dbContext.RackJobQueue.Where(x => x.RackJobQueue_Id == qId).First();
                if (rackJob == null)
                {
                    result.success = false;
                    result.errMessage = "Cannot find this Job in queue.";
                    return result;
                }
                RackJobHubOutDtlDTO item = new RackJobHubOutDtlDTO();
                if (rackJob.DocType == EnumQueueDocType.JO.ToString())
                {
                    var itmFirst = _dbContext.JobOrderRaws.Where(x => x.JobOrder_Id == rackJob.Doc_Id && x.BalQty > 0).OrderBy(x => x.CreatedDate).FirstOrDefault();
                    if (itmFirst != null)
                    {
                        item.Detail_Id = itmFirst.JobOrderRaws_Id.ToString();
                        item.Id = itmFirst.JobOrder_Id;
                        item.Item_Id = itmFirst.Item_Id;
                        item.Qty = itmFirst.BalQty;
                    }
                }
                else
                {
                    var itmFirst = _dbContext.JobOrderEmergencyDetail.Where(x => x.JobOrderEmergency_Id == rackJob.Doc_Id && x.BalQty > 0).OrderBy(x => x.CreatedDate).FirstOrDefault();
                    if (itmFirst != null)
                    {
                        item.Detail_Id = itmFirst.JobOrderEmergencyDetail_Id.ToString();
                        item.Id = itmFirst.JobOrderEmergency_Id;
                        item.Item_Id = itmFirst.Item_Id;
                        item.Qty = itmFirst.BalQty;
                    }
                }
                if (item.Detail_Id.ToString() == "")
                {
                    result.success = false;
                    result.errMessage = "No Reel need to take.";
                    return result;
                }

                if (slotCode != "-")
                {
                    var slot = _dbContext.Slot.Where(x => x.SlotCode == slotCode).FirstOrDefault();
                    if (slot != null)
                    {
                        var rpt = new RackJobReport();
                        rpt.LoginIP = devId;
                        rpt.InfoType = "ERR";
                        rpt.InfoEvent = methodName;
                        rpt.InfoMessage1 = "Cannot get Reel on [" + slot.SlotCode + "].";
                        _dbContext.RackJobReport.Add(rpt);


                        slot.NeedCheck = true;
                        slot.CheckRemark = "Cannot get Reel";
                        _dbContext.SaveChanges();

                        PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, "Slot Error: Cannot get Reel on [" + slot.SlotCode + "].", "", true);

                    }
                }

                var rack = _dbContext.Slot.Where(x => x.IsActive == true && x.NeedCheck == false && x.HasReel == true && x.ReelNo == "0").FirstOrDefault();
                if (rack == null)
                {
                    result.success = false;
                    result.errMessage = "No Reel need to take.";
                    return result;
                }
                var reel = _dbContext.Reel.Where(x => x.Reel_Id == rack.Reel_Id).FirstOrDefault();
                if (reel == null)
                {
                    result.success = false;
                    result.errMessage = "No Reel need to take.";
                    return result;
                }

                // *** special checking in trolley really have at least 1 free slot
                int bottomSlotRow = 0;
                int bottomSlotCol = 0;

                //calculate slot
                var slotUsage = _dbContext.SlotCalculation.Where(x => x.MaxThickness >= reel.ActualHeight).OrderBy(x => x.MaxThickness).FirstOrDefault();
                if (slotUsage == null)
                {
                    result.success = false;
                    result.errMessage = "Please setup Slot Calculation.";
                    return result;
                }

                SlotFreeReqDTO reqSlot = new SlotFreeReqDTO();
                reqSlot.ColNo = 0;
                reqSlot.IsLeft = true;
                reqSlot.TotalSlot = slotUsage.ReserveSlot;
                ServiceResponseModel<SlotFreeDTO> rSlot = await _trolleyService.GetFreeTrolleySlot_BySlot_ASC(reqSlot);
                if (rSlot.data != null)
                {
                    if (rSlot.data.Row1 > 0)
                    {
                        bottomSlotCol = rSlot.data.ColNo;
                        bottomSlotRow = rSlot.data.Row1;
                    }
                }

                if (bottomSlotCol == 0 && bottomSlotRow == 0)
                {
                    result.success = false;
                    result.errMessage = "Trolley no empty slot is ready.";
                    result.errStackTrace = "-1";
                    return result;
                }

                result.success = true;
                result.data.Slot_Id = rack.Slot_Id;
                result.data.SlotCode = rack.SlotCode;
                result.data.ColNo = rack.ColNo;
                result.data.RowNo = rack.RowNo;
                result.data.IsLeft = rack.IsLeft;
                result.data.ReelId = rack.Reel_Id.ToString();
                result.data.ReelCode = reel.ReelCode;
                result.data.ActualHeight = reel.ActualHeight;
                result.data.Id = item.Detail_Id;
                //var slot = _dbContext.Slot.Where(x => x.SlotCode == result.data.SlotCode).FirstOrDefault();
                //if (slot != null)
                //{
                //    result.data.IsLeft = slot.IsLeft;
                //}
                return result;

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        [HttpGet("RetrieveTray/{slotCode}")]
        public async Task<ServiceResponseModel<int>> RetrieveTray(string slotCode)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            result.data = 0;
            string methodName = "RetrieveTray";

            var configRack = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking1.ToString()).FirstOrDefault();
            if (configRack == null)
            {
                result.errMessage = "Please set IP Address. ";
                return result;
            }

            var slot = _dbContext.Slot.Where(x => x.SlotCode == slotCode).FirstOrDefault();
            if (slot == null)
            {
                result.errMessage = "Cannot find Slot [" + slotCode + "]. ";
                return result;
            }

            //// *** testing
            //result.success = true;
            //result.data = 1;
            //return result;
            //// *** testing

            // 2. check plc which column is ready
            string plcIp = configRack.ConfigValue;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                //PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                // step 1 : left or right
                int registerAddress = 4298;
                //int valueToWrite = 0;
                int valueToWrite = slot.IsLeft ? 0 : 1;
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                // step 2 : x-pulses
                //byte[] bytes = BitConverter.GetBytes(57832);
                byte[] bytes = BitConverter.GetBytes(slot.XPulse);
                int highBinary = BitConverter.ToUInt16(bytes, 0);
                int lowBinary = BitConverter.ToUInt16(bytes, 2);
                registerAddress = 4300;
                valueToWrite = 0;
                modbusClient.WriteSingleRegister(registerAddress, lowBinary);
                registerAddress = 4299;
                valueToWrite = 0;
                modbusClient.WriteSingleRegister(registerAddress, highBinary);

                // step 3 : y-pulses
                //bytes = BitConverter.GetBytes(4930);
                bytes = BitConverter.GetBytes(slot.YPulse);
                highBinary = BitConverter.ToUInt16(bytes, 0);
                lowBinary = BitConverter.ToUInt16(bytes, 2);
                registerAddress = 4301;
                valueToWrite = 0;
                modbusClient.WriteSingleRegister(registerAddress, highBinary);
                registerAddress = 4302;
                valueToWrite = 0;
                modbusClient.WriteSingleRegister(registerAddress, lowBinary);

                // step 4 : 
                registerAddress = 4310;
                //valueToWrite = 634;
                valueToWrite = slot.QRXPulse;
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                // step 5 : 
                registerAddress = 4311;
                //valueToWrite = 377;
                valueToWrite = slot.QRYPulse;
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                // step 6 : 
                registerAddress = 4312;
                valueToWrite = 7;
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                // step Last : 
                registerAddress = 4297;
                valueToWrite = 2;
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                result.success = true;
                result.data = 1;

                var _slotOList = _dbContext.Slot.Where(x => x.Reel_Id == slot.Reel_Id).ToList();
                foreach (var _so in _slotOList)
                {
                    _so.Reel_Id = new Guid("00000000-0000-0000-0000-000000000000");
                    _so.ReelNo = "0";
                    _so.HasReel = false;
                    _so.HasEmptyTray = false;
                    await _dbContext.SaveChangesAsync();
                }

                slot.Reel_Id = new Guid("00000000-0000-0000-0000-000000000000");
                slot.ReelNo = "0";
                slot.HasReel = false;
                slot.HasEmptyTray = false;
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                //PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, "Disconnected.", "");
            }

            return result;
        }

        [HttpGet("GetRetrieveTrayStatus")]
        public ServiceResponseModel<int> GetRetrieveTrayStatus()
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            result.data = 0;
            string methodName = "GetRetrieveTrayStatus";

            var configRack = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking1.ToString()).FirstOrDefault();
            if (configRack == null)
            {
                result.errMessage = "Please set IP Address. ";
                result.data = 0;
                return result;
            }

            //// *** testing
            //result.success = true;
            //result.errMessage = "Done";
            //result.data = 2;
            //return result;
            //// *** testings

            DateTime dtRun = DateTime.Now;
            bool exit = false;

            string decimalText = "";
            int value = 0;

            string plcIp = configRack.ConfigValue;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                while (!exit)
                {
                    int startAddress = 4212;
                    int numRegisters = 1;
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        value = registers[i];
                    }

                    if (value == 1)
                    {
                        exit = true;
                    }
                    if ((DateTime.Now - dtRun).TotalSeconds > 30)
                    {
                        result.errMessage = "Timeout. Cannot get Empty Tray Status.";
                        exit = true;
                    }
                }

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            result.success = value == 1;
            result.data = value;

            return result;
        }

        [HttpGet("GetBottomSlot/{slotCode}/{actHeight}/{side}")]
        public async Task<ServiceResponseModel<TrolleySlotDTO>> GetBottomSlot(string slotCode, int actHeight, string side)
        {
            ServiceResponseModel<TrolleySlotDTO> result = new ServiceResponseModel<TrolleySlotDTO>();
            result.data = new TrolleySlotDTO();
            string methodName = "GetBottomSlot";

            try
            {
                var claims = User.Identities.First().Claims.ToList();
                string devId = claims?.FirstOrDefault(x => x.Type.Equals("DeviceId", StringComparison.OrdinalIgnoreCase))?.Value ?? "";

                if (slotCode != "-")
                {
                    var slotChg = _dbContext.TrolleySlot.Where(x => x.TrolleySlotCode == slotCode).FirstOrDefault();
                    if (slotChg != null)
                    {
                        var rpt = new RackJobReport();
                        rpt.LoginIP = devId;
                        rpt.InfoType = "ERR";
                        rpt.InfoEvent = methodName;
                        rpt.InfoMessage1 = "Cannot put Reel on [" + slotChg.TrolleySlotCode + "].";
                        _dbContext.RackJobReport.Add(rpt);


                        //slotChg.IsActive = false;
                        slotChg.NeedCheck = true;
                        slotChg.CheckRemark = "Cannot put Reel.";
                        _dbContext.SaveChanges();

                        PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, "Slot Error: Cannot put Reel on [" + slotChg.TrolleySlotCode + "].", "", true);

                    }
                }

                int bottomSlotRow = 0;
                int bottomSlotCol = 0;

                //calculate slot
                var slotUsage = _dbContext.SlotCalculation.Where(x => x.MaxThickness >= actHeight).OrderBy(x => x.MaxThickness).FirstOrDefault();
                if (slotUsage == null)
                {
                    result.success = false;
                    result.errMessage = "Please setup Slot Calculation.";
                    return result;
                }

                SlotFreeReqDTO reqSlot = new SlotFreeReqDTO();
                reqSlot.ColNo = 0;
                reqSlot.IsLeft = side == "A";
                reqSlot.TotalSlot = slotUsage.ReserveSlot;
                ServiceResponseModel<SlotFreeDTO> rSlot = await _trolleyService.GetFreeTrolleySlot_BySlot_ASC(reqSlot);
                if (rSlot.data != null)
                {
                    if (rSlot.data.Row1 > 0)
                    {
                        bottomSlotCol = rSlot.data.ColNo;
                        bottomSlotRow = rSlot.data.Row1;
                    }
                }

                if (bottomSlotCol == 0 && bottomSlotRow == 0)
                {
                    result.success = false;
                    result.errMessage = "No empty slot is ready.";
                    result.errStackTrace = "-1";
                    return result;
                }

                var slot = _dbContext.TrolleySlot.Where(x => x.RowNo == bottomSlotRow && x.ColNo == bottomSlotCol && x.IsLeft == reqSlot.IsLeft).FirstOrDefault();
                if (slot == null)
                {
                    result.success = false;
                    result.errMessage = "Cannot find trolley slot for Column [" + bottomSlotCol + "] Row [" + bottomSlotRow + "].";
                    return result;
                }
                var slotDTO = _mapper.Map<TrolleySlotDTO>(slot);
                slotDTO.TotalSlot = slotUsage.ReserveSlot;

                result.success = true;
                result.data = slotDTO;
                return result;

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        [HttpGet("PutAway/{slotCode}/{slotReserve}")]
        public ServiceResponseModel<int> PutAway(string slotCode, int slotReserve)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            string methodName = "PutAway";

            var configRack = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking1.ToString()).FirstOrDefault();
            if (configRack == null)
            {
                result.errMessage = "Please set IP Address. ";
                result.data = 0;
                return result;
            }

            var slot = _dbContext.TrolleySlot.Where(x => x.TrolleySlotCode == slotCode).FirstOrDefault();
            if (slot == null)
            {
                result.errMessage = "Cannot find Trolley Slot [" + slotCode + "]. ";
                result.data = 0;
                return result;
            }

            //// *** testing
            //result.success = true;
            //result.data = 1;
            //return result;
            //// *** testing

            // 2. check plc which column is ready
            int value = 0;
            string plcIp = configRack.ConfigValue; // "192.168.100.150";
            //if (slot.ColNo == 2) { plcIp = "192.168.100.151"; }
            //if (slot.ColNo == 3) { plcIp = "192.168.100.152"; }
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                // step 1 : left or right
                int registerAddress = 4298;
                //int valueToWrite = 0;
                int valueToWrite = 1;
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                // step 2 : x-pulses
                //byte[] bytes = BitConverter.GetBytes(57832);
                byte[] bytes = BitConverter.GetBytes(slot.XPulse);
                int highBinary = BitConverter.ToUInt16(bytes, 0);
                int lowBinary = BitConverter.ToUInt16(bytes, 2);
                registerAddress = 4300;
                valueToWrite = 0;
                modbusClient.WriteSingleRegister(registerAddress, lowBinary);
                registerAddress = 4299;
                valueToWrite = 0;
                modbusClient.WriteSingleRegister(registerAddress, highBinary);

                // step 3 : y-pulses
                //bytes = BitConverter.GetBytes(4930);
                bytes = BitConverter.GetBytes(slot.YPulse);
                highBinary = BitConverter.ToUInt16(bytes, 0);
                lowBinary = BitConverter.ToUInt16(bytes, 2);
                registerAddress = 4301;
                valueToWrite = 0;
                modbusClient.WriteSingleRegister(registerAddress, highBinary);
                registerAddress = 4302;
                valueToWrite = 0;
                modbusClient.WriteSingleRegister(registerAddress, lowBinary);

                // step 4 : 
                registerAddress = 4310;
                //valueToWrite = 634;
                valueToWrite = slot.QRXPulse;
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                // step 5 : 
                registerAddress = 4311;
                //valueToWrite = 377;
                valueToWrite = slot.QRYPulse;
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                // step 6 : 
                registerAddress = 4312;
                valueToWrite = 7;
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                // step Last : 
                registerAddress = 4297;
                valueToWrite = 1;
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);


                result.success = true;
                result.data = 1;

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            return result;
        }

        [HttpGet("GetPutAwayStatus")]
        public ServiceResponseModel<int> GetPutAwayStatus()
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            result.data = 0;
            string methodName = "GetPutAwayStatus";

            var configRack = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking1.ToString()).FirstOrDefault();
            if (configRack == null)
            {
                result.errMessage = "Please set IP Address. ";
                result.data = 0;
                return result;
            }

            //// *** testing
            //result.success = true;
            //result.errMessage = "Done Put Away.";
            //result.data = 0;
            //return result;
            //// *** testing

            DateTime dtRun = DateTime.Now;
            bool exit = false;

            string decimalText = "";
            int value = -1;

            string plcIp = configRack.ConfigValue;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                while (!exit)
                {
                    int startAddress = 4196; // 4229;
                    int numRegisters = 1;
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        value = registers[i];
                    }

                    if (value == 0) // 1 means tengah buat, 0 means complete
                    {
                        exit = true;
                    }
                    if ((DateTime.Now - dtRun).TotalSeconds > 30)
                    {
                        result.errMessage = "Timeout. Cannot get Reel ID.";
                        exit = true;
                    }
                }

                result.success = value == 0;
                result.data = value;

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            return result;
        }

        [HttpGet("UpdateReelIntoTrolley/{trolleyId}/{reelId}/{slotCode}/{slotReserve}/{dtlId}/{qId}")]
        public async Task<ServiceResponseModel<int>> UpdateReelIntoTrolley(long trolleyId, string reelId, string slotCode, int slotReserve, string dtlId, long qId)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            result.data = -1;
            string methodName = "UpdateReelIntoTrolley";

            try
            {
                var _trolley = _dbContext.Trolley.Find(trolleyId);
                if (_trolley == null)
                {
                    result.errMessage = "Trolley is not found.";
                    return result;
                }
                var _reel = _dbContext.Reel.Where(x => x.Reel_Id.ToString() == reelId).FirstOrDefault();
                if (_reel == null)
                {
                    result.errMessage = "Reel is not found.";
                    return result;
                }
                var _slot = _dbContext.TrolleySlot.Where(x => x.TrolleySlotCode == slotCode).FirstOrDefault();
                if (_slot == null)
                {
                    result.errMessage = "Trolley Slot is not found.";
                    return result;
                }

                var rackJob = _dbContext.RackJobQueue.Where(x => x.RackJobQueue_Id == qId).First();
                if (rackJob == null)
                {
                    result.success = false;
                    result.errMessage = "Cannot find this Job in queue.";
                    return result;
                }

                // temp hide
                // 1.1 update reel status
                _reel.StatusIdx = (int)EnumReelStatus.InTrolley;
                _reel.Status = EnumReelStatus.Out.ToString();
                await _dbContext.SaveChangesAsync();

                // 2. update detail tables
                if (rackJob.DocType == EnumQueueDocType.JO.ToString())
                {
                    var itmFirst = _dbContext.JobOrderRaws.Where(x => x.JobOrderRaws_Id.ToString() == dtlId).OrderBy(x => x.JobOrderDetail_Id).FirstOrDefault();
                    if (itmFirst != null)
                    {
                        itmFirst.BalQty = 0;
                    }
                }
                else
                {
                    var itmFirst = _dbContext.JobOrderEmergencyDetail.Where(x => x.JobOrderEmergency_Id == Convert.ToInt64(dtlId)).FirstOrDefault();
                    if (itmFirst != null)
                    {
                        itmFirst.BalQty = 0;
                    }
                }

                // 4. update slot set reelId
                _slot.Reel_Id = _reel.Reel_Id;
                _slot.ReelNo = "0";
                _slot.HasReel = true;
                await _dbContext.SaveChangesAsync();

                // 5. update other slot if is reserved
                if (slotReserve > 1)
                {
                    for (int iR = 1; iR < slotReserve; iR++)
                    {
                        var _slotO = _dbContext.TrolleySlot.Where(x => x.IsLeft == _slot.IsLeft && x.ColNo == _slot.ColNo && x.RowNo == (_slot.RowNo + iR)).FirstOrDefault();
                        if (_slotO == null)
                        {
                            result.errMessage = "Other Slot is not found.";
                            return result;
                        }
                        _slotO.Reel_Id = _reel.Reel_Id;
                        _slotO.ReelNo = iR.ToString();
                        _slotO.HasReel = true;
                        await _dbContext.SaveChangesAsync();
                    }
                }

                // 1.2 update slot id
                _reel.Slot_Id = _slot.TrolleySlot_Id;
                _reel.Status = EnumReelStatus.InTrolley.ToString();
                _reel.StatusIdx = (int)EnumReelStatus.InTrolley;
                _reel.IsReady = false;
                await _dbContext.SaveChangesAsync();

                // 6. Checking loader all column status and update
                int r = 0;
                var colBal = _dbContext.Slot.Where(x => x.HasReel == true).FirstOrDefault();
                if (colBal == null)
                {
                    // means all is out
                    r = 2;
                }

                result.success = true;
                result.data = r;
            }
            catch (Exception ex)
            {
                //PLCLogHelper.Instance.InsertPLCHubOutLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        [HttpGet("StopHubOut/{qId}")]
        public async Task<ServiceResponseModel<bool>> StopHubOut(long qId)
        {
            ServiceResponseModel<bool> result = new ServiceResponseModel<bool>();
            result.data = false;

            try
            {
                // 1. if loader is fully empty then remove q
                var q = _dbContext.RackJobQueue.Where(x => x.RackJobQueue_Id == qId).FirstOrDefault();
                if (q != null)
                {
                    _dbContext.RackJobQueue.Remove(q);
                    _dbContext.SaveChanges();

                    var sqlSP = await _dbContext.Database.ExecuteSqlInterpolatedAsync($"EXEC {GeneralStatic.SP_Q_Requeue} ");
                }

                // 2. update srms
                var srms = _dbContext.RackJob.First();

                var log = new RackJobLog();
                log.CurrentJobType = srms.CurrentJobType;
                log.Loader_Id = srms.Loader_Id;
                log.Trolley_Id = srms.Trolley_Id;
                log.RackJobQueue_Id = srms.RackJobQueue_Id;
                log.StartDate = srms.StartDate;
                log.EndDate = DateTime.Now;
                log.LoginIP = srms.LoginIP;
                _dbContext.RackJobLog.Add(log);

                srms.StartDate = DateTime.Now;
                srms.CurrentJobType = "";
                srms.RackJobQueue_Id = 0;
                srms.Loader_Id = 0;
                srms.Trolley_Id = 0;
                srms.LoginIP = "";
                _dbContext.SaveChanges();

                result.success = true;
                result.data = true;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        [HttpGet("GetUpcomingReels/{qId}/{skipRow}/{lastId}")]
        public async Task<ServiceResponseModel<List<RackJobHubOutDtlDTO>>> GetUpcomingReels(long qId, int skipRow, string lastId)
        {
            ServiceResponseModel<List<RackJobHubOutDtlDTO>> result = new ServiceResponseModel<List<RackJobHubOutDtlDTO>>();

            var rackJob = _dbContext.RackJobQueue.Where(x => x.RackJobQueue_Id == qId).First();
            if (rackJob == null)
            {
                result.success = false;
                result.data = new List<RackJobHubOutDtlDTO>();
                result.errMessage = "Cannot find this Job in queue.";
                return result;
            }

            List<RackJobHubOutDtlDTO> huboutReels = new List<RackJobHubOutDtlDTO>();
            int takeRow = skipRow > 0 ? 1 : 10;
            bool exist = true;

            if (rackJob.DocType == EnumQueueDocType.JO.ToString())
            {
                //var itemList = _dbContext.JobOrderRaws.Where(x => x.JobOrder_Id == rackJob.Doc_Id && x.BalQty > 0 && x.JobOrderRaws_Id.ToString() > "").OrderBy(x => x.JobOrderDetail_Id).OrderBy(x => x.CreatedDate).Skip(skipRow).Take(takeRow).ToList();
                var itemList = _dbContext.JobOrderRaws
                        .AsEnumerable()
                        .Where(x => x.JobOrder_Id == rackJob.Doc_Id && x.BalQty > 0 && string.Compare(x.JobOrderRaws_Id.ToString(), lastId) >= 0)
                        .OrderBy(x => x.JobOrderDetail_Id).OrderBy(x => x.CreatedDate).Skip(skipRow).Take(takeRow).ToList();

                foreach (var dtl in itemList)
                {
                    exist = true;
                    var item = _dbContext.Item.Where(x => x.Item_Id == dtl.Item_Id).FirstOrDefault();
                    var reelList = _dbContext.Reel.Where(x => x.Item_Id == dtl.Item_Id && x.IsReady == true && x.Status == EnumReelStatus.IsReady.ToString()).OrderBy(x => x.ExpiryDate).ToList();
                    foreach (var r in reelList)
                    {
                        if (!huboutReels.Where(x => x.Reel_Id == r.Reel_Id.ToString()).Any())
                        {
                            exist = true;
                            huboutReels.Add(new RackJobHubOutDtlDTO
                            {
                                Detail_Id = dtl.JobOrderRaws_Id.ToString(),
                                Reel_Id = r == null ? "" : r.Reel_Id.ToString(),
                                Item_Id = dtl.Item_Id,
                                ItemCode = item == null ? "" : item.ItemCode,
                                Qty = dtl.Qty,
                            });
                            break;
                        }
                    }
                    if (!exist)
                    {
                        huboutReels.Add(new RackJobHubOutDtlDTO
                        {
                            Detail_Id = dtl.JobOrderRaws_Id.ToString(),
                            Reel_Id = "",
                            Item_Id = dtl.Item_Id,
                            ItemCode = item == null ? "" : item.ItemCode,
                            Qty = dtl.Qty,
                        });
                    }
                }
            }
            else
            {
                var itemList = _dbContext.JobOrderEmergencyDetail.Where(x => x.JobOrderEmergency_Id == rackJob.Doc_Id && x.BalQty > 0 && x.JobOrderEmergency_Id > Convert.ToInt64(lastId)).OrderBy(x => x.CreatedDate).OrderBy(x => x.CreatedDate).Skip(skipRow).Take(takeRow).ToList();
                foreach (var dtl in itemList)
                {
                    exist = true;
                    var item = _dbContext.Item.Where(x => x.Item_Id == dtl.Item_Id).FirstOrDefault();
                    var reelList = _dbContext.Reel.Where(x => x.Item_Id == dtl.Item_Id && x.IsReady == true).OrderBy(x => x.ExpiryDate).ToList();
                    foreach (var r in reelList)
                    {
                        if (!huboutReels.Where(x => x.Reel_Id == r.Reel_Id.ToString()).Any())
                        {
                            exist = true;
                            huboutReels.Add(new RackJobHubOutDtlDTO
                            {
                                Detail_Id = dtl.JobOrderEmergencyDetail_Id.ToString(),
                                Reel_Id = r == null ? "" : r.Reel_Id.ToString(),
                                Item_Id = dtl.Item_Id,
                                ItemCode = item == null ? "" : item.ItemCode,
                                Qty = dtl.Qty,
                            });
                            break;
                        }
                    }
                    if (!exist)
                    {
                        huboutReels.Add(new RackJobHubOutDtlDTO
                        {
                            Detail_Id = dtl.JobOrderEmergencyDetail_Id.ToString(),
                            Reel_Id = "",
                            Item_Id = dtl.Item_Id,
                            ItemCode = item == null ? "" : item.ItemCode,
                            Qty = dtl.Qty,
                        });
                    }
                }
            }

            result.data = huboutReels;
            result.totalRecords = huboutReels.Count;

            return result;
        }

        [HttpGet("GetErrorLog/{qId}")]
        public async Task<ServiceResponseModel<List<LogDTO>>> GetErrorLog(long qId)
        {
            ServiceResponseModel<List<LogDTO>> result = new ServiceResponseModel<List<LogDTO>>();
            result.data = new List<LogDTO>();

            try
            {
                var srms = await _dbContext.RackJobLog.Where(x => x.RackJobQueue_Id == qId).FirstOrDefaultAsync();
                if (srms != null)
                {
                    var list = await _dbContext.PLCHubOutLog.Where(x => x.CreatedDate >= srms.StartDate && x.CreatedDate <= srms.EndDate && x.IsErr).ToListAsync();
                    foreach (var l in list)
                    {
                        result.data.Add(new LogDTO
                        {
                            CreatedDate = l.CreatedDate,
                            EventName = l.EventName,
                            Remark1 = l.Remark1,
                            Remark2 = l.Remark2,
                            Id = l.Loader_Id
                        });
                    }
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
