using DShop2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Repository.Components
{
	public class BestRattingViewComponent : ViewComponent
	{
		private readonly DShopContext _dataContext;

		public BestRattingViewComponent(DShopContext dataContext)
		{
			_dataContext = dataContext;
		}
		public async Task<IViewComponentResult> InvokeAsync()
		{

			var bestRatingProducts = await _dataContext.Products
			 .Where(p => p.Status != 0)
			 .Join(_dataContext.Ratings.Where(r => r.Status != 0),
			 p => p.Id,
			 r => r.ProductId,
			 (p, r) => new { p, r })
			 .GroupBy(x => new
			 {
				 x.p.Id,
				 x.p.ProductName,
				 x.p.Image,
				 x.p.Price,
				 x.p.Stock,
				 x.p.BrandId,
				 x.p.CategoryId

			 })
			 .Select(g => new ProductViewModel
			 {
				 Id = g.Key.Id,
				 ProductName = g.Key.ProductName,
				 Image = g.Key.Image,
				 Price = g.Key.Price,
				 Stock = g.Key.Stock,
				 BrandName = _dataContext.Brands.FirstOrDefault(b => b.Id == g.Key.BrandId).BrandName,
				 CategoryName = _dataContext.Categories.FirstOrDefault(c => c.Id == g.Key.CategoryId).CategoryName,
				 AveragePoint = g.Average(x => x.r.Star)
			 })
			 .OrderByDescending(x => x.AveragePoint)
			 .Take(3)
			 .ToListAsync();

			return View(bestRatingProducts);

		}
	}
}
