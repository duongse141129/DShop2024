using DShop2024.EnumData;
using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DShop2024.Controllers
{
    [Authorize(Roles = RoleName.Customer)]
    public class RatingController : Controller
	{
		private readonly DShopContext _context;
		private readonly UserManager<AppUserModel> _userManager;
		public RatingController(DShopContext context, UserManager<AppUserModel> userManager)
		{
			_context = context;
			_userManager = userManager;
		}
		public IActionResult Index()
		{
			return View();
		}
		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CommentProduct(RatingModel rating)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.GetUserAsync(this.User);
				var ratingModel = new RatingModel
				{
					ProductId = rating.ProductId,
					Comment = rating.Comment,
					RatingDateTime = DateTime.Now,
					Star = rating.Star,
					UserId = user.Id,
					Status = 1
				};
				try
				{
					_context.Ratings.Add(ratingModel);
					await _context.SaveChangesAsync();

					TempData[DShopConst.TEMPDATA_SUCCESS] = "Feedback product successfully";
					return RedirectToAction("Details", "ShopProducts", new { Id = rating.ProductId });
				}
				catch (Exception ex)
				{
					TempData[DShopConst.TEMPDATA_ERROR] = "Feedback product fail " + ex.Message;
					return RedirectToAction("Details", "ShopProducts", new { Id = rating.ProductId });
				}

			}
			TempData[DShopConst.TEMPDATA_ERROR] = "Please fill all value ";
			return RedirectToAction("Details", "ShopProducts", new { Id = rating.ProductId });

		}
	}
}
