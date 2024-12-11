using DShop2024.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Controllers
{
	public class BrandController : Controller
	{
		private readonly DShopContext _dataContext;

		public BrandController(DShopContext context)
		{
			_dataContext = context;
		}
		public async Task<IActionResult> Index(string slug = "")
		{
			BrandModel brand = await _dataContext.Brands
										.Where(c => c.Status == 1)
										.Where(c => c.Slug == slug)
										.FirstOrDefaultAsync();
			if (brand == null)
			{
				return RedirectToAction("Index");
			}

			var productByBrand = await _dataContext.Products.Where(p => p.BrandId == brand.Id).ToListAsync();
			productByBrand.OrderByDescending(c => c.Id);
			return View(productByBrand);

		}
	}
}
