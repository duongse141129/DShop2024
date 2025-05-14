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
            var shippingList = await _context.Shippings.ToListAsync();
            ViewBag.Shippings = shippingList;
			return View();
		}

        [HttpPost]
        [Route("StoreShipping")]
        public async Task<IActionResult> StoreShipping(ShippingModel shippingModel,string tinh, decimal price)
        {
            shippingModel.Price = price;       
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
            catch (Exception ex)
            {

                return Ok(new { success = false, message = "Add shipping fail "+ex.Message });
            }
        }

        [Route("Delete")]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }

            ShippingModel shipping = await _context.Shippings.FindAsync(Id);
            if (shipping == null)
            {
                return NotFound();
            }
            try
            {
                _context.Shippings.Remove(shipping);
                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Delete Shipping successful";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Delete Shipping fail "+ex.Message;
                return RedirectToAction("Index");
            }

        }
	}
}
