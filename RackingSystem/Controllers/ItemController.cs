using Microsoft.AspNetCore.Mvc;
using RackingSystem.Data;
using RackingSystem.Models.Slot;
using RackingSystem.Models;
using RackingSystem.Services.ItemServices;
using RackingSystem.Models.Item;
using Microsoft.CodeAnalysis.Elfie.Model.Tree;
using RackingSystem.Models.GRN;
using Newtonsoft.Json;
using RackingSystem.Models.User;

namespace RackingSystem.Controllers
{
    public class ItemController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IItemService _itemService;

        public ItemController(AppDbContext context, IItemService itemService)
        {
            _context = context;
            _itemService = itemService;
        }

        public IActionResult ItemList()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpSTOCK";
            ViewData["ActiveTab"] = "ItemList";
            ViewData["Title"] = "Item List";
            return View();
        }

        [HttpPost]
        public async Task<ServiceResponseModel<List<ItemListDTO>>> GetItemList([FromBody] ItemSearchReqDTO req)
        {
            if (req == null)
            {
                ServiceResponseModel<List<ItemListDTO>> rErr = new ServiceResponseModel<List<ItemListDTO>>();
                rErr.errMessage = "Empty parameter.";
                return rErr;
            }
            int ttl = -1;
            if (req.page == 1)
            {
                ServiceResponseModel<int> rTotal = await _itemService.GetItemTotalCount(req);
                if (rTotal.success)
                {
                    ttl = rTotal.data;
                }
                else
                {
                    ServiceResponseModel<List<ItemListDTO>> rErr = new ServiceResponseModel<List<ItemListDTO>>();
                    rErr.errMessage = rTotal.errMessage;
                    rErr.errStackTrace = rTotal.errStackTrace;
                    return rErr;
                }
            }
            ServiceResponseModel<List<ItemListDTO>> result = await _itemService.GetItemList(req);
            result.totalRecords = ttl;
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> SaveItem([FromBody] ItemDTO itemReq)
        {
            ServiceResponseModel<ItemDTO> result = await _itemService.SaveItem(itemReq);
            return new JsonResult(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteItem([FromBody] ItemDTO itemReq)
        {
            ServiceResponseModel<ItemDTO> result = await _itemService.DeleteItem(itemReq);
            return new JsonResult(result);
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<ItemListDTO>>> GetActiveItemList()
        {
            ServiceResponseModel<List<ItemListDTO>> result = await _itemService.GetActiveItemList();
            return result;
        }

        public IActionResult ItemGroupList()
        {
            ViewBag.PermissionList = new List<int>();
            string s = HttpContext.Session.GetString("xSession") ?? "";
            if (s != "")
            {
                UserSessionDTO data = JsonConvert.DeserializeObject<UserSessionDTO>(s) ?? new UserSessionDTO();
                ViewBag.PermissionList = data.UACIdList;
            }

            ViewData["ActiveGroup"] = "grpSTOCK";
            ViewData["ActiveTab"] = "ItemGroupList";
            ViewData["Title"] = "Item Group List";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<ItemGroupListDTO>>> GetItemGroupList()
        {
            ServiceResponseModel<List<ItemGroupListDTO>> result = await _itemService.GetItemGroupList();
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> SaveItemGroup([FromBody] ItemGroupDTO itemReq)
        {
            ServiceResponseModel<ItemGroupDTO> result = await _itemService.SaveItemGroup(itemReq);
            return new JsonResult(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteItemGroup([FromBody] ItemGroupDTO itemReq)
        {
            ServiceResponseModel<ItemGroupDTO> result = await _itemService.DeleteItemGroup(itemReq);
            return new JsonResult(result);
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<ItemGroupListDTO>>> GetActiveItemGroupList()
        {
            ServiceResponseModel<List<ItemGroupListDTO>> result = await _itemService.GetActiveItemGroupList();
            return result;
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> SaveExcelItem([FromBody] List<ItemExcelReqDTO> slots)
        {
            ServiceResponseModel<List<ItemExcelReqDTO>> result = await _itemService.SaveExcelItem(slots);

            return new JsonResult(result);

        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<ItemListDTO>>> GetFinishedItemList()
        {
            ServiceResponseModel<List<ItemListDTO>> result = await _itemService.GetFinishedItemList();
            return result;
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<ItemListDTO>>> GetRawItemList()
        {
            ServiceResponseModel<List<ItemListDTO>> result = await _itemService.GetRawItemList();
            return result;
        }
    }
}
