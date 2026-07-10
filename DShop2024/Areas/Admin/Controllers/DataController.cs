using AutoMapper;
using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.Repository;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
    [SidebarMenu(Menu.Admin.Catalog, SubMenu.Catalog.Product)]
    public class DataController : Controller
    {
        private readonly DShopContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly IMapper _mapper;

        public DataController(DShopContext context, IWebHostEnvironment webHostEnvironment, UserManager<AppUserModel> userManager, IMapper mapper)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(string search = "", string brand_by = "", string category_by = "", bool isSale = false,
                      string sortColumn = "Id",
                      string sortOrder = "desc",
                        [FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 100)
        {
            var userAdmin = await _userManager.GetUserAsync(this.User);
            if (userAdmin.UserName != DShopConst.ADMIN_DSHOP)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Access denied ";
                return RedirectToAction("Index");
            }

            var now = DateTime.Now;
            var query = _context.Products
                .Where(p => p.Status != 0)
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
                    p.CategoryId,
                    CategoryName = p.Category.CategoryName,
                    AveragePoint = p.Ratings.Any(r => r.Status != 0) ? p.Ratings.Where(r => r.Status != 0).Average(r => r.Star) : 0,
                    QuantitySold = p.OrderDetails.Where(od => od.Order.Status != 0).Sum(od => (int?)od.Quantity) ?? 0,
                    WishlistCount = p.WishLists.Count(),
                    Capacity = p.Capacity,
                    Dimension = p.Dimension,
                    Weight = p.Weight,
                    Material = p.Material,
                    MainPocket = p.MainPocket,
                    WaterResistance = p.WaterResistance,
                    USBChargingPort = p.USBChargingPort,
                    LaptopPocket = p.LaptopPocket,
                    IsOnSale = p.Sales.Any(s => s.Status != 0 && now >= s.SaleStartDate && now <= s.SaleEndDate),
                    SalePrice = p.Sales.Where(s => s.Status != 0 && now >= s.SaleStartDate && now <= s.SaleEndDate)
                                        .OrderByDescending(s => s.SaleStartDate)
                                        .Select(s => (decimal?)s.SalePrice)
                                        .FirstOrDefault()
                });

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.ProductName.Contains(search));
            if (!string.IsNullOrEmpty(brand_by))
                query = query.Where(p => p.BrandId == int.Parse(brand_by));
            if (!string.IsNullOrEmpty(category_by))
                query = query.Where(p => p.CategoryId == int.Parse(category_by));
            if (isSale)
                query = query.Where(p => p.IsOnSale);

            query = (sortColumn, sortOrder) switch
            {
                ("ProductName", "asc") => query.OrderBy(p => p.ProductName),
                ("ProductName", _) => query.OrderByDescending(p => p.ProductName),
                ("Price", "asc") => query.OrderBy(p => p.IsOnSale ? p.SalePrice : p.Price),
                ("Price", _) => query.OrderByDescending(p => p.IsOnSale ? p.SalePrice : p.Price),
                ("OriginalPrice", "asc") => query.OrderBy(p => p.OriginalPrice),
                ("OriginalPrice", _) => query.OrderByDescending(p => p.OriginalPrice),
                ("Brand", "asc") => query.OrderBy(p => p.BrandName),
                ("Brand", _) => query.OrderByDescending(p => p.BrandName),
                ("Category", "asc") => query.OrderBy(p => p.CategoryName),
                ("Category", _) => query.OrderByDescending(p => p.CategoryName),
                ("Stock", "asc") => query.OrderBy(p => p.Stock),
                ("Stock", _) => query.OrderByDescending(p => p.Stock),
                ("AveragePoint", "asc") => query.OrderBy(p => p.AveragePoint),
                ("AveragePoint", _) => query.OrderByDescending(p => p.AveragePoint),
                ("QuantitySold", "asc") => query.OrderBy(p => p.QuantitySold),
                ("QuantitySold", _) => query.OrderByDescending(p => p.QuantitySold),
                ("WishlistCount", "asc") => query.OrderBy(p => p.WishlistCount),
                ("WishlistCount", _) => query.OrderByDescending(p => p.WishlistCount),
                ("Id", "asc") => query.OrderBy(p => p.Id),
                _ => query.OrderByDescending(p => p.Id),
            };

            int totalProduct = await query.CountAsync();
            if (pagesSize <= 0) pagesSize = 10;
            int countPages = (int)Math.Ceiling((double)totalProduct / pagesSize);
            if (currentPage > countPages) currentPage = countPages;
            if (currentPage < 1) currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new
                {
                    p = pageNumber,
                    pagesSize = pagesSize,
                    search = search,
                    brand_by = brand_by,
                    category_by = category_by,
                    isSale = isSale,
                    sortColumn = sortColumn,
                    sortOrder = sortOrder
                })
            };

            var pageData = await query.Skip((currentPage - 1) * pagesSize)
                                       .Take(pagesSize)
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
                BrandName = p.BrandName,
                CategoryName = p.CategoryName,
                AveragePoint = p.AveragePoint,
                QuantitySold = p.QuantitySold,
                WishlistCount = p.WishlistCount,
                SalePrice = p.SalePrice,
                IsOnSale = p.IsOnSale,
                Capacity = p.Capacity,
                Dimension = p.Dimension,
                Weight = p.Weight,
                Material = p.Material,
                MainPocket = p.MainPocket,
                WaterResistance = p.WaterResistance,
                USBChargingPort = p.USBChargingPort,
                LaptopPocket = p.LaptopPocket,
            }).ToList();

            ViewBag.pagingModel = pagingModel;
            ViewBag.Categories = new SelectList(_context.Categories.Where(c => c.Status != 0), "Id", "CategoryName");
            ViewBag.Brands = new SelectList(_context.Brands.Where(b => b.Status != 0), "Id", "BrandName");
            ViewBag.Search = search;
            ViewBag.PageSize = pagesSize;
            ViewBag.IsSale = isSale;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductDataPartial", productVMs);
            }
            return View(productVMs);
        }


        public async Task<ActionResult> ViewSale()
        {
            var sales = await _context.Sales
                            .Where(s => s.Status != 0)
                            .Include(p => p.Product)
                            .OrderByDescending(o => o.SaleStartDate).ToListAsync();

            var saleVMs = _mapper.Map<List<SaleViewModel>>(sales);
            return View(saleVMs);
        }


    }
}
