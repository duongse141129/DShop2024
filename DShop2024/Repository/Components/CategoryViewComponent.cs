using DShop2024.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Repository.Components
{
	public class CategoryViewComponent : ViewComponent
	{
		private readonly DShopContext _dataContext;

		public CategoryViewComponent(DShopContext dataContext)
		{
			_dataContext = dataContext;
		}
		public async Task<IViewComponentResult> InvokeAsync()
		{
			var categories = await _dataContext.Categories
								.Where(p => p.Status == 1)
								.ToListAsync();

			return View(categories);

		}
	}
}
