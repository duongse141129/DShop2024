using DShop2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Repository.Components
{
    public class AvailableSaleViewComponent : ViewComponent
    {
        private readonly DShopContext _context;

        public AvailableSaleViewComponent(DShopContext context)
        {
            _context = context;

        }

        public async Task<IViewComponentResult> InvokeAsync(string uiDesignType)
        {
            var now = DateTime.Now;

            var availableSaleProducts = await _context.Products
                .Where(p => p.Status != 0 && p.Sales.Any(s => s.Status != 0 &&
                                                     now >= s.SaleStartDate &&
                                                     now <= s.SaleEndDate))
                .Select(p => new
                {
                    Product = p,
                    RatingCount = p.Ratings.Count(r => r.Status != 0),
                    AveragePoint = p.Ratings
                        .Where(r => r.Status != 0)
                        .Average(r => (double?)r.Star) ?? 0,
                    CurrentSalePrice = p.Sales
                        .Where(s => s.Status != 0 &&
                                    now >= s.SaleStartDate &&
                                    now <= s.SaleEndDate)
                        .Select(s => (decimal?)s.SalePrice)
                        .FirstOrDefault()
                })
                .OrderByDescending(x =>
                    x.CurrentSalePrice.HasValue
                        ? ((x.Product.Price - x.CurrentSalePrice.Value) / x.Product.Price)
                        : 0)
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

                    IsOnSale = true,
                    SalePrice = x.CurrentSalePrice
                })
                .ToListAsync();

            if (uiDesignType == "Details")
            {
                return View("Details", availableSaleProducts.Take(5).ToList());
            }
            return View("Default", availableSaleProducts);
        }
    }
}
