using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DShop2024.EnumData;
using Microsoft.AspNetCore.Authorization;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = RoleName.Administrator)]
	public class PromotionController : Controller
    {
        private readonly DShopContext _context;

        public PromotionController(DShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Promotions.Where(p => p.Status != 0).ToListAsync());
        }
        
    }
}
