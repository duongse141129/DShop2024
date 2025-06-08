using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.ViewModels;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;


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

        public async Task<IActionResult> Index()
        {
            ViewBag.laptopPocketTypes = ProductEnumData.laptopPocketTypes;

			IQueryable<ProductModel> listProduct = _dataContext.Products.Where(p => p.Status != 0 && p.Stock > 0)
                                                    .Include(p => p.Brand)
                                                    .Include(p => p.Category)
                                                    .Include(r => r.Ratings);
            var products = await listProduct.OrderByDescending(p => p.Id)
                        .Take(10)
                        .Select( g => new ProductViewModel {
                            Id = g.Id,
                            ProductName = g.ProductName,
                            Image = g.Image,
                            Price = g.Price,
                            Stock = g.Stock,
                            BrandName = _dataContext.Brands.FirstOrDefault(b => b.Id == g.BrandId).BrandName,
                            CategoryName = _dataContext.Categories.FirstOrDefault(c => c.Id == g.CategoryId).CategoryName,
                            AveragePoint =  g.Ratings.Any() ? g.Ratings.Where(r => r.ProductId == g.Id && r.Status != 0).Average( r => r.Star) : 0
                        })                     
                        .ToListAsync();

            var slider = await _dataContext.Banners.Where(b => b.Status != 0).ToListAsync();
            ViewBag.Banners = slider;

            var categories = await _dataContext.Categories.Where(b => b.Status != 0).ToListAsync();
            ViewBag.Categories = categories;

            var brands = await _dataContext.Brands.Where(b => b.Status != 0).ToListAsync();
            ViewBag.Brands = brands;

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

		[Authorize]
		[HttpPost]
        public async Task<IActionResult> AddToWishList(int? Id)
		{
            if (Id == null)
            {
                return NotFound();
            }
            ProductModel product = await _dataContext.Products
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (product == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
			var chechExit = await (_dataContext.WishLists.Where(co => co.UserId == user.Id).Where(co => co.ProductId == Id)).FirstOrDefaultAsync();
			if (chechExit != null)
			{
				TempData[DShopConst.TEMPDATA_ERROR] = "The product is already in in your wishlist";
                return Ok(new { success = false, Message = "Add to wishList fail. The product is already in in your wishlist" });
            }

            try
            {
                WishListModel wishList = new WishListModel
                {
                    ProductId = product.Id,
                    UserId = user.Id
                };
                _dataContext.WishLists.Add(wishList);

                await _dataContext.SaveChangesAsync();
                return Ok(new {success = true, Message = "Add to wishList successful"});
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Message = "Add to wishList fail "+ex.Message });
            }
		}

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToCompare(int? Id)
		{
            if (Id == null)
            {
                //return NotFound();
				return Ok(new { success = false, Message = "NotFound" });
			}
            ProductModel product = await _dataContext.Products
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (product == null)
            {
                //return NotFound();
				return Ok(new { success = false, Message = "NotFound" });
			}

            var user = await _userManager.GetUserAsync(User);

            var countConpare = await (_dataContext.Compares.Where(co => co.UserId == user.Id)).CountAsync();
            if(countConpare == 5)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Maximum 5 product in your list compare";
				//return NoContent();
				return Ok(new { success = false, Message = "Maximum 5 product in your list compare" });
			}

            var chechExit = await (_dataContext.Compares.Where(co => co.UserId == user.Id).Where(co => co.ProductId == Id)).FirstOrDefaultAsync();
            if(chechExit != null)
            {                
                TempData[DShopConst.TEMPDATA_ERROR] = "Product is exit in your list compare";
                //return NoContent();
				return Ok(new { success = false, Message = "Add to compare fail. The product already exists in your list compare" });
			}
			try
			{
                CompareModel compare = new CompareModel
                {
                    ProductId = product.Id,
                    UserId = user.Id
                };
                _dataContext.Compares.Add(compare);
                await _dataContext.SaveChangesAsync();
				return Ok(new { success = true, Message = "Add to compare successful" });
			}
			catch (Exception ex)
			{
                return Ok(new { success = false, Message = "Add to compare fail " + ex.Message });
			}
		}

		[Authorize]
		public async Task<IActionResult> WishList()
		{
			var user = await _userManager.GetUserAsync(this.User);
            List<ProductModel> wishListProduct = await (from p in _dataContext.Products
                                                       join w in _dataContext.WishLists on p.Id equals w.ProductId
                                                       where w.UserId == user.Id
                                                       select p)
                                                       .Include(b => b.Brand)
                                                       .Include(c => c.Category)
                                                       .ToListAsync();
            return View(wishListProduct);
		}

		[Authorize]
		public async Task<IActionResult> Compare()
		{
            var user = await _userManager.GetUserAsync(this.User);
            List<ProductModel> compareProduct = await (from p in _dataContext.Products
                                        join co in _dataContext.Compares on p.Id equals co.ProductId
                                        where  co.UserId == user.Id 
                                        select p).ToListAsync();
            return View(compareProduct);
		}

		[Authorize]
		public async Task<IActionResult> DeleteCompare(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(this.User);
            CompareModel compare = await _dataContext.Compares.Where( co => co.ProductId == Id )
                                                                .Where(co => co.UserId == user.Id)
                                                                .FirstOrDefaultAsync();

            try
            {
                _dataContext.Compares.Remove(compare);
                await _dataContext.SaveChangesAsync();

                TempData[DShopConst.TEMPDATA_SUCCESS] = "Remove compare success";
                return RedirectToAction("Compare");
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Remove compare fail "+ex.Message;
                return RedirectToAction("Compare");
            }

        }

		[Authorize]
		public async Task<IActionResult> DeleteAllCompare()
        {
            var user = await _userManager.GetUserAsync(this.User);
            List<CompareModel> compareProduct = await (from co in _dataContext.Compares
                                                       where co.UserId == user.Id
                                                       select co).ToListAsync();

            try
            {
                foreach (var compare in compareProduct)
                {
                    _dataContext.Compares.Remove(compare);
                    await _dataContext.SaveChangesAsync();
                }

                TempData[DShopConst.TEMPDATA_SUCCESS] = "Clear all compare success";
                return RedirectToAction("Compare");
            }
            catch (Exception ex)
            {

                TempData[DShopConst.TEMPDATA_ERROR] = "Clear all compare fail "+ ex.Message;
                return RedirectToAction("Compare");
            }

        }

		[Authorize]
		public async Task<IActionResult> DeleteWishList(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(this.User);
            WishListModel wishList = await _dataContext.WishLists.Where(co => co.ProductId == Id)
                                                                .Where(co => co.UserId == user.Id)
                                                                .FirstOrDefaultAsync();
            if (wishList == null)
            {
                return NotFound();
            }
            try
            {
                _dataContext.WishLists.Remove(wishList);
                await _dataContext.SaveChangesAsync();

                TempData[DShopConst.TEMPDATA_SUCCESS] = "Remove wishList success";
                return RedirectToAction("WishList");
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Remove wishList fail "+ex.Message;
                return RedirectToAction("WishList");
            }


        }

        [Authorize]
        public async Task<IActionResult> DeleteAllWishList()
        {
            var user = await _userManager.GetUserAsync(this.User);
            List<WishListModel> wishlistModels = await (from co in _dataContext.WishLists
                                                       where co.UserId == user.Id
                                                       select co).ToListAsync();

            try
            {
                foreach (var wishlist in wishlistModels)
                {
                    _dataContext.WishLists.Remove(wishlist);
                    await _dataContext.SaveChangesAsync();
                }

                TempData[DShopConst.TEMPDATA_SUCCESS] = "Clear all wishlist success";
                return RedirectToAction("WishList");
            }
            catch (Exception ex)
            {

                TempData[DShopConst.TEMPDATA_ERROR] = "Clear all wishlist fail " + ex.Message;
                return RedirectToAction("WishList");
            }

        }

        public async Task<IActionResult> AboutUs()
        {
            var infomationShop = await _dataContext.InformationShops.FirstOrDefaultAsync();
            return View(infomationShop);
        }


    }
}
