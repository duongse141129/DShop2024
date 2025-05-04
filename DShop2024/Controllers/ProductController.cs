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
				if (productById == null)
				{
					return NotFound();
				}



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


				double pointAvarge = 0.0;
				bool checkUserOrder = false;
				RatingModel feedback = new RatingModel();
				List<RatingModel> ratings = new List<RatingModel>();
				bool isInWishList = false;
				bool isInCompare = false;
				bool isFeedBack = false;

				var count = await listRating.CountAsync();

				if(count > 0)
				{
					pointAvarge = Math.Round(listRating.Average(p => p.Star), 1);
				}
				
				if (user != null)
				{
					var checkOrder = await (from o in _dataContext.Orders
											join od in _dataContext.OrderDetails on o.Id equals od.OrderId
											where o.UserId == user.Id && od.ProductId == Id && o.Status == 4
											select o).FirstOrDefaultAsync();

					if (checkOrder != null)
					{
						checkUserOrder = true;
						feedback = await listRating.Where(u => u.UserId == user.Id && u.ProductId == Id).FirstOrDefaultAsync();
						if (feedback != null)
						{
							isFeedBack = true;
							listRating = listRating.Where(u => u.UserId != user.Id);
						}
					}
					isInWishList = await _dataContext.WishLists.AnyAsync(w => w.UserId == user.Id && w.ProductId == Id);
					isInCompare = await _dataContext.Compares.AnyAsync(w => w.UserId == user.Id && w.ProductId == Id);
				}
				
				if (count > 0)
				{
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
					Feedback = feedback,
					IsInCompare = isInCompare,
					IsInWishlist = isInWishList,
					IsFeedback = isFeedBack
					
				};

				return View(viewModel);
			}
			catch (Exception ex)
			{

				TempData["error"] = "fail " + ex.Message;
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
