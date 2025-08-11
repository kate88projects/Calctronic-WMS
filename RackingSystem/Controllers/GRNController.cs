using Microsoft.AspNetCore.Mvc;
using RackingSystem.Data;
using RackingSystem.Models.Slot;
using RackingSystem.Models;
using RackingSystem.Services.GRNServices;
using RackingSystem.Models.GRN;

namespace RackingSystem.Controllers
{
    public class GRNController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IGRNService _grnService;

        public GRNController(AppDbContext context, IGRNService grnService)
        {
            _context = context;
            _grnService = grnService;
        }

        public IActionResult NewGRN()
        {
            ViewData["Title"] = "New GRN";
            return View();
        }

        [HttpPost]
        public async Task<ServiceResponseModel<GRNDtlDTO>> SaveGRNDtl([FromBody] GRNDtlReqDTO req)
        {
            if (req == null)
            {
                ServiceResponseModel<GRNDtlDTO> rErr = new ServiceResponseModel<GRNDtlDTO>();
                rErr.errMessage = "Please select item.";
                return rErr;
            }
            ServiceResponseModel<GRNDtlDTO> result = await _grnService.SaveGRNDtl(req);
            return result;
        }

        public IActionResult GRNDetailList()
        {
            ViewData["Title"] = "GRN List";
            return View();
        }

        [HttpPost]
        public async Task<ServiceResponseModel<List<GRNDtlListDTO>>> GetGRNDetailList([FromBody] GRNSearchReqDTO req)
        {
            if (req == null)
            {
                ServiceResponseModel<List<GRNDtlListDTO>> rErr = new ServiceResponseModel<List<GRNDtlListDTO>>();
                rErr.errMessage = "Empty parameter.";
                return rErr;
            }
            int ttl = -1;
            if (req.page == 1)
            {
                ServiceResponseModel<int> rTotal = await _grnService.GetGRNDetailTotalCount(req);
                if (rTotal.success)
                {
                    ttl = rTotal.data;
                }
                else
                {
                    ServiceResponseModel<List<GRNDtlListDTO>> rErr = new ServiceResponseModel<List<GRNDtlListDTO>>();
                    rErr.errMessage = rTotal.errMessage;
                    rErr.errStackTrace = rTotal.errStackTrace;
                    return rErr;
                }
            }
            ServiceResponseModel<List<GRNDtlListDTO>> result = await _grnService.GetGRNDetailList(req);
            result.totalRecords = ttl;
            return result;
        }

    }
}
