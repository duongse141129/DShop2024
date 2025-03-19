using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "ADMIN")]
	public class CouponController : Controller
	{
		private readonly DShopContext _context;

		public CouponController(DShopContext context)
		{
			_context = context;
		}
		public async Task<IActionResult> Index()
		{
            var listCoupon = await _context.Coupons
                                .Where(c => c.Status != 0)
                                .Include( c => c.Promotion)
                                .ToListAsync();
            

            ViewBag.listPromotion = new SelectList(_context.Promotions.Where(b => b.Status != 0), "Id", "CategoryCouponName");

            return View(listCoupon);
		}

		[HttpGet]
		public IActionResult Create()
		{
			ViewBag.listPromotion = new SelectList(_context.Promotions.Where(b => b.Status != 0), "Id", "CategoryCouponName");
			return View();
		}

		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( CouponModel couponModel)
        {

            if (ModelState.IsValid)
            {
				try
				{
                    if(couponModel.DateExpired < couponModel.DateStart)
                    {
                        TempData["error"] = "DateExpired must >= date start";
                        return RedirectToAction(nameof(Index));
                    } 
                    if(couponModel.DateStart < DateTime.Now.Date)
                    {
                        TempData["error"] = "Cannot choose date in the past";
                        return RedirectToAction(nameof(Index));
                    }


                    couponModel.Status = 1;
                    _context.Add(couponModel);
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Add coupon successful";
                    return RedirectToAction(nameof(Index));
                }
				catch (Exception ex)
				{
                    TempData["error"] = "Add coupon fail "+ ex.Message;
                    return RedirectToAction(nameof(Index));
                }
            }
            TempData["error"] = "Check all fields";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["error"] = "Not found ";
                return RedirectToAction(nameof(Index));
            }
            try
            {
                var couponModel = await _context.Coupons
                .FirstOrDefaultAsync(m => m.Id == id);
                couponModel.Status = 0;
                _context.Coupons.Update(couponModel);
                await _context.SaveChangesAsync();
                TempData["success"] = "Delete coupon successful " ;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["error"] = "Delete coupon fail " + ex.Message;
                return RedirectToAction(nameof(Index));
            }           
        }


    }
}
