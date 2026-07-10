using DShop2024.EnumData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Repository.Components
{
    public class AvailableCouponViewComponent : ViewComponent
    {
        private readonly DShopContext _context;

        public AvailableCouponViewComponent(DShopContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var today= DateTime.Today;
            var coupons = await _context.Coupons
             .Where(p => p.Status != 0 && p.Quantity > 0 && p.DateStart.Date <= today &&  today <= p.DateExpired.Date)
             .Include(p => p.Promotion).Where( p => p.Promotion.CategoryCouponName != DShopConst.NEW_CUSTOMER)
             .OrderByDescending(p => p.Id)
             .ToListAsync();
            return View(coupons);

        }
    }
}
