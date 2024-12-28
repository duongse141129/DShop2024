using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Shipping")]
    [Authorize(Roles = "ADMIN")]
    public class ShippingController : Controller
	{
        private readonly DShopContext _context;

        public ShippingController(DShopContext context)
        {
            _context = context;
        }

        [Route("Index")]
        public async Task<IActionResult> Index()
		{ 
            var shippingList = await _context.Shippings.Where(s => s.Status != 0).ToListAsync();
            ViewBag.Shippings = shippingList;
			return View();
		}

        [HttpPost]
        [Route("StoreShipping")]
        public async Task<IActionResult> StoreShipping(ShippingModel shippingModel,string tinh, string quan, string phuong, decimal price)
        {
            shippingModel.City = tinh;
            shippingModel.District = quan;
            shippingModel.Ward = phuong;
            shippingModel.Price = price;
            shippingModel.Status = 1;

            try
            {
                var existingShipping = await _context.Shippings.AnyAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);

                if(existingShipping)
                {
                    return Ok(new { duplicate = true, message = "Duplicate data" });
                }
                _context.Shippings.Add(shippingModel);
                await _context.SaveChangesAsync();
                return Ok(new {success = true, message = "Add shipping successful" });
            }
            catch (Exception)
            {

                return Ok(new { success = false, message = "Add shipping fail" });
            }
        }

        [Route("Delete")]
        public async Task<IActionResult> Delete(int Id)
        {
            ShippingModel shipping = await _context.Shippings.FindAsync(Id);
            _context.Shippings.Remove(shipping);    
            await _context.SaveChangesAsync();
            TempData["success"] = "Delete Shipping successful";
            return RedirectToAction("Index");
        }
	}
}
