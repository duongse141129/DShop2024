using DShop2024.Models;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Dashboard")]
    [Authorize(Roles = "ADMIN")]
    public class DashboardController : Controller
    {
        private readonly DShopContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DashboardController(DShopContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var count_product = _dataContext.Products.Count();
            var count_order = _dataContext.Orders.Count();
            var count_category = _dataContext.Categories.Count();
            var count_user = _dataContext.Users.Count();
            ViewBag.CountProduct = count_product;
            ViewBag.CountOrder = count_order;
            ViewBag.CountCategory = count_category;
            ViewBag.CountUser = count_user;
            return View();
        }

        //[HttpPost]
        //[Route("SubmitFilterDate")]
        //public IActionResult SubmitFilterDate(string filterdate)
        //{
        //    var dateselect = DateTime.Parse(filterdate).ToString("yyyy-MM-dd");
        //    var chartData = _dataContext.Orders
        //   .Where(o => o.CreatedDate.ToString("yyyy-MM-dd") == dateselect) // Optional: Filter by date
        //  .Join(_dataContext.OrderDetails,
        //      o => o.Id,
        //      od => od.OrderId,
        //      (o, od) => new StatisticalModel
        //      {
        //          date = o.CreatedDate,
        //          revenue = od.Quantity * od.Price, // Calculate revenue based on order details
        //          orders = 1 // Assuming each order detail represents one order
        //      })
        //  .GroupBy(s => s.date)
        //  .Select(group => new StatisticalModel
        //  {
        //      date = group.Key,
        //      revenue = group.Sum(s => s.revenue),
        //      orders = group.Count()
        //  })
        //  .ToList();

        //    return Json(chartData);
        //}
        //[HttpPost]
        //[Route("SelectFilterDate")]
        //public IActionResult SelectFilterDate(string filterdate)
        //{
        //    var chartData = new List<StatisticalModel>();
        //    // Initialize as empty list
        //    var today = DateTime.Today;
        //    var month = new DateTime(today.Year, today.Month, 1);
        //    var first = month.AddMonths(-1);
        //    var last = month.AddDays(-1);


        //    if (filterdate == "last_month")
        //    {
        //        chartData = _dataContext.Orders
        //       .Where(o => o.CreatedDate > first && o.CreatedDate < today)

        //       .Join(_dataContext.OrderDetails,
        //         o => o.Id,
        //         od => od.OrderId,
        //         (o, od) => new StatisticalModel
        //         {
        //             date = o.CreatedDate,
        //             revenue = od.Quantity * od.Price, // Calculate revenue based on order details
        //             orders = 1 // Assuming each order detail represents one order
        //         })
        //         .GroupBy(s => s.date)
        //         .Select(group => new StatisticalModel
        //         {
        //             date = group.Key,
        //             revenue = group.Sum(s => s.revenue),
        //             orders = group.Count()
        //         })
        //         .ToList();
        //    }


        //    return Json(chartData);
        //}

        [HttpPost]
        [Route("GetChartData")]
        public async Task<IActionResult> GetChartData()
        {
            var chartData = await _dataContext.Orders
              .Join(_dataContext.OrderDetails,
                  o => o.Id,
                  od => od.OrderId,
                  (o, od) => new StatisticalModel
                  {
                      date = o.CreatedDate,
                      revenue = od.Quantity * od.Price, // Calculate revenue based on order details
                      orders = 1 // Assuming each order detail represents one order
                  })
              .GroupBy(s => s.date.Month)
              .Select(group => new StatisticalViewModel
              {
                  date = group.Key,
                  revenue = group.Sum(s => s.revenue),
                  orders = group.Count()
              })
              .OrderBy(s => s.date)
              .ToListAsync();

            var x = Json(chartData);
            return x;
        }


    }
}
