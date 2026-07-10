using AutoMapper;
using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.Repository;
using DShop2024.Services.Recommend;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;



namespace DShop2024.Controllers
{
    [SidebarMenu(Menu.Home.Shop)]
    public class ShopProductsController : Controller
	{
		private readonly DShopContext _context;
		private readonly UserManager<AppUserModel> _userManager;
        private readonly IMapper _mapper;
        private readonly IRecommendationService _rec;

        public ShopProductsController(DShopContext context, UserManager<AppUserModel> userManager, IMapper mapper, IRecommendationService rec)
		{
			_context = context;
			_userManager = userManager;
			_mapper = mapper;
            _rec = rec;

        }


        public async Task<IActionResult> Index(ProductFilter filter)
        {
            var now = DateTime.Now;
            var query = _context.Products
                .Where(p => p.Status != 0 && p.Stock > 0)
                .Select(p => new
                {
                    p.Id,
                    p.ProductName,
                    p.Slug,
                    p.MainImage,
                    p.Price,
                    p.OriginalPrice,
                    p.Stock,
                    p.BrandId,
                    BrandName = p.Brand.BrandName,
                    BrandSlug = p.Brand.Slug,
                    p.CategoryId,
                    CategoryName = p.Category.CategoryName,
                    CategorySlug = p.Category.Slug,
                    p.Capacity,
                    p.Dimension,
                    p.Weight,
                    p.Material,
                    p.MainPocket,
                    p.WaterResistance,
                    p.USBChargingPort,
                    p.LaptopPocket,
                    AveragePoint = p.Ratings.Any() ? p.Ratings.Average(r => r.Star) : 0,
                    IsOnSale = p.Sales.Any(s => s.Status != 0 && now >= s.SaleStartDate && now <= s.SaleEndDate),
                    SalePrice = p.Sales.Where(s => s.Status != 0 && now >= s.SaleStartDate && now <= s.SaleEndDate)
                                        .OrderByDescending(s => s.SaleStartDate)
                                        .Select(s => (decimal?)s.SalePrice)
                                        .FirstOrDefault()
                });

            if (!string.IsNullOrEmpty(filter.CategorySlug))
                query = query.Where(p => p.CategorySlug == filter.CategorySlug);

            if (filter.BrandSlugs.Count() > 0 && filter.BrandSlugs.Any(s => s != "all"))
            {
                var activeSlugs = filter.BrandSlugs.Where(s => s != "all").ToList();
                if (activeSlugs.Any())
                    query = query.Where(p => activeSlugs.Contains(p.BrandSlug));
            }

            if (!string.IsNullOrEmpty(filter.SearchName))
                query = query.Where(p => p.ProductName.Contains(filter.SearchName));

            if (decimal.TryParse(filter.LaptopPocket, out decimal lpValue))
                query = query.Where(p => p.LaptopPocket >= lpValue);

            if (filter.WaterResistance)
                query = query.Where(p => p.WaterResistance == true);

            if (filter.USBChargingPort)
                query = query.Where(p => p.USBChargingPort == true);

            if (filter.IsSale)
                query = query.Where(p => p.IsOnSale);

            if (filter.Above4AveragePotint)
                query = query.Where(p => p.AveragePoint >= 4.0);

            if (decimal.TryParse(filter.StartPrice, out decimal sPrice) && decimal.TryParse(filter.EndPrice, out decimal ePrice))
            {
                query = query.Where(p => (p.IsOnSale ? p.SalePrice.Value : p.Price) >= sPrice
                                       && (p.IsOnSale ? p.SalePrice.Value : p.Price) <= ePrice);
            }

            query = filter.SortByList switch
            {
                "Price: Low to High" => query.OrderBy(p => p.IsOnSale ? p.SalePrice : p.Price),
                "Price: High to Low" => query.OrderByDescending(p => p.IsOnSale ? p.SalePrice : p.Price),
                "Newest Arrivals" => query.OrderByDescending(p => p.Id),
                "Oldest" => query.OrderBy(p => p.Id),
                _ => query.OrderByDescending(p => p.Id)
            };

            int totalProduct = await query.CountAsync();
            if (filter.PagesSize <= 0)
                filter.PagesSize = 9;
            int countPages = (int)Math.Ceiling((double)totalProduct / filter.PagesSize);

            if (filter.P > countPages)
                filter.P = countPages;
            if (filter.P < 1)
                filter.P = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = filter.P,
                generateUrl = (pageNumber) =>
                {
                    var routeValues = new RouteValueDictionary {
                { "p", pageNumber },
                { "pagesSize", filter.PagesSize },
                { "searchName", filter.SearchName },
                { "CategorySlug", filter.CategorySlug },
                { "sortBy", filter.SortByList },
                { "startprice", filter.StartPrice },
                { "endPrice", filter.EndPrice },
                { "laptopPocket", filter.LaptopPocket },
                { "waterResistance", filter.WaterResistance },
                { "USBChargingPort", filter.USBChargingPort }
            };

                    if (filter.BrandSlugs != null)
                    {
                        for (int i = 0; i < filter.BrandSlugs.Count; i++)
                            routeValues[$"BrandSlugs[{i}]"] = filter.BrandSlugs[i];
                    }
                    return Url.Action("Index", routeValues);
                }
            };

            ViewBag.pagingModel = pagingModel;

            var pageData = await query.Skip((filter.P - 1) * filter.PagesSize)
                                       .Take(filter.PagesSize)
                                       .ToListAsync();

            var productVMs = pageData.Select(p => new ProductViewModel
            {
                Id = p.Id,
                ProductName = p.ProductName,
                Slug = p.Slug,
                MainImage = p.MainImage,
                Price = p.Price,
                OriginalPrice = p.OriginalPrice,
                Stock = p.Stock,
                BrandId = p.BrandId,
                BrandName = p.BrandName,
                CategoryId = p.CategoryId,
                CategoryName = p.CategoryName,
                AveragePoint = p.AveragePoint,
                Capacity = p.Capacity,
                Dimension = p.Dimension,
                Weight = p.Weight,
                Material = p.Material,
                MainPocket = p.MainPocket,
                WaterResistance = p.WaterResistance,
                USBChargingPort = p.USBChargingPort,
                LaptopPocket = p.LaptopPocket,
                SalePrice = p.SalePrice,
                IsOnSale = p.IsOnSale
            }).ToList();

            ShopProductViewModel shopProduct = new ShopProductViewModel
            {
                Products = productVMs,
                Filter = filter,
                Paging = pagingModel,
                SortByList = new SelectList(ProductEnumData.SortByList, filter.SortByList),
                LaptopPocketTypes = ProductEnumData.laptopPocketTypes
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ListFilterProductPartial", shopProduct);
            return View(shopProduct);
        }

        public async Task<IActionResult> Details(int? Id, [FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 5)
        {
            if (!Id.HasValue) return NotFound();

            var product = await _context.Products
                                .Include(p => p.Brand)
                                .Include(p => p.Category)
                                .Include(p => p.Images)
                                .Include(p => p.Sales)
                                .FirstOrDefaultAsync(p => p.Id == Id && p.Status != 0);

            if (product == null) return NotFound();


            var relatedProducts = await _context.Products.Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.Status != 0)
                                                            .Take(5)
                                                            .Include(p => p.Brand)
                                                            .Include(p => p.Ratings)
                                                            .Include(p => p.Sales)
                                                            .Select(g => _mapper.Map<ProductViewModel>(g)).ToListAsync();

            _rec.AddRecentlyViewedProductAsync(product.Id);

            IQueryable<RatingModel> ratingQuery = _context.Ratings
                                    .Where(p => p.ProductId == Id && p.Status != 0)
                                    .OrderByDescending(r => r.RatingDateTime);
            var totalRating = await ratingQuery.CountAsync();
            if (pagesSize <= 0)
                pagesSize = 5;
            int countPages = (int)Math.Ceiling((double)totalRating / 5);

            if (currentPage > countPages)
                currentPage = countPages;
            if (currentPage < 1)
                currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Details", new
                {
                    p = pageNumber,
                    pagesSize = pagesSize,
                    Id = Id
                })
            };
            var avgRating = totalRating > 0 ? Math.Round(await ratingQuery.AverageAsync(r => r.Star), 1) : 0;
            var ratings = await ratingQuery
                    .Include(c => c.User)
                    .OrderByDescending(r => r.RatingDateTime)
                    .Skip((Math.Max(1, currentPage) - 1) * pagesSize).Take(pagesSize).ToListAsync();

            var user = await _userManager.GetUserAsync(User);
            bool isOrder = false, isInWish = false, isInComp = false, isFeedback = false;
            RatingModel myFeedback = null;

            if (user != null)
            {
                isOrder = await _context.OrderDetails.AnyAsync(od => od.Order.UserId == user.Id && od.ProductId == Id && od.Order.Status == 4);
                isInWish = await _context.WishLists.AnyAsync(w => w.UserId == user.Id && w.ProductId == Id);
                isInComp = await _context.Compares.AnyAsync(w => w.UserId == user.Id && w.ProductId == Id);

                if (isOrder)
                {
                    myFeedback = await ratingQuery.FirstOrDefaultAsync(r => r.UserId == user.Id);
                    isFeedback = myFeedback != null;
                }
            }

            var now = DateTime.Now;
            var activeSale = product.Sales?.FirstOrDefault(s => s.Status != 0 && s.SaleStartDate <= now && s.SaleEndDate >= now);
            bool showSaleEnd = activeSale != null && (activeSale.SaleEndDate - now).TotalDays <= 7;

            var viewModel = new ProductDetailViewModel
            {
                ProductDetail = product,
                Point = avgRating,
                listRating = ratings,
                relatedProducts = relatedProducts,
                IsOrder = isOrder,
                Feedback = myFeedback ?? new RatingModel(),
                IsInWishlist = isInWish,
                IsInCompare = isInComp,
                IsFeedback = isFeedback,
                ExistingImages = product.Images != null ? product.Images.Select(p => p.ImagePath).ToList() : new List<string>(),
                IsOnSale = activeSale != null,
                SalePrice = activeSale?.SalePrice,
                SaleEndDate = activeSale?.SaleEndDate,
                ShowEndSale = showSaleEnd,
                Paging = pagingModel
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_FeedbackListPartial", viewModel);
            }
            return View(viewModel);
        }


        public async Task<IActionResult> GetDetailProductBySlug(string slug)
		{          
            if (String.IsNullOrEmpty(slug))
            {
                return NotFound();
            }
            var productModelbySlug = await _context.Products
                .FirstOrDefaultAsync(m => m.Slug == slug  && m.Status != 0);
            if (productModelbySlug == null)
            {
				return NotFound();
			}
			return RedirectToAction("Details", "ShopProducts", new { Id = productModelbySlug.Id });	
		}

        public async Task<IActionResult> GetListProductByBrandSlug(string slug)
        {
            if (String.IsNullOrEmpty(slug))
            {
                return NotFound();
            }
            var brandModelbySlug = await _context.Brands
                .FirstOrDefaultAsync(m => m.Slug == slug && m.Status != 0);
            if (brandModelbySlug == null)
            {
                return NotFound();
            }
            return RedirectToAction("Index", "ShopProducts", new { BrandSlugs = brandModelbySlug.Slug });
        }

        public async Task<IActionResult> GetListProductByCategorySlug(string slug)
        {
            if (String.IsNullOrEmpty(slug))
            {
                return NotFound();
            }
            var categoryModelbySlug = await _context.Categories
                .FirstOrDefaultAsync(m => m.Slug == slug && m.Status != 0);
            if (categoryModelbySlug == null)
            {
                return NotFound();
            }
            return RedirectToAction("Index", "ShopProducts", new { CategorySlug = categoryModelbySlug.Slug });
        }


        [HttpGet]
        public async Task<IActionResult> SearchLive(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return PartialView("_SearchResultPartial", new List<ProductModel>());
            ViewBag.Keyword = keyword;
            var products = await _context.Products
                .Where(x => x.Status != 0 && x.ProductName.Contains(keyword))
                .OrderBy(x => x.ProductName)
                .Take(5) 
                .Include(x => x.Brand)    
                .Include(x => x.Category)    
                .ToListAsync();

            return PartialView("_SearchResultPartial", products);
        }



    }
}
