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
            var now = DateTime.Now;
            var bestRatingProducts = await _context.Products.AsNoTracking()
                                        .Where(p => p.Status != 0)
                                        .Select(p => new
                                        {
                                            Product = p,
                                            RatingCount = p.Ratings.Count(r => r.Status != 0),
                                            AveragePoint = p.Ratings
                                        .Where(r => r.Status != 0)
                                        .Average(r => (double?)r.Star) ?? 0
                                        })
                                        .Where(x => x.RatingCount > 10)
                                        .OrderByDescending(x => x.AveragePoint)
                                        .Take(8)
                                        .Select(x => new ProductViewModel
                                        {
                                            Id = x.Product.Id,
                                            ProductName = x.Product.ProductName,
                                            MainImage = x.Product.MainImage,
                                            Price = x.Product.Price,
                                            OriginalPrice = x.Product.OriginalPrice,
                                            Stock = x.Product.Stock,
                                            BrandName = x.Product.Brand.BrandName,
                                            CategoryName = x.Product.Category.CategoryName,

                                            AveragePoint = x.AveragePoint,

                                            IsOnSale = x.Product.Sales.Any(s => s.Status != 0 &&
                                                                                    now >= s.SaleStartDate &&
                                                                                    now <= s.SaleEndDate),
                                                                                    SalePrice = x.Product.Sales
                                                                                    .Where(s => s.Status != 0 &&
                                                                                            now >= s.SaleStartDate &&
                                                                                            now <= s.SaleEndDate)
                                                                                            .Select(s => (decimal?)s.SalePrice)
                                                                                            .FirstOrDefault()
                                        })
                                        .ToListAsync();

            return View(bestRatingProducts);



        }
    }
}
