using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class ProductManageController : Controller
	{
		private readonly DShopContext _dataContext;

		public ProductManageController(DShopContext context)
		{
			_dataContext = context;
		}
		public async Task<IActionResult> Index()
		{
			var products = await _dataContext.Products.Where(p => p.Status ==1)
															.Include(p => p.Category)
															.Include(p => p.Brand)
															.OrderByDescending(p => p.Id)
															.ToListAsync();

			return View(products);
		}
	}
}
