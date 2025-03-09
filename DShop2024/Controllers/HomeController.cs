using DShop2024.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;

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

        public IActionResult Index()
        {
            var products = _dataContext.Products.Where(p => p.Status == 1)
                                        .Include(p => p.Brand)
                                        .Include(p => p.Category)
                                        .ToList();

            var slider = _dataContext.Banners.Where(b => b.Status == 1).ToList();
            ViewBag.Banners = slider;
            return View(products);
        }

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

        public async Task<IActionResult> Contact()
        {
            var contact = await _dataContext.Contacts.FirstOrDefaultAsync();
            return View(contact);
        }

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
