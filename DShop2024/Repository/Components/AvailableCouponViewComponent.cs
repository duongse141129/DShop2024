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
            var coupons = await _context.Coupons
             .Where(p => p.Status != 0 && p.Quantity > 0 && p.DateExpired >= DateTime.Today && p.DateStart <= DateTime.Today)
             .Include(p => p.Promotion).ToListAsync();
            return View(coupons);

        }
    }
}
