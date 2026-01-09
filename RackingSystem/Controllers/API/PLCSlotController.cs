using AutoMapper;
using EasyModbus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using RackingSystem.Data;
using RackingSystem.Data.Maintenances;
using RackingSystem.General;
using RackingSystem.Helpers;
using RackingSystem.Models;
using RackingSystem.Services.LoaderServices;
using RackingSystem.Services.SlotServices;
using System.Reflection.Metadata;

namespace RackingSystem.Controllers.API
{
    [Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme}")]
    [ApiController]
    [Route("api/[controller]")]
    public class PLCSlotController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly ISlotService _slotService;
        private readonly IMapper _mapper;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public PLCSlotController(AppDbContext dbContext, ISlotService slotService, IMapper mapper, IDbContextFactory<AppDbContext> contextFactory)
        {
            _dbContext = dbContext;
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

        [HttpGet("SetScanPulseStart/{colNo}/{rowNo}")]
        public async Task<ServiceResponseModel<int>> SetScanPulseStart(int colNo, int rowNo)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            string methodName = "SetLoaderUnload";

            var configRack = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking1.ToString()).FirstOrDefault();
            if (configRack == null)
            {
                result.errMessage = "Please set IP Address. ";
                result.data = 0;
                return result;
            }

            var slot = _dbContext.Slot.Where(x => x.ColNo == colNo && x.RowNo == rowNo).FirstOrDefault();
            if (slot == null)
            {
                result.errMessage = "Cannot find Slot for Row No [" + rowNo + "]. ";
                result.data = 0;
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

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                // step 1 : left or right
                int registerAddress = 4298;
                int valueToWrite = slot.IsLeft ? 0 : 1;
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                // step 2 : x-pulses
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
                bytes = BitConverter.GetBytes(slot.YPulse);
                highBinary = BitConverter.ToUInt16(bytes, 0);
                lowBinary = BitConverter.ToUInt16(bytes, 2);
                registerAddress = 4302;
                valueToWrite = 0;
                modbusClient.WriteSingleRegister(registerAddress, lowBinary);
                registerAddress = 4301;
                valueToWrite = 0;
                modbusClient.WriteSingleRegister(registerAddress, highBinary);

                // step 4 : 
                registerAddress = 4310;
                valueToWrite = 0;
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                // step 5 : 
                registerAddress = 4311;
                valueToWrite = 0;
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                // step 6 : 
                registerAddress = 4312;
                valueToWrite = 7;
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                // step Last : 
                registerAddress = 4297;
                valueToWrite = 4;
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

        [HttpGet("ReadPulse/{colNo}/{rowNo}")]
        public async Task<ServiceResponseModel<string>> ReadPulse(int colNo, int rowNo)
        {
            ServiceResponseModel<string> result = new ServiceResponseModel<string>();
            string methodName = "StartBarcodeScanner";

            var configRack = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking1.ToString()).FirstOrDefault();
            if (configRack == null)
            {
                result.errMessage = "Please set IP Address. ";
                result.data = "0 || 0";
                return result;
            }

            var slot = _dbContext.Slot.Where(x => x.ColNo == colNo && x.RowNo == rowNo).FirstOrDefault();
            if (slot == null)
            {
                result.errMessage = "Cannot find Slot for Row No [" + rowNo + "]. ";
                result.data = "0 || 0";
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

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int startAddress = 4223;
                int numRegisters = 1;

                while (!exit)
                {
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                        value = registers[i];
                    }

                    if (value > 0)
                    {
                        exit = true;
                    }
                    if ((DateTime.Now - dtRun).TotalSeconds > 15)
                    {
                        result.errMessage = "Timeout. Cannot get Status.";
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
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
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
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
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
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
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
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                        decimalText = getDecimalText(registers[i]);
                        if (decimalText.Contains("\0"))
                        {
                            decimalText = decimalText.Substring(0, 1);
                        }
                        qrYText = qrYText + decimalText;
                    }

                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Get X {qrXText} Y {qrYText}", "");

                    slot.LastQRXPulse = slot.QRXPulse;
                    slot.LastQRYPulse = slot.QRYPulse;
                    slot.LastQRReadTime = DateTime.Now;

                    int qrX = 0;
                    int qrY = 0;
                    int.TryParse(qrXText, out qrX);
                    int.TryParse(qrYText, out qrY);
                    if (qrX > 0 || qrY > 0)
                    {
                        slot.QRXPulse = qrX; 
                        slot.QRYPulse = qrY;
                        _dbContext.SaveChanges();

                        result.data = qrXText + " || " + qrYText;
                        result.success = true;
                    }

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

        [HttpGet("SetScanTrolleyPulseStart/{side}/{colNo}/{rowNo}")]
        public async Task<ServiceResponseModel<int>> SetScanTrolleyPulseStart(string side, int colNo, int rowNo)
        {
            ServiceResponseModel<int> result = new ServiceResponseModel<int>();
            string methodName = "SetLoaderUnload";

            var configRack = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking1.ToString()).FirstOrDefault();
            if (configRack == null)
            {
                result.errMessage = "Please set IP Address. ";
                result.data = 0;
                return result;
            }
            bool isLeft = side == "A";
            var slot = _dbContext.TrolleySlot.Where(x => x.ColNo == colNo && x.RowNo == rowNo && x.IsLeft == isLeft).FirstOrDefault();
            if (slot == null)
            {
                result.errMessage = "Cannot find Trolley Slot for Row No [" + rowNo + "]. ";
                result.data = 0;
                return result;
            }

            //// *** testing
            //result.success = true;
            //result.data = 1;
            //return result;
            //// *** testing

            // 2. check plc which column is ready
            string plcIp = configRack.ConfigValue.ToString();
            //if (colNo == 2) { plcIp = "192.168.100.151"; }
            //if (colNo == 3) { plcIp = "192.168.100.152"; }
            int port = 502;

            ModbusClient modbusClient = new ModbusClient(plcIp, port);
            try
            {
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                // step 1 : left or right
                int registerAddress = 4298;
                int valueToWrite = 1;
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                // step 2 : x-pulses
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
                bytes = BitConverter.GetBytes(slot.YPulse);
                highBinary = BitConverter.ToUInt16(bytes, 0);
                lowBinary = BitConverter.ToUInt16(bytes, 2);
                registerAddress = 4302;
                valueToWrite = 0;
                modbusClient.WriteSingleRegister(registerAddress, lowBinary);
                registerAddress = 4301;
                valueToWrite = 0;
                modbusClient.WriteSingleRegister(registerAddress, highBinary);

                // step 4 : 
                registerAddress = 4310;
                valueToWrite = 0;
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                // step 5 : 
                registerAddress = 4311;
                valueToWrite = 0;
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                // step 6 : 
                registerAddress = 4312;
                valueToWrite = 7;
                modbusClient.WriteSingleRegister(registerAddress, valueToWrite);

                // step Last : 
                registerAddress = 4297;
                valueToWrite = 4;
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

        [HttpGet("ReadTrolleyPulse/{side}/{colNo}/{rowNo}")]
        public async Task<ServiceResponseModel<string>> ReadTrolleyPulse(string side, int colNo, int rowNo)
        {
            ServiceResponseModel<string> result = new ServiceResponseModel<string>();
            string methodName = "StartBarcodeScanner";

            var configRack = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking1.ToString()).FirstOrDefault();
            if (configRack == null)
            {
                result.errMessage = "Please set IP Address. ";
                result.data = "0 || 0";
                return result;
            }

            bool isLeft = side == "A";
            var slot = _dbContext.TrolleySlot.Where(x => x.ColNo == colNo && x.RowNo == rowNo && x.IsLeft == isLeft).FirstOrDefault();
            if (slot == null)
            {
                result.errMessage = "Cannot find Trolley Slot for Row No [" + rowNo + "]. ";
                result.data = "0 , 0";
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

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int startAddress = 4223;
                int numRegisters = 1;

                while (!exit)
                {
                    int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);
                    for (int i = 0; i < registers.Length; i++)
                    {
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                        value = registers[i];
                    }

                    if (value > 0)
                    {
                        exit = true;
                    }
                    if ((DateTime.Now - dtRun).TotalSeconds > 15)
                    {
                        result.errMessage = "Timeout. Cannot get Status.";
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
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
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
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
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
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
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
                        PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                        decimalText = getDecimalText(registers[i]);
                        if (decimalText.Contains("\0"))
                        {
                            decimalText = decimalText.Substring(0, 1);
                        }
                        qrYText = qrYText + decimalText;
                    }

                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Get X {qrXText} Y {qrYText}", "");

                    slot.LastQRXPulse = slot.QRXPulse;
                    slot.LastQRYPulse = slot.QRYPulse;
                    slot.LastQRReadTime = DateTime.Now;

                    int qrX = 0;
                    int qrY = 0;
                    int.TryParse(qrXText, out qrX);
                    int.TryParse(qrYText, out qrY);
                    if (qrX > 0 || qrY > 0)
                    {
                        slot.QRXPulse = qrX;
                        slot.QRYPulse = qrY;
                        _dbContext.SaveChanges();

                        result.data = qrXText + " , " + qrYText;
                        result.success = true;
                    }

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

    }
}
