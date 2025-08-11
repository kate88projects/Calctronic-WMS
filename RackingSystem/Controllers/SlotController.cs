using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.Models.Slot;
using RackingSystem.Models;
using RackingSystem.Services.SlotServices;

namespace RackingSystem.Controllers
{
    public class SlotController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ISlotService _slotService;

        public SlotController(AppDbContext context, ISlotService slotService)
        {
            _context = context;
            _slotService = slotService;
        }

        public IActionResult SlotList()
        {
            ViewData["Title"] = "Slot List";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<SlotListDTO>>> GetSlotList()
        {
            ServiceResponseModel<List<SlotListDTO>> result = await _slotService.GetSlotList();
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> SaveSlot([FromBody] SlotDTO slotReq)
        {
            ServiceResponseModel<SlotDTO> result = await _slotService.SaveSlot(slotReq);
            return new JsonResult(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSlot([FromBody] SlotDTO slotReq)
        {
            ServiceResponseModel<SlotDTO> result = await _slotService.DeleteSlot(slotReq);
            return new JsonResult(result);
        }

        public IActionResult SlotStatus()
        {
            ViewData["Title"] = "Slot Status";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<SlotListDTO>>> GetSlotStatus_ByColumn(string req)
        {
            if (req == null)
            {
                ServiceResponseModel<List<SlotListDTO>> rErr = new ServiceResponseModel<List<SlotListDTO>> ();
                rErr.errMessage = "Please insert column.";
                return rErr;
            }
            int colNo = 0;
            bool isNum = int.TryParse(req, out colNo);
            ServiceResponseModel<List<SlotListDTO>> result = await _slotService.GetSlotStatus_ByColumn(colNo);
            return result;
        }

    }
}
