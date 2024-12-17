using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class UserController : Controller
    {
        
		private UserManager<AppUserModel> _userManager;
		private RoleManager<IdentityRole> _roleManager;

		public UserController(UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;

		}
        [HttpGet]
        // GET: Admin/BrandManage
        public async Task<IActionResult> Index()
        {
            return View(await _userManager.Users.OrderByDescending(u => u.Id).ToListAsync());
        }

		[HttpGet]
		// GET: Admin/BrandManage
		public async Task<IActionResult> Create()
		{
			var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            var user = new AppUserModel();
            return View(user);
		}


	}
}
