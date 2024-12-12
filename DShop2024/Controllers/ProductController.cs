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
			var product = await _dataContext.Products
										.Where(p => p.Id == Id)
										.Where(p => p.Status == 1)
										.Include(p => p.Brand)
										.Include(p => p.Category)
										.FirstOrDefaultAsync();
			
			return View(product);
		}
	}
}
