using DShop2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Repository.Components
{
	public class BestSellerViewComponent : ViewComponent
	{
		private readonly DShopContext _context;

		public BestSellerViewComponent(DShopContext context)
		{
			_context = context;
		}
		public async Task<IViewComponentResult> InvokeAsync()
		{
            var now = DateTime.Now;
            var bestSaleProducts = await _context.Products
                .Where(p => p.Status != 0)
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    ProductName = p.ProductName,
                    MainImage = p.MainImage,
                    Price = p.Price,
                    OriginalPrice = p.OriginalPrice,
                    Stock = p.Stock,
                    BrandName = p.Brand.BrandName,
                    CategoryName = p.Category.CategoryName,

                    QuantitySold = p.OrderDetails.Where(od => od.Order.Status != 0).Sum(od => (int?)od.Quantity) ?? 0,
                    AveragePoint = p.Ratings.Where(r => r.Status != 0).Average(r => (double?)r.Star) ?? 0,

                    SalePrice = p.Sales
                                    .Where(s => s.Status != 0
                                             && now >= s.SaleStartDate
                                             && now <= s.SaleEndDate)
                                    .Select(s => (decimal?)s.SalePrice)
                        .FirstOrDefault(),
                    IsOnSale = p.Sales.Any(s =>
                                        s.Status != 0 &&
                                        now >= s.SaleStartDate &&
                                        now <= s.SaleEndDate)
                })
                .Where(p => p.QuantitySold > 0)
                .OrderByDescending(p => p.QuantitySold)
                .Take(8)
                .ToListAsync();
            return View(bestSaleProducts);
        }
	}
}
