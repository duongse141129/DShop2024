using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Repository.Components
{
    public class ContactViewComponent : ViewComponent
    {
        private readonly DShopContext _context;

        public ContactViewComponent(DShopContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var contacts = await _context.Contacts.Where(c => c.Status == 1).Include(u => u.User).OrderByDescending(d => d.DateSent).ToListAsync();
            return View(contacts);
        }
    }
}
