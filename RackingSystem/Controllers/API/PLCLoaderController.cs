using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RackingSystem.Data;
using RackingSystem.Models.Slot;
using RackingSystem.Models;
using RackingSystem.Services.SlotServices;
using EasyModbus;
using System.Composition;
using RackingSystem.General;
using RackingSystem.Helpers;
using System.Reflection;
using RackingSystem.Data.Maintenances;
using RackingSystem.Models.Item;
using RackingSystem.Models.Reel;
using Microsoft.EntityFrameworkCore;

namespace RackingSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class PLCLoaderController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public PLCLoaderController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        internal string getDecimalText(int input)
        {
            if (input == 0) { return ""; }
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

        [HttpGet("GetLoaderId/{loaderId}")]
        public ServiceResponseModel<string> GetLoaderId(long loaderId)
        {
            ServiceResponseModel<string> result = new ServiceResponseModel<string>();
            string methodName = "GetLoaderId";

            try
            {
                var _loader = _dbContext.Loader.Find(loaderId);
                if (_loader == null)
                {
                    result.errMessage = "Loader not found.";
                    return result;
                }
                if (_loader.IsActive == false)
                {
                    result.errMessage = "Loader is not active.";
                    result.data = "";
                    return result;
                }

                // *** testing
                result.success = true;
                result.data = "00AS01";
                return result;
                // *** testing

                string loaderString = "";
                string decimalText = "";

                string plcIp = _loader.IPAddr;
                int port = 502;

                ModbusClient modbusClient = new ModbusClient(plcIp, port);
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                // first addr
                int startAddress = 4196;
                int numRegisters = 1;
                int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                    decimalText = getDecimalText(registers[i]);
                    loaderString = loaderString + decimalText;
                }

                // second addr
                startAddress = 4197;
                numRegisters = 1;
                registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                    decimalText = getDecimalText(registers[i]);
                    loaderString = loaderString + decimalText;
                }

                // third addr
                startAddress = 4198;
                numRegisters = 1;
                registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                    decimalText = getDecimalText(registers[i]);
                    loaderString = loaderString + decimalText;
                }

                // forth addr
                startAddress = 4199;
                numRegisters = 1;
                registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                    decimalText = getDecimalText(registers[i]);
                    loaderString = loaderString + decimalText;
                }

                result.success = _loader.LoaderCode == loaderString;
                result.data = loaderString;
                result.errMessage = "Selected loader [" + _loader.LoaderCode + "] is difference from reading [" + loaderString + "].";

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

        [HttpGet("VerifyItem/{reelCode}")]
        public ServiceResponseModel<ReelDTO> VerifyItem(string reelCode)
        {
            ServiceResponseModel<ReelDTO> result = new ServiceResponseModel<ReelDTO>();

            try
            {
                var _reel = _dbContext.Reel.Where(x => x.ReelCode == reelCode).FirstOrDefault();
                if (_reel == null)
                {
                    result.errMessage = "Reel is not found.";
                    result.data = new ReelDTO();
                    return result;
                }
                if (_reel.Status != EnumReelStatus.WaitingLoader.ToString())
                {
                    result.errMessage = "Reel is in status [" + _reel.Status + "].";
                    result.data = new ReelDTO();
                    return result;
                }
                var _item = _dbContext.Item.Find(_reel.Item_Id);
                if (_item == null)
                {
                    result.errMessage = "Item is not found.";
                    result.data = new ReelDTO();
                    return result;
                }
                if (_item.IsActive == false)
                {
                    result.errMessage = "Item is not active.";
                    result.data = new ReelDTO();
                    return result;
                }
                var _rDi = _dbContext.ReelDimension.Find(_item.ReelDimension_Id);
                if (_rDi == null)
                {
                    result.errMessage = "Item Reel Dimension is not found.";
                    result.data = new ReelDTO();
                    return result;
                }

                result.success = true;
                var dto = new ReelDTO();
                dto.Item_Id = _item.Item_Id;
                dto.ItemCode = _item.ItemCode;
                dto.UOM = _item.UOM;
                dto.Description = _item.Description;
                dto.ReelCode = _reel.ReelCode;
                dto.Thickness = _rDi.Thickness;
                result.data = dto;

            }
            catch (Exception ex)
            {
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        [HttpGet("CheckColumnHeight/{loaderId}/{colNo}/{height}")]
        public ServiceResponseModel<int> CheckColumnHeight(long loaderId, int colNo, int height)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            string methodName = "CheckColumnHeight";

            try
            {
                // 1. check db for available height
                int minHeight = 0;
                var configMinHeight = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.Loader_ColMinReserve.ToString()).FirstOrDefault();
                if (configMinHeight != null)
                {
                    minHeight = Convert.ToInt16(configMinHeight.ConfigValue);
                }

                var _loader = _dbContext.Loader.Find(loaderId);
                if (_loader == null)
                {
                    result.errMessage = "Loader is not found.";
                    result.data = 0;
                    return result;
                }
                if (_loader.IsActive == false)
                {
                    result.errMessage = "Loader is not active.";
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

                result.data = _loaderCol.BalanceHeight;
                if (_loaderCol.BalanceHeight <= minHeight && minHeight != 0)
                {
                    result.success = false;
                    result.errMessage = "Loader Column [" + colNo + "] is full.";
                }
                else
                {
                    if (_loaderCol.BalanceHeight < height)
                    {
                        result.success = false;
                        result.errMessage = "Loader Column [" + colNo + "] balance height [" + _loaderCol.BalanceHeight + "] is less than estimate height.";
                    }
                    else
                    {
                        result.success = true;
                    }
                }
                return result;

                // 2. check plc is quandrant actual height
                int plcActualHeight = 0;
                string plcIp = _loader.IPAddr;
                int port = 502;

                ModbusClient modbusClient = new ModbusClient(plcIp, port);
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int startAddress = 4210;
                if (colNo == 2)
                {
                    startAddress = 4211;
                }
                else if (colNo == 3)
                {
                    startAddress = 4212;
                }
                else if (colNo == 4)
                {
                    startAddress = 4213;
                }
                int numRegisters = 1;

                int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);

                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                    plcActualHeight = registers[i];
                }

                result.data = registers[0];
                if (plcActualHeight >= height)
                {
                    result.success = true;
                }
                else
                {
                    result.success = false;
                    result.errMessage = "Loader Column [" + colNo + "] is full. [2]";
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

        [HttpGet("GetAvailableColumnByHeight/{loaderId}/{height}")]
        public ServiceResponseModel<List<int>> GetAvailableColumnByHeight(long loaderId, int height)
        {
            ServiceResponseModel<List<int>> result = new ServiceResponseModel<List<int>>();
            result.data = new List<int>();
            string methodName = "CheckColumnHeight";

            try
            {
                // 1. check db for available height
                int minHeight = 0;
                var configMinHeight = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.Loader_ColMinReserve.ToString()).FirstOrDefault();
                if (configMinHeight != null)
                {
                    minHeight = Convert.ToInt16(configMinHeight.ConfigValue);
                }

                var _loader = _dbContext.Loader.Find(loaderId);
                if (_loader == null)
                {
                    result.errMessage = "Loader is not found.";
                    result.data.Add(0);
                    result.data.Add(0);
                    return result;
                }
                if (_loader.IsActive == false)
                {
                    result.errMessage = "Loader is not active.";
                    result.data.Add(0);
                    result.data.Add(0);
                    return result;
                }
                var _loaderCols = _dbContext.LoaderColumn.Where(x => x.Loader_Id == loaderId).OrderBy(x => x.ColNo).ToList();
                if (_loaderCols == null)
                {
                    result.errMessage = "Loader Column is not found.";
                    result.data.Add(0);
                    result.data.Add(0);
                    return result;
                }

                foreach (var _loaderCol in _loaderCols)
                {
                    if (_loaderCol.BalanceHeight <= minHeight && minHeight != 0)
                    {
                        continue;
                    }

                    if (_loaderCol.BalanceHeight >= height)
                    {
                        result.success = true;
                        result.data.Add(_loaderCol.ColNo);
                        result.data.Add(_loaderCol.BalanceHeight);
                        return result;
                    }
                }
                result.success = false;
                result.errMessage = "All Loader Column is full.";
                result.data.Add(0);
                result.data.Add(0);
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

        [HttpGet("GetCurrentColumn/{loaderId}")]
        public ServiceResponseModel<int> GetCurrentColumn(long loaderId)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            string methodName = "GetCurrentColumn";

            try
            {
                // 1. check db for available height
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
                int plcActualHeight = 0;
                string plcIp = _loader.IPAddr;
                int port = 502;

                ModbusClient modbusClient = new ModbusClient(plcIp, port);
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int startAddress = 4210;
                int numRegisters = 1;

                int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);

                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                    plcActualHeight = registers[i];
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

        [HttpGet("TurnColumn/{loaderId}/{iCol}")]
        public ServiceResponseModel<int> TurnColumn(long loaderId, int iCol)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            string methodName = "TurnColumn";

            try
            {
                // 1. check db for available height
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
                int plcActualHeight = 0;
                string plcIp = _loader.IPAddr;
                int port = 502;

                ModbusClient modbusClient = new ModbusClient(plcIp, port);
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int startAddress = 4314;
                if (iCol == 3)
                {
                    startAddress = 4315;
                }
                else if (iCol == 4)
                {
                    startAddress = 4316;
                }
                int numRegisters = 1;

                int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);

                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                    plcActualHeight = registers[i];
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

        [HttpGet("GetForkReady/{loaderId}/{colNo}")]
        public ServiceResponseModel<int> GetForkReady(long loaderId, int colNo)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            string methodName = "GetColumnReady";

            try
            {
                var _loader = _dbContext.Loader.Find(loaderId);
                if (_loader == null)
                {
                    result.errMessage = "Loader not found.";
                    return result;
                }

                //// *** testing
                //result.success = true;
                //result.data = 1;
                //return result;
                //// *** testing

                int value = 0;
                DateTime dtRun = DateTime.Now;
                bool exit = false;

                string plcIp = _loader.IPAddr;
                int port = 502;

                ModbusClient modbusClient = new ModbusClient(plcIp, port);
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int startAddress = 4224;
                int numRegisters = 1;

                while (!exit)
                {
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                        value = registers[i];
                    }

                    if (value == 1)
                    {
                        exit = true;
                    }
                    if ((DateTime.Now - dtRun).TotalMinutes > 3)
                    {
                        result.errMessage = "Timeout. Cannot get Fork value.";
                        exit = true;
                    }
                }

                result.success = value == 1;
                result.data = value;

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

        [HttpGet("GetReelActualHeight/{loaderId}/{colNo}")]
        public ServiceResponseModel<List<int>> GetReelActualHeight(long loaderId, int colNo)
        {
            ServiceResponseModel<List<int>> result = new ServiceResponseModel<List<int>>();
            result.data = new List<int>();
            string methodName = "GetReelHeight";

            try
            {
                var _loader = _dbContext.Loader.Find(loaderId);
                if (_loader == null)
                {
                    result.errMessage = "Loader not found.";
                    result.data.Add(1);
                    result.data.Add(0);
                    return result;
                }
                var _loaderCol = _dbContext.LoaderColumn.Where(x => x.Loader_Id == loaderId && x.ColNo == colNo).FirstOrDefault();
                if (_loaderCol == null)
                {
                    result.errMessage = "Loader Column is not found.";
                    result.data.Add(1);
                    result.data.Add(0);
                    return result;
                }

                //// *** testing
                //result.success = false;
                //result.errMessage = "Cannot get Actual Height, please try again.";
                //result.data.Add(1);
                //result.data.Add(0);
                //result.errMessage = "Loader Column [" + colNo + "] is full.";
                //result.data.Add(2);
                //result.data.Add(15);
                //return result;
                //// *** testing

                string decimalText = "";
                int height = 0;

                string plcIp = _loader.IPAddr;
                int port = 502;

                ModbusClient modbusClient = new ModbusClient(plcIp, port);
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int startAddress = 4216;
                int numRegisters = 1;
                int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                    //decimalText = getDecimalText(registers[i]);
                    byte[] bytes = BitConverter.GetBytes(registers[i]);
                    float floatValue = BitConverter.ToSingle(bytes, 0);
                    float roundedValue = (float)((int)(floatValue * 1000)) / 1000;
                    height = Convert.ToInt32(roundedValue); 
                }

                result.success = _loaderCol.BalanceHeight >= height;
                result.data.Add(_loaderCol.BalanceHeight >= height ? 0 : 2);
                result.data.Add(height);
                if (_loaderCol.BalanceHeight < height)
                {
                    result.errMessage = "Loader Column [" + colNo + "] is full.";
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

        [HttpGet("UpdateReelIntoLoader/{loaderId}/{colNo}/{reelCode}/{actHeight}")]
        public async Task<ServiceResponseModel<int>> UpdateReelIntoLoader(long loaderId, int colNo, string reelCode, int actHeight)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            string methodName = "UpdateReelIntoLoader";

            try
            {
                // 1. check db for available height
                var _loader = _dbContext.Loader.Find(loaderId);
                if (_loader == null)
                {
                    result.errMessage = "Loader is not found.";
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
                var _reel = _dbContext.Reel.Where(x => x.ReelCode == reelCode).FirstOrDefault();
                if (_reel == null)
                {
                    result.errMessage = "Reel is not found.";
                    result.data = 0;
                    return result;
                }
                var _item = _dbContext.Item.Find(_reel.Item_Id);
                if (_item == null)
                {
                    result.errMessage = "Item is not found.";
                    result.data = 0;
                    return result;
                }

                _reel.StatusIdx = (int)EnumReelStatus.InLoader;
                _reel.Status = EnumReelStatus.InLoader.ToString();
                _reel.ActualHeight = actHeight;
                await _dbContext.SaveChangesAsync();

                _loaderCol.BalanceHeight = _loaderCol.BalanceHeight - actHeight;
                await _dbContext.SaveChangesAsync();

                LoaderReel _loaderReel = new LoaderReel();
                _loaderReel.Loader_Id = loaderId;
                _loaderReel.ColNo = colNo;
                _loaderReel.Reel_Id = _reel.Reel_Id;
                _dbContext.LoaderReel.Add(_loaderReel);
                await _dbContext.SaveChangesAsync();

                _loader.Status = EnumLoaderStatus.Loaded.ToString();
                await _dbContext.SaveChangesAsync();

                result.success = true;
                result.data = _loaderCol.BalanceHeight;
            }
            catch (Exception ex)
            {
                //PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Error: " + ex.Message, "");
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }

            return result;
        }

        [HttpGet("EndTask/{loaderId}")]
        public ServiceResponseModel<int> EndTask(long loaderId)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            string methodName = "EndTask";

            try
            {
                // 1. check db for available height
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
                int value = 0;
                string plcIp = _loader.IPAddr;
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

    }
}
