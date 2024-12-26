using DShop2024.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
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

                return StatusCode(500, "Add to wishlist fail");
            }
		}

		public async Task<IActionResult> AddToCompare(int Id)
		{
			var user = await _userManager.GetUserAsync(User);

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
            var wishListProduct = await (from w in _dataContext.WishLists
                                         join p in _dataContext.Products on w.ProductId equals p.Id
                                         join u in _dataContext.Users on w.UserId equals u.Id
                                         select new { User = u, Product = p, WishList = w }).ToListAsync();
            return View(wishListProduct);
		}

		public async Task<IActionResult> Compare()
		{
			var compareProduct = await (from c in _dataContext.Compares
										 join p in _dataContext.Products on c.ProductId equals p.Id
										 join u in _dataContext.Users on c.UserId equals u.Id
										 select new { User = u, Product = p, Compare = c }).ToListAsync();
			return View(compareProduct);
		}

        public async Task<IActionResult> DeleteCompare(int Id)
        {
            CompareModel compare = await _dataContext.Compares.FindAsync(Id);
            
            _dataContext.Compares.Remove(compare);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Remove compare success";
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
