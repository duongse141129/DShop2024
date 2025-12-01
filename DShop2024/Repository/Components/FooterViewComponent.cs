using DShop2024.ViewModels;
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
            FooterViewModel footerViewModel = new FooterViewModel(); 
            var informationShop = await _context.InformationShops               
                                .FirstOrDefaultAsync();
            footerViewModel.informationShop = informationShop;
            footerViewModel.payments = await _context.Payments.Where(s => s.Status != 0).Select( p => p.Logo).ToListAsync();
            return View(footerViewModel);

        }
    }
}
