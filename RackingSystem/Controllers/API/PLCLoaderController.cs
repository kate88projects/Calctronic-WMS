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

        [HttpGet]
        public ServiceResponseModel<bool> GetLoaderId()
        {
            ServiceResponseModel<bool> result = new ServiceResponseModel<bool>();
            string methodName = "GetLoaderId";

            try
            {
                var configIP = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IP_Loader.ToString()).First();
                if (configIP == null)
                {
                    result.errMessage = "Configuration IP Loader has not yet set.";
                    return result;
                }
                var configPort = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_Port_Loader.ToString()).First();
                if (configPort == null)
                {
                    result.errMessage = "Configuration Port Loader has not yet set.";
                    return result;
                }

                string plcIp = configIP.ConfigValue;
                int port = Convert.ToInt32(configPort.ConfigValue);

                ModbusClient modbusClient = new ModbusClient(plcIp, port);
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int startAddress = 4117;
                int numRegisters = 1;

                int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);

                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                }

                result.success = true;
                result.data = registers[0] == 1;

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


        [HttpGet]
        public ServiceResponseModel<string> CheckColumnActualHeight(long loaderId, int colNo, int estHeight)
        {
            ServiceResponseModel<string> result = new ServiceResponseModel<string>();
            string methodName = "CheckColumnActualHeight";

            try
            {
                var configIP = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IP_Loader.ToString()).First();
                if (configIP == null)
                {
                    result.errMessage = "Configuration IP Loader has not yet set.";
                    result.data = "";
                    return result;
                }
                var configPort = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_Port_Loader.ToString()).First();
                if (configPort == null)
                {
                    result.errMessage = "Configuration Port Loader has not yet set.";
                    result.data = "";
                    return result;
                }

                // 1. check db for available height
                var loader = _dbContext.Loader.Where(x => x.Loader_Id == loaderId).FirstOrDefault();
                if (loader == null)
                {
                    result.errMessage = "Loader is not found.";
                    result.data = "";
                    return result;
                }
                if (loader.IsActive == false)
                {
                    result.errMessage = "Loader is not active.";
                    result.data = "";
                    return result;
                }
                var loaderCol = _dbContext.LoaderColumn.Where(x => x.Loader_Id == loaderId && x.ColNo == colNo).FirstOrDefault();
                if (loaderCol == null)
                {
                    result.errMessage = "Loader Column is not found.";
                    result.data = "";
                    return result;
                }

                if (loaderCol.BalanceHeight < estHeight)
                {
                    result.errMessage = "Loader Column [" + colNo + "] is full.";
                    result.data = "";
                    return result;
                }

                // 2. check plc is quandrant absense
                string plcIp = configIP.ConfigValue;
                int port = Convert.ToInt32(configPort.ConfigValue);

                ModbusClient modbusClient = new ModbusClient(plcIp, port);
                modbusClient.Connect();

                PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, "Connected to Delta PLC.", "");

                int startAddress = 4119;
                if (colNo == 2)
                {
                    startAddress = 4120;
                }
                else if (colNo == 3)
                {
                    startAddress = 4121;
                }
                else if (colNo == 4)
                {
                    startAddress = 4122;
                }
                int numRegisters = 1;

                int[] registers = modbusClient.ReadHoldingRegisters(startAddress, numRegisters);

                for (int i = 0; i < registers.Length; i++)
                {
                    PLCLogHelper.Instance.InsertPLCLoaderLog(_dbContext, 0, methodName, $"Register {startAddress + i}: {registers[i]}", "");
                }

                result.data = registers[0].ToString();
                if (registers[0] == 0)
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

        [HttpGet]
        public ServiceResponseModel<string> UpdateReelIntoLoader(long loaderId, int colNo, int actualHeight, string reelId)
        {
            ServiceResponseModel<string> result = new ServiceResponseModel<string>();
            string methodName = "UpdateReelIntoLoader";

            try
            {
                var configIP = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IP_Loader.ToString()).First();
                if (configIP == null)
                {
                    result.errMessage = "Configuration IP Loader has not yet set.";
                    result.data = "";
                    return result;
                }
                var configPort = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_Port_Loader.ToString()).First();
                if (configPort == null)
                {
                    result.errMessage = "Configuration Port Loader has not yet set.";
                    result.data = "";
                    return result;
                }

                // 1. check db for available height
                var loader = _dbContext.Loader.Where(x => x.Loader_Id == loaderId).FirstOrDefault();
                if (loader == null)
                {
                    result.errMessage = "Loader is not found.";
                    result.data = "";
                    return result;
                }
                var loaderCol = _dbContext.LoaderColumn.Where(x => x.Loader_Id == loaderId && x.ColNo == colNo).FirstOrDefault();
                if (loaderCol == null)
                {
                    result.errMessage = "Loader Column is not found.";
                    result.data = "";
                    return result;
                }

                result.success = true;

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
