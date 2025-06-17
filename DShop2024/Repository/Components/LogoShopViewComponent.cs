using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Repository.Components
{
    public class LogoShopViewComponent : ViewComponent
    {
        private readonly DShopContext _context;

        public LogoShopViewComponent(DShopContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var informationShop = await _context.InformationShops
                                .FirstOrDefaultAsync();

            return View(informationShop);

        }
    }
}
