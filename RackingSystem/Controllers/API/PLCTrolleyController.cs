using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RackingSystem.Data;
using RackingSystem.General;
using RackingSystem.Models.RackJob;
using RackingSystem.Models.Trolley;
using RackingSystem.Models;
using RackingSystem.Services.SlotServices;
using RackingSystem.Services.TrolleyServices;
using EasyModbus;
using RackingSystem.Helpers;
using RackingSystem.Data.RackJob;
using RackingSystem.Models.Slot;
using RackingSystem.Data.Maintenances;
using Newtonsoft.Json.Linq;
using System.Reflection;
using RackingSystem.Models.Log;
using Microsoft.Data.SqlClient;

namespace RackingSystem.Controllers.API
{
    [Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme}")]
    [ApiController]
    [Route("api/[controller]")]
    public class PLCTrolleyController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly ISlotService _slotService;
        private readonly ITrolleyService _trolleyService;
        private readonly IMapper _mapper;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public PLCTrolleyController(AppDbContext dbContext, ISlotService slotService, ITrolleyService trolleyService, IMapper mapper, IDbContextFactory<AppDbContext> contextFactory)
        {
            _dbContext = dbContext;
            _slotService = slotService;
            _trolleyService = trolleyService;
            _mapper = mapper;
            _contextFactory = contextFactory;
        }

        internal string getDecimalText(int input)
        {
            if (input == 0) { return ""; }
            try
            {
                int intValue = input;
                string hexString = intValue.ToString("X");
                string resultText = "";
                for (int i = 0; i < hexString.Length; i += 2)
                {
                    string hexPair = hexString.Substring(i, 2);
                    int charCode = Convert.ToInt32(hexPair, 16);
                    resultText += (char)charCode;
                }
                return resultText;
            }
            catch
            {

            }
            return "";
        }

        [HttpPost("UpdateRackJobJson")]
        public async Task<ServiceResponseModel<bool>> UpdateRackJobJson([FromBody] RackJobTrolleyJsonDTO req)
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
        public ServiceResponseModel<RackJobTrolleyJsonDTO> GetRackJobJson()
        {
            ServiceResponseModel<RackJobTrolleyJsonDTO> result = new ServiceResponseModel<RackJobTrolleyJsonDTO>();
            result.data = new RackJobTrolleyJsonDTO();
            try
            {
                var _job = _dbContext.RackJob.FirstOrDefault();
                if (_job != null)
                {
                    result.data = JsonConvert.DeserializeObject<RackJobTrolleyJsonDTO>(_job.Json) ?? new RackJobTrolleyJsonDTO();
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

        [HttpGet("StartDrawerIn/{req}/{qId}")]
        public async Task<ServiceResponseModel<RackJobTrolleyDTO>> StartDrawerIn(string req, long qId)
        {
            ServiceResponseModel<RackJobTrolleyDTO> result = new ServiceResponseModel<RackJobTrolleyDTO>();
            result.data = new RackJobTrolleyDTO();

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

                var fLock = GetFlexilockStatus();
                if (fLock.success == false)
                {
                    result.success = false;
                    result.errMessage = "Please check Flexilock.";
                    return result;
                }

                var srms = _dbContext.RackJob.First();
                srms.StartDate = DateTime.Now;
                srms.CurrentJobType = rackJob.DocType;
                srms.RackJobQueue_Id = qId;
                srms.Trolley_Id = r.data.Trolley_Id;
                srms.LoginIP = devId;
                _dbContext.SaveChanges();

                RackJobTrolleyDTO json = JsonConvert.DeserializeObject<RackJobTrolleyDTO>(srms.Json) ?? new RackJobTrolleyDTO();
                result.data = json;
                result.data.TrolleyInfo = r.data;
                result.success = true;
                return result;
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

                PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                int startAddress = 4208;
                int numRegisters = 1;
                int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                    lock1 = registers[i].ToString();
                }

                // second addr
                lock2 = "1";
                //startAddress = 4209;
                //numRegisters = 1;
                //registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                //for (int i = 0; i < registers.Length; i++)
                //{
                //    PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                //    decimalText = getDecimalText(registers[i]);
                //    lock2 = decimalText;
                //}

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            if (string.IsNullOrEmpty(lock1)) { lock1 = "0"; }
            if (string.IsNullOrEmpty(lock2)) { lock2 = "0"; }
            result.success = lock1 == "2" && lock2 == "1";
            result.data.Add(Convert.ToInt32(lock1));
            result.data.Add(Convert.ToInt32(lock2));
            return result;
        }

        [HttpGet("CheckRetrieveSlot/{trolleyId}/{side}/{colNo}/{rowNo}")]
        public async Task<ServiceResponseModel<string>> CheckRetrieveSlot(long trolleyId, string side, int colNo, int rowNo)
        {
            ServiceResponseModel<string> result = new ServiceResponseModel<string>();
            result.data = "";
            string methodName = "CheckRetrieveSlot";

            var configRack = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking1.ToString()).FirstOrDefault();
            if (configRack == null)
            {
                result.errMessage = "Please set IP Address. ";
                return result;
            }

            bool left = side == "A";
            var slot = _dbContext.TrolleySlot.Where(x => x.Trolley_Id == trolleyId && x.IsLeft == left && x.ColNo == colNo && x.RowNo == rowNo).FirstOrDefault();
            if (slot == null)
            {
                result.errMessage = "Cannot find Trolley Slot [" + colNo + " - " + rowNo + "]. ";
                return result;
            }

            if (slot.IsActive == false)
            {
                result.errMessage = "Trolley Slot [" + colNo + " - " + rowNo + "] is inactive. ";
                return result;
            }

            if (slot.NeedCheck == true)
            {
                result.errMessage = "Trolley Slot [" + colNo + " - " + rowNo + "] is needed for checking. ";
                return result;
            }

            if (slot.HasReel)
            {
                result.errMessage = "Trolley Slot [" + colNo + " - " + rowNo + "] has reel. ";
                return result;
            }

            result.data = slot.TrolleySlotCode;
            result.success = true;

            //// *** testing
            //result.success = true;
            //result.data = 1;
            //return result;
            //// *** testing

            return result;
        }

        [HttpGet("RetrieveTray/{trolleyId}/{side}/{colNo}/{rowNo}")]
        public async Task<ServiceResponseModel<string>> RetrieveTray(long trolleyId, string side, int colNo, int rowNo)
        {
            ServiceResponseModel<string> result = new ServiceResponseModel<string>();
            result.data = side;
            string methodName = "RetrieveTray";

            var configRack = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking1.ToString()).FirstOrDefault();
            if (configRack == null)
            {
                result.errMessage = "Please set IP Address. ";
                return result;
            }

            bool left = side == "A";
            var slot = _dbContext.TrolleySlot.Where(x => x.Trolley_Id == trolleyId && x.IsLeft == left && x.ColNo == colNo && x.RowNo == rowNo).FirstOrDefault();
            if (slot == null)
            {
                result.errMessage = "Cannot find Trolley Slot [" + colNo + " - " + rowNo + "]. ";
                return result;
            }

            result.errStackTrace = slot.TrolleySlotCode;

            //// *** testing
            //// double check slot_id
            //string slotCode = GetSlotIDByIP(configRack.ConfigValue);
            //var slotChk = _dbContext.TrolleySlot.Where(x => x.Trolley_Id == trolleyId && x.TrolleySlotCode == slotCode).FirstOrDefault();
            //if (slotChk == null)
            //{
            //    result.errMessage = "Cannot find Trolley Slot Returned [" + slotCode+ "]. ";
            //}
            //else
            //{
            //    side= slotChk.IsLeft ? "A" : "B";
            //    ReadPulseByIP(configRack.ConfigValue, trolleyId, slotCode);    
            //}
            //result.success = true;
            //result.data = side;
            //return result;
            //// *** testing

            // 2. check plc which column is ready
            string plcIp = configRack.ConfigValue;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                //PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                // step 1 : left or right
                int registerAddress = 4298;
                //int valueToWrite = 0;
                int valueToWrite = 1; // slot.IsLeft ? 0 : 1;
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

                await Task.Delay(2000);

                // double check slot_id
                string slotCode = GetSlotIDByIP(configRack.ConfigValue);
                var slotChk = _dbContext.TrolleySlot.Where(x => x.Trolley_Id == trolleyId && x.TrolleySlotCode == slotCode).FirstOrDefault();
                if (slotChk == null)
                {
                    result.errMessage = "Cannot find Trolley Slot Returned [" + slotCode + "]. ";
                }
                else
                {
                    side = slotChk.IsLeft ? "A" : "B";
                    ReadTrolleyPulseByIP(configRack.ConfigValue, trolleyId, slotCode);
                }

                result.success = true;
                result.data = side;

            }
            catch (Exception ex)
            {
                //PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                //PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Disconnected.", "");
            }

            return result;
        }

        [HttpGet("GetRetrieveTrayStatus/{trolleyId}")]
        public async Task<ServiceResponseModel<RackJobSlotInfo>> GetRetrieveTrayStatus(long trolleyId)
        {
            ServiceResponseModel<RackJobSlotInfo> result = new ServiceResponseModel<RackJobSlotInfo>();
            result.data = new RackJobSlotInfo();
            string methodName = "GetRetrieveTrayStatus";

            var configRack = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking1.ToString()).FirstOrDefault();
            if (configRack == null)
            {
                result.errMessage = "Please set IP Address. ";
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

            int value = 0;

            string plcIp = configRack.ConfigValue;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                while (!exit)
                {
                    // address to check retrieve status
                    int startAddress = 4212;
                    int numRegisters = 1;
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        value = registers[i];
                    }

                    if (value == 1)
                    {
                        exit = true;
                    }
                    if ((DateTime.Now - dtRun).TotalSeconds > 2)
                    {
                        result.errMessage = "Timeout. Cannot get Empty Tray Status.";
                        exit = true;
                    }
                }

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            if (value == 0)
            {
                int valueErr = -1;
                modbusClient = new ModbusClient(plcIp, port);
                try
                {
                    await Task.Delay(1000);

                    modbusClient.Connect();

                    PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                    // address to check have drawer or not. 0 :: for retrieve if have drawer, 1 :: for retrieve if no drawer, 2 :: for putaway if have drawer
                    int startAddress = 4231;
                    int numRegisters = 1;
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        valueErr = registers[i];
                    }

                    if (valueErr == 0)
                    {
                        try
                        {
                            exit = false;
                            //modbusClient.Connect();

                            PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                            while (!exit)
                            {
                                startAddress = 4212;
                                numRegisters = 1;
                                registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                                for (int i = 0; i < registers.Length; i++)
                                {
                                    PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                                    value = registers[i];
                                }

                                if (value == 1)
                                {
                                    exit = true;
                                }
                                if ((DateTime.Now - dtRun).TotalSeconds > 15)
                                {
                                    result.errMessage = "Timeout. Cannot get Empty Tray Status.";
                                    exit = true;
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                            result.errMessage = ex.Message;
                            result.errStackTrace = ex.StackTrace ?? "";
                        }
                        finally
                        {
                            //modbusClient.Disconnect();
                            PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Disconnected.", "", false);
                        }
                    }
                    result.errMessage = valueErr.ToString();
                }
                catch (Exception ex)
                {
                    PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                    result.errMessage = ex.Message;
                    result.errStackTrace = ex.StackTrace ?? "";
                }
                finally
                {
                    modbusClient.Disconnect();
                    PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Disconnected.", "", false);
                }
            }

            // double check slot_id
            string slotCode = GetSlotIDByIP(configRack.ConfigValue);
            var slotChk = _dbContext.Slot.Where(x => x.SlotCode == slotCode).FirstOrDefault();
            if (slotChk == null)
            {
                result.errMessage = "Cannot find Slot Retrieve [" + slotCode + "]. ";
            }
            else
            {
                var pulses = ReadTrolleyPulseByIP(configRack.ConfigValue, trolleyId, slotCode);
                result.data.SlotCode = slotCode;
                result.data.QRXPulse = pulses[0];
                result.data.QRYPulse = pulses[1];
                result.data.QRXPulseDiffer = pulses[2];
                result.data.QRXPulseDiffer = pulses[3];
            }

            result.success = value == 1;
            result.data.data = value.ToString();

            return result;
        }

        [HttpGet("GetEmptySlot/{slotCode}")]
        public async Task<ServiceResponseModel<SlotDTO>> GetEmptySlot(string slotCode)
        {
            ServiceResponseModel<SlotDTO> result = new ServiceResponseModel<SlotDTO>();
            result.data = new SlotDTO();
            string methodName = "GetBottomSlot";

            try
            {
                var claims = User.Identities.First().Claims.ToList();
                string devId = claims?.FirstOrDefault(x => x.Type.Equals("DeviceId", StringComparison.OrdinalIgnoreCase))?.Value ?? "";

                if (slotCode != "-")
                {
                    var slotChg = _dbContext.Slot.Where(x => x.SlotCode == slotCode).FirstOrDefault();
                    if (slotChg != null)
                    {
                        var rpt = new RackJobReport();
                        rpt.LoginIP = devId;
                        rpt.InfoType = "ERR";
                        rpt.InfoEvent = methodName;
                        rpt.InfoMessage1 = "Cannot put Reel on [" + slotChg.SlotCode + "].";
                        _dbContext.RackJobReport.Add(rpt);


                        //slotChg.IsActive = false;
                        slotChg.NeedCheck = true;
                        slotChg.CheckRemark = "Cannot put Reel.";
                        _dbContext.SaveChanges();
                    }
                }

                int bottomSlotRow = 0;
                int bottomSlotCol = 0;
                var colList = _dbContext.SlotColumnSetting.OrderBy(x => x.EmptyDrawer_IN_Idx).ToList();
                foreach (var col in colList)
                {
                    SlotFreeReqDTO req = new SlotFreeReqDTO();
                    req.ColNo = col.ColNo;
                    req.TotalSlot = 1;

                    ServiceResponseModel<SlotFreeDTO> r = await _slotService.GetFreeSlot_ByColumn_ASC(req);
                    if (r.data != null)
                    {
                        if (r.data.Row1 > 0)
                        {
                            bottomSlotCol = col.ColNo;
                            bottomSlotRow = r.data.Row1; // + slotUsage.ReserveSlot - 1;
                            break;
                        }
                    }
                }

                var slot = _dbContext.Slot.Where(x => x.ColNo == bottomSlotCol && x.RowNo == bottomSlotRow).FirstOrDefault();
                if (slot == null)
                {
                    result.success = false;
                    result.errMessage = "Cannot find empty slot.";
                    return result;
                }
                var slotDTO = _mapper.Map<SlotDTO>(slot);
                slotDTO.TotalSlot = 1;

                result.success = true;
                result.data = slotDTO;
                return result;

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", false);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        [HttpGet("PutAway/{slotCode}")]
        public ServiceResponseModel<int> PutAway(string slotCode)
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

            var slot = _dbContext.Slot.Where(x => x.SlotCode == slotCode).FirstOrDefault();
            if (slot == null)
            {
                result.errMessage = "Cannot find Slot [" + slotCode + "]. ";
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

                PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

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
                valueToWrite = 1;
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);


                result.success = true;
                result.data = 1;

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            return result;
        }

        [HttpGet("GetPutAwayStatus/{slotCode}")]
        public ServiceResponseModel<RackJobSlotInfo> GetPutAwayStatus(string slotCode)
        {
            ServiceResponseModel<RackJobSlotInfo> result = new ServiceResponseModel<RackJobSlotInfo>();
            result.data = new RackJobSlotInfo();
            string methodName = "GetPutAwayStatus";

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
            //result.errMessage = "Done Put Away.";
            //result.data = 0;
            //slot.HasEmptyTray = true;
            //_dbContext.SaveChanges();
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

                PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                while (!exit)
                {
                    int startAddress = 4196; // 4229;
                    int numRegisters = 1;
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        value = registers[i];
                    }

                    if (value == 0) // 1 means tengah buat, 0 means complete
                    {
                        slot.HasEmptyTray = true;
                        _dbContext.SaveChanges();

                        exit = true;
                    }
                    if ((DateTime.Now - dtRun).TotalSeconds > 30)
                    {
                        result.errMessage = "Timeout. Cannot get Reel ID.";
                        exit = true;
                    }
                }

                // double check slot_id
                string slotCodeRead = GetSlotIDByIP(configRack.ConfigValue);
                var slotChk = _dbContext.Slot.Where(x => x.SlotCode == slotCodeRead).FirstOrDefault();
                if (slotChk == null)
                {
                    result.errMessage = "Cannot find Slot Retrieve [" + slotCodeRead + "]. ";
                }
                else
                {
                    var pulses = ReadPulseByIP(configRack.ConfigValue, slotCodeRead);
                    result.data.SlotCode = slotCode;
                    result.data.QRXPulse = pulses[0];
                    result.data.QRYPulse = pulses[1];
                    result.data.QRXPulseDiffer = pulses[2];
                    result.data.QRXPulseDiffer = pulses[3];
                }

                result.success = value == 0;
                result.data.data = value.ToString();

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Disconnected.", "", false);
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
                    var list = await _dbContext.PLCTrolleyLog.Where(x => x.CreatedDate >= srms.StartDate && x.CreatedDate <= srms.EndDate && x.IsErr).ToListAsync();
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

        [HttpGet("StartTrolleyOut/{code}")]
        public async Task<ServiceResponseModel<List<TrolleySlotDTO>>> StartTrolleyOut(string code)
        {
            ServiceResponseModel<List<TrolleySlotDTO>> result = new ServiceResponseModel<List<TrolleySlotDTO>>();
            result.data = new List<TrolleySlotDTO>();

            try
            {
                var claims = User.Identities.First().Claims.ToList();
                string devId = claims?.FirstOrDefault(x => x.Type.Equals("DeviceId", StringComparison.OrdinalIgnoreCase))?.Value ?? "";

                var tr = await _dbContext.Trolley.Where(x => x.TrolleyCode == code).FirstOrDefaultAsync();
                if (tr == null)
                {
                    result.errMessage = "Cannot find this Trolley.";
                    return result;
                }

                var r = await _trolleyService.GetTrolleySlotList();
                result.success = true;
                result.data = r.data;

            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
            }
            return result;
        }

        [HttpGet("SearchTrolleyOut/{trCode}/{itemCode}")]
        public async Task<ServiceResponseModel<List<TrolleySlotReelDTO>>> SearchTrolleyOut(string trCode, string itemCode)
        {
            ServiceResponseModel<List<TrolleySlotReelDTO>> result = new ServiceResponseModel<List<TrolleySlotReelDTO>>();
            result.data = new List<TrolleySlotReelDTO>();
            string methodName = "SearchTrolleyOut";

            try
            {
                var claims = User.Identities.First().Claims.ToList();
                string devId = claims?.FirstOrDefault(x => x.Type.Equals("DeviceId", StringComparison.OrdinalIgnoreCase))?.Value ?? "";

                var tr = await _dbContext.Trolley.Where(x => x.TrolleyCode == trCode).FirstOrDefaultAsync();
                if (tr == null)
                {
                    result.errMessage = "Cannot find this Trolley.";
                    return result;
                }

                var parameters = new[]
                {
                    new SqlParameter("@TrolleyCode", trCode),
                    new SqlParameter("@ItemCode", itemCode),
                };

                string sql = "EXECUTE dbo.TrolleySlot_GET_SEARCHLIST @TrolleyCode,@ItemCode ";
                var listDTO = await _dbContext.SP_TrolleySlotSearchList.FromSqlRaw(sql, parameters).ToListAsync();
                if (listDTO.Count == 0)
                {
                    result.success = false;
                    result.errMessage = "No item found in this Trolley.";
                }
                else
                {
                    result.success = true;
                    result.data = listDTO;

                    int port = 1502;
                    foreach (var dtl in listDTO)
                    {
                        string plcIp = dtl.IPAdd1;
                        if (dtl.ColNo == 2) { plcIp = dtl.IPAdd2; }
                        if (dtl.ColNo == 3) { plcIp = dtl.IPAdd3; }
                        ModbusClient modbusClient = new ModbusClient(plcIp, port);
                        try
                        {
                            modbusClient.Connect();

                            PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                            modbusClient.WriteSingleCoil(dtl.RowNo - 1, true);
                        }
                        catch (Exception ex)
                        {
                            PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                            result.errMessage = ex.Message;
                            result.errStackTrace = ex.StackTrace ?? "";
                        }
                        finally
                        {
                            modbusClient.Disconnect();
                            PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Disconnected.", "", false);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
            }
            return result;
        }

        [HttpPost("TurnOffTrolleyOut")]
        public async Task<ServiceResponseModel<bool>> TurnOffTrolleyOut([FromBody] List<long> req)
        {
            ServiceResponseModel<bool> result = new ServiceResponseModel<bool>();
            result.data = false;
            string methodName = "TurnOffTrolleyOut";
            if (req == null)
            {
                result.errMessage = "No body.";
                return result;
            }

            try
            {
                var listDTO = _dbContext.TrolleySlot.Where(x => req.Contains(x.TrolleySlot_Id)).ToList();
                int port = 1502;
                foreach (var dtl in listDTO)
                {
                    var tr = _dbContext.Trolley.Where(x => x.Trolley_Id == dtl.Trolley_Id).FirstOrDefault();
                    if (tr == null) { continue; }

                    string plcIp = tr.IPAdd1;
                    if (dtl.ColNo == 2) { plcIp = tr.IPAdd2; }
                    if (dtl.ColNo == 3) { plcIp = tr.IPAdd3; }
                    ModbusClient modbusClient = new ModbusClient(plcIp, port);
                    try
                    {
                        modbusClient.Connect();

                        PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                        modbusClient.WriteSingleCoil(dtl.RowNo - 1, true);
                    }
                    catch (Exception ex)
                    {
                        PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                        result.errMessage = ex.Message;
                        result.errStackTrace = ex.StackTrace ?? "";
                    }
                    finally
                    {
                        modbusClient.Disconnect();
                        PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Disconnected.", "", false);
                    }
                }

                foreach (var dtl in listDTO)
                {
                    var _reel = _dbContext.Reel.Where(x => x.Reel_Id == dtl.Reel_Id).FirstOrDefault();
                    if (_reel == null)
                    {
                        result.errMessage = "Reel is not found.";
                        return result;
                    }

                    var tr = _dbContext.Trolley.Where(x => x.Trolley_Id == dtl.Trolley_Id).FirstOrDefault();
                    if (tr == null) { continue; }

                    var _slotOList = _dbContext.TrolleySlot.Where(x => x.Reel_Id == dtl.Reel_Id).ToList();
                    foreach (var _so in _slotOList)
                    {
                        _so.Reel_Id = new Guid("00000000-0000-0000-0000-000000000000");
                        _so.ReelNo = "0";
                        _so.HasReel = false;
                    }

                    var slot = _dbContext.TrolleySlot.Where(x => x.TrolleySlot_Id == dtl.TrolleySlot_Id).FirstOrDefault();
                    if (slot != null)
                    {
                        slot.Reel_Id = new Guid("00000000-0000-0000-0000-000000000000");
                        slot.ReelNo = "0";
                        slot.HasReel = false;
                    }

                    _reel.Status = EnumReelStatus.Out.ToString();
                    _reel.StatusIdx = (int)EnumReelStatus.Out;
                    _reel.IsReady = false;
                    
                }
                await _dbContext.SaveChangesAsync();
                result.success = true;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
            }
            return result;
        }

        internal string GetSlotIDByIP(string ip)
        {
            string methodName = "GetSlotIDByIP";
            string result = "";

            DateTime dtRun = DateTime.Now;
            bool exit = false;

            string decimalText = "";
            string slotID = "";
            int value = 0;

            string plcIp = ip;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                int startAddress = 4201;
                int numRegisters = 1;

                while (!exit)
                {
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        value = registers[i];
                        decimalText = getDecimalText(registers[i]);
                    }

                    if (value > 0)
                    {
                        exit = true;
                        slotID = slotID + decimalText;
                    }
                    if ((DateTime.Now - dtRun).TotalSeconds > 3)
                    {
                        PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Timeout. Cannot get Slot ID..", "", false);
                        exit = true;
                    }
                }

                if (slotID != "")
                {
                    // second addr
                    startAddress = 4202;
                    numRegisters = 1;
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        decimalText = getDecimalText(registers[i]);
                        if (decimalText.Contains("\0"))
                        {
                            decimalText = decimalText.Substring(0, 1);
                        }
                        slotID = slotID + decimalText;
                    }

                    // third addr
                    startAddress = 4203;
                    numRegisters = 1;
                    registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        decimalText = getDecimalText(registers[i]);
                        if (decimalText.Contains("\0"))
                        {
                            decimalText = decimalText.Substring(0, 1);
                        }
                        slotID = slotID + decimalText;
                    }

                    // forth addr
                    startAddress = 4204;
                    numRegisters = 1;
                    registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        decimalText = getDecimalText(registers[i]);
                        if (decimalText.Contains("\0"))
                        {
                            decimalText = decimalText.Substring(0, 1);
                        }
                        slotID = slotID + decimalText;
                    }

                    // fifth addr
                    startAddress = 4205;
                    numRegisters = 1;
                    registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        decimalText = getDecimalText(registers[i]);
                        if (decimalText.Contains("\0"))
                        {
                            decimalText = decimalText.Substring(0, 1);
                        }
                        slotID = slotID + decimalText;
                    }

                    // sixth addr
                    startAddress = 4206;
                    numRegisters = 1;
                    registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        decimalText = getDecimalText(registers[i]);
                        if (decimalText.Contains("\0"))
                        {
                            decimalText = decimalText.Substring(0, 1);
                        }
                        slotID = slotID + decimalText;
                    }

                    // seventh addr
                    startAddress = 4207;
                    numRegisters = 1;
                    registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        decimalText = getDecimalText(registers[i]);
                        if (decimalText.Contains("\0"))
                        {
                            decimalText = decimalText.Substring(0, 1);
                        }
                        slotID = slotID + decimalText;
                    }

                    PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, $"Get Reel ID {slotID}", "", false);

                    // *** testing
                    //slotID = "A00000018";
                    result = slotID;
                }

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                //result.errMessage = ex.Message;
                //result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            return result;
        }

        internal List<int> ReadPulseByIP(string ip, string slotCode)
        {
            List<int> result = new List<int>();
            result.Add(0);
            result.Add(0);
            result.Add(0);
            result.Add(0);

            string methodName = "ReadPulse";

            var slot = _dbContext.Slot.Where(x => x.SlotCode == slotCode).FirstOrDefault();
            if (slot == null)
            {
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: Cannot find Slot Code [" + slotCode + "].", "", true);
                return result;
            }

            //// *** testing
            //result.success = true;
            //result.data = 1;
            //return result;
            //// *** testing

            // 2. check plc which column is ready
            string plcIp = ip;
            int port = 502;

            DateTime dtRun = DateTime.Now;
            bool exit = false;
            string decimalText = "";
            string qrXText = "";
            string qrYText = "";
            int value = 0;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                int startAddress = 4223;
                int numRegisters = 1;

                while (!exit)
                {
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        value = registers[i];
                    }

                    if (value > 0)
                    {
                        exit = true;
                    }
                    if ((DateTime.Now - dtRun).TotalSeconds > 3)
                    {
                        //result.errMessage = "Timeout. Cannot get Status.";
                        exit = true;
                    }
                }

                if (value > 0)
                {
                    // first addr
                    startAddress = 4208;
                    numRegisters = 1;
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        decimalText = getDecimalText(registers[i]);
                        if (decimalText.Contains("\0"))
                        {
                            decimalText = decimalText.Substring(0, 1);
                        }
                        qrXText = qrXText + decimalText;
                    }

                    // second addr
                    startAddress = 4209;
                    numRegisters = 1;
                    registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        decimalText = getDecimalText(registers[i]);
                        if (decimalText.Contains("\0"))
                        {
                            decimalText = decimalText.Substring(0, 1);
                        }
                        qrXText = qrXText + decimalText;
                    }

                    // third addr
                    startAddress = 4210;
                    numRegisters = 1;
                    registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        decimalText = getDecimalText(registers[i]);
                        if (decimalText.Contains("\0"))
                        {
                            decimalText = decimalText.Substring(0, 1);
                        }
                        qrYText = qrYText + decimalText;
                    }

                    // forth addr
                    startAddress = 4211;
                    numRegisters = 1;
                    registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        decimalText = getDecimalText(registers[i]);
                        if (decimalText.Contains("\0"))
                        {
                            decimalText = decimalText.Substring(0, 1);
                        }
                        qrYText = qrYText + decimalText;
                    }

                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Get X {qrXText} Y {qrYText}", "", false);

                    int qrX = 0;
                    int qrY = 0;
                    int.TryParse(qrXText, out qrX);
                    int.TryParse(qrYText, out qrY);
                    if (qrX > 0 || qrY > 0)
                    {
                        result[0] = qrX;
                        result[1] = qrY;
                        result[2] = qrX - slot.QRXPulse;
                        result[3] = qrY - slot.QRYPulse;

                        //slot.QRXPulse = qrX;
                        //slot.QRYPulse = qrY;
                        //_dbContext.SaveChanges();

                    }

                }

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                //result.errMessage = ex.Message;
                //result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            return result;
        }

        internal List<int> ReadTrolleyPulseByIP(string ip, long trolleyId, string slotCode)
        {
            List<int> result = new List<int>();
            result.Add(0);
            result.Add(0);

            string methodName = "ReadPulse";

            var slot = _dbContext.TrolleySlot.Where(x => x.Trolley_Id == trolleyId && x.TrolleySlotCode == slotCode).FirstOrDefault();
            if (slot == null)
            {
                PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Error: Cannot find Trolley Id [" + trolleyId + "] Slot Code [" + slotCode + "].", "", true);
                return result;
            }

            //// *** testing
            //result.success = true;
            //result.data = 1;
            //return result;
            //// *** testing

            // 2. check plc which column is ready
            string plcIp = ip;
            int port = 502;

            DateTime dtRun = DateTime.Now;
            bool exit = false;
            string decimalText = "";
            string qrXText = "";
            string qrYText = "";
            int value = 0;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                int startAddress = 4223;
                int numRegisters = 1;

                while (!exit)
                {
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        value = registers[i];
                    }

                    if (value > 0)
                    {
                        exit = true;
                    }
                    if ((DateTime.Now - dtRun).TotalSeconds > 3)
                    {
                        //result.errMessage = "Timeout. Cannot get Status.";
                        exit = true;
                    }
                }

                if (value > 0)
                {
                    // first addr
                    startAddress = 4208;
                    numRegisters = 1;
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        decimalText = getDecimalText(registers[i]);
                        if (decimalText.Contains("\0"))
                        {
                            decimalText = decimalText.Substring(0, 1);
                        }
                        qrXText = qrXText + decimalText;
                    }

                    // second addr
                    startAddress = 4209;
                    numRegisters = 1;
                    registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        decimalText = getDecimalText(registers[i]);
                        if (decimalText.Contains("\0"))
                        {
                            decimalText = decimalText.Substring(0, 1);
                        }
                        qrXText = qrXText + decimalText;
                    }

                    // third addr
                    startAddress = 4210;
                    numRegisters = 1;
                    registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        decimalText = getDecimalText(registers[i]);
                        if (decimalText.Contains("\0"))
                        {
                            decimalText = decimalText.Substring(0, 1);
                        }
                        qrYText = qrYText + decimalText;
                    }

                    // forth addr
                    startAddress = 4211;
                    numRegisters = 1;
                    registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        decimalText = getDecimalText(registers[i]);
                        if (decimalText.Contains("\0"))
                        {
                            decimalText = decimalText.Substring(0, 1);
                        }
                        qrYText = qrYText + decimalText;
                    }

                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Get X {qrXText} Y {qrYText}", "", false);

                    int qrX = 0;
                    int qrY = 0;
                    int.TryParse(qrXText, out qrX);
                    int.TryParse(qrYText, out qrY);
                    if (qrX > 0 || qrY > 0)
                    {
                        //slot.QRXPulse = qrX;
                        //slot.QRYPulse = qrY;
                        //_dbContext.SaveChanges();

                        result[0] = qrX;
                        result[1] = qrY;
                    }

                }

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                //result.errMessage = ex.Message;
                //result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCTrolleyLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            return result;
        }

    }
}
