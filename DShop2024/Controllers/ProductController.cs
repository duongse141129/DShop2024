using DShop2024.Models;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Controllers
{
	public class ProductController : Controller
	{
		private readonly DShopContext _dataContext;

		public ProductController(DShopContext context)
		{
			_dataContext = context;
		}
		public IActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> Details(int Id)
		{
			if(Id == null) return RedirectToAction("Index");
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

			var viewModel = new ProductDetailViewModel
			{
				ProductDetail = productById,
				Rating = productById.Rating
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
				var ratingModel = new RatingModel
				{
					ProductId = rating.ProductId,
					Comment = rating.Comment,
					RatingDateTime = DateTime.Now,
					Star = rating.Star
				};
				_dataContext.Ratings.Add(ratingModel);
				await _dataContext.SaveChangesAsync();

				TempData["success"] = "Feedback product successful";
				return RedirectToAction(Request.Headers["Referer"]);
			}
			return RedirectToAction("Detail", new {Id = rating.ProductId});
		}


	}
}
