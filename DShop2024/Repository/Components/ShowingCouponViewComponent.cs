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
            var today = DateTime.Today;
            var coupons = await _context.Coupons
             .Where(p => p.Status == (int)CouponEnumData.StatusCoupon.Showing && p.Quantity > 0 && p.DateStart.Date <= today && today <= p.DateExpired.Date)
             .Include(p => p.Promotion).Where(p => p.Promotion.CategoryCouponName != DShopConst.NEW_CUSTOMER)
             .OrderBy(p => p.PromotionId)
             .ToListAsync();
            return View(coupons);

        }
    }
}
