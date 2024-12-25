using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Repository.Components
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly DShopContext _dataContext;

        public FooterViewComponent(DShopContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var contact = await _dataContext.Contacts               
                                .FirstOrDefaultAsync();

            return View(contact);

        }
    }
}
