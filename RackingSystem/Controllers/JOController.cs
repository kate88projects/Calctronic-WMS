using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RackingSystem.Data;
using RackingSystem.Models.GRN;
using RackingSystem.Models;
using RackingSystem.Models.User;
using RackingSystem.Services.GRNServices;
using RackingSystem.Services.JOServices;
using RackingSystem.Models.JO;
using RackingSystem.Data.JO;

namespace RackingSystem.Controllers
{
    public class JOController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IJOService _joService;

        public JOController(AppDbContext context, IJOService joService)
        {
            _context = context;
            _joService = joService;
        }

        public IActionResult JOList()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpJO";
            ViewData["ActiveTab"] = "JOList";
            ViewData["Title"] = "JO List";
            return View();
        }

        //[HttpPost]
        //public async Task<ServiceResponseModel<List<JOListDTO>>> GetGRNDetailList([FromBody] JOSearchReqDTO req)
        //{
        //    if (req == null)
        //    {
        //        ServiceResponseModel<List<JOListDTO>> rErr = new ServiceResponseModel<List<JOListDTO>>();
        //        rErr.errMessage = "Empty parameter.";
        //        return rErr;
        //    }
        //    int ttl = -1;
        //    if (req.page == 1)
        //    {
        //        ServiceResponseModel<int> rTotal = await _joService.GetGRNDetailTotalCount(req);
        //        if (rTotal.success)
        //        {
        //            ttl = rTotal.data;
        //        }
        //        else
        //        {
        //            ServiceResponseModel<List<JOListDTO>> rErr = new ServiceResponseModel<List<JOListDTO>>();
        //            rErr.errMessage = rTotal.errMessage;
        //            rErr.errStackTrace = rTotal.errStackTrace;
        //            return rErr;
        //        }
        //    }
        //    ServiceResponseModel<List<GRNDtlListDTO>> result = await _joService.GetGRNDetailList(req);
        //    result.totalRecords = ttl;
        //    return result;
        //}


        [HttpPost]
        public async Task<IActionResult> SaveJob([FromBody] JOReqDTO job)
        {
            ServiceResponseModel<JOReqDTO> result = await _joService.SaveJob(job);
            return new JsonResult(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteJob([FromBody] JOListDTO job)
        {
            ServiceResponseModel<JOListDTO> result = await _joService.DeleteJob(job);
            return new JsonResult(result);
        }
    }
}
