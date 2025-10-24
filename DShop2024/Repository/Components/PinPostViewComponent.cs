using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Repository.Components
{
    public class PinPostViewComponent : ViewComponent
    {
        private readonly DShopContext _context;

        public PinPostViewComponent(DShopContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var pinPosts = await _context.Posts.Where(p => p.Status != 0 && p.IsPin == true)
                                                .Include(P => P.CreateBy)
                                                .Include(P => P.PostSubjects)
                                                .ThenInclude(p => p.Subject)
                                                .ToListAsync();
            ViewBag.pinPosts = pinPosts;

            return View(pinPosts);

        }
    }
}
