using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Repository.Components
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly DShopContext _context;

        public FooterViewComponent(DShopContext context)
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
