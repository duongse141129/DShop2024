using DShop2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Repository.Components
{
	public class BrandViewComponent : ViewComponent
	{
		private readonly DShopContext _context;

		public BrandViewComponent(DShopContext context)
		{
			_context = context;
		}
		public async Task<IViewComponentResult> InvokeAsync()
		{
			var brands = await _context.Brands
								.Where(b => b.Status != 0)
								.Join(_context.Products.Where(p => p.Status != 0),
								b => b.Id,
								p => p.BrandId,
								(b, p) => new { b, p })
								.GroupBy(x => new
								{
									x.b.Slug,
									x.b.BrandName
								})
								.Select( g => new BrandViewModel
								{
									Slug = g.Key.Slug, 
									BrandName = g.Key.BrandName,
									CountProduct = g.Count()
								})
								.ToListAsync();

			return View(brands);

		}
	}
}
