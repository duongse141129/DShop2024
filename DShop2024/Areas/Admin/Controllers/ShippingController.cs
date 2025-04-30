using DShop2024.EnumData;
using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Shipping")]
    [Authorize(Roles = RoleName.Administrator)]
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
        public async Task<IActionResult> StoreShipping(ShippingModel shippingModel,string tinh, decimal price)
        {
   
            
            shippingModel.Price = price;
            shippingModel.Status = 1;         

            try
            {
                var existingShipping = await _context.Shippings.FirstOrDefaultAsync(x => x.Province == tinh );

                if(existingShipping != null)
                {
                    return Ok(new { duplicate = true, message = "Duplicate data" });
                }
                shippingModel.Province = tinh;
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
