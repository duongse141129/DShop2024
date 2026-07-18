using AutoMapper;
using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Recommendations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Globalization;
using System.Text.RegularExpressions;





namespace DShop2024.Services.Recommend
{
    public class RecommendationService : IRecommendationService
    {
        private readonly DShopContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private const string ScoringPoolCacheKey = "reco:scoring-pool";
        private static readonly TimeSpan ScoringPoolCacheDuration = TimeSpan.FromMinutes(5);

        public RecommendationService(DShopContext context, IHttpContextAccessor httpContextAccessor, IMapper mapper,  IMemoryCache cache)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _cache = cache;
        }


        private List<int> GetIdsRecentlyViewedProduct()
        {
            var rvproduct = _httpContextAccessor.HttpContext.Request.Cookies[DShopConst.RECENTLY_VIEWED_PRODUCTS];
            List<int> recentlyViewedIdProducts;
            if (rvproduct == null)
            {
                return new List<int>();
            }
            recentlyViewedIdProducts = JsonConvert.DeserializeObject<List<int>>(rvproduct);

            return recentlyViewedIdProducts;
        }
        public void AddRecentlyViewedProductAsync(int productId)
        {
            var recentlyViewedProducts = GetIdsRecentlyViewedProduct();
            if (!recentlyViewedProducts.Contains(productId))
            {
                recentlyViewedProducts.Add(productId);
            }
            if(recentlyViewedProducts.Count > 20)
            {
                recentlyViewedProducts.RemoveAt(0);
            }
            var recentProducts = JsonConvert.SerializeObject(recentlyViewedProducts, new JsonSerializerSettings() { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore });
            var cookieOptionss = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(2),
                Secure = true,
                SameSite = SameSiteMode.Strict,
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append(DShopConst.RECENTLY_VIEWED_PRODUCTS, recentProducts, cookieOptionss);
        }

        public async Task<List<ProductViewModel>> GetRecentlyViewedProductsAsync()
        {
            var recentlyViewedIdProducts = GetIdsRecentlyViewedProduct();
            List<ProductViewModel> recentlyViewedProducts = new List<ProductViewModel>();
            if (recentlyViewedIdProducts.Count > 0)
            {
                foreach (var item in recentlyViewedIdProducts)
                {
                    ProductModel model = await _context.Products
                                                .Include(r => r.Ratings)
                                                .Include(r => r.Category)
                                                .Include(r => r.Brand)
                                                .Include(r => r.Sales)
                                                .FirstOrDefaultAsync(p => p.Status != 0 && p.Stock > 0 && p.Id == item);
                    if (model != null)
                    {
                        var modelVM = _mapper.Map<ProductViewModel>(model);
                        recentlyViewedProducts.Add(modelVM);
                    }
                }
            }
            return recentlyViewedProducts;
        }




        private static class RecoWeight
        {
            public const double Category = 35;
            public const double Brand = 20;
            public const double Price = 15;
            public const double Capacity = 5;
            public const double Material = 5;
            public const double Dimension = 5;
            public const double WaterResistance = 5;
            public const double USBChargingPort = 5;
            public const double LaptopPocket = 5;


            public const double SaleBoost = 10; 
        }

        private class ScoringCandidate
        {
            public int Id { get; set; }
            public int CategoryId { get; set; }
            public int BrandId { get; set; }
            public double Price { get; set; }
            public int Capacity { get; set; }
            public string Material { get; set; }
            public double? DimensionVolume { get; set; } 
            public string Dimension { get; set; }         
            public bool WaterResistance { get; set; }
            public bool USBChargingPort { get; set; }
            public decimal? LaptopPocket { get; set; }
            public bool IsOnSale { get; set; }
            public double AveragePoint { get; set; }
        }

        private class ViewedProfile
        {
            public int Id { get; set; }
            public int CategoryId { get; set; }
            public int BrandId { get; set; }
            public double Price { get; set; }
            public int Capacity { get; set; }
            public string Material { get; set; }
            public double? DimensionVolume { get; set; }
            public string Dimension { get; set; }
            public bool WaterResistance { get; set; }
            public bool USBChargingPort { get; set; }
            public decimal? LaptopPocket { get; set; }
        }


        private async Task<List<ScoringCandidate>> GetScoringPoolAsync()
        {
            if (_cache.TryGetValue(ScoringPoolCacheKey, out List<ScoringCandidate> cached))
                return cached;

            var now = DateTime.UtcNow;
            var pool = await _context.Products
                .AsNoTracking()
                .Where(p => p.Status != 0 && p.Stock > 0)
                .Select(p => new ScoringCandidate
                {
                    Id = p.Id,
                    CategoryId = p.CategoryId,
                    BrandId = p.BrandId,
                    Price = (double)p.Price,
                    Capacity = p.Capacity,
                    Material = p.Material,
                    Dimension = p.Dimension,
                    WaterResistance = p.WaterResistance,
                    USBChargingPort = p.USBChargingPort,
                    LaptopPocket = p.LaptopPocket,
                    IsOnSale = p.Sales.Any(s =>
                                    s.Status != 0 &&
                                    now >= s.SaleStartDate &&
                                    now <= s.SaleEndDate),
                    AveragePoint = p.Ratings.Where(r => r.Status != 0).Average(r => (double?)r.Star) ?? 0,
                })
                .Where(p => p.AveragePoint > 3) 
                .ToListAsync();

            foreach (var p in pool)
            {
                p.DimensionVolume = TryParseVolume(p.Dimension);
            }

            _cache.Set(ScoringPoolCacheKey, pool, ScoringPoolCacheDuration);
            return pool;
        }

        //  Content-based(attribute-based) recommender system, built from two layers:
        //-	Weighted multi-attribute similarity scoring
        //-	Item-to-item retrieval
        //There's a third layer worth naming separately


        public async Task<List<ProductViewModel>> GetRecommendedProductsAsync(int topN = 10)
        {
            var viewedProducts = await GetRecentlyViewedProductsAsync();
            if (viewedProducts.Count == 0)
                return await _context.Products.Where(p => p.Status != 0)
                                                .Where(r => r.Ratings.Where(rt => rt.Status != 0).Count() > 10)
                                                .OrderByDescending(b => b.Ratings.Where(r => r.Status != 0).Average(r => r.Star))
                                                .Include(p => p.Ratings)
                                                .Include(p => p.Brand)
                                                .Include(p => p.Category)
                                                .Include(r => r.Sales)
                                                .Take(topN)
                                                .Select(c => _mapper.Map<ProductViewModel>(c))
                                                .ToListAsync();

            var viewedIds = viewedProducts.Select(p => p.Id).ToHashSet();

            var viewedProfiles = viewedProducts.Select(v => new ViewedProfile
            {
                Id = v.Id,
                CategoryId = v.CategoryId,
                BrandId = v.BrandId,
                Price = (double)v.Price,
                Capacity = v.Capacity,
                Material = v.Material,
                Dimension = v.Dimension,
                DimensionVolume = TryParseVolume(v.Dimension),
                WaterResistance = v.WaterResistance,
                USBChargingPort = v.USBChargingPort,
                LaptopPocket = v.LaptopPocket
            }).ToList();

            var pool = await GetScoringPoolAsync();

            var topIds = pool
                .Where(c => !viewedIds.Contains(c.Id))
                .Select(c =>
                {
                    double similarity = viewedProfiles.Max(v => CalculateSimilarity(c, v));
                    double finalScore = similarity + (c.IsOnSale ? RecoWeight.SaleBoost : 0);
                    return (Candidate: c, FinalScore: finalScore);
                })
                .OrderByDescending(x => x.FinalScore)
                .ThenByDescending(x => x.Candidate.AveragePoint)
                .Take(topN)
                .Select(x => x.Candidate.Id)
                .ToList();

            if (topIds.Count == 0)
                return new List<ProductViewModel>();

            var winners = await _context.Products
                .AsNoTracking()
                .Include(p => p.Ratings)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Sales)
                .AsSplitQuery()
                .Where(p => topIds.Contains(p.Id))
                .ToListAsync();

            var mappedWinners = _mapper.Map<List<ProductViewModel>>(winners);

            var rankLookup = topIds.Select((id, index) => (id, index)).ToDictionary(x => x.id, x => x.index);
            return mappedWinners.OrderBy(p => rankLookup[p.Id]).ToList();
        }

        private double CalculateSimilarity(ScoringCandidate candidate, ViewedProfile viewed)
        {
            double score = 0;

            score += (candidate.CategoryId == viewed.CategoryId ? 1 : 0) * RecoWeight.Category;
            score += (candidate.BrandId == viewed.BrandId ? 1 : 0) * RecoWeight.Brand;
            score += (candidate.WaterResistance == viewed.WaterResistance ? 1 : 0) * RecoWeight.WaterResistance;
            score += (candidate.USBChargingPort == viewed.USBChargingPort ? 1 : 0) * RecoWeight.USBChargingPort;

            score += NumericSimilarity(candidate.Price, viewed.Price) * RecoWeight.Price;
            score += NumericSimilarity(candidate.Capacity, viewed.Capacity) * RecoWeight.Capacity;

            score += StringSimilarity(candidate.Material, viewed.Material) * RecoWeight.Material;

            score += DimensionSimilarity(
                candidate.DimensionVolume, candidate.Dimension,
                viewed.DimensionVolume, viewed.Dimension) * RecoWeight.Dimension;

            score += NullableNumericSimilarity(candidate.LaptopPocket, viewed.LaptopPocket) * RecoWeight.LaptopPocket;

            return score;
        }

        private double NumericSimilarity(double a, double b)
        {
            if (a == 0 && b == 0) return 1;
            double max = Math.Max(Math.Abs(a), Math.Abs(b));
            if (max == 0) return 1;
            double diff = Math.Abs(a - b) / max;
            return Math.Max(0, 1 - diff);
        }

        private double NullableNumericSimilarity(decimal? a, decimal? b)
        {
            if (!a.HasValue && !b.HasValue) return 1;
            if (!a.HasValue || !b.HasValue) return 0;
            return NumericSimilarity((double)a.Value, (double)b.Value);
        }

        private double StringSimilarity(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a) && string.IsNullOrWhiteSpace(b)) return 1;
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return 0;
            return string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        }

        private double DimensionSimilarity(double? volA, string rawA, double? volB, string rawB)
        {
            if (volA.HasValue && volB.HasValue)
                return NumericSimilarity(volA.Value, volB.Value);
            return StringSimilarity(rawA, rawB);
        }

        private double? TryParseVolume(string dimension)
        {
            if (string.IsNullOrWhiteSpace(dimension)) return null;
            var normalized = dimension.Replace(',', '.');
            var numbers = Regex.Matches(normalized, @"\d+(\.\d+)?")
                .Select(m => double.Parse(m.Value, CultureInfo.InvariantCulture))
                .ToList();
            if (numbers.Count < 2) return null;
            return numbers.Aggregate(1.0, (acc, n) => acc * n);
        }
    }
}
