using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            var listCoupon = await _context.Coupons.ToListAsync();
            ViewBag.listCoupon = listCoupon;
			return View();
		}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( CouponModel couponModel)
        {

            if (ModelState.IsValid)
            {
                _context.Add(couponModel);
                await _context.SaveChangesAsync();
                TempData["success"] = "Add coupon successful";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
    }
}
