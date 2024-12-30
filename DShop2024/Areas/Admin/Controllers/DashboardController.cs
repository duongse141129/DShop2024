using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Dashboard")]
    [Authorize(Roles = "ADMIN")]
    public class DashboardController : Controller
    {
        private readonly DShopContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DashboardController(DShopContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [Route("Index")]
        public IActionResult Index()
        {
            var countProduct = _context.Products.Count();
            var countOrder = _context.Orders.Count();
            var countCategory = _context.Categories.Count();
            var countUser = _context.Users.Count();
            ViewBag.CountProduct = countProduct;   
            ViewBag.CountOrder = countOrder;   
            ViewBag.CountCategory = countCategory;   
            ViewBag.CountUser = countUser;

            return View();
        }

        [HttpPost]
        [Route("GetChartData")]
        public IActionResult GetChartData()
        {
            var data = _context.Statisticals.Select(s => new {
                                         date = s.DateCreate.ToString("yyyy-MM-dd"),
                                         sold = s.Sold,
                                         quantity = s.Quantity,
                                         revenua = s.Revenue,
                                         profit = s.Profit     
                                        }).ToList();
            return Json(data);
        }

        [HttpPost]
        [Route("GetChartDataBySelect")]
        public IActionResult GetChartDataBySelect(DateTime startDate, DateTime endDate)
        {
            var data =  _context.Statisticals.
                Where(s => s.DateCreate >= startDate && s.DateCreate <= endDate)
                .Select(s => new
                {
                    date = s.DateCreate.ToString("yyyy-MM-dd"),
                    sold = s.Sold,
                    quantity = s.Quantity,
                    revenua = s.Revenue,
                    profit = s.Profit
                }).ToList();

            return Json(data);
        }

        [HttpPost]
        [Route("FilterData")]
        public IActionResult FilterData(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Statisticals.AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(s => s.DateCreate >= fromDate);
            }
            if(toDate.HasValue)
            {
                query = query.Where(s => s.DateCreate >= toDate);
            }

            var data = query.Select(s => new
            {
                date = s.DateCreate.ToString("yyyy-MM-dd"),
                sold = s.Sold,
                quantity = s.Quantity,
                revenua = s.Revenue,
                profit = s.Profit
            }).ToList();

            return Json(data);
        }
    }
}
