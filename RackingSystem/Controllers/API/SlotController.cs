using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RackingSystem.Data;
using RackingSystem.Models;
using RackingSystem.Models.Slot;
using RackingSystem.Services.SlotServices;
using System;

namespace RackingSystem.Controllers.API
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SlotController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly ISlotService _slotService;

        public SlotController(AppDbContext dbContext, ISlotService slotService)
        {
            _dbContext = dbContext;
            _slotService = slotService;
        }

        [HttpGet("getBinList")]
        public async Task<ServiceResponseModel<List<SlotListDTO>>> GetBinList()
        {
            ServiceResponseModel<List<SlotListDTO>> result = await _slotService.GetSlotList();
            return result;
        }
    }
}
