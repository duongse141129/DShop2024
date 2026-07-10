using DShop2024.EnumData;
using DShop2024.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
    [SidebarMenu(Menu.Admin.Promotion, SubMenu.Promotion.Promotion)]
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
