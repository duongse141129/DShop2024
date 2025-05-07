using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DShop2024.Models;
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

        // GET: Admin/PromotionModels
        public async Task<IActionResult> Index()
        {
            return View(await _context.Promotions.Where(p => p.Status != 0).ToListAsync());
        }
        
    }
}
