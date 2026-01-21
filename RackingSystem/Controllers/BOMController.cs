using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using RackingSystem.Data;
using RackingSystem.Models;
using RackingSystem.Models.BOM;
using RackingSystem.Models.Item;
using RackingSystem.Models.User;
using RackingSystem.Services.BOMServices;
using System.Drawing.Text;

namespace RackingSystem.Controllers
{
    public class BOMController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IBOMService _bomService;

        public BOMController(AppDbContext context, IBOMService bomService)
        {
            _context = context;
            _bomService = bomService;
        }

        public IActionResult BOMList()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpBOM";
            ViewData["ActiveTab"] = "BOMList";
            ViewData["Title"] = "BOM List";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<BOMListDTO>> GetBOM(long bomId)
        {
            ServiceResponseModel<BOMListDTO> result = await _bomService.GetBOM(bomId);
            return result;
        }

        [HttpPost]
        public async Task<ServiceResponseModel<List<BOMListDTO>>> GetBOMList([FromBody] BOMSearchReqDTO req)
        {
            if (req == null)
            {
                ServiceResponseModel<List<BOMListDTO>> rErr = new ServiceResponseModel<List<BOMListDTO>>();
                rErr.errMessage = "Empty parameter.";
                return rErr;
            }
            int ttl = -1;
            if (req.page == 1)
            {
                ServiceResponseModel<int> rTotal = await _bomService.GetBOMTotalCount(req);
                if (rTotal.success)
                {
                    ttl = rTotal.data;
                }
                else
                {
                    ServiceResponseModel<List<BOMListDTO>> rErr = new ServiceResponseModel<List<BOMListDTO>>();
                    rErr.errMessage = rTotal.errMessage;
                    rErr.errStackTrace = rTotal.errStackTrace;
                    return rErr;
                }
            }
            ServiceResponseModel<List<BOMListDTO>> result = await _bomService.GetBOMList(req);
            result.totalRecords = ttl;
            return result;
        }

        [HttpPost]
        public async Task<ServiceResponseModel<List<BOMDtlDTO>>> GetBOMDetail([FromBody] long bomId)
        {
            ServiceResponseModel<List<BOMDtlDTO>> result = await _bomService.GetBOMDetail(bomId);
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> SaveBOM([FromBody] BOMDtlReqDTO bom)
        {
            ServiceResponseModel<BOMDtlReqDTO> result = await _bomService.SaveBOM(bom);
            return new JsonResult(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBOM([FromBody] BOMListDTO bom)
        {
            ServiceResponseModel<BOMListDTO> result = await _bomService.DeleteBOM(bom);
            return new JsonResult(result);
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<BOMListReqDTO>>> GetActiveBOMList()
        {
            ServiceResponseModel<List<BOMListReqDTO>> result = await _bomService.GetActiveBOMList();
            return result;
        }

        public IActionResult BOMDetails(int id, string mode)
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpBOM";
            ViewData["ActiveTab"] = "BOMDetails";
            ViewData["Title"] = "BOM Details";
            ViewBag.Mode = mode;

            BOMListDTO bomData = new BOMListDTO();

            if (id != 0)
            {
                var bom = GetBOM(id);
                bom.Wait();
                var bomHeader = bom.Result;
                bomData = bomHeader.data ?? new BOMListDTO();
            }

            return View(bomData);
        }

        [HttpPost]
        public async Task<IActionResult> SaveExcelBOM([FromBody] BOMExcelReqDTO boms)
        {
            ServiceResponseModel<BOMExcelReqDTO> result = await _bomService.SaveExcelBOM(boms);
            return new JsonResult(result);
        }
    }
}
