using AutoMapper;
using EasyModbus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RackingSystem.Data;
using RackingSystem.Data.Maintenances;
using RackingSystem.Data.RackJob;
using RackingSystem.General;
using RackingSystem.Helpers;
using RackingSystem.Migrations;
using RackingSystem.Models;
using RackingSystem.Models.API;
using RackingSystem.Models.Loader;
using RackingSystem.Models.Log;
using RackingSystem.Models.RackJob;
using RackingSystem.Models.Reel;
using RackingSystem.Models.Slot;
using RackingSystem.Models.User;
using RackingSystem.Services.LoaderServices;
using RackingSystem.Services.SlotServices;
using System.IO;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RackingSystem.Controllers.API
{
    [Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme}")]
    [ApiController]
    [Route("api/[controller]")]
    public class PLCHubInController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly ILoaderService _loaderService;
        private readonly ISlotService _slotService;
        private readonly IMapper _mapper;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public PLCHubInController(AppDbContext dbContext, ILoaderService loaderService, ISlotService slotService, IMapper mapper, IDbContextFactory<AppDbContext> contextFactory)
        {
            _dbContext = dbContext;
            _loaderService = loaderService;
            _slotService = slotService;
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
        public async Task<ServiceResponseModel<bool>> UpdateRackJobJson([FromBody] RackJobHubInJsonDTO req)
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
        public ServiceResponseModel<RackJobHubInJsonDTO> GetRackJobJson()
        {
            ServiceResponseModel<RackJobHubInJsonDTO> result = new ServiceResponseModel<RackJobHubInJsonDTO>();
            result.data = new RackJobHubInJsonDTO();
            try
            {
                var _job = _dbContext.RackJob.FirstOrDefault();
                if (_job != null)
                {
                    result.data = JsonConvert.DeserializeObject<RackJobHubInJsonDTO>(_job.Json) ?? new RackJobHubInJsonDTO();
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

        [HttpGet("GetLoaderInfo_PendingToUnLoad/{req}")]
        public async Task<ServiceResponseModel<LoaderDTO>> GetLoaderInfo_PendingToUnLoad(string req)
        {
            ServiceResponseModel<LoaderDTO> result = await _loaderService.GetLoaderInfo(req, true, EnumLoaderStatus.Loaded, null);
            if (result.data == null)
            {
                result.success = false;
                result.errMessage = "This loader is not for unloading.";
                return result;
            }
            if (result.data.Col1TotalReels > 0 || result.data.Col2TotalReels > 0 || result.data.Col3TotalReels > 0 || result.data.Col4TotalReels > 0)
            {
                return result;
            }
            else
            {
                result.success = false;
                result.errMessage = "No Reels to unload.";
                return result;
            }
        }

        [HttpGet("StartUnload/{req}/{qId}")]
        public async Task<ServiceResponseModel<RackJobHubInDTO>> StartUnload(string req, long qId)
        {
            ServiceResponseModel<RackJobHubInDTO> result = new ServiceResponseModel<RackJobHubInDTO>();
            result.data = new RackJobHubInDTO();

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
                ServiceResponseModel<LoaderDTO> r = await _loaderService.GetLoaderInfo(req, true, EnumLoaderStatus.Loaded, null);
                if (r.data == null)
                {
                    result.success = false;
                    result.errMessage = "This loader is not for unloading.";
                    return result;
                }
                if (r.data.Col1TotalReels > 0 || r.data.Col2TotalReels > 0 || r.data.Col3TotalReels > 0 || r.data.Col4TotalReels > 0)
                {
                    if (rackJob.Doc_Id != r.data.Loader_Id)
                    {
                        result.success = false;
                        result.errMessage = "This Loader is not for this Queue.";
                        return result;
                    }

                    var srms = _dbContext.RackJob.First();
                    srms.StartDate = DateTime.Now;
                    srms.CurrentJobType = EnumQueueDocType.Loader.ToString();
                    srms.RackJobQueue_Id = qId;
                    srms.Loader_Id = r.data.Loader_Id;
                    srms.LoginIP = devId;
                    srms.TotalCount = r.data.Col1TotalReels + r.data.Col2TotalReels + r.data.Col3TotalReels + r.data.Col4TotalReels;
                    _dbContext.SaveChanges();

                    RackJobHubInDTO json = JsonConvert.DeserializeObject<RackJobHubInDTO>(srms.Json) ?? new RackJobHubInDTO();
                    result.data = json;
                    result.data.LoaderInfo = r.data;
                    result.success = true;
                    return result;
                }
                else
                {
                    result.success = false;
                    result.errMessage = "No Reels to unload.";
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
            }
            return result;
        }

        [HttpGet("StopUnload/{qId}")]
        public async Task<ServiceResponseModel<bool>> StopUnload(long qId)
        {
            ServiceResponseModel<bool> result = new ServiceResponseModel<bool>();
            result.data = false;

            try
            {
                // 1. if loader is fully empty then remove q
                var q = _dbContext.RackJobQueue.Where(x => x.RackJobQueue_Id == qId).FirstOrDefault();
                if (q != null)
                {
                    var colBal = _dbContext.LoaderReel.Where(x => x.Loader_Id == q.Doc_Id).FirstOrDefault();
                    if (colBal == null)
                    {
                        _dbContext.RackJobQueue.Remove(q);
                        _dbContext.SaveChanges();

                        var sqlSP = await _dbContext.Database.ExecuteSqlInterpolatedAsync($"EXEC {GeneralStatic.SP_Q_Requeue} ");
                    }
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

            //// *** testing
            //result.success = true;
            //result.errMessage = "Right is locked. Left is locked. ";
            //result.data.Add(1);
            //result.data.Add(1);
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

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                int startAddress = 4208;
                int numRegisters = 1;
                int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
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
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            if (string.IsNullOrEmpty(lock1)) { lock1 = "0"; }
            if (string.IsNullOrEmpty(lock2)) { lock2 = "0"; }
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

            //// *** testing
            //result.success = true;
            //result.data = 1;
            //return result;
            //// *** testing

            // 2. check plc which column is ready
            string plcIp = _loader.IPAddr;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                int registerAddress = 4320; 
                int valueToWrite = 2;  

                // Write the single holding register
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                result.success = true;
                result.data = 2;
            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            return result;
        }

        [HttpGet("GetUnlodingStatus/{loaderId}")]
        public async Task<ServiceResponseModel<List<string>>> GetUnlodingStatusAsync(long loaderId)
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

            //// *** testing
            //result.success = true;
            //result.errMessage = "";
            //result.errStackTrace = "";
            //result.data.Add("2");
            //result.data.Add("16");
            //return result;
            //// *** testing

            DateTime dtRun = DateTime.Now;
            bool exit = false;

            string decimalText = "";
            string val1 = "";
            string val2 = "";

            int port = 502;

            // 1. check Gantry Position = Scan Position
            ModbusClient modbusClient1 = new ModbusClient(configGantry.ConfigValue, port);
            try
            {
                modbusClient1.Connect();

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                while (!exit)
                {

                    int startAddress = 4216;
                    int numRegisters = 1;
                    int[] registers = modbusClient1.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        val1 = registers[i].ToString();
                    }

                    if (val1 == "2")
                    {
                        exit = true;
                    }
                    if ((DateTime.Now - dtRun).TotalSeconds > 30)
                    {
                        result.errMessage = "Timeout. Cannot get Unload Status.";
                        exit = true;
                    }
                }

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient1.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            exit = false;

            // 2. check AudoLoader Position = Scan Position
            ModbusClient modbusClient2 = new ModbusClient(_loader.IPAddr, port);
            try
            {
                modbusClient2.Connect();

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                while (!exit)
                {
                    int startAddress = 4225;
                    int numRegisters = 1;
                    int[] registers = modbusClient2.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        val2 = registers[i].ToString();
                    }


                    if (val2 == "16")
                    {
                        exit = true;
                    }
                    if ((DateTime.Now - dtRun).TotalSeconds > 30)    
                    {
                        result.errMessage = "Timeout. Cannot get Reel ID.";
                        exit = true;
                    }
                }

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient2.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            result.success = val1 == "2" && val2 == "16";
            result.data.Add(val1);
            result.data.Add(val2);

            
            return result;
        }

        [HttpGet("GetEmptyTraySlot/{slotCode}")]
        public async Task<ServiceResponseModel<Slot_DrawerFreeDTO>> GetEmptyTraySlot(string slotCode)
        {
            ServiceResponseModel<Slot_DrawerFreeDTO> result = new ServiceResponseModel<Slot_DrawerFreeDTO>();
            result.data = new Slot_DrawerFreeDTO();
            string methodName = "GetEmptyTraySlot";

            try
            {
                var claims = User.Identities.First().Claims.ToList();
                string devId = claims?.FirstOrDefault(x => x.Type.Equals("DeviceId", StringComparison.OrdinalIgnoreCase))?.Value ?? "";

                if (slotCode != "-")
                {
                    var slot = _dbContext.Slot.Where(x => x.SlotCode == slotCode).FirstOrDefault();
                    if (slot != null)
                    {
                        var rpt = new RackJobReport();
                        rpt.LoginIP = devId;
                        rpt.InfoType = "ERR";
                        rpt.InfoEvent = methodName;
                        rpt.InfoMessage1 = "Cannot get Empty Drawer on [" + slot.SlotCode + "].";
                        _dbContext.RackJobReport.Add(rpt);


                        slot.NeedCheck = true;
                        slot.CheckRemark = "Cannot get Empty Drawer";
                        _dbContext.SaveChanges();

                        PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Slot Error: Cannot get Empty Drawer on [" + slot.SlotCode + "].", "", true);

                    }
                }

                ServiceResponseModel<List<Slot_DrawerFreeDTO>> r = await _slotService.GetFreeSlot_Drawer_ByColumn();
                if (r.success)
                {
                    if (r.data.Count == 0)
                    {
                        result.success = false;
                        result.errMessage = "No Empty Tray is ready.";
                        return result;
                    }
                }
                else
                {
                    result.success = false;
                    result.errMessage = "No Empty Tray is ready.";
                    return result;
                }

                result.success = true;
                result.data = r.data[0];
                //var slot = _dbContext.Slot.Where(x => x.SlotCode == result.data.SlotCode).FirstOrDefault();
                //if (slot != null)
                //{
                //    result.data.IsLeft = slot.IsLeft;
                //}
                return result;

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        [HttpGet("ScanReelGetReelID/")]
        public async Task<ServiceResponseModel<TrayReelDTO>> ScanReelGetReelID()
        {
            ServiceResponseModel<TrayReelDTO> result = new ServiceResponseModel<TrayReelDTO>();
            result.data = new TrayReelDTO();
            string methodName = "ScanReelGetReelID";

            var configRack = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking1.ToString()).FirstOrDefault();
            if (configRack == null)
            {
                result.errMessage = "Please set IP Address. ";
                return result;
            }

            var configGantry = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Gantry1.ToString()).FirstOrDefault();
            if (configGantry == null)
            {
                result.errMessage = "Please set IP Address. ";
                return result;
            }

            ServiceResponseModel<TrayReelDTO> r2 = new ServiceResponseModel<TrayReelDTO>();
            r2.data = new TrayReelDTO();

            var r2a = await StartBarcodeScannerByIP(configGantry.ConfigValue);
            if (r2a.success)
            {
                r2.data.successStartScan = true;

                var r2b = await GetReelIDByIP(configGantry.ConfigValue);
                if (r2b.success)
                {
                    r2.data.successScan = true;
                    r2.data.ScannedBarcode = r2b.data;

                    Reel? reel = _dbContext.Reel.FirstOrDefault(x => x.ReelCode == r2b.data);
                    if (reel != null)
                    {
                        Item? item = await _dbContext.Item.FirstOrDefaultAsync(x => x.Item_Id == reel.Item_Id);
                        if (item != null)
                        {
                            r2.data.successReel = true;
                            r2.data.Reel_Id = reel.Reel_Id;
                            r2.data.ReelCode = reel.ReelCode;
                            r2.data.Item_Id = reel.Item_Id;
                            r2.data.ItemCode = item.ItemCode;
                            r2.data.UOM = item.UOM;
                            r2.data.Description = item.Description;
                            r2.data.Qty = reel.Qty;
                            r2.data.ActualHeight = reel.ActualHeight;
                            r2.data.Status = reel.Status;

                            r2.success = true;
                        }
                        else
                        {
                            r2.data.errMessageReel = "Cannot get Item info. Reel ID [" + r2b.data + "].";
                        }
                    }
                    else
                    {
                        r2.data.errMessageReel = "Cannot get Reel info. Reel ID [" + r2b.data + "].";
                    }
                }
            }

            result = r2;

            return r2;
        }

        //[HttpGet("RetrieveTrayAndReel/{slotCode}/{loaderId}/{actHeight}")]
        //public async Task<ServiceResponseModel<TrayReelDTO>> RetrieveTrayAndReel(string slotCode, long loaderId, int actHeight)
        //{
        //    ServiceResponseModel<TrayReelDTO> result = new ServiceResponseModel<TrayReelDTO>();
        //    result.data = new TrayReelDTO();
        //    string methodName = "RetrieveTrayAndReel";

        //    var configRack = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking1.ToString()).FirstOrDefault();
        //    if (configRack == null)
        //    {
        //        result.errMessage = "Please set IP Address. ";
        //        return result;
        //    }

        //    var configGantry = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Gantry1.ToString()).FirstOrDefault();
        //    if (configGantry == null)
        //    {
        //        result.errMessage = "Please set IP Address. ";
        //        return result;
        //    }

        //    var slot = _dbContext.Slot.Where(x => x.SlotCode == slotCode).FirstOrDefault();
        //    if (slot == null)
        //    {
        //        result.errMessage = "Cannot find Slot [" + slotCode + "]. ";
        //        return result;
        //    }

        //    var _loader = _dbContext.Loader.Where(x => x.Loader_Id == loaderId).FirstOrDefault();
        //    if (_loader == null)
        //    {
        //        result.errMessage = "Loader is not found.";
        //        return result;
        //    }

        //    // SET Retrieve first
        //    ServiceResponseModel<int> r1 = await RetrieveEmptyTray(configRack.ConfigValue, slotCode);

        //    await Task.Delay(2000);

        //    // SET Actual Height
        //    ServiceResponseModel<TrayReelDTO> r2 = new ServiceResponseModel<TrayReelDTO>();
        //    r2.data = new TrayReelDTO();
        //    r2.data.successStartScan = true;
        //    r2.data.successScan = true;
        //    var r2c = await SetActualHeightByIP(_loader.IPAddr, actHeight);
        //    if (r2c.success)
        //    {
        //        r2.data.successSetH = true;
        //        r2.success = true;
        //    }
        //    else
        //    {
        //        r2.data.errMessageSetH = r2c.errMessage;
        //    }

        //    try
        //    {
        //        result.data = r2.data;
        //        result.data.successTray = r1.success;
        //        result.data.errMessageTray = r1.errMessage;

        //        result.success = r1.success && r2.success;
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
        //        result.errMessage = ex.Message;
        //        result.errStackTrace = ex.StackTrace ?? "";
        //    }

        //    return result;
        //}

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

            // 2. check plc which column is ready
            string plcIp = configRack.ConfigValue;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

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

                slot.HasEmptyTray = false;
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }


            return result;
        }

        //internal async Task<ServiceResponseModel<int>> RetrieveEmptyTray(string ipAddr, string slotCode)
        //{
        //    ServiceResponseModel<int> result = new ServiceResponseModel<int>();
        //    string methodName = "RetrieveEmptyTray";

        //    //var configRack = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking1.ToString()).FirstOrDefault();
        //    //if (configRack == null)
        //    //{
        //    //    result.errMessage = "Please set IP Address. ";
        //    //    result.data = 0;
        //    //    return result;
        //    //}

        //    var slot = _dbContext.Slot.Where(x => x.SlotCode == slotCode).FirstOrDefault();
        //    if (slot == null)
        //    {
        //        result.errMessage = "Cannot find Slot [" + slotCode + "]. ";
        //        result.data = 0;
        //        return result;
        //    }

        //    //// *** testing
        //    //result.success = true;
        //    //result.data = 1;
        //    //return result;
        //    //// *** testing

        //    // 2. check plc which column is ready
        //    string plcIp = ipAddr;
        //    int port = 502;

        //    ModbusClient modbusClient = new ModbusClient(plcIp, port);
        //    try
        //    {
        //        modbusClient.Connect();

        //        //PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

        //        // step 1 : left or right
        //        int registerAddress = 4298;
        //        //int valueToWrite = 0;
        //        int valueToWrite = slot.IsLeft ? 0 : 1;
        //        modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

        //        // step 2 : x-pulses
        //        //byte[] bytes = BitConverter.GetBytes(57832);
        //        byte[] bytes = BitConverter.GetBytes(slot.XPulse);
        //        int highBinary = BitConverter.ToUInt16(bytes, 0);
        //        int lowBinary = BitConverter.ToUInt16(bytes, 2);
        //        registerAddress = 4300;
        //        valueToWrite = 0;
        //        modbusClient.WriteSingleRegister(registerAddress, lowBinary);
        //        registerAddress = 4299;
        //        valueToWrite = 0;
        //        modbusClient.WriteSingleRegister(registerAddress, highBinary);

        //        // step 3 : y-pulses
        //        //bytes = BitConverter.GetBytes(4930);
        //        bytes = BitConverter.GetBytes(slot.YPulse);
        //        highBinary = BitConverter.ToUInt16(bytes, 0);
        //        lowBinary = BitConverter.ToUInt16(bytes, 2);
        //        registerAddress = 4301;
        //        valueToWrite = 0;
        //        modbusClient.WriteSingleRegister(registerAddress, highBinary);
        //        registerAddress = 4302;
        //        valueToWrite = 0;
        //        modbusClient.WriteSingleRegister(registerAddress, lowBinary);

        //        // step 4 : 
        //        registerAddress = 4310;
        //        //valueToWrite = 634;
        //        valueToWrite = slot.QRXPulse;
        //        modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

        //        // step 5 : 
        //        registerAddress = 4311;
        //        //valueToWrite = 377;
        //        valueToWrite = slot.QRYPulse;
        //        modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

        //        // step 6 : 
        //        registerAddress = 4312;
        //        valueToWrite = 7;
        //        modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

        //        // step Last : 
        //        registerAddress = 4297;
        //        valueToWrite = 2;
        //        modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

        //        result.success = true;
        //        result.data = 1;

        //        slot.HasEmptyTray = false;
        //        await _dbContext.SaveChangesAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        //PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
        //        result.errMessage = ex.Message;
        //        result.errStackTrace = ex.StackTrace ?? "";
        //    }
        //    finally
        //    {
        //        modbusClient.Disconnect();
        //        //PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "");
        //    }

        //    return result;
        //}

        [HttpGet("StartBarcodeScanner")]
        public async Task<ServiceResponseModel<int>> StartBarcodeScanner()
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

            result = await StartBarcodeScannerByIP(configGantry.ConfigValue);

            return result;
        }

        internal async Task<ServiceResponseModel<int>> StartBarcodeScannerByIP(string ipAddr)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            string methodName = "StartBarcodeScanner";

            //// *** testing
            //result.success = true;
            //result.data = 1;
            //return result;
            //// *** testing

            // 2. check plc which column is ready
            string plcIp = ipAddr;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                int registerAddress = 4308;
                int valueToWrite = 1;

                // Write the single holding register
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                result.success = true;
                result.data = 1;
            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            return result;
        }

        [HttpGet("GetReelID")]
        public async Task<ServiceResponseModel<TrayReelDTO>> GetReelID()
        {
            ServiceResponseModel<TrayReelDTO> result = new ServiceResponseModel<TrayReelDTO>();
            result.data = new TrayReelDTO();
            string methodName = "GetReelID";

            var configGantry = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Gantry1.ToString()).FirstOrDefault();
            if (configGantry == null)
            {
                result.errMessage = "Please set IP Address. ";
                return result;
            }

            var r2b = await GetReelIDByIP(configGantry.ConfigValue);
            if (r2b.success)
            {
                result.data.successScan = true;
                result.data.ScannedBarcode = r2b.data;

                Reel? reel = _dbContext.Reel.FirstOrDefault(x => x.ReelCode == r2b.data);
                if (reel != null)
                {
                    Item? item = await _dbContext.Item.FirstOrDefaultAsync(x => x.Item_Id == reel.Item_Id);
                    if (item != null)
                    {
                        result.data.successReel = true;
                        result.data.Reel_Id = reel.Reel_Id;
                        result.data.ReelCode = reel.ReelCode;
                        result.data.Item_Id = reel.Item_Id;
                        result.data.ItemCode = item.ItemCode;
                        result.data.UOM = item.UOM;
                        result.data.Description = item.Description;
                        result.data.Qty = reel.Qty;
                        result.data.ActualHeight = reel.ActualHeight;
                        result.data.Status = reel.Status;

                        result.success = true;
                    }
                    else
                    {
                        result.data.errMessageReel = "Cannot get Item info. Reel ID [" + r2b.data + "].";
                    }
                }
                else
                {
                    result.data.errMessageReel = "Cannot get Reel info. Reel ID [" + r2b.data + "].";
                }
            }
            else
            {
                result.data.errMessageScan = r2b.errMessage;
            }
                return result;
        }

        internal async Task<ServiceResponseModel<string>> GetReelIDByIP(string ipAddr)
        {
            ServiceResponseModel<string> result = new ServiceResponseModel<string>();
            result.data = "";
            string methodName = "GetReelIDByIP";

            //// *** testing
            //result.success = true;
            //result.errMessage = "Cannot get Reel ID, please try again.";
            //result.data = "A00000018";
            //return result;
            //// *** testing

            DateTime dtRun = DateTime.Now;
            bool exit = false;

            string decimalText = "";
            string reelID = "";
            int value = 0;

            string plcIp = ipAddr;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                int startAddress = 4218;
                int numRegisters = 1;

                while (!exit)
                {
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        value = registers[i];
                        decimalText = getDecimalText(registers[i]);
                    }

                    if (value > 0)
                    {
                        exit = true;
                        reelID = reelID + decimalText;
                    }
                    if ((DateTime.Now - dtRun).TotalSeconds > 30)
                    {
                        result.errMessage = "Timeout. Cannot get Reel ID.";
                        exit = true;
                    }
                }

                if (reelID != "")
                {
                    // second addr
                    startAddress = 4219;
                    numRegisters = 1;
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        decimalText = getDecimalText(registers[i]);
                        if (decimalText.Contains("\0"))
                        {
                            decimalText = decimalText.Substring(0, 1);
                        }
                        reelID = reelID + decimalText;
                    }

                    // third addr
                    startAddress = 4220;
                    numRegisters = 1;
                    registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        decimalText = getDecimalText(registers[i]);
                        if (decimalText.Contains("\0"))
                        {
                            decimalText = decimalText.Substring(0, 1);
                        }
                        reelID = reelID + decimalText;
                    }

                    // forth addr
                    startAddress = 4221;
                    numRegisters = 1;
                    registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        decimalText = getDecimalText(registers[i]);
                        if (decimalText.Contains("\0"))
                        {
                            decimalText = decimalText.Substring(0, 1);
                        }
                        reelID = reelID + decimalText;
                    }

                    // fifth addr
                    startAddress = 4222;
                    numRegisters = 1;
                    registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        decimalText = getDecimalText(registers[i]);
                        if (decimalText.Contains("\0"))
                        {
                            decimalText = decimalText.Substring(0, 1);
                        }
                        reelID = reelID + decimalText;
                    }

                    // sixth addr
                    startAddress = 4223;
                    numRegisters = 1;
                    registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        decimalText = getDecimalText(registers[i]);
                        if (decimalText.Contains("\0"))
                        {
                            decimalText = decimalText.Substring(0, 1);
                        }
                        reelID = reelID + decimalText;
                    }

                    PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, $"Get Reel ID {reelID}", "", false);

                    // *** testing
                    //reelID = "A00000018";
                    result.data = reelID;
                    result.success = true;
                }

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            return result;
        }

        [HttpGet("SetActualHeight/{loaderId}/{actHeight}")]
        public async Task<ServiceResponseModel<int>> SetActualHeight(long loaderId, int actHeight)
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

            result = await SetActualHeightByIP(_loader.IPAddr, actHeight);

            return result;
        }

        internal async Task<ServiceResponseModel<int>> SetActualHeightByIP(string ipAddr, int actHeight)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            string methodName = "SetActualHeight";

            //// *** testing
            //result.success = true;
            //result.data = 1;
            //return result;
            //// *** testing

            // 2. check plc which column is ready
            string plcIp = ipAddr;
            int port = 502;

            ModbusClient modbusClient1 = new ModbusClient(plcIp, port);
            try
            {
                float acHeight = (float)actHeight; // 11.827f;
                byte[] bytes = BitConverter.GetBytes(acHeight);

                int highBinary = BitConverter.ToUInt16(bytes, 0);
                int lowBinary = BitConverter.ToUInt16(bytes, 2);

                modbusClient1.Connect();

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                int registerAddress = 4318;

                // Write the single holding register
                modbusClient1.WriteSingleRegister(registerAddress, highBinary);
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Set height [" + highBinary + "] to Delta PLC.", "", false);

                registerAddress = 4319;

                // Write the single holding register
                modbusClient1.WriteSingleRegister(registerAddress, lowBinary);
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Set height [" + lowBinary + "] to Delta PLC.", "", false);

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Done set height to Delta PLC.", "", false);

                result.success = true;
                result.data = 2;
            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient1.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            ModbusClient modbusClient2 = new ModbusClient(plcIp, port);
            try
            {
                modbusClient2.Connect();

                //PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int registerAddress = 4319;
                int valueToWrite = actHeight;

                // Write the single holding register
                modbusClient2.WriteSingleRegister(registerAddress, valueToWrite);

                result.success = true;
                result.data = 2;
            }
            catch (Exception ex)
            {
                //PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient2.Disconnect();
                //PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "");
            }

            return result;
        }

        [HttpGet("GetRetrieveTrayStatus")]
        public ServiceResponseModel<RackJobSlotInfo> GetRetrieveTrayStatus()
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
            //result.success = false;
            //result.errMessage = "0";
            //result.data = 2;
            //return result;
            //// *** testing

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

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                while (!exit)
                {
                    int startAddress = 4212;
                    int numRegisters = 1;
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
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
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            if (value == 0)
            {
                int valueErr = -1;
                modbusClient = new ModbusClient(plcIp, port);
                try
                {
                    modbusClient.Connect();

                    PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                    int startAddress = 4231;
                    int numRegisters = 1;
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        valueErr = registers[i];
                    }

                    if (valueErr == 0)
                    {
                        try
                        {
                            exit = false;
                            //modbusClient.Connect();

                            PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                            while (!exit)
                            {
                                startAddress = 4212;
                                numRegisters = 1;
                                registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                                for (int i = 0; i < registers.Length; i++)
                                {
                                    PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
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
                            PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                            result.errMessage = ex.Message;
                            result.errStackTrace = ex.StackTrace ?? "";
                        }
                        finally
                        {
                            //modbusClient.Disconnect();
                            PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "", false);
                        }
                    }
                    result.errMessage = valueErr.ToString();
                }
                catch (Exception ex)
                {
                    PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                    result.errMessage = ex.Message;
                    result.errStackTrace = ex.StackTrace ?? "";
                }
                finally
                {
                    modbusClient.Disconnect();
                    PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "", false);
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
                var pulses = ReadPulseByIP(configRack.ConfigValue, slotCode);
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

        [HttpGet("GetPickStatus")]
        public ServiceResponseModel<int> GetPickStatus()
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            result.data = 0;
            string methodName = "GetPickStatus";

            var configGantry = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Gantry1.ToString()).FirstOrDefault();
            if (configGantry == null)
            {
                result.errMessage = "Please set IP Address. ";
                result.data = 0;
                return result;
            }

            //// *** testing
            //result.success = true;
            //result.errMessage = "Done Picked.";
            //result.data = 3;
            //return result;
            //// *** testing

            DateTime dtRun = DateTime.Now;
            bool exit = false;

            string decimalText = "";
            string value = "";

            string plcIp = configGantry.ConfigValue;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                while (!exit)
                {
                    int startAddress = 4216;
                    int numRegisters = 1;
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        value = registers[i].ToString();
                    }

                    if (value == "1")
                    {
                        exit = true;
                    }
                    if ((DateTime.Now - dtRun).TotalSeconds > 30)
                    {
                        result.errMessage = "Timeout. Cannot get Pick Status.";
                        exit = true;
                    }
                }

                if (string.IsNullOrEmpty(value)) { value = "0"; }
                result.success = value == "1";
                result.data = Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            return result;
        }

        [HttpGet("PutReelOnTray")]
        public ServiceResponseModel<int> PutReelOnTray()
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            string methodName = "PutReelOnTray";

            var configGantry = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Gantry1.ToString()).FirstOrDefault();
            if (configGantry == null)
            {
                result.errMessage = "Please set IP Address. ";
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
            string plcIp = configGantry.ConfigValue;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                int registerAddress = 4307;
                int valueToWrite = 1;

                // Write the single holding register
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                result.success = true;
                result.data = 1;

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            return result;
        }

        [HttpGet("GetPlaceStatus")]
        public async Task<ServiceResponseModel<int>> GetPlaceStatus()
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            result.data = 0;
            string methodName = "GetPlaceStatus";

            var configGantry = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Gantry1.ToString()).FirstOrDefault();
            if (configGantry == null)
            {
                result.errMessage = "Please set IP Address. ";
                result.data = 0;
                return result;
            }

            //// *** testing
            //result.success = true;
            //result.errMessage = "Done Placed.";
            //result.data = 0;
            //return result;
            //// *** testing

            DateTime dtRun = DateTime.Now;
            bool exit = false;

            string decimalText = "";
            int value = 0;

            string plcIp = configGantry.ConfigValue;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                while (!exit)
                {
                    int startAddress = 4198;
                    int numRegisters = 1;
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        value = registers[i];
                    }

                    if (value == 1)
                    {
                        exit = true;
                        result.success = true;
                        result.data = Convert.ToInt32(value);
                    }
                    if ((DateTime.Now - dtRun).TotalSeconds > 30)
                    {
                        result.errMessage = "Timeout. Cannot get Reel ID.";
                        exit = true;
                    }
                }

                if (result.success)
                {
                    result.errMessage = "Done Placed.";

                    // for reset place status
                    int registerAddress = 4309;
                    int valueToWrite = 1;
                    modbusClient.WriteSingleRegister(registerAddress, valueToWrite);
                }
            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            return result;
        }

        [HttpGet("GetBottomSlot/{actHeight}/{slotCode}")]
        public async Task<ServiceResponseModel<SlotDTO>> GetBottomSlot(int actHeight, string slotCode)
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

                        slotChg.NeedCheck = true;
                        slotChg.CheckRemark = "Cannot put Reel.";
                        _dbContext.SaveChanges();

                        PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Slot Error: Cannot put Reel on [" + slotChg.SlotCode + "].", "", true);
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

                //// testing 
                //slotUsage.ReserveSlot = 1;
                //// testing 

                SlotFreeReqDTO reqSlot = new SlotFreeReqDTO();
                reqSlot.ColNo = 0;
                reqSlot.TotalSlot = slotUsage.ReserveSlot;
                ServiceResponseModel<SlotFreeDTO> rSlot = await _slotService.GetFreeSlot_BySlot_ASC(reqSlot);
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
                    var colList = _dbContext.SlotColumnSetting.OrderBy(x => x.Reel_IN_Idx).ToList();
                    foreach (var col in colList)
                    {
                        SlotFreeReqDTO req = new SlotFreeReqDTO();
                        req.ColNo = col.ColNo;
                        req.TotalSlot = slotUsage.ReserveSlot;

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
                }

                if (bottomSlotCol == 0 && bottomSlotRow == 0)
                {
                    result.success = false;
                    result.errMessage = "No empty slot is ready.";
                    return result;
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
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
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
            string plcIp = configRack.ConfigValue;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

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
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            return result;
        }

        [HttpGet("GetPutAwayStatus")]
        public ServiceResponseModel<RackJobSlotInfo> GetPutAwayStatus()
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

            //// *** testing
            //result.success = true;
            //result.errMessage = "Done Put Away.";
            //result.data = 0;
            //return result;
            //// *** testing

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

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                while (!exit)
                {
                    int startAddress = 4196; // 4229;
                    int numRegisters = 1;
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
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

            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "", false);
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
                var pulses = ReadPulseByIP(configRack.ConfigValue, slotCode);
                result.data.SlotCode = slotCode;
                result.data.QRXPulse = pulses[0];
                result.data.QRYPulse = pulses[1];
                result.data.QRXPulseDiffer = pulses[2];
                result.data.QRXPulseDiffer = pulses[3];
            }

            result.success = value == 0;
            result.data.data = value.ToString();

            return result;
        }

        //[HttpGet("UpdateReelIntoRack/{loaderId}/{colNo}/{reelId}/{slotCode}/{slotReserve}")]
        //public async Task<ServiceResponseModel<int>> UpdateReelIntoRack(long loaderId, int colNo, string reelId, string slotCode, int slotReserve)
        //{
        //    ServiceResponseModel<int> result = new ServiceResponseModel<int>();
        //    result.data = -1;
        //    string methodName = "UpdateReelIntoLoader";

        //    try
        //    {
        //        var _loader = _dbContext.Loader.Find(loaderId);
        //        if (_loader == null)
        //        {
        //            result.errMessage = "Loader is not found.";
        //            return result;
        //        }
        //        var _reel = _dbContext.Reel.Where(x => x.Reel_Id.ToString() == reelId).FirstOrDefault();
        //        if (_reel == null)
        //        {
        //            result.errMessage = "Reel is not found.";
        //            return result;
        //        }
        //        var _loaderCol = _dbContext.LoaderColumn.Where(x => x.Loader_Id == loaderId && x.ColNo == colNo).FirstOrDefault();
        //        if (_loaderCol == null)
        //        {
        //            result.errMessage = "Loader Column is not found.";
        //            return result;
        //        }
        //        var _loaderReel = _dbContext.LoaderReel.Where(x => x.Reel_Id.ToString().ToUpper() == reelId.ToUpper()).FirstOrDefault();
        //        if (_loaderReel == null)
        //        {
        //            result.errMessage = "Loader Reel is not found."; 
        //            return result;
        //        }
        //        var _slot = _dbContext.Slot.Where(x => x.SlotCode == slotCode).FirstOrDefault();
        //        if (_slot == null)
        //        {
        //            result.errMessage = "Slot is not found.";
        //            return result;
        //        }

        //        // temp hide
        //        // 1.1 update reel status
        //        _reel.StatusIdx = (int)EnumReelStatus.IsReady;
        //        _reel.Status = EnumReelStatus.IsReady.ToString();
        //        await _dbContext.SaveChangesAsync();

        //        // 2. update loader columne balance height
        //        _loaderCol.BalanceHeight = _loaderCol.BalanceHeight + _reel.ActualHeight;
        //        await _dbContext.SaveChangesAsync();

        //        // 3. remove reel from loaderreel
        //        _dbContext.LoaderReel.Remove(_loaderReel);
        //        await _dbContext.SaveChangesAsync();

        //        // 4. update slot set reelId
        //        _slot.Reel_Id = _reel.Reel_Id;
        //        _slot.ReelNo = "0";
        //        _slot.HasReel = true;
        //        _slot.HasEmptyTray = false;
        //        await _dbContext.SaveChangesAsync();

        //        // 5. update other slot if is reserved
        //        if (slotReserve > 1)
        //        {
        //            for (int iR = 1; iR < slotReserve; iR++)
        //            {
        //                var _slotO = _dbContext.Slot.Where(x => x.ColNo == _slot.ColNo && x.RowNo == (_slot.RowNo + iR)).FirstOrDefault();
        //                if (_slotO == null)
        //                {
        //                    result.errMessage = "Other Slot is not found.";
        //                    return result;
        //                }
        //                _slotO.ReelNo = iR.ToString();
        //                _slotO.HasReel = true;
        //                _slotO.HasEmptyTray = false;
        //                _slotO.Reel_Id = _reel.Reel_Id;
        //                await _dbContext.SaveChangesAsync();
        //            }
        //        }


        //        // 1.2 update slot id
        //        _reel.Slot_Id = _slot.Slot_Id;
        //        await _dbContext.SaveChangesAsync();

        //        // 6. Checking loader all column status and update
        //        int r = 0;
        //        var colBal = _dbContext.LoaderReel.Where(x => x.Loader_Id == loaderId).FirstOrDefault();
        //        if (colBal == null)
        //        {
        //            // means all is out
        //            r = 2;
        //        }
        //        else
        //        {
        //            colBal = _dbContext.LoaderReel.Where(x => x.Loader_Id == loaderId && x.ColNo == colNo).FirstOrDefault();
        //            if (colBal == null)
        //            {
        //                // means cur col all is out
        //                r = 1;
        //            }
        //        }
        //        //// if is whole loader last 2nd reel then need call endtask first
        //        //if (r == 0)
        //        //{
        //        //    var colBal2 = _dbContext.LoaderReel.Where(x => x.Loader_Id == loaderId).Count();
        //        //    if (colBal2 == 2)
        //        //    {
        //        //        r = 5;
        //        //    }
        //        //}


        //        if (r == 2)
        //        {
        //            _loader.Status = EnumLoaderStatus.ReadyToLoad.ToString();
        //            await _dbContext.SaveChangesAsync();
        //        }

        //        result.success = true;
        //        result.data = r;
        //    }
        //    catch (Exception ex)
        //    {
        //        //PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
        //        result.errMessage = ex.Message;
        //        result.errStackTrace = ex.StackTrace ?? "";
        //    }

        //    return result;
        //}

        [HttpGet("UpdateLoaderCount/{loaderId}/{colNo}/{reelCode}")]
        public async Task<ServiceResponseModel<int>> UpdateLoaderCount(long loaderId, int colNo, string reelCode)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            result.data = -1;
            string methodName = "UpdateReelIntoLoader";

            try
            {
                var _loader = _dbContext.Loader.Find(loaderId);
                if (_loader == null)
                {
                    result.errMessage = "Loader is not found.";
                    return result;
                }
                var _reel = _dbContext.Reel.Where(x => x.ReelCode == reelCode).FirstOrDefault();
                if (_reel == null)
                {
                    result.errMessage = "Reel is not found.";
                    return result;
                }
                var _loaderCol = _dbContext.LoaderColumn.Where(x => x.Loader_Id == loaderId && x.ColNo == colNo).FirstOrDefault();
                if (_loaderCol == null)
                {
                    result.errMessage = "Loader Column is not found.";
                    return result;
                }
                var _loaderReel = _dbContext.LoaderReel.Where(x => x.Reel_Id.ToString().ToUpper() == _reel.Reel_Id.ToString().ToUpper()).FirstOrDefault();
                //if (_loaderReel == null)
                //{
                //    result.errMessage = "Loader Reel is not found.";
                //    return result;
                //}

                // temp hide
                // 1.1 update reel status
                _reel.StatusIdx = (int)EnumReelStatus.InPicked;
                _reel.Status = EnumReelStatus.InPicked.ToString();
                await _dbContext.SaveChangesAsync();

                if (_loaderReel != null)
                {
                    // 2. update loader columne balance height
                    _loaderCol.BalanceHeight = _loaderCol.BalanceHeight + _reel.ActualHeight;
                    await _dbContext.SaveChangesAsync();

                    // 3. remove reel from loaderreel
                    _dbContext.LoaderReel.Remove(_loaderReel);
                    await _dbContext.SaveChangesAsync();
                }

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
                    colBal = _dbContext.LoaderReel.Where(x => x.Loader_Id == loaderId && x.ColNo == colNo).FirstOrDefault();
                    if (colBal == null)
                    {
                        // means cur col all is out
                        r = 1;
                    }
                }
                //// if is whole loader last 2nd reel then need call endtask first
                //if (r == 0)
                //{
                //    var colBal2 = _dbContext.LoaderReel.Where(x => x.Loader_Id == loaderId).Count();
                //    if (colBal2 == 2)
                //    {
                //        r = 5;
                //    }
                //}


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
                //PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        [HttpGet("UpdateReelRack/{reelCode}/{slotCode}/{slotReserve}")]
        public async Task<ServiceResponseModel<int>> UpdateReelRack(string reelCode, string slotCode, int slotReserve)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            result.data = -1;
            string methodName = "UpdateReelIntoLoader";

            try
            {
                var _reel = _dbContext.Reel.Where(x => x.ReelCode == reelCode).FirstOrDefault();
                if (_reel == null)
                {
                    result.errMessage = "Reel is not found.";
                    return result;
                }
                var _slot = _dbContext.Slot.Where(x => x.SlotCode == slotCode).FirstOrDefault();
                if (_slot == null)
                {
                    result.errMessage = "Slot is not found.";
                    return result;
                }

                // temp hide
                // 1.1 update reel status
                _reel.StatusIdx = (int)EnumReelStatus.IsReady;
                _reel.Status = EnumReelStatus.IsReady.ToString();
                _reel.IsReady = true;
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
                        var _slotO = _dbContext.Slot.Where(x => x.ColNo == _slot.ColNo && x.RowNo == (_slot.RowNo + iR)).FirstOrDefault();
                        if (_slotO == null)
                        {
                            result.errMessage = "Other Slot is not found.";
                            return result;
                        }
                        _slotO.Reel_Id = _reel.Reel_Id;
                        _slotO.ReelNo = iR.ToString();
                        _slotO.HasReel = true;
                        _slotO.HasEmptyTray = false;
                        await _dbContext.SaveChangesAsync();
                    }
                }


                // 1.2 update slot id
                _reel.Slot_Id = _slot.Slot_Id;
                await _dbContext.SaveChangesAsync();

                result.success = true;
                result.data = 1;
            }
            catch (Exception ex)
            {
                //PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        [HttpGet("UnloadEndTask/{loaderId}")]
        public ServiceResponseModel<int> UnloadEndTask(long loaderId)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            result.data = 0;
            string methodName = "UnloadEndTask";

            var _loader = _dbContext.Loader.Find(loaderId);
            if (_loader == null)
            {
                result.errMessage = "Loader is not found.";
                result.data = 0;
                return result;
            }

            //// *** testing
            //result.success = true;
            //result.errMessage = "Done EndTask.";
            //result.data = 0;
            //return result;
            //// *** testing

            string plcIp = _loader.IPAddr;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                int registerAddress = 4317;
                int valueToWrite = 2;
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                result.success = true;
            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            return result;
        }

        [HttpGet("UnloadTempEndTask/{loaderId}")]
        public ServiceResponseModel<int> UnloadTempEndTask(long loaderId)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            result.data = 0;
            string methodName = "UnloadTempEndTask";

            var _loader = _dbContext.Loader.Find(loaderId);
            if (_loader == null)
            {
                result.errMessage = "Loader is not found.";
                result.data = 0;
                return result;
            }

            //// *** testing
            //result.success = true;
            //result.errMessage = "Done Placed.";
            //result.data = 0;
            //return result;
            //// *** testing

            string plcIp = _loader.IPAddr;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                int registerAddress = 4317;
                int valueToWrite = 1;
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                result.success = true;
            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            return result;
        }

        [HttpGet("SetColumnEmpty/{loaderId}/{curCol}")]
        public async Task<ServiceResponseModel<int>> SetColumnEmpty(long loaderId, int curCol)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            string methodName = "SetColumnEmpty";

            var _loader = _dbContext.Loader.Find(loaderId);
            if (_loader == null)
            {
                result.errMessage = "Loader is not found.";
                result.data = 0;
                return result;
            }

            //// *** testing
            //result.success = true;
            //result.data = 1;
            //return result;
            //// *** testing

            // 2. check plc which column is ready
            string plcIp = _loader.IPAddr;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", curCol.ToString(), false);

                int registerAddress = 4296;
                if (curCol == 2) { registerAddress = 4297; }
                if (curCol == 3) { registerAddress = 4298; }
                if (curCol == 4) { registerAddress = 4299; }
                int valueToWrite = 2;

                // Write the single holding register
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                result.success = true;
                result.data = 2;
            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            return result;
        }

        [HttpGet("GetReadyTurnStatus/{loaderId}")]
        public ServiceResponseModel<int> GetReadyTurnStatus(long loaderId)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            result.data = 0;
            string methodName = "GetReadyTurnStatus";

            var _loader = _dbContext.Loader.Find(loaderId);
            if (_loader == null)
            {
                result.errMessage = "Loader is not found.";
                result.data = 0;
                return result;
            }

            //// *** testing
            //result.success = true;
            //result.errMessage = "Done Picked.";
            //result.data = 3;
            //return result;
            //// *** testing

            DateTime dtRun = DateTime.Now;
            bool exit = false;

            string decimalText = "";
            int value = 0;

            string plcIp = _loader.IPAddr;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                int startAddress = 4225;
                int numRegisters = 1;
                while (!exit)
                {
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        value = registers[i];
                    }

                    string hex2 = value.ToString("X");

                    if (hex2 == "400")
                    {
                        exit = true;
                    }
                    if ((DateTime.Now - dtRun).TotalSeconds > 30)
                    {
                        result.errMessage = "Timeout. Cannot get Reel ID.";
                        exit = true;
                    }
                }

                string hex = value.ToString("X");
                result.success = hex == "400";
                result.data = Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            return result;
        }

        [HttpGet("SetTurnColumn/{loaderId}/{nextCol}")]
        public async Task<ServiceResponseModel<int>> SetTurnColumn(long loaderId, int nextCol)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            string methodName = "SetTurnColumn";

            var _loader = _dbContext.Loader.Find(loaderId);
            if (_loader == null)
            {
                result.errMessage = "Loader is not found.";
                result.data = 0;
                return result;
            }

            //// *** testing
            //result.success = true;
            //result.data = 1;
            //return result;
            //// *** testing

            // 2. check plc which column is ready
            string plcIp = _loader.IPAddr;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                int registerAddress = 4314;

                // Write the single holding register
                modbusClient.WriteSingleRegister(registerAddress, nextCol);

                result.success = true;
                result.data = 2;
            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            return result;
        }

        [HttpGet("GetLoaderMode/{loaderId}")]
        public ServiceResponseModel<int> GetLoaderMode(long loaderId)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            string methodName = "GetLoaderMode";

            //// *** testing
            //result.success = true;
            //return result;
            //// *** testing

            var _loader = _dbContext.Loader.Find(loaderId);
            if (_loader == null)
            {
                result.errMessage = "Loader is not found.";
                result.data = 0;
                return result;
            }

            int value = 0;
            string plcIp = _loader.IPAddr;
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);

            try
            {
                modbusClient.Connect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                int startAddress = 4208;
                int numRegisters = 1;

                int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                value = registers[0];

                result.success = value == 1;
                result.errMessage = "Current mode is [" + value + "], please turn to Auto.";
            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }
            return result;
        }

        [HttpGet("Trolley/{code}")]
        public async Task<ServiceResponseModel<bool>> Trolley(string code)
        {
            ServiceResponseModel<bool> result = new ServiceResponseModel<bool>();
            result.data = false;

            try
            {
                var claims = User.Identities.First().Claims.ToList();
                string devId = claims?.FirstOrDefault(x => x.Type.Equals("DeviceId", StringComparison.OrdinalIgnoreCase))?.Value ?? "";

                result.success = true;
            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
            }
            return result;
        }

        [HttpGet("SetTrolleyOn/{colNo}/{rowNo}")]
        public async Task<ServiceResponseModel<int>> SetTrolleyOn(int colNo, int rowNo)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            string methodName = "SetTrolleyOn";

            // 2. check plc which column is ready
            string plcIp = "";
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                int registerAddress = 1;
                bool valueToWrite = true;

                // Write the single holding register
                modbusClient.WriteSingleCoil(registerAddress, valueToWrite);

                result.success = true;
                result.data = 2;
            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
                PLCLogHelper.Instance.InsertPLCHubInLog(_dbContext, 0, methodName, "Disconnected.", "", false);
            }

            return result;
        }

        [HttpGet("GetUpcomingReels/{loaderId}/{skipRow}")]
        public async Task<ServiceResponseModel<List<LoaderReelDtlDTO>>> GetUpcomingReels(long loaderId, int skipRow)
        {
            ServiceResponseModel<List<LoaderReelDtlDTO>> result = new ServiceResponseModel<List<LoaderReelDtlDTO>>();

            var _loader = _dbContext.Loader.Find(loaderId);
            if (_loader == null)
            {
                result.errMessage = "Loader is not found.";
                result.data = new List<LoaderReelDtlDTO>();
                return result;
            }

            var reels = new List<LoaderReelDtlDTO>();
            int takeRow = skipRow > 0 ? 1 : 10;

            var list = await _dbContext.LoaderReel.Where(x => x.Loader_Id == loaderId).OrderBy(x => x.ColNo).Skip(skipRow).Take(takeRow).ToListAsync();
            foreach (var dtl in list)
            {
                var reel = _dbContext.Reel.Where(x => x.Reel_Id == dtl.Reel_Id).FirstOrDefault();
                reels.Add(new LoaderReelDtlDTO
                {
                    Loader_Id = loaderId,
                    LoaderCode = _loader.LoaderCode,

                    Reel_Id = dtl.Reel_Id,
                    ReelCode = reel == null ? "" : reel.ReelCode,
                    ItemCode = reel == null ? "" : reel.ItemCode,
                    Qty = reel == null ? 0 : reel.Qty,
                });
            }
            result.data = reels;
            result.totalRecords = reels.Count;

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
                    var list = await _dbContext.PLCHubInLog.Where(x => x.CreatedDate >= srms.StartDate && x.CreatedDate <= srms.EndDate && x.IsErr).ToListAsync();
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

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "", false);

                int startAddress = 4201;
                int numRegisters = 1;

                while (!exit)
                {
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        value = registers[i];
                        decimalText = getDecimalText(registers[i]);
                    }

                    if (value > 0)
                    {
                        exit = true;
                        slotID = slotID + decimalText;
                    }
                    if ((DateTime.Now - dtRun).TotalSeconds > 1)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Timeout. Cannot get Slot ID..", "", false);
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
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
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
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
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
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
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
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
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
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
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
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "", false);
                        decimalText = getDecimalText(registers[i]);
                        if (decimalText.Contains("\0"))
                        {
                            decimalText = decimalText.Substring(0, 1);
                        }
                        slotID = slotID + decimalText;
                    }

                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Get Reel ID {slotID}", "", false);

                    // *** testing
                    //slotID = "A00000018";
                    result = slotID;
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
                    if ((DateTime.Now - dtRun).TotalSeconds > 1)
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

    }
}
