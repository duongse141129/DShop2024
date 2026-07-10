using AutoMapper;
using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.Utilities;
using DShop2024.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;





namespace DShop2024.Services.Recommend
{
    public class RecommendationService : IRecommendationService
    {
        private readonly DShopContext _context;
        private const int CatVecSize = 16; // for Material, Category, Brand
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public RecommendationService(DShopContext context, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
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
                Expires = DateTime.UtcNow.AddMinutes(2),
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


        public async Task<IEnumerable<ProductViewModel>> RecommendForRecentlyViewedAsync(int topN = 10, double popularityWeight = 1.3)
        {
            var recentIds = GetIdsRecentlyViewedProduct();
            if (recentIds.Count == 0)
                return await _context.Products.Where(p => p.Status != 0)
                                                .Where(r => r.Ratings.Where(rt => rt.Status != 0).Count() > 10)
                                                .OrderByDescending(b => b.Ratings.Where(r => r.Status != 0).Average(r => r.Star))
                                                .Include(p => p.Ratings)
                                                .Include(p => p.Brand)
                                                .Include(p => p.Category)
                                                .Include(r => r.Sales)
                                                .Take(topN)
                                                .Select( c => _mapper.Map<ProductViewModel>(c))
                                                .ToListAsync();

            var all = await _context.Products.Where(p => p.Status != 0)
                                            .Include(p => p.Ratings)
                                            .Include(p => p.Brand)
                                            .Include(p => p.Category)
                                            .Include(r => r.Sales)
                                            .AsNoTracking()
                                            .AsSplitQuery()
                                            .Select(c => _mapper.Map<ProductViewModel>(c))
                                            .ToListAsync();

            // Precompute normalization bounds
            double minPrice = all.Min(b => (double)b.Price), maxPrice = all.Max(b => (double)b.Price);
            double minRatings = all.Min(b => b.AveragePoint), maxRatings = all.Max(b => b.AveragePoint);
            double minCap = all.Min(b => b.Capacity), maxCap = all.Max(b => b.Capacity);

            // For dimensions, we use total volume proxy h*w*d
            var volumes = all.Select(b => {
                var (h, w, d) = FeatureUtils.ParseDimensions(b.Dimension);
                return h * w * d;
            }).ToArray();
            double minVol = volumes.Min(), maxVol = volumes.Max();

            // Build feature vectors
            var vectors = all.Select((b, i) => ToVector(b, volumes[i], minPrice, maxPrice,
                                                        minRatings, maxRatings, minCap, maxCap,
                                                        minVol, maxVol)).ToArray();

            // Aggregate recent vector as mean of recently viewed
            var recentVecs = recentIds
                .Select(id => {
                    var idx = all.FindIndex(x => x.Id == id);
                    return idx >= 0 ? vectors[idx] : null;
                })
                .Where(v => v != null)
                .ToArray();

            if (recentVecs.Length == 0)
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

            var userProfile = MeanVector(recentVecs!);

            // Compute similarity
            var scores = new List<(ProductViewModel b, double score)>(all.Count);
            foreach (var (b, i) in all.Select((b, i) => (b, i)))
            {
                if (recentIds.Contains(b.Id)) continue; // exclude viewed items
                double sim = CosineSimilarity(userProfile, vectors[i]);

                // Popularity blend (ratings normalized to [0,1])
                double pop = Normalize(b.AveragePoint, minRatings, maxRatings);
                double final = (1 - popularityWeight) * sim + popularityWeight * pop;

                scores.Add((b, final));
            }

            return scores
                .OrderByDescending(t => t.score)
                .Take(topN)
                .Select(t => t.b)
                .ToList();
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

        private static double[] ToVector(
            ProductViewModel b, double volume,
            double minPrice, double maxPrice,
            double minRatings, double maxRatings,
            double minCap, double maxCap,
            double minVol, double maxVol)
        {
            var price = Normalize((double)b.Price, minPrice, maxPrice);
            var ratings = Normalize(b.AveragePoint, minRatings, maxRatings);
            var cap = Normalize(b.Capacity, minCap, maxCap);
            var vol = Normalize(volume, minVol, maxVol);

            var material = FeatureUtils.OneHotHash(b.Material, CatVecSize);
            var category = FeatureUtils.OneHotHash(b.CategoryName, CatVecSize);
            var brand = FeatureUtils.OneHotHash(b.BrandName, CatVecSize);

            var bools = new double[]
            {
            b.WaterResistance ? 1 : 0,
            b.USBChargingPort ? 1 : 0,
            b.LaptopPocket != null ? 1 : 0
            };

            return Concat(
                new[] { price, ratings, cap, vol },
                bools, material, category, brand
            );
        }

        private static double Normalize(double x, double min, double max)
        {
            if (max <= min) return 0;
            var v = (x - min) / (max - min);
            return double.IsFinite(v) ? Math.Clamp(v, 0, 1) : 0;
        }

        private static double[] Concat(params double[][] arrays)
        {
            var len = arrays.Sum(a => a.Length);
            var result = new double[len];
            int offset = 0;
            foreach (var a in arrays)
            {
                Array.Copy(a, 0, result, offset, a.Length);
                offset += a.Length;
            }
            return result;
        }

        private static double[] MeanVector(double[][] vectors)
        {
            int n = vectors[0].Length;
            var mean = new double[n];
            foreach (var v in vectors)
                for (int i = 0; i < n; i++) mean[i] += v[i];
            for (int i = 0; i < n; i++) mean[i] /= vectors.Length;
            return mean;
        }

        private static double CosineSimilarity(double[] a, double[] b)
        {
            double dot = 0, na = 0, nb = 0;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                na += a[i] * a[i];
                nb += b[i] * b[i];
            }
            if (na == 0 || nb == 0) return 0;
            return dot / (Math.Sqrt(na) * Math.Sqrt(nb));
        }
    }
}
