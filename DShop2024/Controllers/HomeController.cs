using AutoMapper;
using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Drawing.Printing;


namespace DShop2024.Controllers
{
    public class HomeController : Controller
    {
        private readonly DShopContext _context;
		private readonly UserManager<AppUserModel> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, DShopContext context,UserManager<AppUserModel> userManager, IMapper mapper)
        {
            _logger = logger;
            _context = context;
			_userManager = userManager;
            _mapper = mapper;


        }


        public async Task<IActionResult> Index()
        {
            ViewBag.sidebar = Menu.Home.Home;
            ViewBag.laptopPocketTypes = ProductEnumData.laptopPocketTypes;

			IQueryable<ProductModel> listProduct = _context.Products.Where(p => p.Status != 0 && p.Stock > 0)
                                                    .Include(p => p.Brand)
                                                    .Include(p => p.Category)
                                                    .Include(r => r.Ratings);
            var products = await listProduct.OrderByDescending(p => p.CreateDate)
                        .Take(10)
                        .Select( g => _mapper.Map<ProductViewModel>(g))                     
                        .ToListAsync();

            var slider = await _context.Banners.Where(b => b.Status != 0).ToListAsync();
            ViewBag.Banners = slider;

            var categories = await _context.Categories.Where(b => b.Status != 0).ToListAsync();
            ViewBag.Categories = categories;

            var brands = await _context.Brands.Where(b => b.Status != 0).ToListAsync();
            ViewBag.Brands = brands;

            return View(products);
        }


        public async Task<IActionResult> Privacy()
        {
            return View(await _context.Policies.Where(p => p.Status != 0).ToListAsync());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statuscode)
        {
            if (statuscode == 404)
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
                return Ok(new { success = false, Message = "NotFound" });
            }
            ProductModel product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (product == null)
            {
                return Ok(new { success = false, Message = "NotFound" });
            }

            var user = await _userManager.GetUserAsync(User);
			var chechExit = await (_context.WishLists.Where(co => co.UserId == user.Id).Where(co => co.ProductId == Id)).FirstOrDefaultAsync();
			if (chechExit != null)
			{
                return Ok(new { success = false, Message = "The product is already in in your wishlist" });
            }
            try
            {
                WishListModel wishList = new WishListModel
                {
                    ProductId = product.Id,
                    UserId = user.Id
                };
                _context.WishLists.Add(wishList);

                await _context.SaveChangesAsync();
                return Ok(new {success = true, Message = "Add to wishList successful"});
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Message = ex.Message });
            }
		}

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToCompare(int? Id)
		{
            if (Id == null)
            {
				return Ok(new { success = false, Message = "NotFound" });
			}
            ProductModel product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (product == null)
            {
				return Ok(new { success = false, Message = "NotFound" });
			}

            var user = await _userManager.GetUserAsync(User);

            var countConpare = await (_context.Compares.Where(co => co.UserId == user.Id)).CountAsync();
            if(countConpare == 5)
            {
				return Ok(new { success = false, Message = "Maximum 5 product in your list compare" });
			}

            var chechExit = await (_context.Compares.Where(co => co.UserId == user.Id).Where(co => co.ProductId == Id)).FirstOrDefaultAsync();
            if(chechExit != null)
            {   
				return Ok(new { success = false, Message = "The product already exists in your list compare" });
			}
			try
			{
                CompareModel compare = new CompareModel
                {
                    ProductId = product.Id,
                    UserId = user.Id
                };
                _context.Compares.Add(compare);
                await _context.SaveChangesAsync();
				return Ok(new { success = true, Message = "Add to compare successful" });
			}
			catch (Exception ex)
			{
                return Ok(new { success = false, Message = ex.Message });
			}
		}

		[Authorize]
		public async Task<IActionResult> WishList([FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 10)
		{
			var user = await _userManager.GetUserAsync(this.User);
            IQueryable<ProductModel> wishListProduct = from p in _context.Products
                                                           join w in _context.WishLists on p.Id equals w.ProductId
                                                           where w.UserId == user.Id
                                                           select p;
            int totalWishlistProduct = wishListProduct.Count();
            if (pagesSize <= 0)
                pagesSize = 10;
            int countPages = (int)Math.Ceiling((double)totalWishlistProduct / 10);

            if (currentPage > countPages)
                currentPage = countPages;
            if (currentPage < 1)
                currentPage = 1;
            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("WishList", new
                {
                    p = pageNumber,
                    pagesSize = pagesSize
                })
            };
            var products = await wishListProduct.Skip((currentPage - 1) * pagesSize)
                        .Take(pagesSize)
                        .Include(p => p.Brand)
                        .Include(p => p.Category)
                        .Include(p => p.Ratings)
                        .Select( g => _mapper.Map<ProductViewModel>(g))
                        .ToListAsync();
            ViewBag.pagingModel = pagingModel;
            return View(products);
		}

		[Authorize]
		public async Task<IActionResult> Compare()
		{
            var user = await _userManager.GetUserAsync(this.User);
            List<ProductModel> compareProduct = await (from p in _context.Products
                                                       join co in _context.Compares on p.Id equals co.ProductId
                                                       where co.UserId == user.Id
                                                       select p)
                                        .Include(p => p.Ratings)
                                        .ToListAsync();
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
            CompareModel compare = await _context.Compares.Where( co => co.ProductId == Id )
                                                                .Where(co => co.UserId == user.Id)
                                                                .FirstOrDefaultAsync();

            try
            {
                _context.Compares.Remove(compare);
                await _context.SaveChangesAsync();

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
            List<CompareModel> compareProduct = await (from co in _context.Compares
                                                       where co.UserId == user.Id
                                                       select co).ToListAsync();

            try
            {
                foreach (var compare in compareProduct)
                {
                    _context.Compares.Remove(compare);
                    await _context.SaveChangesAsync();
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
            WishListModel wishList = await _context.WishLists.Where(co => co.ProductId == Id)
                                                                .Where(co => co.UserId == user.Id)
                                                                .FirstOrDefaultAsync();
            if (wishList == null)
            {
                return NotFound();
            }
            try
            {
                _context.WishLists.Remove(wishList);
                await _context.SaveChangesAsync();

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
            List<WishListModel> wishlistModels = await (from co in _context.WishLists
                                                       where co.UserId == user.Id
                                                       select co).ToListAsync();

            try
            {
                foreach (var wishlist in wishlistModels)
                {
                    _context.WishLists.Remove(wishlist);
                    await _context.SaveChangesAsync();
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
            var infomationShop = await _context.InformationShops.FirstOrDefaultAsync();
            return View(infomationShop);
        }


    }
}
