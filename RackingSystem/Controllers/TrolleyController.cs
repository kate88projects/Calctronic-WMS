using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RackingSystem.Data;
using RackingSystem.Models;
using RackingSystem.Models.Trolley;
using RackingSystem.Models.User;
using RackingSystem.Services.TrolleyServices;
using System.Collections.Generic;
using System.Security.Policy;

namespace RackingSystem.Controllers
{
    public class TrolleyController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ITrolleyService _trolleyService;

        public TrolleyController(AppDbContext context, ITrolleyService trolleyService)
        {
            _context = context;
            _trolleyService = trolleyService;
        }

        public IActionResult TrolleyList()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpMM";
            ViewData["ActiveTab"] = "TrolleyList";
            ViewData["Title"] = "Trolley List";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<TrolleyListDTO>>> GetTrolleyList()
        {
            ServiceResponseModel<List<TrolleyListDTO>> result = await _trolleyService.GetTrolleyList();
            return result;
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<TrolleyListDTO>>> GetActiveTrolleyList()
        {
            ServiceResponseModel<List<TrolleyListDTO>> result = await _trolleyService.GetActiveTrolleyList();
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> SaveTrolley([FromBody] TrolleyListDTO trolley)
        {
            ServiceResponseModel<TrolleyListDTO> result = await _trolleyService.SaveTrolley(trolley);
            return new JsonResult(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTrolley([FromBody] TrolleyListDTO trolley)
        {
            ServiceResponseModel<TrolleyListDTO> result = await _trolleyService.DeleteTrolley(trolley);
            return new JsonResult(result);
        }

        //Trolley slot
        public IActionResult TrolleySlot()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpMM";
            ViewData["ActiveTab"] = "TrolleySlot";
            ViewData["Title"] = "Trolley Slot";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<TrolleySlotDTO>>> GetTrolleySlotList()
        {
            ServiceResponseModel<List<TrolleySlotDTO>> result = await _trolleyService.GetTrolleySlotList();
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> SaveTrolleySlot([FromBody] TrolleySlotDTO trolleySlot)
        {
            ServiceResponseModel<TrolleySlotDTO> result = await _trolleyService.SaveTrolleySlot(trolleySlot);
            return new JsonResult(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTrolleySlot([FromBody] TrolleySlotDTO trolleySlot)
        {
            ServiceResponseModel<TrolleySlotDTO> result = await _trolleyService.DeleteTrolleySlot(trolleySlot);
            return new JsonResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> SaveRangeOfTrolleySlot([FromBody] TrolleySlotRangeDTO tsRanges)
        {
            ServiceResponseModel<TrolleySlotRangeDTO> result = await _trolleyService.SaveRangeOfTrolleySlot(tsRanges);
            return new JsonResult(result);
        }

        public async Task<IActionResult> SaveExcelTrolleySlot([FromBody] List<TrolleySlotDTO> trolleySlot)
        {
            ServiceResponseModel<List<TrolleySlotDTO>> result = await _trolleyService.SaveExcelTrolleySlot(trolleySlot);
            return new JsonResult(result);
        }

        public async Task<IActionResult> UpdateExcelTSPulses([FromBody] List<TrolleySlotDTO> tsPulses)
        {
            ServiceResponseModel<List<TrolleySlotDTO>> result = await _trolleyService.UpdateExcelTSPulses(tsPulses);
            return new JsonResult(result);
        }

        public IActionResult TrolleyUpdatePulse()
        {
            ViewBag.xToken = "";
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
                ViewBag.xToken = data.Token;
            }

            ViewData["ActiveGroup"] = "grpMM";
            ViewData["ActiveTab"] = "TrolleyUpdatePulse";
            ViewData["Title"] = "Trolley Update Pulses";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<TrolleySlotDTO>>> GetTrolleySlotStatus_BySide(string side)
        {
            ServiceResponseModel<List<TrolleySlotDTO>> result = await _trolleyService.GetTrolleySlotList();
            List<TrolleySlotDTO> data = new List<TrolleySlotDTO>();
            result.errMessage = "0,0,0,0,0,0";
            if (result.success)
            {
                var isLeft = side == "A";
                data = result.data.Where(x => x.IsLeft == isLeft).ToList().OrderByDescending(x => x.RowNo).ToList();
                result.data = data;

                int ttlIn = result.data.Where(x => x.IsLeft == isLeft && x.IsActive == false).Count();
                int ttlC = result.data.Where(x => x.IsLeft == isLeft && x.NeedCheck == true).Count();
                int ttlET = 0; // result.data.Where(x => x.IsLeft == isLeft && x.HasEmptyTray == true).Count();
                int ttlTR = result.data.Where(x => x.IsLeft == isLeft && x.HasReel && x.ReelNo == "0").Count();
                int ttlTRN = result.data.Where(x => x.IsLeft == isLeft && x.HasReel && x.ReelNo != "0").Count();
                int ttlA = result.data.Where(x => x.IsLeft == isLeft && x.HasReel == false && x.IsActive == true && x.NeedCheck == false).Count();

                result.errMessage = ttlIn + "," + ttlC + "," + ttlET + "," + ttlTR + "," + ttlTRN + "," + ttlA;

            }

            return result;
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<TrolleySlotDTO>>> GetTrolleySlotStatus(string side)
        {
            ServiceResponseModel<List<TrolleySlotDTO>> result = await _trolleyService.GetTrolleySlotList();
            List<TrolleySlotDTO> data = new List<TrolleySlotDTO>();
            result.errMessage = "0,0,0,0,0,0";
            if (result.success)
            {
                int ttlIn = result.data.Where(x => x.IsActive == false).Count();
                int ttlC = result.data.Where(x => x.NeedCheck == true).Count();
                int ttlET = 0; // result.data.Where(x => x.IsLeft == isLeft && x.HasEmptyTray == true).Count();
                int ttlTR = result.data.Where(x => x.HasReel && x.ReelNo == "0").Count();
                int ttlTRN = result.data.Where(x => x.HasReel && x.ReelNo != "0").Count();
                int ttlA = result.data.Where(x => x.HasReel == false && x.IsActive == true && x.NeedCheck == false).Count();

                result.errMessage = ttlIn + "," + ttlC + "," + ttlET + "," + ttlTR + "," + ttlTRN + "," + ttlA;

                var isLeft = side == "A";
                data = result.data.Where(x => x.IsLeft == isLeft).ToList().OrderByDescending(x => x.RowNo).ToList();
                result.data = data;

            }

            return result;
        }

        public IActionResult TrolleyColumnStatus()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpRACKING";
            ViewData["ActiveTab"] = "TrolleyColumnStatus";
            ViewData["Title"] = "Trolley Column Status";
            return View();
        }

        public IActionResult TrolleyDetailList()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpRACKING";
            ViewData["ActiveTab"] = "TrolleyDetailList";
            ViewData["Title"] = "Trolley Detail List";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<TrolleyReelDtlDTO>>> GetTrolleyDetailList(long id)
        {
            ServiceResponseModel<List<TrolleyReelDtlDTO>> result = await _trolleyService.GetTrolleyReelDtlList(id);
            result.errMessage = "-,0,0,0,0,0";
            if (result.success)
            {
                //string grpInfo = "";
                //var grps = result.data.GroupBy(x => x.ItemGroupCode);
                //foreach (var igrp in grps)
                //{
                //    grpInfo += igrp.Key;
                //}
                int ttlC = result.data.Where(x => x.NeedCheck == true).Count();
                int ttlTRAC1 = result.data.Where(x => x.IsLeft && x.IsActive == true && x.ColNo == 1).Count();
                int ttlTRAC2 = result.data.Where(x => x.IsLeft && x.IsActive == true && x.ColNo == 2).Count();
                int ttlTRAC3 = result.data.Where(x => x.IsLeft && x.IsActive == true && x.ColNo == 3).Count();
                int ttlTRAC1R = result.data.Where(x => x.IsLeft && x.HasReel && x.ReelNo == "0" && x.ColNo == 1).Count();
                int ttlTRAC2R = result.data.Where(x => x.IsLeft && x.HasReel && x.ReelNo == "0" && x.ColNo == 2).Count();
                int ttlTRAC3R = result.data.Where(x => x.IsLeft && x.HasReel && x.ReelNo == "0" && x.ColNo == 3).Count();
                int ttlEAC1 = result.data.Where(x => x.IsLeft && x.HasReel == false && x.IsActive == true && x.NeedCheck == false && x.ColNo == 1).Count();
                int ttlEAC2 = result.data.Where(x => x.IsLeft && x.HasReel == false && x.IsActive == true && x.NeedCheck == false && x.ColNo == 2).Count();
                int ttlEAC3 = result.data.Where(x => x.IsLeft && x.HasReel == false && x.IsActive == true && x.NeedCheck == false && x.ColNo == 3).Count();

                int ttlTRAB1 = result.data.Where(x => !x.IsLeft && x.IsActive == true && x.ColNo == 1).Count();
                int ttlTRAB2 = result.data.Where(x => !x.IsLeft && x.IsActive == true && x.ColNo == 2).Count();
                int ttlTRAB3 = result.data.Where(x => !x.IsLeft && x.IsActive == true && x.ColNo == 3).Count();
                int ttlTRBC1R = result.data.Where(x => !x.IsLeft && x.HasReel && x.ReelNo == "0" && x.ColNo == 1).Count();
                int ttlTRBC2R = result.data.Where(x => !x.IsLeft && x.HasReel && x.ReelNo == "0" && x.ColNo == 2).Count();
                int ttlTRBC3R = result.data.Where(x => !x.IsLeft && x.HasReel && x.ReelNo == "0" && x.ColNo == 3).Count();
                int ttlEBC1 = result.data.Where(x => !x.IsLeft && x.HasReel == false && x.IsActive == true && x.NeedCheck == false && x.ColNo == 1).Count();
                int ttlEBC2 = result.data.Where(x => !x.IsLeft && x.HasReel == false && x.IsActive == true && x.NeedCheck == false && x.ColNo == 2).Count();
                int ttlEBC3 = result.data.Where(x => !x.IsLeft && x.HasReel == false && x.IsActive == true && x.NeedCheck == false && x.ColNo == 3).Count();

                var colSummary = new[]
                {
                    new
                    {
                        Side = "A",
                        Column = "1",
                        TotalReels = ttlTRAC1R,
                        Balanced = ttlEAC1,
                        Used = ttlTRAC1 - ttlEAC1,
                        Total = ttlTRAC1
                    },
                    new
                    {
                        Side = "A",
                        Column = "2",
                        TotalReels = ttlTRAC2R,
                        Balanced = ttlEAC2,
                        Used = ttlTRAC2 - ttlEAC2,
                        Total = ttlTRAC2
                    },
                    new
                    {
                        Side = "A",
                        Column = "3",
                        TotalReels = ttlTRAC3R,
                        Balanced = ttlEAC3,
                        Used = ttlTRAC3 - ttlEAC3,
                        Total = ttlTRAC3
                    },
                    new
                    {
                        Side = "B",
                        Column = "1",
                        TotalReels = ttlTRBC1R,
                        Balanced = ttlEBC1,
                        Used = ttlTRAB1 - ttlEBC1,
                        Total = ttlTRAB1
                    },
                    new
                    {
                        Side = "B",
                        Column = "2",
                        TotalReels = ttlTRBC2R,
                        Balanced = ttlEBC2,
                        Used = ttlTRAB2 - ttlEBC2,
                        Total = ttlTRAB2
                    },
                    new
                    {
                        Side = "B",
                        Column = "3",
                        TotalReels = ttlTRBC3R,
                        Balanced = ttlEBC3,
                        Used = ttlTRAB3 - ttlEBC3,
                        Total = ttlTRAB3
                    }
                };

                result.errList = colSummary.Select(summary =>
                   $"{summary.Side},{summary.Column},{summary.TotalReels},{summary.Balanced},{summary.Used},{summary.Total}"
                ).ToList();

                result.errStackTrace = ttlC.ToString();
                result.data = result.data.Where(x => x.HasReel && x.ReelNo == "0").ToList();
            }

            return result;
        }

        //[HttpGet]
        //public async Task<ServiceResponseModel<List<>>> GetTrolleyColumn()
        //{

        //}

    }
}

