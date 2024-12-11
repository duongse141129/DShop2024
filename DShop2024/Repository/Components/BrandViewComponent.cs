using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Repository.Components
{
	public class BrandViewComponent : ViewComponent
	{
		private readonly DShopContext _dataContext;

		public BrandViewComponent(DShopContext dataContext)
		{
			_dataContext = dataContext;
		}
		public async Task<IViewComponentResult> InvokeAsync()
		{
			var brands = await _dataContext.Brands
								.Where(p => p.Status == 1)
								.ToListAsync();

			return View(brands);

		}
	}
}
