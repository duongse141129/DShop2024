using DShop2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Repository.Components
{
    public class RecentPostViewComponent : ViewComponent
    {
        private readonly DShopContext _context;

        public RecentPostViewComponent(DShopContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var recentPosts = await _context.Posts.Where(p => p.Status != 0)
                                                    .Include(P => P.CreateBy)
                                                    .Include(P => P.PostSubjects)
                                                    .ThenInclude(p => p.Subject)
                                                    .OrderByDescending(p => p.DateUpdated)
                                                    .Take(3)
                                                    .ToListAsync();
            return View(recentPosts);

        }
    }
}
