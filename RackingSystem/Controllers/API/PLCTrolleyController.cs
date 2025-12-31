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

        [HttpGet("StartDrawerIn/{req}/{qId}")]
        public async Task<ServiceResponseModel<RackJobHubOutDTO>> StartDrawerIn(string req, long qId)
        {
            ServiceResponseModel<RackJobHubOutDTO> result = new ServiceResponseModel<RackJobHubOutDTO>();
            result.data = new RackJobHubOutDTO();

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

                RackJobHubOutDTO json = JsonConvert.DeserializeObject<RackJobHubOutDTO>(srms.Json) ?? new RackJobHubOutDTO();
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

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int startAddress = 4208;
                int numRegisters = 1;
                int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                    lock1 = registers[i].ToString();
                }

                // second addr
                lock2 = "1";
                //startAddress = 4209;
                //numRegisters = 1;
                //registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                //for (int i = 0; i < registers.Length; i++)
                //{
                //    PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                //    decimalText = getDecimalText(registers[i]);
                //    lock2 = decimalText;
                //}

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "");
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
        public async Task<ServiceResponseModel<int>> RetrieveTray(long trolleyId, string side, int colNo, int rowNo)
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

            bool left = side == "A";
            var slot = _dbContext.TrolleySlot.Where(x => x.Trolley_Id == trolleyId && x.IsLeft == left && x.ColNo == colNo && x.RowNo == rowNo).FirstOrDefault();
            if (slot == null)
            {
                result.errMessage = "Cannot find Trolley Slot [" + colNo + " - " + rowNo + "]. ";
                return result;
            }

            result.errStackTrace = slot.TrolleySlotCode;

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

                //PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

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

                result.success = true;
                result.data = 1;

            }
            catch (Exception ex)
            {
                //PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                //PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "");
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

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                while (!exit)
                {
                    int startAddress = 4212;
                    int numRegisters = 1;
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
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
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "");
            }

            if (value == 0)
            {
                int valueErr = -1;
                modbusClient = new ModbusClient(plcIp, port);
                try
                {
                    modbusClient.Connect();

                    PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                    int startAddress = 4231;
                    int numRegisters = 1;
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                        valueErr = registers[i];
                    }

                    if (valueErr == 0)
                    {
                        try
                        {
                            exit = false;
                            //modbusClient.Connect();

                            PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                            while (!exit)
                            {
                                startAddress = 4212;
                                numRegisters = 1;
                                registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                                for (int i = 0; i < registers.Length; i++)
                                {
                                    PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
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
                            PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                            result.errMessage = ex.Message;
                            result.errStackTrace = ex.StackTrace ?? "";
                        }
                        finally
                        {
                            //modbusClient.Disconnect();
                            PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "");
                        }
                    }
                    result.errMessage = valueErr.ToString();
                }
                catch (Exception ex)
                {
                    PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                    result.errMessage = ex.Message;
                    result.errStackTrace = ex.StackTrace ?? "";
                }
                finally
                {
                    modbusClient.Disconnect();
                    PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "");
                }
            }

            result.success = value == 1;
            result.data = value;

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
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
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

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

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
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "");
            }

            return result;
        }

        [HttpGet("GetPutAwayStatus/{slotCode}")]
        public ServiceResponseModel<int> GetPutAwayStatus(string slotCode)
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

            var slot = _dbContext.Slot.Where(x => x.SlotCode == slotCode).FirstOrDefault();
            if (slot == null)
            {
                result.errMessage = "Cannot find Slot [" + slotCode + "]. ";
                result.data = 0;
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

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                while (!exit)
                {
                    int startAddress = 4196; // 4229;
                    int numRegisters = 1;
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
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

                result.success = value == 0;
                result.data = value;

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "");
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

    }
}
