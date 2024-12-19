using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class RoleController : Controller
    {

        private readonly DShopContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(DShopContext context, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Roles.OrderByDescending(p => p.Id).ToListAsync());
        }


        public IActionResult Create()
        {
            return View();
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] IdentityRole roleModel)
        {
            if (ModelState.IsValid)
            {
                if (!_roleManager.RoleExistsAsync(roleModel.Name).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(roleModel.Name)).GetAwaiter().GetResult();
                    TempData["success"] = "Create delete successful";
                }
                return Redirect("Index");
            }
            return View();
        }

        public async Task<IActionResult> Delete(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(Id);
            if (role == null)
            {
                return NotFound();
            }
            try
            {
                await _roleManager.DeleteAsync(role);
                TempData["success"] = "Role delete successful";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error delete");
                
            }
            return Redirect("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string Id)
        {
            if(string.IsNullOrEmpty(Id))
            {
                return NotFound();
            }
            var role = await _roleManager.FindByIdAsync(Id);
            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string Id, IdentityRole model)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return NotFound();
            }
            if(ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(Id);
                if(role == null)
                {
                    return NotFound();
                }
                role.Name = model.Name;
                try
                {
                    await _roleManager.UpdateAsync(role);
                    TempData["success"] = "Role update successful";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error update");
                }
            }
            return View(model ?? new IdentityRole { Id = Id});
        }

    }
}
