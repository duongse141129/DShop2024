using DShop2024.EnumData;
using DShop2024.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleName.Administrator)]
    [SidebarMenu(Menu.Admin.UserManagement, SubMenu.UserManagement.Role)]
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
                if (String.IsNullOrEmpty(roleModel.Name))
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "The role name field is required.";
                    return View();
                }

                if (!_roleManager.RoleExistsAsync(roleModel.Name).GetAwaiter().GetResult())
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(roleModel.Name));
                    if (!result.Succeeded)
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = "Create role fail.";
                        return RedirectToAction("Create");
                    }
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Create successful";
                    return RedirectToAction("Index");
                }
                TempData[DShopConst.TEMPDATA_ERROR] = "Create role fail. Role already exists ";
                return RedirectToAction("Create");
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
            if (role.Name == RoleName.Administrator)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Can not delete role Admin ";
                return RedirectToAction("Index");
            }

            try
            {
                await _roleManager.DeleteAsync(role);
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Role delete successful";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Role delete fail"+ ex.Message;              
            }
            return Redirect("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string Id)
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
            if (role.Name == RoleName.Administrator)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Can not modify role Admin ";
                return RedirectToAction("Index");
            }
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
            var role = await _roleManager.FindByIdAsync(Id);
            if (role == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                role.Name = model.Name;
                try
                {
                    var exitRole = await _roleManager.RoleExistsAsync(model.Name);
                    if (exitRole)
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = "Role update fail. Role already exists ";
                        return RedirectToAction("Edit", new { Id = Id });
                    }

                    await _roleManager.UpdateAsync(role);
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Role update successful";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "Role update fail "+ ex.Message;
                    return RedirectToAction("Edit", new { Id = Id});
                }
            }
            return View(model ?? new IdentityRole { Id = Id});
        }

    }
}
