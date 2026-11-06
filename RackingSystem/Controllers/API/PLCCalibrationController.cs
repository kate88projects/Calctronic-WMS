using EasyModbus;
using Microsoft.AspNetCore.Mvc;
using RackingSystem.Data;
using RackingSystem.General;
using RackingSystem.Helpers;
using RackingSystem.Models;

namespace RackingSystem.Controllers.API
{
    // Calibration SRMS :: for each slot of a column, take a drawer from one trolley slot, put away, read QR,
    // retrieve from the same slot, return drawer to the same trolley slot. The cycle is driven by CalibrationSRMS.cshtml.
    // Only moves the robot and reads status. No change to Slot / TrolleySlot / Reel data.
    [ApiController]
    [Route("api/[controller]")]
    public class PLCCalibrationController : Controller
    {
        private readonly AppDbContext _dbContext;

        private const int CMD_PUTAWAY = 1;
        private const int CMD_RETRIEVE = 2;

        public PLCCalibrationController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("GetTrolleys")]
        public ServiceResponseModel<List<object>> GetTrolleys()
        {
            ServiceResponseModel<List<object>> result = new ServiceResponseModel<List<object>>();
            result.data = _dbContext.Trolley.Where(x => x.IsActive).OrderBy(x => x.TrolleyCode)
                .Select(x => (object)new { x.Trolley_Id, x.TrolleyCode }).ToList();
            result.success = true;
            return result;
        }

        [HttpGet("GetColumns")]
        public ServiceResponseModel<List<object>> GetColumns()
        {
            ServiceResponseModel<List<object>> result = new ServiceResponseModel<List<object>>();
            result.data = _dbContext.Slot.GroupBy(x => new { x.ColNo, x.IsLeft })
                .Select(g => new { g.Key.ColNo, g.Key.IsLeft, TotalSlot = g.Count() })
                .OrderBy(x => x.ColNo).ToList()
                .Select(x => (object)x).ToList();
            result.success = true;
            return result;
        }

        [HttpGet("GetColumnSlots/{colNo}")]
        public ServiceResponseModel<List<object>> GetColumnSlots(int colNo)
        {
            ServiceResponseModel<List<object>> result = new ServiceResponseModel<List<object>>();
            result.data = _dbContext.Slot.Where(x => x.ColNo == colNo).OrderBy(x => x.RowNo)
                .Select(x => (object)new { x.SlotCode, x.RowNo, x.IsActive, x.HasReel, x.HasEmptyTray, x.NeedCheck, x.QRXPulse, x.QRYPulse })
                .ToList();
            result.success = true;
            return result;
        }

        // trolley -> robot
        [HttpGet("RetrieveTrolley/{trolleyId}/{side}/{colNo}/{rowNo}")]
        public async Task<ServiceResponseModel<string>> RetrieveTrolley(long trolleyId, string side, int colNo, int rowNo)
        {
            return await MoveTrolley("CalibRetrieveTrolley", trolleyId, side, colNo, rowNo, CMD_RETRIEVE);
        }

        // robot -> trolley
        [HttpGet("PutAwayTrolley/{trolleyId}/{side}/{colNo}/{rowNo}")]
        public async Task<ServiceResponseModel<string>> PutAwayTrolley(long trolleyId, string side, int colNo, int rowNo)
        {
            return await MoveTrolley("CalibPutAwayTrolley", trolleyId, side, colNo, rowNo, CMD_PUTAWAY);
        }

        // robot -> rack slot
        [HttpGet("PutAwayRack/{slotCode}")]
        public async Task<ServiceResponseModel<string>> PutAwayRack(string slotCode)
        {
            return await MoveRack("CalibPutAwayRack", slotCode, CMD_PUTAWAY);
        }

        // rack slot -> robot
        [HttpGet("RetrieveRack/{slotCode}")]
        public async Task<ServiceResponseModel<string>> RetrieveRack(string slotCode)
        {
            return await MoveRack("CalibRetrieveRack", slotCode, CMD_RETRIEVE);
        }

        // 4222 :: 1 working, 2 complete, 3 no drawer
        [HttpGet("GetRetrieveTrolleyStatus")]
        public ServiceResponseModel<string> GetRetrieveTrolleyStatus()
        {
            return PollStatus("CalibRetrieveTrolleyStatus", 4222, new[] { 2 }, new[] { 3 }, "No drawer at trolley slot.");
        }

        // 4196 :: 1 working, 2 complete, 3 have tray
        [HttpGet("GetPutAwayStatus")]
        public ServiceResponseModel<string> GetPutAwayStatus()
        {
            return PollStatus("CalibPutAwayStatus", 4196, new[] { 2 }, new[] { 3 }, "Have Tray, slot is occupied.");
        }

        // 4212 :: 1 complete
        [HttpGet("GetRetrieveRackStatus")]
        public ServiceResponseModel<string> GetRetrieveRackStatus()
        {
            return PollStatus("CalibRetrieveRackStatus", 4212, new[] { 1 }, new int[0], "");
        }

        private async Task<ServiceResponseModel<string>> MoveTrolley(string methodName, long trolleyId, string side, int colNo, int rowNo, int cmd)
        {
            ServiceResponseModel<string> result = new ServiceResponseModel<string>();
            result.data = "";

            bool left = side == "A";
            var slot = _dbContext.TrolleySlot.Where(x => x.Trolley_Id == trolleyId && x.IsLeft == left && x.ColNo == colNo && x.RowNo == rowNo).FirstOrDefault();
            if (slot == null)
            {
                result.errMessage = "Cannot find Trolley Slot [" + side + " " + colNo + " - " + rowNo + "]. ";
                return result;
            }

            // trolley side always 1, same as Drawer In / Hub Out
            return await SendMove(methodName, slot.TrolleySlotCode, 1, slot.XPulse, slot.YPulse, slot.QRXPulse, slot.QRYPulse, cmd);
        }

        private async Task<ServiceResponseModel<string>> MoveRack(string methodName, string slotCode, int cmd)
        {
            ServiceResponseModel<string> result = new ServiceResponseModel<string>();
            result.data = "";

            var slot = _dbContext.Slot.Where(x => x.SlotCode == slotCode).FirstOrDefault();
            if (slot == null)
            {
                result.errMessage = "Cannot find Slot [" + slotCode + "]. ";
                return result;
            }

            return await SendMove(methodName, slot.SlotCode, slot.IsLeft ? 0 : 1, slot.XPulse, slot.YPulse, slot.QRXPulse, slot.QRYPulse, cmd);
        }

        private async Task<ServiceResponseModel<string>> SendMove(string methodName, string slotCode, int side, int xPulse, int yPulse, int qrX, int qrY, int cmd)
        {
            ServiceResponseModel<string> result = new ServiceResponseModel<string>();
            result.data = slotCode;

            var configRack = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking1.ToString()).FirstOrDefault();
            if (configRack == null)
            {
                result.errMessage = "Please set IP Address. ";
                return result;
            }

            // robot is shared, do not move while a rack job is running
            var srms = _dbContext.RackJob.FirstOrDefault();
            if (srms != null && srms.RackJobQueue_Id != 0)
            {
                result.errMessage = "A rack job is running. Stop the job before calibration.";
                return result;
            }

            ModbusClient modbusClient = new ModbusClient(configRack.ConfigValue, 502);
            try
            {
                modbusClient.Connect();
                PLCLogHelper.Instance.InsertPLCCalibrationLog(_dbContext, 0, methodName, "Connected to Delta PLC.", slotCode, false);

                WriteRegister(modbusClient, methodName, 4298, side);

                // pulses :: lower word on 4299 / 4301, upper word on 4300 / 4302
                byte[] bytes = BitConverter.GetBytes(xPulse);
                WriteRegister(modbusClient, methodName, 4300, BitConverter.ToUInt16(bytes, 2));
                WriteRegister(modbusClient, methodName, 4299, BitConverter.ToUInt16(bytes, 0));

                bytes = BitConverter.GetBytes(yPulse);
                WriteRegister(modbusClient, methodName, 4301, BitConverter.ToUInt16(bytes, 0));
                WriteRegister(modbusClient, methodName, 4302, BitConverter.ToUInt16(bytes, 2));

                WriteRegister(modbusClient, methodName, 4310, qrX);
                WriteRegister(modbusClient, methodName, 4311, qrY);
                WriteRegister(modbusClient, methodName, 4312, 7);

                WriteRegister(modbusClient, methodName, 4297, cmd);
                PLCLogHelper.Instance.InsertPLCCalibrationLog(_dbContext, 0, methodName, $"Register 4297 : {cmd}", slotCode, false);

                result.success = true;
            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCCalibrationLog(_dbContext, 0, methodName, "Error: " + ex.Message, slotCode, true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
            }

            return result;
        }

        private void WriteRegister(ModbusClient modbusClient, string methodName, int address, int value)
        {
            modbusClient.WriteSingleRegister(address, value);
            PLCLogHelper.Instance.InsertPLCAddressResponse(_dbContext, address, "Write", value, methodName, false);
        }

        // data :: last value read. success when value in doneValues.
        private ServiceResponseModel<string> PollStatus(string methodName, int address, int[] doneValues, int[] failValues, string failMessage)
        {
            ServiceResponseModel<string> result = new ServiceResponseModel<string>();
            result.data = "";

            var configRack = _dbContext.Configuration.Where(x => x.ConfigTitle == EnumConfiguration.PLC_IPAddr_Racking1.ToString()).FirstOrDefault();
            if (configRack == null)
            {
                result.errMessage = "Please set IP Address. ";
                return result;
            }

            DateTime dtRun = DateTime.Now;
            int value = -1;
            ModbusClient modbusClient = new ModbusClient(configRack.ConfigValue, 502);
            try
            {
                modbusClient.Connect();
                while (true)
                {
                    int[] registers = modbusClient.ReadHoldingRegisters(address, 1);
                    value = registers[0];

                    if (doneValues.Contains(value))
                    {
                        result.success = true;
                        break;
                    }
                    if (failValues.Contains(value))
                    {
                        result.errMessage = failMessage;
                        break;
                    }
                    if ((DateTime.Now - dtRun).TotalSeconds > 30)
                    {
                        result.errMessage = "Timeout. Robot did not report finished.";
                        break;
                    }
                    Thread.Sleep(200);
                }
                PLCLogHelper.Instance.InsertPLCAddressResponse(_dbContext, address, "Read", value, methodName, !result.success);
                PLCLogHelper.Instance.InsertPLCCalibrationLog(_dbContext, 0, methodName, $"Register {address} : {value}", result.errMessage, !result.success);
            }
            catch (Exception ex)
            {
                PLCLogHelper.Instance.InsertPLCCalibrationLog(_dbContext, 0, methodName, "Error: " + ex.Message, "", true);
                result.errMessage = ex.Message;
                result.errStackTrace = ex.StackTrace ?? "";
            }
            finally
            {
                modbusClient.Disconnect();
            }

            result.data = value.ToString();
            return result;
        }
    }
}
