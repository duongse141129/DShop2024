using DShop2024.Models;
using DShop2024.ViewModels;
using Humanizer;
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
        public async Task<IActionResult> Index()
        {
            var count_product = _dataContext.Products.Count();
            var count_order = _dataContext.Orders.Count();
            var count_category = _dataContext.Categories.Count();
            var count_brand = _dataContext.Brands.Count();
            var count_user = _dataContext.Users.Count();
            ViewBag.CountProduct = count_product;
            ViewBag.CountOrder = count_order;
            ViewBag.CountCategory = count_category;
            ViewBag.Brand = count_brand;
            ViewBag.CountUser = count_user;


            var bestSaleProduct = await _dataContext.Products
            .Where(p => p.Status != 0 && _dataContext.OrderDetails.Any(od => od.Status != 0 && od.Order.Status != 0))
            .GroupBy(p => p.ProductName)
            .Select(g => new
            {
                ProductName = g.Key,
                QuantitySold = g.SelectMany(p => p.OrderDetails)
            .Where(o => o.Status != 0 && o.Order.Status != 0 )
            .Sum(od => od.Quantity)
            })
            .OrderByDescending(g => g.QuantitySold)
            .Take(5)
            .ToListAsync();

            //var bestSaleProduct = await (from p in _dataContext.Products
            //                             join od in _dataContext.OrderDetails on p.Id equals od.ProductId
            //                             join o in _dataContext.Orders on od.OrderId equals o.Id
            //                             where o.Status != 0 && od.Status != 0 && o.Status != 0
            //                             group od by od.Quantity into g 
            //                             select new { ProductName = p.ProductName, QuantitySold = g.Sum() }
            //                           ).ToListAsync();
                                       



            var productsSoldByBrand = _dataContext.Brands
                            .Where(b => b.Status != 0)
                            .Join(_dataContext.Products.Where(p => p.Status != 0),
                            b => b.Id,
                            p => p.BrandId,
                            (b, p) => new { b, p })
                            .Join(_dataContext.OrderDetails.Where(od => od.Status != 0),
                            bp => bp.p.Id,
                            od => od.ProductId,
                            (bp, od) => new { bp.b, bp.p, od })
                            .Join(_dataContext.Orders.Where(o => o.Status != 0),
                            bpo => bpo.od.OrderId,
                            o => o.Id,
                            (bpo, o) => new { bpo.b, bpo.od })
                            .GroupBy(x => x.b.BrandName)
                            .Select(g => new
                            {
                                BrandName = g.Key,
                                QuantitySold = g.Sum(x => x.od.Quantity)
                            })
                            .ToList();

            var productsSoldByCategory = _dataContext.Categories
                         .Where(c => c.Status != 0)
                         .Join(_dataContext.Products.Where(p => p.Status != 0),
                         b => b.Id,
                         p => p.CategoryId,
                         (b, p) => new { b, p })
                         .Join(_dataContext.OrderDetails.Where(od => od.Status != 0),
                         bp => bp.p.Id,
                         od => od.ProductId,
                         (bp, od) => new { bp.b, bp.p, od })
                         .Join(_dataContext.Orders.Where(o => o.Status != 0),
                         bpo => bpo.od.OrderId,
                         o => o.Id,
                         (bpo, o) => new { bpo.b, bpo.od })
                         .GroupBy(x => x.b.CategoryName)
                         .Select(g => new
                         {
                             BrandName = g.Key,
                             QuantitySold = g.Sum(x => x.od.Quantity)
                         })
                         .ToList();

            return View();
        }

        [HttpPost]
        [Route("SubmitFilterDate")]
        public IActionResult SubmitFilterDate(string filterdate)
        {
            var dateselect = DateTime.Parse(filterdate).ToString("yyyy-MM-dd");
            var chartData = _dataContext.Orders
           .Where(o => o.CreatedDate.ToString("yyyy-MM-dd") == dateselect) // Optional: Filter by date
          .Join(_dataContext.OrderDetails,
              o => o.Id,
              od => od.OrderId,
              (o, od) => new StatisticalModel
              {
                  date = o.CreatedDate,
                  revenue = od.Quantity * od.Price, // Calculate revenue based on order details
                  orders = 1 // Assuming each order detail represents one order
              })
          .GroupBy(s => s.date)
          .Select(group => new StatisticalModel
          {
              date = group.Key,
              revenue = group.Sum(s => s.revenue),
              orders = group.Count()
          })
          .ToList();

            return Json(chartData);
        }

        [HttpPost]
        [Route("SelectFilterDate")]
        public async Task<IActionResult> SelectFilterDate(string filterdate)
        {
            var chartData = new List<StatisticalModel>();
            // Initialize as empty list
            var today = DateTime.Today;
            var month = new DateTime(today.Year, today.Month, 1);
            var first = month.AddMonths(-1);
            var last = month.AddDays(-1);


            if (filterdate == "last_month")
            {
                chartData = _dataContext.Orders
               .Where(o => o.CreatedDate > first && o.CreatedDate < today)

               .Join(_dataContext.OrderDetails,
                 o => o.Id,
                 od => od.OrderId,
                 (o, od) => new StatisticalModel
                 {
                     date = o.CreatedDate,
                     revenue = od.Quantity * od.Price, // Calculate revenue based on order details
                     orders = 1 // Assuming each order detail represents one order
                 })
                 .GroupBy(s => s.date.Date)
                 .Select(group => new StatisticalModel
                 {
                     date = group.Key.Date,
                     revenue = group.Sum(s => s.revenue),
                     orders = group.Count()
                 })
                 .ToList();
            }
            if (filterdate == "this_month")
            {
                var yearNow = DateTime.Now.Year;
                var monthNow = DateTime.Now.Month;

                var chartDataTM = await _dataContext.Orders
               .Where(o => o.CreatedDate.Month > monthNow-1 && o.CreatedDate.Month <= monthNow && o.CreatedDate.Year == yearNow)
               .Join(_dataContext.OrderDetails,
                 o => o.Id,
                 od => od.OrderId,
                 (o, od) => new StatisticalModel
                 {
                     date = o.CreatedDate,
                     revenue = od.Quantity * od.Price, // Calculate revenue based on order details
                     orders = 1 // Assuming each order detail represents one order
                 })
                 .GroupBy(s => s.date.Day)
                 .Select(group => new StatisticalViewModel
                 {
                     date = group.Key,
                     revenue = group.Sum(s => s.revenue),
                     orders = group.Count()
                 })
                 .ToListAsync();

                var x = Json(chartDataTM);
                return x;
            }
            if (filterdate == "last_year")
            {
                var yearNow = DateTime.Now.Year;

                var chartDataLS = await _dataContext.Orders
                        .Where (o => o.CreatedDate.Year == yearNow -1)
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

                        var x = Json(chartDataLS);
                        return x;
            }
            //if (filterdate == "all_year")
            //{
            //    var yearNow = DateTime.Today.Year;
            //    var chartDataTY = await _dataContext.Orders
            //         .Where(o => o.CreatedDate.Year == yearNow && o.Status != 0 )
            //  .Join(_dataContext.OrderDetails,
            //      o => o.Id,
            //      od => od.OrderId,
            //      (o, od) => new StatisticalModel
            //      {
            //          date = o.CreatedDate,
            //          revenue = od.Quantity * od.Price, // Calculate revenue based on order details
            //      })
            //  .GroupBy(s => s.date.Month)
            //  .Select(group => new StatisticalViewModel
            //  {
            //      date = group.Key,
            //      revenue = group.Sum(s => s.revenue)
            //  })
            //  .OrderBy(s => s.date)
            //  .ToListAsync();

            //    var x = Json(chartDataTY);
            //    return x;
            //}

            if (filterdate == "all_year")
            {
                var yearNow = DateTime.Today.Year;

                var chartDataAY = _dataContext.Orders
               .Where(o => o.CreatedDate.Year == yearNow && o.Status != 0) // Optional: Filter by date
              .Join(_dataContext.OrderDetails,
                  o => o.Id,
                  od => od.OrderId,
                  (o, od) => new StatisticalModel
                  {
                      date = o.CreatedDate,
                      revenue = od.Quantity * od.Price, // Calculate revenue based on order details
                      orders = o.Id // Assuming each order detail represents one order
                  })
              .GroupBy(s => s.date.Month)
              .Select(group => new StatisticalViewModel
              {
                  date = group.Key,
                  revenue = group.Sum(s => s.revenue),
                  orders = group.Count()
              })
              .Distinct()
              .ToList();

                //foreach (var item in chartDataAY)
                //{
                //    item.orders = await _dataContext.Orders
                //        .Where(o => o.Status != 0 && o.CreatedDate.Year == yearNow 
                //        && o.CreatedDate.Month == item.date)
                //        .CountAsync();
                //}

                var x = Json(chartDataAY);
                return x;
              
            }


            return Json(chartData);
        }

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

        [HttpPost]
        [Route("GetChartBrand")]
        public async Task<IActionResult> GetChartBrand()
        {
            
            var chartDataBrand = await _dataContext.Brands.Where(c => c.Status != 0).ToListAsync();
            List<NameAndValuecs> nav = new List<NameAndValuecs>
            {
                new NameAndValuecs{label = "asd", value= 50},
                new NameAndValuecs{label = "gfh", value= 35},
                new NameAndValuecs{label = "rty", value= 55},
                new NameAndValuecs{label = "khhjkh", value= 10},
                new NameAndValuecs{label = "ghj", value= 30},
                new NameAndValuecs{label = "werew", value= 2},
            };
            var x = Json(nav);
            return x;
        }


        [HttpPost]
        [Route("GetChartcategories")]
        public async Task<IActionResult> GetChartcategories()
        {
            var chartData = await _dataContext.Categories.Where(c => c.Status != 0).ToListAsync();
            var x = Json(chartData);
            return x;
        }


    }
}
