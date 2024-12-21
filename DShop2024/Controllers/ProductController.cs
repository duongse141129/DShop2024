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
										.FirstOrDefaultAsync();

			var relatedProducts = await _dataContext.Products
									.Where(p => p.Category.Id == productById.CategoryId && p.Id != productById.Id)
									.Take(3)
									.ToListAsync();
			ViewBag.relatedProducts = relatedProducts;


			return View(productById);
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
	}
}
