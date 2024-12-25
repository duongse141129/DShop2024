using DShop2024.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace DShop2024.Controllers
{
    public class HomeController : Controller
    {
        private readonly DShopContext _dataContext;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, DShopContext context)
        {
            _logger = logger;
            _dataContext = context;
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
    }
}
