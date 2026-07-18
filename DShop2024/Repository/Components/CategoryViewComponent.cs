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
		public async Task<IViewComponentResult> InvokeAsync(string uiDesignType, string selectedCategory = "")
		{
			var categories = await _context.Categories.AsNoTracking()
                                .Where(p => p.Status != 0)
								.ToListAsync();
            if (uiDesignType == "Details")
            {
                return View("Details", categories);
            }
            ViewBag.SelectedCategory = selectedCategory;
            return View("Default", categories);

		}
	}
}
