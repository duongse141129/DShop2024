using DShop2024.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DShop2024.Controllers
{
	public class AccountController : Controller
	{
		private UserManager<AppUserModel> _userManager;
		private SignInManager<AppUserModel> _signInManager;

		public AccountController(UserManager<AppUserModel> userManager, SignInManager<AppUserModel> signInManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
		}

		public IActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> Login()
		{
			return View();
		}
		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(UserModel user)
		{
			if (ModelState.IsValid)
			{
				AppUserModel userModel = new AppUserModel { UserName =user.UserName, Email =user.Email };
				IdentityResult result = await _userManager.CreateAsync(userModel);
				if (result.Succeeded)
				{
					TempData["success"] = "Register account success";
					return Redirect("/Account");
				}
				foreach(IdentityError error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}

			}
			return View(user);
		}
	}
}
