using AutoMapper;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Repository.Components
{
	public class BestRattingViewComponent : ViewComponent
	{
		private readonly DShopContext _context;

        public BestRattingViewComponent(DShopContext context)
		{
			_context = context;
		}
		public async Task<IViewComponentResult> InvokeAsync()
		{

			var bestRatingProducts = await _context.Products
			 .Where(p => p.Status != 0)
			 .Join(_context.Ratings.Where(r => r.Status != 0),
			 p => p.Id,
			 r => r.ProductId,
			 (p, r) => new { p, r })
			 .GroupBy(x => new
			 {
				 x.p.Id,
				 x.p.ProductName,
				 x.p.MainImage,
				 x.p.Price,
				 x.p.Stock,
				 x.p.BrandId,
				 x.p.CategoryId

			 })
			 .Select(g => new ProductViewModel
			 {
				 Id = g.Key.Id,
				 ProductName = g.Key.ProductName,
				 MainImage = g.Key.MainImage,
				 Price = g.Key.Price,
				 Stock = g.Key.Stock,
				 BrandName = _context.Brands.FirstOrDefault(b => b.Id == g.Key.BrandId).BrandName,
				 CategoryName = _context.Categories.FirstOrDefault(c => c.Id == g.Key.CategoryId).CategoryName,
				 AveragePoint = g.Average(x => x.r.Star)
			 })
			 .OrderByDescending(x => x.AveragePoint)
			 .Take(8)
			 .ToListAsync();

			return View(bestRatingProducts);

		}
	}
}
