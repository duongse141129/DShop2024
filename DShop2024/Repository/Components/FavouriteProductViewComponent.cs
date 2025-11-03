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
                            WishlistCount = w.Count
                        })
                        .Select(g => new ProductViewModel
                        {
                                 Id = g.Product.Id,
                                 ProductName = g.Product.ProductName,
                                 MainImage = g.Product.MainImage,
                                 Price = g.Product.Price,
                                 Stock = g.Product.Stock,
                                 BrandName = _context.Brands.FirstOrDefault(b => b.Id == g.Product.BrandId).BrandName,
                                 CategoryName = _context.Categories.FirstOrDefault(c => c.Id == g.Product.CategoryId).CategoryName,
                                 AveragePoint = g.Product.Ratings.Any() ? g.Product.Ratings.Where(r => r.ProductId == g.Product.Id && r.Status != 0).Average(r => r.Star) : 0,
                                 WishlistCount = g.WishlistCount
                        })
                    .OrderByDescending(g => g.WishlistCount)           
                    .Take(8)
                    .ToListAsync();
            return View(topWishlistedProducts);

        }
    }
}
