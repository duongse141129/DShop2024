using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Repository.Components
{
	public class CategoryViewComponent : ViewComponent
	{
		private readonly DShopContext _context;

		public CategoryViewComponent(DShopContext context)
		{
			_context = context;
		}
		public async Task<IViewComponentResult> InvokeAsync()
		{
			var categories = await _context.Categories
								.Where(p => p.Status != 0)
								.ToListAsync();

			return View(categories);

		}
	}
}
