using AutoMapper;
using EasyModbus;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RackingSystem.Data;
using RackingSystem.Data.Maintenances;
using RackingSystem.General;
using RackingSystem.Helpers;
using RackingSystem.Models;
using RackingSystem.Models.Reel;
using RackingSystem.Models.Slot;
using RackingSystem.Services.SlotServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RackingSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class PLCHubInController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly ISlotService _slotService;
        private readonly IMapper _mapper;

        public PLCHubInController(AppDbContext dbContext, ISlotService slotService, IMapper mapper)
        {
            _dbContext = dbContext;
            _slotService = slotService;
            _mapper = mapper;
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

        [HttpGet("GetGripperStatus")]
        public ServiceResponseModel<int> GetGripperStatus()
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            result.data = 0;
            string methodName = "GetGripperStatus";

            try
            {
                var config = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking.ToString()).FirstOrDefault();
                if (config == null)
                {
                    result.errMessage = "Please set IP Address. ";
                    result.data = 0;
                    return result;
                }

                // *** testing
                result.success = true;
                result.errMessage = "Gripper is Ready.";
                result.data = 0;
                return result;
                // *** testing

                string decimalText = "";
                string value = "";

                string plcIp = config.ConfigValue;
                int port = 502;

                ModbusClient modbusClient = new ModbusClient(plcIp, port);
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int startAddress = 4199;
                int numRegisters = 1;
                int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                    decimalText = getDecimalText(registers[i]);
                    value = decimalText;
                }

                result.success = value == "1";
                result.data = Convert.ToInt32(value);
                if (value == "0")
                {
                    result.errMessage = "Gripper is Not Ready. ";
                }
                else if (value == "1")
                {
                    result.errMessage = "Gripper is Ready.";
                }
                else if (value == "2")
                {
                    result.errMessage = "Gripper is in Auto Run. ";
                }
                else
                {
                    result.errMessage = "Gripper is Error. ";
                }

                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Disconnected.", "");

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        [HttpGet("GetFlexilockStatus")]
        public ServiceResponseModel<List<int>> GetFlexilockStatus()
        {
            ServiceResponseModel<List<int>> result = new ServiceResponseModel<List<int>>();
            result.data = new List<int>();
            string methodName = "GetFlexilockStatus";

            try
            {
                var config = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking.ToString()).FirstOrDefault();
                if (config == null)
                {
                    result.errMessage = "Please set IP Address. ";
                    result.data.Add(0);
                    result.data.Add(0);
                    return result;
                }

                // *** testing
                result.success = true;
                result.errMessage = "Right is locked. Left is locked. ";
                result.data.Add(2);
                result.data.Add(2);
                return result;
                // *** testing

                string decimalText = "";
                string lock1 = "";
                string lock2 = "";

                string plcIp = config.ConfigValue;
                int port = 502;

                ModbusClient modbusClient = new ModbusClient(plcIp, port);
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int startAddress = 4197;
                int numRegisters = 1;
                int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                    decimalText = getDecimalText(registers[i]);
                    lock1 = decimalText;
                }

                // second addr
                startAddress = 4198;
                numRegisters = 1;
                registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                    decimalText = getDecimalText(registers[i]);
                    lock2 = decimalText;
                }

                result.success = lock1 == "2" && lock2 == "2";
                result.data.Add(Convert.ToInt32(lock1));
                result.data.Add(Convert.ToInt32(lock2));
                if (lock1 == "0")
                {
                    result.errMessage = "Right is Unlock. ";
                }
                else
                {
                    result.errMessage = "Right is locked. ";
                }
                if (lock2 == "0")
                {
                    result.errMessage = result.errMessage + "Left is Unlock.";
                }
                else
                {
                    result.errMessage = result.errMessage + "Left is locked.";
                }

                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Disconnected.", "");

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        [HttpGet("GetEmptyTraySlot")]
        public async Task<ServiceResponseModel<Slot_DrawerFreeDTO>> GetEmptyTraySlot()
        {
            ServiceResponseModel<Slot_DrawerFreeDTO> result = new ServiceResponseModel<Slot_DrawerFreeDTO>();
            result.data = new Slot_DrawerFreeDTO();
            string methodName = "GetEmptyTraySlot";

            try
            {
                ServiceResponseModel<List<Slot_DrawerFreeDTO>> r = await _slotService.GetFreeSlot_Drawer_ByColumn();
                if (r.data.Count == 0)
                {
                    result.success = false;
                    result.errMessage = "No Empty Tray is ready.";
                    return result;
                }

                result.success = true;
                result.data = r.data[0];
                return result;

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        [HttpGet("RetrieveEmptyTray/{slotCode}")]
        public async Task<ServiceResponseModel<int>> RetrieveEmptyTray(string slotCode)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            string methodName = "RetrieveEmptyTray";

            try
            {
                var config = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking.ToString()).FirstOrDefault();
                if (config == null)
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

                // *** testing
                slot.HasEmptyTray = false;
                await _dbContext.SaveChangesAsync();

                result.success = true;
                result.data = 1;
                return result;
                // *** testing

                // 2. check plc which column is ready
                int value = 0;
                string plcIp = config.ConfigValue;
                int port = 502;

                ModbusClient modbusClient = new ModbusClient(plcIp, port);
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int startAddress = 4317;
                int numRegisters = 1;

                int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);

                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                    value = registers[i];
                }

                result.success = true;
                result.data = registers[0];

                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Disconnected.", "");

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        [HttpGet("GetReelID")]
        public ServiceResponseModel<ReelDTO> GetReelID()
        {
            ServiceResponseModel<ReelDTO> result = new ServiceResponseModel<ReelDTO>();
            result.data = new ReelDTO();
            string methodName = "GetReelID";

            try
            {
                var config = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking.ToString()).FirstOrDefault();
                if (config == null)
                {
                    result.errMessage = "Please set IP Address. ";
                    return result;
                }

                // *** testing
                result.success = false;
                result.errMessage = "Cannot get Reel ID, please try again.";
                string a = "";
                Reel? reel = _dbContext.Reel.FirstOrDefault(x => x.ReelCode == a);
                if (reel != null)
                {
                    Item? item = _dbContext.Item.FirstOrDefault(x => x.Item_Id == reel.Item_Id);
                    if (item != null)
                    {
                        ReelDTO data = new ReelDTO();
                        data.Reel_Id = reel.Reel_Id;
                        data.ReelCode = reel.ReelCode;
                        data.Item_Id = reel.Item_Id;
                        data.ItemCode = item.ItemCode;
                        data.UOM = item.UOM;
                        data.Description = item.Description;
                        data.Qty = reel.Qty;
                        data.ActualHeight = reel.ActualHeight;
                        data.ExpiryDate = reel.ExpiryDate;

                        result.success = true;
                        result.data = data;
                    }
                }

                return result;
                // *** testing

                DateTime dtRun = DateTime.Now;
                bool exit = false;

                string decimalText = "";
                string reelID = "";
                int value = 0;

                string plcIp = config.ConfigValue;
                int port = 502;

                ModbusClient modbusClient = new ModbusClient(plcIp, port);
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int startAddress = 4207;
                int numRegisters = 1;

                while (!exit)
                {
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                        value = registers[i];
                        decimalText = getDecimalText(registers[i]);
                    }

                    if (value > 0)
                    {
                        exit = true;
                        reelID = reelID + decimalText;
                    }
                    if ((DateTime.Now - dtRun).TotalMinutes > 3)
                    {
                        result.errMessage = "Timeout. Cannot get Reel ID.";
                        exit = true;
                    }
                }

                if (exit)
                {
                    // second addr
                    startAddress = 4208;
                    numRegisters = 1;
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                        decimalText = getDecimalText(registers[i]);
                        reelID = reelID + decimalText;
                    }

                    // third addr
                    startAddress = 4209;
                    numRegisters = 1;
                    registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                        decimalText = getDecimalText(registers[i]);
                        reelID = reelID + decimalText;
                    }

                    // forth addr
                    startAddress = 4210;
                    numRegisters = 1;
                    registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                        decimalText = getDecimalText(registers[i]);
                        reelID = reelID + decimalText;
                    }

                    // fifth addr
                    startAddress = 4211;
                    numRegisters = 1;
                    registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                        decimalText = getDecimalText(registers[i]);
                        reelID = reelID + decimalText;
                    }

                    // sixth addr
                    startAddress = 4212;
                    numRegisters = 1;
                    registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                        decimalText = getDecimalText(registers[i]);
                        reelID = reelID + decimalText;
                    }

                    reel = _dbContext.Reel.FirstOrDefault(x => x.ReelCode == reelID);
                    if (reel != null)
                    {
                        Item? item = _dbContext.Item.FirstOrDefault(x => x.Item_Id == reel.Item_Id);
                        if (item != null)
                        {
                            ReelDTO data = new ReelDTO();
                            data.Reel_Id = reel.Reel_Id;
                            data.ReelCode = reel.ReelCode;
                            data.Item_Id = reel.Item_Id;
                            data.ItemCode = item.ItemCode;
                            data.UOM = item.UOM;
                            data.Description = item.Description;
                            data.Qty = reel.Qty;
                            data.ActualHeight = reel.ActualHeight;
                            data.ExpiryDate = reel.ExpiryDate;

                            result.success = true;
                            result.data = data;
                            return result;
                        }
                    }
                    result.errMessage = "Reel ID [" + reelID + "] not exist.";
                }

                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Disconnected.", "");

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        [HttpGet("GetBottomSlot/{actHeight}")]
        public async Task<ServiceResponseModel<SlotDTO>> GetBottomSlot(int actHeight)
        {
            ServiceResponseModel<SlotDTO> result = new ServiceResponseModel<SlotDTO>();
            result.data = new SlotDTO();
            string methodName = "GetBottomSlot";

            try
            {
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

                var colList = _dbContext.SlotColumnSetting.OrderBy(x => x.Reel_IN_Idx).ToList();
                foreach (var col in colList)
                {
                    SlotFreeReqDTO req = new SlotFreeReqDTO();
                    req.ColNo = col.ColNo;
                    req.TotalSlot = slotUsage.ReserveSlot;

                    ServiceResponseModel<SlotFreeDTO> r = await _slotService.GetFreeSlot_ByColumn_ASC(req);
                    if (r.data.Row1 > 0)
                    {
                        bottomSlotCol = col.ColNo;
                        bottomSlotRow = r.data.Row1 + slotUsage.ReserveSlot - 1;
                        break;
                    }
                }
                
                var slot = _dbContext.Slot.Where(x => x.RowNo == bottomSlotRow && x.ColNo == bottomSlotCol).FirstOrDefault();
                if (slot == null)
                {
                    result.success = false;
                    result.errMessage = "Cannot find slot for Column [" + bottomSlotCol + "] Row [" + bottomSlotRow + "].";
                    return result;
                }
                var slotDTO = _mapper.Map<SlotDTO>(slot);
                slotDTO.TotalSlot = slotUsage.ReserveSlot;

                result.success = true;
                result.data = slotDTO;
                return result;

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        [HttpGet("PutAway/{slotCode}/{slotReserve}")]
        public ServiceResponseModel<int> PutAway(string slotCode, int slotReserve)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            string methodName = "RetrieveEmptyTray";

            try
            {
                var config = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking.ToString()).FirstOrDefault();
                if (config == null)
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

                // *** testing
                result.success = true;
                result.data = 1;
                return result;
                // *** testing

                // 2. check plc which column is ready
                int value = 0;
                string plcIp = config.ConfigValue;
                int port = 502;

                ModbusClient modbusClient = new ModbusClient(plcIp, port);
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int startAddress = 4317;
                int numRegisters = 1;

                int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);

                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                    value = registers[i];
                }

                result.success = true;
                result.data = registers[0];

                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Disconnected.", "");

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        [HttpGet("GetPickStatus")]
        public ServiceResponseModel<int> GetPickStatus()
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            result.data = 0;
            string methodName = "GetPickStatus";

            try
            {
                var config = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking.ToString()).FirstOrDefault();
                if (config == null)
                {
                    result.errMessage = "Please set IP Address. ";
                    result.data = 0;
                    return result;
                }

                // *** testing
                result.success = true;
                result.errMessage = "Done Picked.";
                result.data = 0;
                return result;
                // *** testing

                string decimalText = "";
                string value = "";

                string plcIp = config.ConfigValue;
                int port = 502;

                ModbusClient modbusClient = new ModbusClient(plcIp, port);
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int startAddress = 4228;
                int numRegisters = 1;
                int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                    decimalText = getDecimalText(registers[i]);
                    value = decimalText;
                }

                result.success = value == "1";
                result.data = Convert.ToInt32(value);
                if (value == "1")
                {
                    result.errMessage = "Done Picked.";
                }
                else
                {
                    result.errMessage = "Error";
                }

                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Disconnected.", "");

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        [HttpGet("GetPlaceStatus")]
        public ServiceResponseModel<int> GetPlaceStatus()
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            result.data = 0;
            string methodName = "GetPlaceStatus";

            try
            {
                var config = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking.ToString()).FirstOrDefault();
                if (config == null)
                {
                    result.errMessage = "Please set IP Address. ";
                    result.data = 0;
                    return result;
                }

                // *** testing
                result.success = true;
                result.errMessage = "Done Placed.";
                result.data = 0;
                return result;
                // *** testing

                string decimalText = "";
                string value = "";

                string plcIp = config.ConfigValue;
                int port = 502;

                ModbusClient modbusClient = new ModbusClient(plcIp, port);
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int startAddress = 4229;
                int numRegisters = 1;
                int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                    decimalText = getDecimalText(registers[i]);
                    value = decimalText;
                }

                result.success = value == "1";
                result.data = Convert.ToInt32(value);
                if (value == "1")
                {
                    result.errMessage = "Done Placed.";
                }
                else
                {
                    result.errMessage = "Error";
                }

                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Disconnected.", "");

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        [HttpGet("UpdateReelIntoRack/{loaderId}/{colNo}/{reelId}/{slotCode}/{slotReserve}")]
        public async Task<ServiceResponseModel<int>> UpdateReelIntoRack(long loaderId, int colNo, string reelId, string slotCode, int slotReserve)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            string methodName = "UpdateReelIntoLoader";

            try
            {
                var _loader = _dbContext.Loader.Find(loaderId);
                if (_loader == null)
                {
                    result.errMessage = "Loader is not found.";
                    result.data = 0;
                    return result;
                }
                var _reel = _dbContext.Reel.Where(x => x.Reel_Id.ToString() == reelId).FirstOrDefault();
                if (_reel == null)
                {
                    result.errMessage = "Reel is not found.";
                    result.data = 0;
                    return result;
                }
                var _loaderCol = _dbContext.LoaderColumn.Where(x => x.Loader_Id == loaderId && x.ColNo == colNo).FirstOrDefault();
                if (_loaderCol == null)
                {
                    result.errMessage = "Loader Column is not found.";
                    result.data = 0;
                    return result;
                }
                var _loaderReel = _dbContext.LoaderReel.Where(x => x.Reel_Id.ToString() == reelId).FirstOrDefault();
                if (_loaderReel == null)
                {
                    result.errMessage = "Loader Reel is not found.";
                    result.data = 0;
                    return result;
                }
                var _slot = _dbContext.Slot.Where(x => x.SlotCode == slotCode).FirstOrDefault();
                if (_slot == null)
                {
                    result.errMessage = "Slot is not found.";
                    result.data = 0;
                    return result;
                }

                // 1.1 update reel status
                _reel.StatusIdx = (int)EnumReelStatus.IsReady;
                _reel.Status = EnumReelStatus.IsReady.ToString();
                await _dbContext.SaveChangesAsync();

                // 2. update loader columne balance height
                _loaderCol.BalanceHeight = _loaderCol.BalanceHeight + _reel.ActualHeight;
                await _dbContext.SaveChangesAsync();

                // 3. remove reel from loaderreel
                _dbContext.LoaderReel.Remove(_loaderReel);
                await _dbContext.SaveChangesAsync();

                // 4. update slot set reelId
                _slot.Reel_Id = _reel.Reel_Id;
                _slot.ReelNo = "0";
                _slot.HasReel = true;
                _slot.HasEmptyTray = false;
                await _dbContext.SaveChangesAsync();

                // 5. update other slot if is reserved
                if (slotReserve > 1)
                {
                    for (int iR = 1; iR < slotReserve; iR++)
                    {
                        var _slotO = _dbContext.Slot.Where(x => x.ColNo == _slot.ColNo && x.RowNo == (_slot.RowNo - iR)).FirstOrDefault();
                        if (_slotO == null)
                        {
                            result.errMessage = "Other Slot is not found.";
                            result.data = 0;
                            return result;
                        }
                        _slotO.ReelNo = iR.ToString();
                        _slotO.HasReel = true;
                        _slotO.HasEmptyTray = false;
                        await _dbContext.SaveChangesAsync();
                    }
                }


                // 1.2 update slot id
                _reel.Slot_Id = _slot.Slot_Id;
                await _dbContext.SaveChangesAsync();

                // 6. Checking loader all column status and update
                int r = 0;
                var colBal = _dbContext.LoaderReel.Where(x => x.Loader_Id == loaderId).FirstOrDefault();
                if (colBal == null)
                {
                    // means all is out
                    r = 2;
                }
                else
                {
                    var colCurBal = _dbContext.LoaderReel.Where(x => x.Loader_Id == loaderId && x.ColNo == colNo).FirstOrDefault();
                    if (colBal == null)
                    {
                        // means cur col all is out
                        r = 1;
                    }
                }

                if (r == 2)
                {
                    _loader.Status = EnumLoaderStatus.ReadyToLoad.ToString();
                    await _dbContext.SaveChangesAsync();
                }

                result.success = true;
                result.data = r;
            }
            catch (Exception ex)
            {
                //PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

    }
}
