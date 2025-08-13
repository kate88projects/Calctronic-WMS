using Microsoft.AspNetCore.Mvc;
using RackingSystem.Models.Item;
using RackingSystem.Models;
using RackingSystem.Data;
using RackingSystem.Services.SettingServices;
using RackingSystem.Models.Setting;
using RackingSystem.Services.ItemServices;

namespace RackingSystem.Controllers
{
    public class SettingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ISettingService _setService;

        public SettingController(AppDbContext context, ISettingService setService)
        {
            _context = context;
            _setService = setService;
        }

        public IActionResult ReelDimensionList()
        {
            ViewData["Title"] = "Reel Dimension";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<ReelDimensionListDTO>>> GetReelDimensionList()
        {
            ServiceResponseModel<List<ReelDimensionListDTO>> result = await _setService.GetReelDimensionList();
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> SaveReelDimension([FromBody] ReelDimensionDTO itemReq)
        {
            ServiceResponseModel<ReelDimensionDTO> result = await _setService.SaveReelDimension(itemReq);
            return new JsonResult(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteReelDimension([FromBody] ReelDimensionDTO itemReq)
        {
            ServiceResponseModel<ReelDimensionDTO> result = await _setService.DeleteReelDimension(itemReq);
            return new JsonResult(result);
        }
        
        [HttpGet]
        public async Task<ServiceResponseModel<List<ReelDimensionListDTO>>> GetReelDimensionList_DDL()
        {
            ServiceResponseModel<List<ReelDimensionListDTO>> result = await _setService.GetReelDimensionList_DDL();
            return result;
        }

        public IActionResult SlotCalculationList()
        {
            ViewData["Title"] = "Slot Calculation";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<SlotCalculationListDTO>>> GetSlotCalculationList()
        {
            ServiceResponseModel<List<SlotCalculationListDTO>> result = await _setService.GetSlotCalculationList();
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> SaveSlotCalculation([FromBody] SlotCalculationDTO itemReq)
        {
            ServiceResponseModel<SlotCalculationDTO> result = await _setService.SaveSlotCalculation(itemReq);
            return new JsonResult(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSlotCalculation([FromBody] SlotCalculationDTO itemReq)
        {
            ServiceResponseModel<SlotCalculationDTO> result = await _setService.DeleteSlotCalculation(itemReq);
            return new JsonResult(result);
        }

    }
}
