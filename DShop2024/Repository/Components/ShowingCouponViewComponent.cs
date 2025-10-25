using DShop2024.EnumData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Repository.Components
{
    public class ShowingCouponViewComponent : ViewComponent
    {
        private readonly DShopContext _context;

        public ShowingCouponViewComponent(DShopContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {

            var coupons = await _context.Coupons
             .Where(p => p.Status == (int)CouponEnumData.StatusCoupon.Showing && p.Quantity > 0 && p.DateExpired >= DateTime.Today)
             .Include(p => p.Promotion).ToListAsync();
            return View(coupons);

        }
    }
}
