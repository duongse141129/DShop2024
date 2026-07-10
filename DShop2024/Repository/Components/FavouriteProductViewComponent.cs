using DShop2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Repository.Components
{
    public class FavouriteProductViewComponent : ViewComponent
    {
        private readonly DShopContext _context;

        public FavouriteProductViewComponent(DShopContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var now = DateTime.Now;

            var topWishlistedProducts = await _context.WishLists
                .GroupBy(w => w.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Count = g.Count()
                })
                .Join(_context.Products,
                    w => w.ProductId,
                    p => p.Id,
                    (w, p) => new
                    {
                        Product = p,
                        WishlistCount = w.Count,

                        CurrentSalePrice = p.Sales
                            .Where(s => s.Status != 0
                                     && now >= s.SaleStartDate
                                     && now <= s.SaleEndDate)
                            .Select(s => (decimal?)s.SalePrice)
                            .FirstOrDefault()
                    })
                .Select(g => new ProductViewModel
                {
                    Id = g.Product.Id,
                    ProductName = g.Product.ProductName,
                    MainImage = g.Product.MainImage,
                    Price = g.Product.Price,
                    OriginalPrice = g.Product.OriginalPrice,
                    Stock = g.Product.Stock,

                    BrandName = g.Product.Brand.BrandName,
                    CategoryName = g.Product.Category.CategoryName,

                    AveragePoint = g.Product.Ratings
                        .Where(r => r.Status != 0)
                        .Average(r => (double?)r.Star) ?? 0,

                    WishlistCount = g.WishlistCount,

                    SalePrice = g.CurrentSalePrice,
                    IsOnSale = g.CurrentSalePrice != null
                })
                .OrderByDescending(g => g.WishlistCount)
                .Take(8)
                .ToListAsync();

            return View(topWishlistedProducts);

        }
    }
}
