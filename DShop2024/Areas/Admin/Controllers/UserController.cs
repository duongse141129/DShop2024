using App.Areas.Identity.Models.UserViewModels;
using DShop2024.Areas.Admin.Models.User;
using DShop2024.EnumData;
using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = RoleName.Administrator)]
	public class UserController : Controller
    {
        
		private UserManager<AppUserModel> _userManager;
		private RoleManager<IdentityRole> _roleManager;
        private readonly DShopContext _context;

        public UserController(UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager, DShopContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;

        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //var userWithRoles = await (from u in _context.Users
            //                           join ur in _context.UserRoles on u.Id equals ur.UserId
            //                           join r in _context.Roles on ur.RoleId equals r.Id
            //                           orderby u.Status descending, r.Name                       
            //                           select new { User = u, RoleName = r.Name }).ToListAsync();
            var userWithRole = await _userManager.Users
                            .Select( u => new UserWithRoleViewModel { User = u  })
                            .ToListAsync();
            foreach (var user in userWithRole)
            {
                var roles = await _userManager.GetRolesAsync(user.User);
                user.RoleName = roles.FirstOrDefault();
            }

            var userWithRoleVM =  userWithRole.OrderByDescending(u => u.User.Status).ThenBy(u => u.RoleName);

            return View(userWithRoleVM);
		}

		[HttpGet]
		public async Task<IActionResult> Create()
		{
			var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            var user = new CreateUserRequest();
            return View(user);
		}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserRequest createUserRequest)
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            if (ModelState.IsValid)
            {
                AppUserModel user = new AppUserModel { 
                    UserName = createUserRequest.UserName,
                    Email = createUserRequest.Email                  
                };
                try
                {
                    var createUserResult = await _userManager.CreateAsync(user, createUserRequest.Password);
                    if (createUserResult.Succeeded)
                    {
                        var createUser = await _userManager.FindByEmailAsync(user.Email);
                        var role = _roleManager.FindByIdAsync(createUserRequest.RoleId);
                        var addToRoleResult = await _userManager.AddToRoleAsync(user, role.Result.Name);
                        if (!addToRoleResult.Succeeded)
                        {
                            TempData["error"] = "Add role for user fail";
                            return RedirectToAction("Index", "User");
                        }

                        TempData["success"] = "Create user successful";
                        return RedirectToAction("Index", "User");
                    }
                    TempData["error"] = "Create user fail";
                    return View(new CreateUserRequest());
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Create user fail "+ ex.Message;
                    return View(new CreateUserRequest());

                }
            }
            TempData["error"] = "Model isn't valid. Input all values";
            return View(new CreateUserRequest());
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(Id);
            if(user == null)
            {

                return NotFound(); 
            }

            var chechAdmin = await _userManager.IsInRoleAsync(user, RoleName.Administrator);
            if (chechAdmin)
            {
                TempData["error"] = "Can not delete Admin ";
                return RedirectToAction("Index");
            }

            user.Status = 0;
            var deleteResult = await _userManager.UpdateAsync(user);
            //_context.Users.Update(user);
            await _context.SaveChangesAsync();

            if (!deleteResult.Succeeded)
            {
                return View("Error");
            }
            TempData["success"] = "Delete successful";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> RecoverAccount(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {

                return NotFound();
            }
            user.Status = 1;
            var deleteResult = await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            if (!deleteResult.Succeeded)
            {
                TempData["error"] = "Recover account fail";
                return RedirectToAction("Index");
            }
            TempData["success"] = "Recover account successful";
            return RedirectToAction("Index");
        }


        private void AddIdentityErrors(IdentityResult result)
        {
            foreach(var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        [HttpGet]
        public async Task<IActionResult> SetPassword(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["error"] = "Not found user";
                return RedirectToAction("Index", "User");
            }

            var user = await _userManager.FindByIdAsync(id);

            var chechAdmin = await _userManager.IsInRoleAsync(user, RoleName.Administrator);
            if (chechAdmin)
            {
                TempData["error"] = "Can not modify Admin ";
                return RedirectToAction("Index");
            }

            ViewBag.userName = user.UserName;
            ViewBag.id = user.Id;

            if (user == null)
            {
                TempData["error"] = $"Not found user id = {id}";
                return RedirectToAction("Index", "User");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(string id, SetPasswordUserRequest model)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["error"] = "Not found user";
                return RedirectToAction("Index", "User");
            }
            var user = await _userManager.FindByIdAsync(id);
            ViewBag.userName = user.UserName;
            ViewBag.id = user.Id;
            if (user == null)
            {
                TempData["error"] = $"Not found user id = {id}";
                return RedirectToAction("Index", "User");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            await _userManager.RemovePasswordAsync(user);

            var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                TempData["error"] = $"Set password user {user.UserName} fail"+ addPasswordResult.Errors.ToString();
                return View(model);
            }
            TempData["success"] = $"Set password user {user.UserName} successful";
            return RedirectToAction("Index", "User");
        }



        [HttpGet]
        public async Task<IActionResult> SetRole(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["error"] = "Not found user";
                return RedirectToAction("Index", "User");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["error"] = $"Not found user id = {id}";
                return RedirectToAction("Index", "User");
            }

            var chechAdmin = await _userManager.IsInRoleAsync(user, RoleName.Administrator);
            if (chechAdmin)
            {
                TempData["error"] = "Can not modify Admin ";
                return RedirectToAction("Index");
            }

            var roles = await _roleManager.Roles.ToListAsync();
            var roleUser = await _userManager.GetRolesAsync(user);
            if (roleUser.Count > 0)
            {
                var idRole = roles.Where(r => r.Name == roleUser.FirstOrDefault()).FirstOrDefault();
                ViewBag.Roles = new SelectList(roles, "Id", "Name", idRole.Id);
                return View(user);
            }
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetRole(string userId, string roleId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["error"] = "Not found user";
                return RedirectToAction("Index", "User");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["error"] = $"Not found user id = {userId}";
                return RedirectToAction("Index", "User");
            }
            try
            {
                var roleUser = await _userManager.GetRolesAsync(user);
                var resultDelete = await _userManager.RemoveFromRolesAsync(user, roleUser);
                if (!resultDelete.Succeeded)
                {
                    TempData["error"] = $"RemoveFromRolesAsync user {user.UserName} fail ";
                    return RedirectToAction("SetRole", "User", new { id = userId });
                }

                var AddRoles = await _roleManager.FindByIdAsync(roleId);
                var resultAdd = await _userManager.AddToRoleAsync(user, AddRoles.Name);

                TempData["success"] = $"Set password user {user.UserName} successful";
                return RedirectToAction("SetRole", "User", new {id = userId });
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Set role user {user.UserName} fail "+ ex.Message;
                return RedirectToAction("SetRole", "User", new { id = userId });
            }
        }


    }
}
