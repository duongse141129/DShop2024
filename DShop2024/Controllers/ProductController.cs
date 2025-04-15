using DShop2024.Models;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Drawing.Printing;
using System.Security.Cryptography;
using static DShop2024.EnumData.Product;

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

		public async Task<IActionResult> Details(int? Id, [FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 5)
		{
			try
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


				var rvproduct = Request.Cookies["RecentlyViewedProducts"];
				List<ProductModel> recentlyViewedProducts;
				if (rvproduct == null)
				{
					recentlyViewedProducts = new List<ProductModel>();
				}
				else
				{

					recentlyViewedProducts = JsonConvert.DeserializeObject<List<ProductModel>>(rvproduct);
				}


				var checkAdd = recentlyViewedProducts.Any(p => p.Id == productById.Id);
				if (!checkAdd)
				{
					recentlyViewedProducts.Add(productById);
					if (recentlyViewedProducts.Count > 3)
					{
						recentlyViewedProducts.RemoveAt(0);
					}
				}
				var recentProducts = JsonConvert.SerializeObject(recentlyViewedProducts, new JsonSerializerSettings() { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore });
				var cookieOptionss = new CookieOptions
				{
					HttpOnly = true,
					Expires = DateTime.UtcNow.AddMinutes(1),
					Secure = true,
					SameSite = SameSiteMode.Strict,
				};
				Response.Cookies.Append("RecentlyViewedProducts", recentProducts, cookieOptionss);
				ViewBag.recentlyViewedProducts = recentlyViewedProducts;


				var user = await _userManager.GetUserAsync(this.User);
				IQueryable<RatingModel> listRating = _dataContext.Ratings
										.Where(p => p.ProductId == Id)
										.Where(r => r.Status == 1)
										.Include(c => c.User);


				var pointAvarge = 0.0;
				bool checkUserOrder = false;
				RatingModel feedback = new RatingModel();
				List<RatingModel> ratings = new List<RatingModel>();

				var count = await listRating.CountAsync();
				if (count > 0)
				{
					pointAvarge = listRating.Average(p => p.Star);


					if (user != null)
					{
						feedback = listRating.Where(u => u.UserId == user.Id).FirstOrDefault();

						//listRating.Remove(feedback);

						var checkOrder = await (from o in _dataContext.Orders
												join od in _dataContext.OrderDetails on o.Id equals od.OrderId
												where o.UserId == user.Id && od.ProductId == productById.Id && o.Status == 4
												select o).FirstOrDefaultAsync();
						if (checkOrder != null)
						{
							checkUserOrder = true;
						}
					}


					int totalRating = listRating.Count();
					if (pagesSize <= 0)
						pagesSize = 5;
					int countPages = (int)Math.Ceiling((double)totalRating / 5);

					if (currentPage > countPages)
						currentPage = countPages;
					if (currentPage < 1)
						currentPage = 1;

					var pagingModel = new PagingModel()
					{
						countpages = countPages,
						currentpage = currentPage,
						generateUrl = (pageNumber) => Url.Action("Details", new
						{
							p = pageNumber,
							pagesSize = pagesSize,
							Id = Id
						})
					};

					ratings = await listRating.Where(r => r.Id != feedback.Id)
								.Skip((currentPage - 1) * pagesSize)
								.Take(pagesSize).ToListAsync();

					ViewBag.pagingModel = pagingModel;






				}



				var viewModel = new ProductDetailViewModel
				{
					ProductDetail = productById,
					Point = pointAvarge,
					listRating = ratings,
					IsOrder = checkUserOrder,
					Feedback = feedback
				};

				return View(viewModel);
			}
			catch (Exception ex)
			{

				TempData["error"] = "fail dou to " + ex.Message;
				return View();
			}

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
