using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RackingSystem.Data;
using RackingSystem.Models;
using RackingSystem.Models.GRN;
using RackingSystem.Models.Reel;
using RackingSystem.Models.User;
using RackingSystem.Services.ReelServices;

namespace RackingSystem.Controllers
{
    [Authorize(AuthenticationSchemes = "MyAuthCookie")]
    public class ReelController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IReelService _reelService;

        public ReelController(AppDbContext context, IReelService reelService)
        {
            _context = context;
            _reelService = reelService;
        }

        public IActionResult AvailableReelList()
        {
            ViewBag.PermissionList = new List<int>();
            //string s = HttpContext.Session.GetString("xSession") ?? "";
            //if (s != "")
            //{
            //    UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
            //    ViewBag.PermissionList = data.UACIdList;
            //}
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var uacClaim = User.FindFirst("UACIdList")?.Value;

                if (uacClaim != null)
                {
                    List<int> uacIdList = uacClaim.Split(',').Select(int.Parse).ToList();
                    ViewBag.PermissionList = uacIdList;
                }
            }

            ViewData["ActiveGroup"] = "grpRACKING";
            ViewData["ActiveTab"] = "AvailableReelList";
            ViewData["Title"] = "Available Reel List";
            return View();
        }

        [HttpPost]
        public async Task<ServiceResponseModel<ReelAvailableResponseDTO>> GetAvailableReelList([FromBody] ReelAvailableSearchReqDTO req)
        {
            if (req == null)
            {
                ServiceResponseModel<ReelAvailableResponseDTO> rErr = new ServiceResponseModel<ReelAvailableResponseDTO>();
                rErr.errMessage = "Empty parameter.";
                return rErr;
            }
            int ttl = -1;
            int ttlW = 0;
            int ttlL = 0;
            int ttlSRMS = 0;
            int ttlT = 0;
            if (req.page == 1)
            {
                ServiceResponseModel<ReelAvailableListDTO> rTotal = await _reelService.GetAvailableReelTotalCount(req);
                if (rTotal.success)
                {
                    ttl = rTotal.data.totalRecord;
                    ttlW = rTotal.data.TotalWaiting;
                    ttlL = rTotal.data.TotalInLoader;
                    ttlSRMS = rTotal.data.TotalSRMS;
                    ttlT = rTotal.data.TotalInTrolley;
                }
                else
                {
                    ServiceResponseModel<ReelAvailableResponseDTO> rErr = new ServiceResponseModel<ReelAvailableResponseDTO>();
                    rErr.errMessage = rTotal.errMessage;
                    rErr.errStackTrace = rTotal.errStackTrace;
                    return rErr;
                }
            }

            ServiceResponseModel<List<ReelAvailableListDTO>> r = await _reelService.GetAvailableReelList(req);
            ServiceResponseModel<ReelAvailableResponseDTO> result = new ServiceResponseModel<ReelAvailableResponseDTO>();
            result.success = true;
            result.totalRecords = ttl;
            result.data = new ReelAvailableResponseDTO();
            result.data.ReelList = r.data;
            result.data.TotalWaiting = ttlW;
            result.data.TotalInLoader = ttlL;
            result.data.TotalSRMS = ttlSRMS;
            result.data.TotalInTrolley = ttlT;

            return result;
        }

        public IActionResult StockAgingReport()
        {
            ViewBag.PermissionList = new List<int>();
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var uacClaim = User.FindFirst("UACIdList")?.Value;
                if (uacClaim != null)
                {
                    List<int> uacIdList = uacClaim.Split(',').Select(int.Parse).ToList();
                    ViewBag.PermissionList = uacIdList;
                }
            }

            ViewData["ActiveGroup"] = "grpRACKING";
            ViewData["ActiveTab"] = "StockAgingReport";
            ViewData["Title"] = "Stock Aging Report";
            return View();
        }

        [HttpPost]
        public async Task<ServiceResponseModel<StockAgingResDTO>> GetStockAgingReport([FromBody] StockAgingReqDTO req)
        {
            if (req == null)
            {
                req = new StockAgingReqDTO();
            }

            ServiceResponseModel<StockAgingResDTO> result = await _reelService.GetExpiredStockAging(req);
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> ExportStockAgingReport([FromBody] StockAgingReqDTO req)
        {
            try
            {
                if (req == null)
                {
                    req = new StockAgingReqDTO();
                }

                // Set large page size to get all data
                req.pageSize = 100000;
                req.page = 1;

                ServiceResponseModel<StockAgingResDTO> result = await _reelService.GetExpiredStockAging(req);

                if (!result.success || result.data == null || result.data.Items.Count == 0)
                {
                    return BadRequest("No data to export");
                }

                var data = result.data;

                // Create Excel file using ClosedXML
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Stock Aging Report");

                    // Add Headers
                    worksheet.Cell(1, 1).Value = "Item Code";
                    worksheet.Cell(1, 2).Value = "Item Description";
                    worksheet.Cell(1, 3).Value = "Item Desc 2";
                    worksheet.Cell(1, 4).Value = "Item Group";
                    worksheet.Cell(1, 5).Value = "Reel Code";
                    worksheet.Cell(1, 6).Value = "Slot Code";
                    worksheet.Cell(1, 7).Value = "Expiry Date";
                    worksheet.Cell(1, 8).Value = "Days Expired";
                    worksheet.Cell(1, 9).Value = "Quantity";
                    worksheet.Cell(1, 10).Value = "Status";

                    // Format header row
                    var headerRow = worksheet.Row(1);
                    headerRow.Style.Font.Bold = true;
                    headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Add data rows
                    int rowNum = 2;
                    decimal totalQty = 0;

                    foreach (var item in data.Items)
                    {
                        worksheet.Cell(rowNum, 1).Value = item.ItemCode;
                        worksheet.Cell(rowNum, 2).Value = item.ItemDescription;
                        worksheet.Cell(rowNum, 3).Value = item.ItemDesc2;
                        worksheet.Cell(rowNum, 4).Value = item.ItemGroupName;
                        worksheet.Cell(rowNum, 5).Value = item.ReelCode;
                        worksheet.Cell(rowNum, 6).Value = item.SlotCode;
                        worksheet.Cell(rowNum, 7).Value = item.ExpiryDate.ToString("yyyy-MM-dd");
                        worksheet.Cell(rowNum, 8).Value = item.DaysExpired;
                        worksheet.Cell(rowNum, 9).Value = item.Qty;
                        worksheet.Cell(rowNum, 10).Value = item.Status;

                        totalQty += item.Qty;
                        rowNum++;
                    }

                    // Add summary row
                    worksheet.Cell(rowNum + 1, 9).Value = "TOTAL:";
                    worksheet.Cell(rowNum + 1, 9).Style.Font.Bold = true;
                    worksheet.Cell(rowNum + 1, 9).Value = totalQty;
                    worksheet.Cell(rowNum + 1, 9).Style.Font.Bold = true;

                    // Add report filter
                    if (req.ToExpiryDate.HasValue)
                    {
                        worksheet.Cell(rowNum + 3, 2).Value = $"Filter by : To Expiry Date = {req.ToExpiryDate.Value:yyyy-MM-dd}";
                    }
                    if (req.FilterExpired != null)
                    {
                        worksheet.Cell(rowNum + 4, 2).Value = "Filter by : " + (req.FilterExpired == true ? "Expired Only" : "Non-Expired Only");
                    }
                    if (req.FilterInSRMS != null)
                    {
                        worksheet.Cell(rowNum + 5, 2).Value = "Filter by : " + (req.FilterInSRMS == true ? "In SRMS Only" : "Not In SRMS Only");
                    }

                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();

                    // Generate file
                    using (var stream = new System.IO.MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();

                        // Return as download
                        var fileName = $"StockAgingReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error exporting data: {ex.Message}");
            }
        }
    }
}
