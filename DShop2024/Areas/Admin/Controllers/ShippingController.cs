using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Shipping")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
    [SidebarMenu(Menu.Admin.Shipping)]
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
            ViewBag.DefaultShipping = DShopConst.DEFAULT_SHIPPING_COST.ToString("#,##0 VND");

            return View();
		}

        [Authorize(Roles = RoleName.Administrator)]
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
                    return Ok(new { success = false, message = "This province or city already exists" });
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

        [Authorize(Roles = RoleName.Administrator)]
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
