using AutoMapper;
using DShop2024.Models;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DShop2024.Repository.Components
{
    public class RecommendProductViewComponent : ViewComponent
    {
        private readonly DShopContext _context;
        private readonly IMapper _mapper;

        public RecommendProductViewComponent(DShopContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var rvproduct = Request.Cookies["RecentlyViewedProducts"];
            List<ProductViewModel> recentlyViewedProducts;
            if (rvproduct == null)
            {
                recentlyViewedProducts = new List<ProductViewModel>();
            }
            else
            {
                recentlyViewedProducts = JsonConvert.DeserializeObject<List<ProductViewModel>>(rvproduct);
            }
            List<ProductViewModel> recomendProduct = new List<ProductViewModel>();

     
            if (recentlyViewedProducts.Count > 0)
            {
                var topBrand = recentlyViewedProducts
                                    .GroupBy(p => p.BrandId)
                                    .Select(g => new { BrandId = g.Key, Count = g.Count() })
                                    .OrderByDescending(g => g.Count)
                                    .FirstOrDefault();

                var topCategory = recentlyViewedProducts
                    .GroupBy(p => p.CategoryId)
                    .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Count)
                    .FirstOrDefault();

                var bestRatedProductByBrand = await _context.Products
                            .Where(p => p.Status != 0)
                            .Where(p => p.Brand.Id == topBrand.BrandId)
                            .OrderByDescending(p => p.Ratings.Any() ? p.Ratings.Where(r => r.ProductId == p.Id && r.Status != 0).Average(r => r.Star) : 0)
                            .Include(b => b.Brand).Include(c => c.Category).Include(c => c.Ratings)
                            .Take(8)
                            .ToListAsync();
                var bestRatedProductByCategory = await _context.Products
                       .Where(p => p.Status != 0)
                       .Where(p => p.Category.Id == topCategory.CategoryId)
                       .OrderByDescending(p => p.Ratings.Any() ? p.Ratings.Where(r => r.ProductId == p.Id && r.Status != 0).Average(r => r.Star) : 0)
                       .Include(b => b.Brand).Include(c => c.Category).Include(c => c.Ratings)
                       .Take(8)
                       .ToListAsync();

                if(topBrand.Count > 1 && topCategory.Count <= 1)
                {
                    recomendProduct = _mapper.Map<List<ProductViewModel>>(bestRatedProductByBrand);
                }
                if (topCategory.Count > 1 && topBrand.Count <= 1 )
                {
                    recomendProduct = _mapper.Map<List<ProductViewModel>>(bestRatedProductByCategory);
                }
                if(topBrand.Count > 1 && topCategory.Count > 1)
                {
                    recomendProduct.AddRange(_mapper.Map<List<ProductViewModel>>(bestRatedProductByBrand.Take(4)));
                    recomendProduct.AddRange(_mapper.Map<List<ProductViewModel>>(bestRatedProductByCategory.Take(4)));
                }
            }
            return View(recomendProduct);

        }
    }
}
