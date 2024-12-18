﻿using DShop2024.Models;
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
        public async Task<IActionResult> Index()
        {
            return View(await _userManager.Users.OrderByDescending(u => u.Id).ToListAsync());
        }

		[HttpGet]
		public async Task<IActionResult> Create()
		{
			var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            var user = new AppUserModel();
            return View(user);
		}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppUserModel user)
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            if (ModelState.IsValid)
            {
                var createUserResult = await _userManager.CreateAsync(user,user.PasswordHash);
                if(createUserResult.Succeeded)
                {
                    TempData["success"] = "Create user successful";
                    return RedirectToAction("Index", "User");
                }
                return View(new AppUserModel());

            }
            else
            {
                TempData["error"] = "Model isn't valid";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                    string errorMessage = string.Join("\n", errors);
                    return BadRequest(errorMessage);
                }
            }

            return View(new AppUserModel());
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
            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                return View("Error");
            }
            TempData["success"] = "Delete successful";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string Id)
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
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string Id,AppUserModel user)
        {
            var exitingUser = await _userManager.FindByIdAsync(Id);
            if(exitingUser == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                exitingUser.UserName = user.UserName;
                exitingUser.PhoneNumber = user.PhoneNumber;
                exitingUser.EmailConfirmed = user.EmailConfirmed;
                exitingUser.RoleId = user.RoleId;


                var updateUserResult = await _userManager.UpdateAsync(exitingUser);
                if (updateUserResult.Succeeded)
                {
                    TempData["success"] = "Create user successful";
                    return RedirectToAction("Index", "User");
                }
                AddIdentityErrors(updateUserResult);
                return View(exitingUser);

            }
            return View();
        }

        private void AddIdentityErrors(IdentityResult result)
        {
            foreach(var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }


    }
}
