using Microsoft.AspNetCore.Mvc;
using RackingSystem.Data;
using RackingSystem.Models.Slot;
using RackingSystem.Models;
using RackingSystem.Services.ItemServices;
using RackingSystem.Models.Item;
using Microsoft.CodeAnalysis.Elfie.Model.Tree;

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
            ViewData["Title"] = "Item List";
            return View();
        }

        [HttpGet]
        public async Task<ServiceResponseModel<List<ItemListDTO>>> GetItemList()
        {
            ServiceResponseModel<List<ItemListDTO>> result = await _itemService.GetItemList();
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

    }
}
