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

        public async Task<IActionResult> Index( ProductFilter filter)
        {
            
            ViewBag.laptopPocketTypes = ProductEnumData.laptopPocketTypes;

            IQueryable<ProductModel> listProduct = _context.Products.Where(p => p.Status != 0 && p.Stock > 0).AsNoTracking();

            if (!string.IsNullOrEmpty(filter.CategorySlug))
                listProduct = listProduct.Where(p => p.Category.Slug == filter.CategorySlug);

            if (filter.BrandSlugs.Count() > 0 && filter.BrandSlugs.Any( s => s != "all") )
            {
                var activeSlugs = filter.BrandSlugs.Where(s => s != "all").ToList();

                if (activeSlugs.Any())
                {
                    listProduct = listProduct.Where(p => activeSlugs.Contains(p.Brand.Slug));
                }

            }

            if (!string.IsNullOrEmpty(filter.SearchName))
                listProduct = listProduct.Where(p => p.ProductName.Contains(filter.SearchName));

            if (decimal.TryParse(filter.LaptopPocket, out decimal lpValue))
                listProduct = listProduct.Where(p => p.LaptopPocket >= lpValue);
            if (filter.WaterResistance)
                listProduct = listProduct.Where(p => p.WaterResistance == true);

            if (filter.USBChargingPort)
                listProduct = listProduct.Where(p => p.USBChargingPort == true);

            if (decimal.TryParse(filter.StartPrice, out decimal sPrice) && decimal.TryParse(filter.EndPrice, out decimal ePrice))
                listProduct = listProduct.Where(p => p.Price >= sPrice && p.Price <= ePrice);

            listProduct = filter.SortByList switch
            {
                "Price: Low to High" => listProduct.OrderBy(p => p.Price),
                "Price: High to Low" => listProduct.OrderByDescending(p => p.Price),
                "Newest Arrivals" => listProduct.OrderByDescending(p => p.Id),
                "Oldest" => listProduct.OrderBy(p => p.Id),
                _ => listProduct.OrderByDescending(p => p.Id)
            };

            int totalProduct = await listProduct.CountAsync();
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
                generateUrl = (pageNumber) => {
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
                        {
                            routeValues[$"BrandSlugs[{i}]"] = filter.BrandSlugs[i];
                        }
                    }

                    return Url.Action("Index", routeValues);
                }
            };

            ViewBag.pagingModel = pagingModel;
            var products = await listProduct.Skip((filter.P - 1) * filter.PagesSize)
                                            .Include(p => p.Ratings)
                                            .Include(p => p.Brand)
                                            .Include(p => p.Category)
                                            .Take(filter.PagesSize)
                                            .ToListAsync();
            var productVMs = _mapper.Map<List<ProductViewModel>>(products);
            ShopProductViewModel shopProduct = new ShopProductViewModel
            {
                Products = productVMs,
                Filter = filter,
                Paging = pagingModel,
                SortByList = new SelectList(ProductEnumData.SortByList, filter.SortByList),
            };
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ListFilterProductPartial", shopProduct);
            }
            return View(shopProduct);

        }


        public async Task<IActionResult> Details(int? Id, [FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 5)
		{
            
            try
			{
				if (Id == null)
				{
					return NotFound();
				}
				var productById = await _context.Products
							.Where(p => p.Id == Id && p.Status != 0)
							.Include(p => p.Brand)
							.Include(p => p.Category)
							.Include(p => p.Ratings)
							.Include(p => p.Images)
                            .AsSplitQuery()
                            .AsNoTracking()
							.FirstOrDefaultAsync();
                if (productById == null)
                {
                    return NotFound();
                }
                List<ProductViewModel> relatedProducts = await _context.Products
										.Where(p => p.Category.Id == productById.CategoryId && p.Id != productById.Id && p.Status != 0)
										.Include (p => p.Brand)
										.Include(p => p.Category)
										.Include(p => p.Ratings)
                                        .AsSplitQuery()
                                        .AsNoTracking()
                                        .Take(5)
										.Select( g => _mapper.Map<ProductViewModel>(g))   
										.ToListAsync();
				ViewBag.relatedProducts = relatedProducts;

                var user = await _userManager.GetUserAsync(this.User);
				IQueryable<RatingModel> listRating = _context.Ratings
										.Where(p => p.ProductId == Id)
										.Where(r => r.Status != 0)
										.Include(c => c.User)
                                        .OrderByDescending(r => r.RatingDateTime);

                _rec.AddRecentlyViewedProductAsync(productById.Id);

                double pointAvarge = 0.0;
				bool checkUserOrder = false;
				RatingModel myFeedback = new RatingModel();
				List<RatingModel> ratings = new List<RatingModel>();
				bool isInWishList = false;
				bool isInCompare = false;
				bool isFeedBack = false;

				if (listRating.Count() > 0)
				{
					pointAvarge = Math.Round(listRating.Average(p => p.Star), 1);
				}

				if (user != null)
				{
					var checkOrder = await (from o in _context.Orders
											join od in _context.OrderDetails on o.Id equals od.OrderId
											where o.UserId == user.Id && od.ProductId == Id && o.Status == 4
											select o).FirstOrDefaultAsync();

					if (checkOrder != null)
					{
						checkUserOrder = true;
						myFeedback = await listRating.Where(u => u.UserId == user.Id && u.ProductId == Id).FirstOrDefaultAsync();
						if (myFeedback != null)
						{
							isFeedBack = true;
							listRating = listRating.Where(u => u.UserId != user.Id);
						}
					}
					isInWishList = await _context.WishLists.AnyAsync(w => w.UserId == user.Id && w.ProductId == Id);
					isInCompare = await _context.Compares.AnyAsync(w => w.UserId == user.Id && w.ProductId == Id);
				}

				var totalRating = await listRating.CountAsync();
				if (totalRating > 0)
				{				
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

					ratings = await listRating
								.Skip((currentPage - 1) * pagesSize)
								.Take(pagesSize).ToListAsync();
					ViewBag.pagingModel = pagingModel;
				}

				var viewModel = new ProductDetailViewModel
				{
					ProductDetail = productById,
					Point = pointAvarge,
					listRating = ratings,
					IsOrder = checkUserOrder,
					Feedback = myFeedback,
					IsInCompare = isInCompare,
					IsInWishlist = isInWishList,
					IsFeedback = isFeedBack,
                    ExistingImages = productById.Images != null ?  productById.Images.Select( p => p.ImagePath).ToList() : new List<string>()

                };
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_FeedbackListPartial", viewModel);
                }

                return View(viewModel);
			}
			catch (Exception ex)
			{

				TempData[DShopConst.TEMPDATA_ERROR] = "fail " + ex.Message;
				return RedirectToAction("Index");
			}

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
