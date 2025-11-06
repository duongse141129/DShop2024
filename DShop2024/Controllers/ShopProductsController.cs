using AutoMapper;
using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DShop2024.Controllers
{
	public class ShopProductsController : Controller
	{
		private readonly DShopContext _context;
		private readonly UserManager<AppUserModel> _userManager;
        private readonly IMapper _mapper;

        public ShopProductsController(DShopContext context, UserManager<AppUserModel> userManager, IMapper mapper)
		{
			_context = context;
			_userManager = userManager;
			_mapper = mapper;

        }

        public async Task<IActionResult> Index(string CategorySlug = "", string BrandSlug = "",
                                                 string searchName = "",
                                            string sortBy = "", string startprice = "", string endPrice = "",
                                            string laptopPocket = "", string waterResistance = "", string USBChargingPort = "",
                                            [FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 9)
        {
            ViewBag.sidebar = Menu.Home.Shop;
            ViewBag.laptopPocketTypes = ProductEnumData.laptopPocketTypes;

            IQueryable<ProductModel> listProduct = _context.Products.Where(p => p.Status != 0 && p.Stock > 0)
                                                    .Include(p => p.Brand)
                                                    .Include(p => p.Category);
            var count = await listProduct.CountAsync();
            if (count > 0)
            {
                if (!String.IsNullOrEmpty(CategorySlug))
                {
                    listProduct = listProduct.Where(c => c.Category.Slug == CategorySlug);
                }
                if (!String.IsNullOrEmpty(BrandSlug))
                {
                    listProduct = listProduct.Where(c => c.Brand.Slug == BrandSlug);
                }
                if (!String.IsNullOrEmpty(searchName))
                {
                    listProduct = listProduct.Where(c => c.ProductName.Contains(searchName));
                }
                if (!String.IsNullOrEmpty(laptopPocket))
                {
                    decimal laptopPocketValue;
                    decimal.TryParse(laptopPocket, out laptopPocketValue);
                    listProduct = listProduct.Where(c => c.LaptopPocket >= laptopPocketValue);
                }
                if (!String.IsNullOrEmpty(waterResistance))
                {
                    listProduct = listProduct.Where(c => c.WaterResistance == true);
                }
                if (!String.IsNullOrEmpty(USBChargingPort))
                {
                    listProduct = listProduct.Where(c => c.USBChargingPort == true);
                }

                if (sortBy == "PriceIncrease")
                {
                    listProduct = listProduct.OrderBy(p => p.Price);
                }
                else if (sortBy == "PriceDecrease")
                {
                    listProduct = listProduct.OrderByDescending(p => p.Price);
                }
                else if (sortBy == "Newest")
                {
                    listProduct = listProduct.OrderByDescending(p => p.Id);
                }
                else if (sortBy == "Oldest")
                {
                    listProduct = listProduct.OrderBy(p => p.Id);
                }
                if (startprice != "" && endPrice != "")
                {
                    decimal startPriceValue;
                    decimal endPriceValue;
                    if (decimal.TryParse(startprice, out startPriceValue) && decimal.TryParse(endPrice, out endPriceValue))
                    {
                        listProduct = listProduct.Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
                    }
                    else
                    {
                        listProduct = listProduct.OrderByDescending(p => p.Id);
                    }
                }
                else
                {
                    listProduct = listProduct.OrderByDescending(p => p.Id);
                }

            }
            var filterSortBy = Enum.GetValues(typeof(ProductEnumData.SortBy))
                        .Cast<ProductEnumData.SortBy>()
                        .Select(v => v.ToString())
                        .ToList();
            ViewBag.sortBy = new SelectList(filterSortBy, sortBy);

            ViewBag.CategorySlug = CategorySlug;
            ViewBag.BrandSlug = BrandSlug;
            ViewBag.searchName = searchName;
            ViewBag.startprice = startprice;
            ViewBag.endPrice = endPrice;
            ViewBag.laptopPocket = laptopPocket;
            ViewBag.waterResistance = waterResistance;
            ViewBag.USBChargingPort = USBChargingPort;

            var slider = _context.Banners.Where(b => b.Status == 1).ToList();
            ViewBag.Banners = slider;

            int totalProduct = listProduct.Count();
            if (pagesSize <= 0)
                pagesSize = 9;
            int countPages = (int)Math.Ceiling((double)totalProduct / 9);

            if (currentPage > countPages)
                currentPage = countPages;
            if (currentPage < 1)
                currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new
                {
                    p = pageNumber,
                    pagesSize = pagesSize,
                    searchName = searchName,
                    startprice = startprice,
                    endPrice = endPrice,
                    CategorySlug = CategorySlug,
                    BrandSlug = BrandSlug,
                    sortBy = sortBy,
                    laptopPocket = laptopPocket,
                    waterResistance = waterResistance,
                    USBChargingPort = USBChargingPort
                })
            };
            ViewBag.pagingModel = pagingModel;
            var products = await listProduct.Skip((currentPage - 1) * pagesSize)
                                            .Include(p => p.Ratings)
                                            .AsSplitQuery()
                                            .AsNoTracking()
                                           .Take(pagesSize)
                                           .ToListAsync();
            var productVMs = _mapper.Map<List<ProductViewModel>>(products);
            return View(productVMs);

        }


        public async Task<IActionResult> Details(int? Id, [FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 5)
		{
            ViewBag.sidebar = Menu.Home.Shop;
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

				var checkAdd = recentlyViewedProducts.Any(p => p.Id == productById.Id);
				if (!checkAdd)
				{
              
                    ProductViewModel pvm = _mapper.Map<ProductViewModel>(productById);
                    pvm.ViewAt= DateTime.Now;
                    recentlyViewedProducts.Add(pvm);
				}
				var recentProducts = JsonConvert.SerializeObject(recentlyViewedProducts, new JsonSerializerSettings() { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore });
				var cookieOptionss = new CookieOptions
				{
					HttpOnly = true,
					Expires = DateTime.UtcNow.AddMinutes(30),
					Secure = true,
					SameSite = SameSiteMode.Strict,
				};
				Response.Cookies.Append("RecentlyViewedProducts", recentProducts, cookieOptionss);
                

                var user = await _userManager.GetUserAsync(this.User);
				IQueryable<RatingModel> listRating = _context.Ratings
										.Where(p => p.ProductId == Id)
										.Where(r => r.Status != 0)
										.Include(c => c.User);


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
            ViewBag.sidebar = Menu.Home.Shop;
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

	}
}
