using DShop2024.Models;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace DShop2024.Controllers
{
	public class ProductController : Controller
	{
		private readonly DShopContext _dataContext;
		private readonly UserManager<AppUserModel> _userManager;

		public ProductController(DShopContext context, UserManager<AppUserModel> userManager)
		{
			_dataContext = context;
			_userManager = userManager;
		}
		public IActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> Details(int? Id)
		{
			if (Id == null)
			{
				return NotFound();
			}

			var productById = await _dataContext.Products
										.Where(p => p.Id == Id)
										.Where(p => p.Status == 1)
										.Include(p => p.Brand)
										.Include(p => p.Category)
										.Include(p => p.Rating)
										.ThenInclude(p => p.User)
										.FirstOrDefaultAsync();

			var relatedProducts = await _dataContext.Products
									.Where(p => p.Category.Id == productById.CategoryId && p.Id != productById.Id)
									.Take(3)
									.ToListAsync();
			ViewBag.relatedProducts = relatedProducts;


			// kiếm template
			//var rvproduct = Request.Cookies["RecentlyViewedProducts"];
			//List<ProductModel> recentlyViewedProducts;
			//if(rvproduct == null)
			//{
			//	recentlyViewedProducts = new List<ProductModel>();
			//}
			//else
			//{
			//	recentlyViewedProducts = JsonConvert.DeserializeObject<List<ProductModel>>(rvproduct);
			//}
			
			//if (!recentlyViewedProducts.Contains(productById))
			//{
			//	recentlyViewedProducts.Add(productById);
			//}		
			//var recentProducts = JsonConvert.SerializeObject(recentlyViewedProducts.Take(10));
			//var cookieOptionss = new CookieOptions
			//{
			//	HttpOnly = true,
			//	Expires = DateTime.UtcNow.AddMinutes(30),
			//	Secure = true,
			//	SameSite = SameSiteMode.Strict,
			//};
			//Response.Cookies.Append("RecentlyViewedProducts", recentProducts, cookieOptionss);
			//ViewBag.recentlyViewedProducts = recentlyViewedProducts;


			var user = await _userManager.GetUserAsync(this.User);
			var listRating = await _dataContext.Ratings
									.Where(p => p.ProductId == Id)
									.Where(r => r.Status == 1)
									.Include(c => c.User)
									.ToListAsync();

			var pointAvarge = 0.0;
			if(listRating.Count >0)
			{
                pointAvarge = listRating.Average(p => p.Star);

            }

			RatingModel feedback = listRating.Where(u => u.UserId == user.Id).FirstOrDefault();

			listRating.Remove(feedback);

			bool checkUserOrder = false;
			var checkOrder = await (from o in _dataContext.Orders
											join od in _dataContext.OrderDetails on o.Id equals od.OrderId
											where o.UserId == user.Id && od.ProductId == productById.Id && o.Status == 4
											select o).FirstOrDefaultAsync();
			if(checkOrder != null)
			{
				checkUserOrder = true;
			}	

			var viewModel = new ProductDetailViewModel
			{
				ProductDetail = productById,
				Point = pointAvarge,
				listRating = listRating,
				IsOrder = checkUserOrder,
				Feedback = feedback
			};

			return View(viewModel);
		}

		public async Task<IActionResult> Search(string searchTerm)
		{
			var products = await _dataContext.Products
										.Where(p => p.ProductName.Contains(searchTerm) || p.Description.Contains(searchTerm))
										.Where(p => p.Status == 1)
										.Include(p => p.Brand)
										.Include(p => p.Category)
										.ToListAsync();
			ViewBag.SearchTerm = searchTerm;

			return View(products);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CommentProduct(RatingModel rating)
		{
			if(ModelState.IsValid)
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
				_dataContext.Ratings.Add(ratingModel);
				await _dataContext.SaveChangesAsync();

				TempData["success"] = "Feedback product successfully";
				return RedirectToAction("Details", new { Id = rating.ProductId });
			}

			return RedirectToAction("Details", new { Id = rating.ProductId });

		}


	}
}
