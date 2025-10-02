using AutoMapper;
using EasyModbus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using RackingSystem.Data;
using RackingSystem.Data.Maintenances;
using RackingSystem.General;
using RackingSystem.Helpers;
using RackingSystem.Models;
using RackingSystem.Models.Loader;
using RackingSystem.Models.Reel;
using RackingSystem.Models.Slot;
using RackingSystem.Services.LoaderServices;
using RackingSystem.Services.SlotServices;
using System.IO;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RackingSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class PLCHubInController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly ILoaderService _loaderService;
        private readonly ISlotService _slotService;
        private readonly IMapper _mapper;

        public PLCHubInController(AppDbContext dbContext, ILoaderService loaderService, ISlotService slotService, IMapper mapper)
        {
            _dbContext = dbContext;
            _loaderService = loaderService;
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

        [HttpGet("GetLoaderInfo_PendingToUnLoad/{req}")]
        public async Task<ServiceResponseModel<LoaderDTO>> GetLoaderInfo_PendingToUnLoad(string req)
        {
            ServiceResponseModel<LoaderDTO> result = await _loaderService.GetLoaderInfo(req, true, EnumLoaderStatus.Loaded, null);
            return result;
        }

        [HttpGet("GetFlexilockStatus")]
        public ServiceResponseModel<List<int>> GetFlexilockStatus()
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
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int startAddress = 4196;
                int numRegisters = 1;
                int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                    decimalText = getDecimalText(registers[i]);
                    lock1 = decimalText;
                }

                // second addr
                startAddress = 4197;
                numRegisters = 1;
                registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                    decimalText = getDecimalText(registers[i]);
                    lock2 = decimalText;
                }

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Disconnected.", "");
            }

            result.success = lock1 == "1" && lock2 == "1";
            result.data.Add(Convert.ToInt32(lock1));
            result.data.Add(Convert.ToInt32(lock2));
            return result;
        }

        [HttpGet("SetLoaderUnload/{loaderId}")]
        public async Task<ServiceResponseModel<int>> SetLoaderUnload(long loaderId)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            string methodName = "SetLoaderUnload";

            var _loader = _dbContext.Loader.Find(loaderId);
            if (_loader == null)
            {
                result.errMessage = "Loader is not found.";
                result.data = 0;
                return result;
            }

            // *** testing
            result.success = true;
            result.data = 1;
            return result;
            // *** testing

            // 2. check plc which column is ready
            string plcIp = _loader.IPAddr;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int registerAddress = 4320; 
                int valueToWrite = 2;  

                // Write the single holding register
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                result.success = true;
                result.data = 2;
            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Disconnected.", "");
            }

            return result;
        }

        [HttpGet("GetUnlodingStatus/{loaderId}")]
        public ServiceResponseModel<List<string>> GetUnlodingStatus(long loaderId)
        {
            ServiceResponseModel<List<string>> result = new ServiceResponseModel<List<string>>();
            result.data = new List<string>();
            string methodName = "GetUnlodingStatus";

            var _loader = _dbContext.Loader.Find(loaderId);
            if (_loader == null)
            {
                result.errMessage = "Loader is not found.";
                result.data.Add("0");
                result.data.Add("");
                return result;
            }
            var configGantry = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Gantry1.ToString()).FirstOrDefault();
            if (configGantry == null)
            {
                result.errMessage = "Please set IP Address. ";
                result.data.Add("0");
                result.data.Add("");
                return result;
            }

            // *** testing
            result.success = true;
            result.errMessage = "";
            result.errStackTrace = "";
            result.data.Add("2");
            result.data.Add("H008");
            return result;
            // *** testing

            string decimalText = "";
            string val1 = "";
            string val2 = "";

            int port = 502;

            // 1. check Gantry Position = Scan Position
            ModbusClient modbusClient1 = new ModbusClient(configGantry.ConfigValue, port);
            try
            {
                modbusClient1.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int startAddress = 4216;
                int numRegisters = 1;
                int[] registers = modbusClient1.ReadHoldingRegisters(startAddress, numRegisters);
                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                    decimalText = getDecimalText(registers[i]);
                    val1 = decimalText;
                }

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient1.Disconnect();
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Disconnected.", "");
            }

            // 2. check AudoLoader Position = Scan Position
            ModbusClient modbusClient2 = new ModbusClient(_loader.IPAddr, port);
            try
            {
                modbusClient2.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int startAddress = 4208;
                int numRegisters = 1;
                int[] registers = modbusClient2.ReadHoldingRegisters(startAddress, numRegisters);
                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                    decimalText = getDecimalText(registers[i]);
                    val2 = decimalText;
                }

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient2.Disconnect();
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Disconnected.", "");
            }

            result.success = val1 == "2" && val2 == "H0008";
            result.data.Add(val1);
            result.data.Add(val2);

            return result;
        }

        [HttpGet("StartBarcodeScanner")]
        internal async Task<ServiceResponseModel<int>> StartBarcodeScanner()
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            string methodName = "StartBarcodeScanner";

            var configGantry = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Gantry1.ToString()).FirstOrDefault();
            if (configGantry == null)
            {
                result.errMessage = "Please set IP Address. ";
                result.data = 0;
                return result;
            }

            // *** testing
            result.success = true;
            result.data = 1;
            return result;
            // *** testing

            // 2. check plc which column is ready
            string plcIp = configGantry.ConfigValue;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int registerAddress = 4308;
                int valueToWrite = 1;

                // Write the single holding register
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                result.success = true;
                result.data = 1;
            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Disconnected.", "");
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

        [HttpGet("RetrieveTrayAndReel/{slotCode}/{loaderId}")]
        public async Task<ServiceResponseModel<List<object>>> RetrieveTrayAndReel(string slotCode, long loaderId)
        {
            ServiceResponseModel<List<object>> result = new ServiceResponseModel<List<object>>();
            result.data = new List<object>();
            string methodName = "RetrieveTrayAndReel";

            var configRack = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking1.ToString()).FirstOrDefault();
            if (configRack == null)
            {
                result.errMessage = "Please set IP Address. ";
                result.data.Add(0);
                result.data.Add(new ReelDTO());
                return result;
            }

            var configGantry = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Gantry1.ToString()).FirstOrDefault();
            if (configGantry == null)
            {
                result.errMessage = "Please set IP Address. ";
                result.data.Add(0);
                result.data.Add(new ReelDTO());
                return result;
            }

            var slot = _dbContext.Slot.Where(x => x.SlotCode == slotCode).FirstOrDefault();
            if (slot == null)
            {
                result.errMessage = "Cannot find Slot [" + slotCode + "]. ";
                result.data.Add(0);
                result.data.Add(new ReelDTO());
                return result;
            }

            // *** testing
            // 1. run retrieve empty tray
            Task<ServiceResponseModel<int>> task1 = Task.Run(async () =>
            {
                //await Task.Delay(2000); // Simulate some work
                ServiceResponseModel<int> r1 = await RetrieveEmptyTray(slotCode);
                return r1;
            });

            // 2. run scan reel and write actual height  
            Task<ServiceResponseModel<ReelDTO>> task2 = Task.Run(async () =>
            {
                //await Task.Delay(3000); // Simulate some work
                var r2a = await StartBarcodeScanner();
                if (r2a.success)
                {
                    var r2b = await ScanReelIDByIP();
                    if (r2b.success)
                    {
                        Reel? reel = _dbContext.Reel.FirstOrDefault(x => x.ReelCode == r2b.data);
                        if (reel != null)
                        {
                            Item? item = await _dbContext.Item.FirstOrDefaultAsync(x => x.Item_Id == reel.Item_Id);
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

                                var r2c = await SetActualHeight(loaderId, reel.ActualHeight);

                                ServiceResponseModel<ReelDTO> r2d = new ServiceResponseModel<ReelDTO>();
                                r2d.success = true;
                                r2d.data = data;
                                return r2d;
                            }
                        }
                    }
                }
                ServiceResponseModel<ReelDTO> err2 = new ServiceResponseModel<ReelDTO>();
                err2.success = false;
                err2.errMessage = "";
                err2.data = new ReelDTO();
                return err2;
            });
            // *** testing

            try
            {
                // 3. wait for both is done 
                await Task.WhenAll(task1, task2);
                ServiceResponseModel<int> result1 = await task1;
                ServiceResponseModel<ReelDTO> result2 = await task2;

                if (result1.success)
                {
                    slot.HasEmptyTray = false;
                    await _dbContext.SaveChangesAsync();
                }
                result.data.Add(result1);
                result.data.Add(result2.data);

                result.success = true;
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
        internal async Task<ServiceResponseModel<int>> RetrieveEmptyTray(string slotCode)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            string methodName = "RetrieveEmptyTray";

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

            // *** testing
            result.success = true;
            result.data = 1;
            return result;
            // *** testing

            // 2. check plc which column is ready
            string plcIp = configRack.ConfigValue;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int registerAddress = 4297;
                int valueToWrite = 2;

                // Write the single holding register
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                result.success = true;
                result.data = 1;

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Disconnected.", "");
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

            // *** testing
            result.success = true;
            result.errMessage = "Done";
            result.data = 2;
            return result;
            // *** testings

            string decimalText = "";
            string value = "";

            string plcIp = configRack.ConfigValue;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int startAddress = 4208;
                int numRegisters = 1;
                int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                    decimalText = getDecimalText(registers[i]);
                    value = decimalText;
                }

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Disconnected.", "");
            }

            result.success = value == "1";
            result.data = Convert.ToInt32(value);

            return result;
        }

        [HttpGet("ScanReelIDByIP")]
        internal async Task<ServiceResponseModel<string>> ScanReelIDByIP()
        {
            ServiceResponseModel<string> result = new ServiceResponseModel<string>();
            result.data = "";
            string methodName = "ScanReelID";

            var configGantry = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Gantry1.ToString()).FirstOrDefault();
            if (configGantry == null)
            {
                result.errMessage = "Please set IP Address. ";
                result.data = "";
                return result;
            }

            // *** testing
            result.success = false;
            result.errMessage = "Cannot get Reel ID, please try again.";
            result.data = "";
            return result;
            // *** testing

            DateTime dtRun = DateTime.Now;
            bool exit = false;

            string decimalText = "";
            string reelID = "";
            int value = 0;

            string plcIp = configGantry.ConfigValue;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int startAddress = 4206;
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
                    startAddress = 4207;
                    numRegisters = 1;
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                        decimalText = getDecimalText(registers[i]);
                        reelID = reelID + decimalText;
                    }

                    // third addr
                    startAddress = 4208;
                    numRegisters = 1;
                    registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                        decimalText = getDecimalText(registers[i]);
                        reelID = reelID + decimalText;
                    }

                    // forth addr
                    startAddress = 4209;
                    numRegisters = 1;
                    registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                        decimalText = getDecimalText(registers[i]);
                        reelID = reelID + decimalText;
                    }

                    // fifth addr
                    startAddress = 4210;
                    numRegisters = 1;
                    registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                        decimalText = getDecimalText(registers[i]);
                        reelID = reelID + decimalText;
                    }

                    // sixth addr
                    startAddress = 4211;
                    numRegisters = 1;
                    registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                        decimalText = getDecimalText(registers[i]);
                        reelID = reelID + decimalText;
                    }

                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Get Reel ID {reelID}", "");

                    result.data = reelID;
                }

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Disconnected.", "");
            }

            return result;
        }

        //[HttpGet("ScanReelID")]
        //public async Task<ServiceResponseModel<ReelDTO>> ScanReelID()
        //{
        //    ServiceResponseModel<ReelDTO> result = new ServiceResponseModel<ReelDTO>();
        //    result.data = new ReelDTO();
        //    string methodName = "ScanReelID";

        //    var configGantry = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Gantry1.ToString()).FirstOrDefault();
        //    if (configGantry == null)
        //    {
        //        result.errMessage = "Please set IP Address. ";
        //        return result;
        //    }

        //    // *** testing
        //    result.success = false;
        //    result.errMessage = "Cannot get Reel ID, please try again.";
        //    string a = "";
        //    Reel? reel = _dbContext.Reel.FirstOrDefault(x => x.ReelCode == a);
        //    if (reel != null)
        //    {
        //        Item? item = await _dbContext.Item.FirstOrDefaultAsync(x => x.Item_Id == reel.Item_Id);
        //        if (item != null)
        //        {
        //            ReelDTO data = new ReelDTO();
        //            data.Reel_Id = reel.Reel_Id;
        //            data.ReelCode = reel.ReelCode;
        //            data.Item_Id = reel.Item_Id;
        //            data.ItemCode = item.ItemCode;
        //            data.UOM = item.UOM;
        //            data.Description = item.Description;
        //            data.Qty = reel.Qty;
        //            data.ActualHeight = reel.ActualHeight;
        //            data.ExpiryDate = reel.ExpiryDate;

        //            result.success = true;
        //            result.data = data;
        //        }
        //    }

        //    return result;
        //    // *** testing

        //    DateTime dtRun = DateTime.Now;
        //    bool exit = false;

        //    string decimalText = "";
        //    string reelID = "";
        //    int value = 0;

        //    string plcIp = configGantry.ConfigValue;
        //    int port = 502;

        //    ModbusClient modbusClient = new ModbusClient(plcIp, port);
        //    try
        //    {
        //        modbusClient.Connect();

        //        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

        //        int startAddress = 4206;
        //        int numRegisters = 1;

        //        while (!exit)
        //        {
        //            int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
        //            for (int i = 0; i < registers.Length; i++)
        //            {
        //                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
        //                value = registers[i];
        //                decimalText = getDecimalText(registers[i]);
        //            }

        //            if (value > 0)
        //            {
        //                exit = true;
        //                reelID = reelID + decimalText;
        //            }
        //            if ((DateTime.Now - dtRun).TotalMinutes > 3)
        //            {
        //                result.errMessage = "Timeout. Cannot get Reel ID.";
        //                exit = true;
        //            }
        //        }

        //        if (exit)
        //        {
        //            // second addr
        //            startAddress = 4207;
        //            numRegisters = 1;
        //            int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
        //            for (int i = 0; i < registers.Length; i++)
        //            {
        //                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
        //                decimalText = getDecimalText(registers[i]);
        //                reelID = reelID + decimalText;
        //            }

        //            // third addr
        //            startAddress = 4208;
        //            numRegisters = 1;
        //            registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
        //            for (int i = 0; i < registers.Length; i++)
        //            {
        //                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
        //                decimalText = getDecimalText(registers[i]);
        //                reelID = reelID + decimalText;
        //            }

        //            // forth addr
        //            startAddress = 4209;
        //            numRegisters = 1;
        //            registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
        //            for (int i = 0; i < registers.Length; i++)
        //            {
        //                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
        //                decimalText = getDecimalText(registers[i]);
        //                reelID = reelID + decimalText;
        //            }

        //            // fifth addr
        //            startAddress = 4210;
        //            numRegisters = 1;
        //            registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
        //            for (int i = 0; i < registers.Length; i++)
        //            {
        //                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
        //                decimalText = getDecimalText(registers[i]);
        //                reelID = reelID + decimalText;
        //            }

        //            // sixth addr
        //            startAddress = 4211;
        //            numRegisters = 1;
        //            registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
        //            for (int i = 0; i < registers.Length; i++)
        //            {
        //                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
        //                decimalText = getDecimalText(registers[i]);
        //                reelID = reelID + decimalText;
        //            }

        //            PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Get Reel ID {reelID}", "");

        //            reel = _dbContext.Reel.FirstOrDefault(x => x.ReelCode == reelID);
        //            if (reel != null)
        //            {
        //                Item? item = _dbContext.Item.FirstOrDefault(x => x.Item_Id == reel.Item_Id);
        //                if (item != null)
        //                {
        //                    ReelDTO data = new ReelDTO();
        //                    data.Reel_Id = reel.Reel_Id;
        //                    data.ReelCode = reel.ReelCode;
        //                    data.Item_Id = reel.Item_Id;
        //                    data.ItemCode = item.ItemCode;
        //                    data.UOM = item.UOM;
        //                    data.Description = item.Description;
        //                    data.Qty = reel.Qty;
        //                    data.ActualHeight = reel.ActualHeight;
        //                    data.ExpiryDate = reel.ExpiryDate;

        //                    result.success = true;
        //                    result.data = data;
        //                    return result;
        //                }
        //            }
        //            result.errMessage = "Reel ID [" + reelID + "] not exist.";
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
        //        result.errMessage = ex.Message;
        //        result.errStackTrace = ex.StackTrace ?? "";
        //    }
        //    finally
        //    {
        //        modbusClient.Disconnect();
        //        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Disconnected.", "");
        //    }

        //    return result;
        //}

        [HttpGet("SetActualHeight/{loaderId}/{actHeight}")]
        internal async Task<ServiceResponseModel<int>> SetActualHeight(long loaderId, int actHeight)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            string methodName = "SetActualHeight";

            var _loader = _dbContext.Loader.Where(x => x.Loader_Id == loaderId).FirstOrDefault();
            if (_loader == null)
            {
                result.errMessage = "Loader is not found.";
                result.data = 0;
                return result;
            }

            // *** testing
            result.success = true;
            result.data = 1;
            return result;
            // *** testing

            // 2. check plc which column is ready
            string plcIp = _loader.IPAddr;
            int port = 502;

            ModbusClient modbusClient1 = new ModbusClient(plcIp, port);
            try
            {
                modbusClient1.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int registerAddress = 4318;
                int valueToWrite = actHeight;

                // Write the single holding register
                modbusClient1.WriteSingleRegister(registerAddress, valueToWrite);

                result.success = true;
                result.data = 2;
            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient1.Disconnect();
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Disconnected.", "");
            }

            ModbusClient modbusClient2 = new ModbusClient(plcIp, port);
            try
            {
                modbusClient2.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int registerAddress = 4319;
                int valueToWrite = actHeight;

                // Write the single holding register
                modbusClient2.WriteSingleRegister(registerAddress, valueToWrite);

                result.success = true;
                result.data = 2;
            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient2.Disconnect();
                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Disconnected.", "");
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
                var config = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking1.ToString()).FirstOrDefault();
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
                var config = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking1.ToString()).FirstOrDefault();
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
                var config = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking1.ToString()).FirstOrDefault();
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

        [HttpGet("GetPutAwayStatus")]
        public ServiceResponseModel<int> GetPutAwayStatus()
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            result.data = 0;
            string methodName = "GetPutAwayStatus";

            try
            {
                var config = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking1.ToString()).FirstOrDefault();
                if (config == null)
                {
                    result.errMessage = "Please set IP Address. ";
                    result.data = 0;
                    return result;
                }

                // *** testing
                result.success = true;
                result.errMessage = "Done Put Away.";
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
