using DShop2024.EnumData;
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
                                .OrderByDescending(c => c.Id)
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
			ViewBag.listPromotion = new SelectList(_context.Promotions.Where(b => b.Status != 0), "Id", "CategoryCouponName");

			if (ModelState.IsValid)
            {
				try
				{
                    var promotion = await _context.Promotions.FindAsync(couponModel.PromotionId);

                    if(couponModel.DateExpired < couponModel.DateStart)
                    {
                        TempData["error"] = "DateExpired must >= date start";
                        return RedirectToAction(nameof(Create));
                    } 
                    if(couponModel.DateStart < DateTime.Now.Date)
                    {
                        TempData["error"] = "Cannot choose date in the past";
                        return RedirectToAction(nameof(Create));
                    }

                    if(promotion.CategoryCouponName.Equals(DShopConst.SUB_SUMTOTAL_DISCOUNT))
                    { 
                        if(couponModel.Value < 1000)
                        {
							TempData["error"] = $"{promotion.CategoryCouponName} must >= 1000";
							return RedirectToAction(nameof(Create));
						}						
					}   
                    if(promotion.CategoryCouponName.Equals(DShopConst.PERCENTAGE_DISCOUNT))
                    {
                        if(couponModel.Value >100 || couponModel.Value <1)
                        {
							TempData["error"] = $"{promotion.CategoryCouponName} must from 1% to 100%";
							return RedirectToAction(nameof(Create));
						}
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
                    return RedirectToAction(nameof(Create));
                }
            }
            TempData["error"] = "Check all fields";
            return RedirectToAction(nameof(Create));
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
