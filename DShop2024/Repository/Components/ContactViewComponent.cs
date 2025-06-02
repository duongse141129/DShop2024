using DShop2024.EnumData;
using DShop2024.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Repository.Components
{
    public class ContactViewComponent : ViewComponent
    {
        private readonly DShopContext _dataContext;

        public ContactViewComponent(DShopContext dataContext)
        {
            _dataContext = dataContext;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var contacts = await _dataContext.Contacts.Where(c => c.Status == 1).Include(u => u.User).ToListAsync();
            return View(contacts);
        }
    }
}
