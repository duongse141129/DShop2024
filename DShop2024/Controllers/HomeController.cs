using DShop2024.EnumData;
using DShop2024.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using static Azure.Core.HttpHeader;

namespace DShop2024.Controllers
{
    public class HomeController : Controller
    {
        private readonly DShopContext _dataContext;
		private readonly UserManager<AppUserModel> _userManager;
		private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, DShopContext context,UserManager<AppUserModel> userManager)
        {
            _logger = logger;
            _dataContext = context;
			_userManager = userManager;

		}

        #region

        //     public async Task<IActionResult> Index(string CategorySlug = "", string BrandSlug = "",
        //									int pg = 1, string searchName = "" ,
        //								string sortBy = "", string startprice = "", string endPrice = "",
        //                                         string laptopPocket = "" , string waterResistance = "", string USBChargingPort = "")
        //     {
        //         //var products = _dataContext.Products.Where(p => p.Status != 0 && p.Stock >0)                                      
        //         //                            .Include(p => p.Brand)
        //         //                            .Include(p => p.Category)
        //         //                            .ToList();

        //         IQueryable<ProductModel> listProduct = _dataContext.Products.Where(p => p.Status != 0 && p.Stock > 0)
        //                                                 .Include(p => p.Brand)
        //                                                 .Include(p => p.Category);
        //var count = await listProduct.CountAsync();
        //         if(count > 0)
        //         {
        //	if (!String.IsNullOrEmpty(CategorySlug))
        //	{
        //		listProduct = listProduct.Where(c => c.Category.Slug == CategorySlug);
        //	}
        //	if (!String.IsNullOrEmpty(BrandSlug))
        //	{
        //		listProduct = listProduct.Where(c => c.Brand.Slug == BrandSlug);
        //	}
        //	if (!String.IsNullOrEmpty(searchName))
        //	{
        //		listProduct = listProduct.Where(c => c.ProductName.Contains(searchName));
        //	}
        //	if (!String.IsNullOrEmpty(laptopPocket))
        //	{
        //		decimal laptopPocketValue;
        //		decimal.TryParse(laptopPocket, out laptopPocketValue);
        //		listProduct = listProduct.Where(c => c.LaptopPocket >= laptopPocketValue);
        //	}
        //	if (!String.IsNullOrEmpty(waterResistance))
        //	{				
        //		listProduct = listProduct.Where(c => c.WaterResistance == true);
        //	}
        //	if (!String.IsNullOrEmpty(USBChargingPort))
        //	{
        //		listProduct = listProduct.Where(c => c.USBChargingPort == true);
        //	}


        //	if (sortBy == "priceIncrease")
        //	{
        //		listProduct = listProduct.OrderBy(p => p.Price);
        //	}
        //	else if (sortBy == "priceDecrease")
        //	{
        //		listProduct = listProduct.OrderByDescending(p => p.Price);
        //	}
        //	else if (sortBy == "newest")
        //	{
        //		listProduct = listProduct.OrderByDescending(p => p.Id);
        //	}
        //	else if (sortBy == "oldest")
        //	{
        //		listProduct = listProduct.OrderBy(p => p.Id);
        //	}
        //	else if (startprice != "" && endPrice != "")
        //	{
        //		decimal startPriceValue;
        //		decimal endPriceValue;
        //		if (decimal.TryParse(startprice, out startPriceValue) && decimal.TryParse(endPrice, out endPriceValue))
        //		{
        //			listProduct = listProduct.Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
        //		}
        //		else
        //		{
        //			listProduct = listProduct.OrderByDescending(p => p.Id);
        //		}
        //	}
        //	else
        //	{
        //		listProduct = listProduct.OrderByDescending(p => p.Id);
        //	}

        //}
        //var filterSortBy = Enum.GetValues(typeof(Product.SortBy))
        //			.Cast<Product.SortBy>()
        //			.Select(v => v.ToString())
        //			.ToList();
        //ViewBag.sortBy = new SelectList(filterSortBy, sortBy);

        //ViewBag.searchName = searchName;
        //ViewBag.startprice = startprice;
        //ViewBag.endPrice = endPrice;
        //ViewBag.laptopPocket = laptopPocket;
        //ViewBag.waterResistance = waterResistance;
        //ViewBag.USBChargingPort = USBChargingPort;


        //int pageSize = 6;
        //if (pg < 1) pg = 1;
        //int recsCount = listProduct.Count();
        //var pager = new Paginate(recsCount, pg, pageSize);
        //int recSkip = (pg - 1) * pageSize;

        //listProduct = listProduct.Skip(recSkip).Take(pager.PageSize);

        //await listProduct.ToListAsync();
        //var slider = _dataContext.Banners.Where(b => b.Status == 1).ToList();
        //         ViewBag.Banners = slider;
        //ViewBag.Pager = pager;


        //return View(listProduct);
        //     }

        #endregion

        #region

        public async Task<IActionResult> Index(string CategorySlug = "", string BrandSlug = "",
                                                 string searchName = "",
                                            string sortBy = "", string startprice = "", string endPrice = "",
                                            string laptopPocket = "", string waterResistance = "", string USBChargingPort = "", 
                                            [FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 6)
        {
            //var products = _dataContext.Products.Where(p => p.Status != 0 && p.Stock >0)                                      
            //                            .Include(p => p.Brand)
            //                            .Include(p => p.Category)
            //                            .ToList();

            IQueryable<ProductModel> listProduct = _dataContext.Products.Where(p => p.Status != 0 && p.Stock > 0)
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


                if (sortBy == "priceIncrease")
                {
                    listProduct = listProduct.OrderBy(p => p.Price);
                }
                else if (sortBy == "priceDecrease")
                {
                    listProduct = listProduct.OrderByDescending(p => p.Price);
                }
                else if (sortBy == "newest")
                {
                    listProduct = listProduct.OrderByDescending(p => p.Id);
                }
                else if (sortBy == "oldest")
                {
                    listProduct = listProduct.OrderBy(p => p.Id);
                }
                else if (startprice != "" && endPrice != "")
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
            var filterSortBy = Enum.GetValues(typeof(Product.SortBy))
                        .Cast<Product.SortBy>()
                        .Select(v => v.ToString())
                        .ToList();
            ViewBag.sortBy = new SelectList(filterSortBy, sortBy);

            ViewBag.searchName = searchName;
            ViewBag.startprice = startprice;
            ViewBag.endPrice = endPrice;
            ViewBag.laptopPocket = laptopPocket;
            ViewBag.waterResistance = waterResistance;
            ViewBag.USBChargingPort = USBChargingPort;


            //int pageSize = 6;
            //if (pg < 1) pg = 1;
            //int recsCount = listProduct.Count();
            //var pager = new Paginate(recsCount, pg, pageSize);
            //int recSkip = (pg - 1) * pageSize;

            //listProduct = listProduct.Skip(recSkip).Take(pager.PageSize);
            //await listProduct.ToListAsync();
            //ViewBag.Pager = pager;

            var slider = _dataContext.Banners.Where(b => b.Status == 1).ToList();
            ViewBag.Banners = slider;

            int totalProduct = listProduct.Count();
            if (pagesSize <= 0)
                pagesSize = 6;
            int countPages = (int)Math.Ceiling((double)totalProduct / 6);

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

            var products = await listProduct.Skip((currentPage - 1) * pagesSize)
                        .Take(pagesSize).ToListAsync();

            ViewBag.pagingModel = pagingModel;


            return View(products);
        }
        #endregion

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statuscode)
        {
            if(statuscode == 404)
            {
                return View("NotFound");
            }

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //public async Task<IActionResult> Contact()
        //{
        //    var contact = await _dataContext.Contacts.FirstOrDefaultAsync();
        //    return View(contact);
        //}
        //public async Task<IActionResult> Contact()
        //{
        //    var contact = await _dataContext.InformationShops.FirstOrDefaultAsync();
        //    return View(contact);
        //}

		public async Task<IActionResult> InformationShop()
		{
			var informationShop = await _dataContext.InformationShops.FirstOrDefaultAsync();
			return View(informationShop);
		}

        [HttpPost]
        public async Task<IActionResult> AddToWishList(int Id)
		{
			var user = await _userManager.GetUserAsync(User);
			var chechExit = await (_dataContext.WishLists.Where(co => co.UserId == user.Id).Where(co => co.ProductId == Id)).FirstOrDefaultAsync();
			if (chechExit != null)
			{
				TempData["error"] = "Product is exit in your list wishlist";
				return NoContent();
			}
			WishListModel wishList = new WishListModel 
            { 
                ProductId = Id,
                UserId = user.Id           
            };
            _dataContext.WishLists.Add(wishList);

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new {success = true, Message = "Add to wishList successful"});
            }
            catch (Exception)
            {

				return NoContent();
			}
		}

        [HttpPost]
        public async Task<IActionResult> AddToCompare(int Id)
		{
			var user = await _userManager.GetUserAsync(User);

            var countConpare = await (_dataContext.Compares.Where(co => co.UserId == user.Id)).CountAsync();
            if(countConpare == 5)
            {
                TempData["error"] = "Maximum 5 product";
				return NoContent();
			}

            var chechExit = await (_dataContext.Compares.Where(co => co.UserId == user.Id).Where(co => co.ProductId == Id)).FirstOrDefaultAsync();
            if(chechExit != null)
            {                
                TempData["error"] = "Product is exit in your list compare";
                return NoContent();
			}


            CompareModel compare = new CompareModel
			{
				ProductId = Id,
				UserId = user.Id
			};
			_dataContext.Compares.Add(compare);

			try
			{
				await _dataContext.SaveChangesAsync();
				return Ok(new { success = true, Message = "Add to compare successful" });
			}
			catch (Exception)
			{

				return StatusCode(500, "Add to compare fail");
			}
		}


		public async Task<IActionResult> WishList()
		{
			var user = await _userManager.GetUserAsync(this.User);
			var wishListProduct = await (from w in _dataContext.WishLists
                                         join p in _dataContext.Products on w.ProductId equals p.Id
                                         join u in _dataContext.Users on w.UserId equals u.Id
										 where w.UserId == user.Id
										 select new { User = u, Product = p, WishList = w }).ToListAsync();
            return View(wishListProduct);
		}

		public async Task<IActionResult> Compare()
		{
            var user = await _userManager.GetUserAsync(this.User);
            List<ProductModel> compareProduct = await (from p in _dataContext.Products
                                        join co in _dataContext.Compares on p.Id equals co.ProductId
                                        where  co.UserId == user.Id 
                                        select p).ToListAsync();
            return View(compareProduct);
		}

        public async Task<IActionResult> DeleteCompare(int Id)
        {
            var user = await _userManager.GetUserAsync(this.User);
            CompareModel compare = await _dataContext.Compares.Where( co => co.ProductId == Id )
                                                                .Where(co => co.UserId == user.Id)
                                                                .FirstOrDefaultAsync();
                                                                        ;
            
            _dataContext.Compares.Remove(compare);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Remove compare success";
            return RedirectToAction("Compare");
        }

        public async Task<IActionResult> DeleteAllCompare()
        {
            var user = await _userManager.GetUserAsync(this.User);
            List<CompareModel> compareProduct = await (from co in _dataContext.Compares
                                                       where co.UserId == user.Id
                                                       select co).ToListAsync();
            foreach (var compare in compareProduct)
            {
                _dataContext.Compares.Remove(compare);
                await _dataContext.SaveChangesAsync();
            }

            TempData["success"] = "Clear all compare success";
            return RedirectToAction("Compare");
        }

        public async Task<IActionResult> DeleteWishList(int Id)
        {
            WishListModel wishList = await _dataContext.WishLists.FindAsync(Id);

            _dataContext.WishLists.Remove(wishList);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Remove wishList success";
            return RedirectToAction("WishList");
        }

    }
}
