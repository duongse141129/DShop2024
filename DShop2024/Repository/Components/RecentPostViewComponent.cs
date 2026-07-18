using AutoMapper;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Repository.Components
{
    public class RecentPostViewComponent : ViewComponent
    {
        private readonly DShopContext _context;
        private readonly IMapper _mapper;

        public RecentPostViewComponent(DShopContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var recentPosts = await _context.Posts.AsNoTracking().Where(p => p.Status != 0)
                                                    .Include(P => P.CreateBy)
                                                    .Include(P => P.PostSubjects)
                                                    .ThenInclude(p => p.Subject)
                                                    .Include(P => P.Likes)
                                                    .Include(P => P.Comments.Where(c => c.Status != 0))
                                                    .OrderByDescending(p => p.DateUpdated)
                                                    .Take(3)
                                                    .ToListAsync();
            var recentPostViewModels = _mapper.Map<List<PostViewModel>>(recentPosts);   
            return View(recentPostViewModels);

        }
    }
}
