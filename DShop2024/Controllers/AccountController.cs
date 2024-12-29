using DShop2024.Models;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DShop2024.Controllers
{
	public class AccountController : Controller
	{
		private UserManager<AppUserModel> _userManager;
		private SignInManager<AppUserModel> _signInManager;
        private readonly DShopContext _dataContext;

        public AccountController(UserManager<AppUserModel> userManager,
								SignInManager<AppUserModel> signInManager,
								DShopContext context)
		{
			_userManager = userManager;
			_signInManager = signInManager;
            _dataContext = context;
        }

		public IActionResult Login(string returnUrl)
		{
			var loginVM = new LoginViewModel { ReturnUrl = returnUrl };
			return View(loginVM);
		}

		[HttpPost]
		public async Task<IActionResult> Login(LoginViewModel loginVM)
		{
			if (ModelState.IsValid)
			{
				Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(loginVM.UserName, loginVM.Password, false, false);
				if (result.Succeeded)
				{
					return Redirect(loginVM.ReturnUrl ?? "/");
				}
				ModelState.AddModelError("", "Invalid Username and Password");
			}
			return View(loginVM);
		}

		public IActionResult Create()
		{
			return View();
		}

        public async Task<IActionResult> History()
        {
			if ((bool)!User.Identity?.IsAuthenticated)
			{
				return RedirectToAction("Login", "Account");
			}
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var userEmail = User.FindFirstValue(ClaimTypes.Email);

			var Orders = await _dataContext.Orders
				.Where(od => od.User.Email == userEmail).OrderByDescending(od => od.Id).ToListAsync();

			ViewBag.userEmail = userEmail;
            return View(Orders);
        }

        public async Task<IActionResult> CancelOrder(int Id)
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
			try
			{
				var order = await _dataContext.Orders.FindAsync(Id);
				order.Status = -1;
				_dataContext.Orders.Update(order);
				await _dataContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				TempData["error"] = ex.Message;
                return RedirectToAction("History");
            }
			return RedirectToAction("History");
        }

        [HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(UserModel user)
		{
			if (ModelState.IsValid)
			{
				AppUserModel userModel = new AppUserModel { UserName =user.UserName, Email =user.Email };
				IdentityResult result = await _userManager.CreateAsync(userModel, user.Password);
				if (result.Succeeded)
				{
					TempData["success"] = "Register account success";
					return Redirect("/Account/Login");
				}
				foreach(IdentityError error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}

			}
			return View(user);
		}

		public async Task<IActionResult> Logout(string returnUrl = "/")
		{
			await _signInManager.SignOutAsync();	
			return Redirect(returnUrl);
		}
	}
}
