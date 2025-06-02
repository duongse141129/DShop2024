using DShop2024.EnumData;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Repository.Components
{
    public class CouponViewComponent : ViewComponent
    {
        private readonly DShopContext _dataContext;

        public CouponViewComponent(DShopContext dataContext)
        {
            _dataContext = dataContext;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {

            var coupons = await _dataContext.Coupons
             .Where(p => p.Status == (int)CouponEnumData.StatusCoupon.Showing && p.Quantity > 0 && p.DateExpired >= DateTime.Today)
             .Include(p => p.Promotion).ToListAsync();
            return View(coupons);

        }
    }
}
