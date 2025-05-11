using DShop2024.EnumData;
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
	[Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
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
            var count_product = _dataContext.Products.Where(p => p.Status != 0).Count();
            var count_order = _dataContext.Orders.Where(p => p.Status != 0).Count();
            var count_category = _dataContext.Categories.Where(p => p.Status != 0).Count();
            var count_brand = _dataContext.Brands.Where(p => p.Status != 0).Count();
            var count_user = _dataContext.Users.Where(p => p.Status != 0).Count();
            ViewBag.CountProduct = count_product;
            ViewBag.CountOrder = count_order;
            ViewBag.CountCategory = count_category;
            ViewBag.Brand = count_brand;
            ViewBag.CountUser = count_user;

            var bestSaleProducts = await _dataContext.Products
                            .Where(p => p.Status != 0)
                            .Join(_dataContext.OrderDetails.Where(od => od.Status != 0),
                            p => p.Id,
                            od => od.ProductId,
                            (p, od) => new { p, od })
                            .Join(_dataContext.Orders.Where(o => o.Status != 0),
                            pod => pod.od.OrderId,
                            o => o.Id,
                            (pod, o) => new { pod.p, pod.od, o })
                            .GroupBy(x => new
                            {
                                x.p.Id,
                                x.p.ProductName,
                                x.p.Image,
                                x.p.OriginalPrice,
                                x.p.Price
                            })
                            .Select(g => new
                            {
                                g.Key.Id,
                                g.Key.ProductName,
                                g.Key.Image,
                                g.Key.OriginalPrice,
                                g.Key.Price,
                                QuantitySold = g.Sum(x => x.od.Quantity)
                            })
                            .OrderByDescending(x => x.QuantitySold)
                            .Take(5)
                            .ToListAsync();
            ViewBag.bestSaleProducts = bestSaleProducts;
            return View();
        }



        [HttpPost]
        [Route("SubmitFilterDate")]
        public async Task<IActionResult> SubmitFilterDate(string dateStart, string dateEnd)
        {
            DateTime dateStartSelect = DateTime.Parse(dateStart);
            DateTime dateEndSelect = DateTime.Parse(dateEnd);
            if(dateEndSelect < dateStartSelect)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Date start must <= date end";
                return NoContent();
            }

            var chartDataRangeDay = await _dataContext.Orders
                            .Where(o => o.Status == 4 && o.CreatedDate.Date >= dateStartSelect.Date && o.CreatedDate.Date <= dateEndSelect.Date)
                            .SelectMany(o => o.OrderDetails.Where(od => od.Status != 0), (o, od) => new { o, od })
                            .GroupBy(x => x.o.CreatedDate.Date)
                            .Select(g => new StatisticalViewModel
                            {
                                date = g.Key.ToShortDateString(),
                                revenue = g.Select(x => x.o.TotalPrice).Distinct().Sum(),
                                profit = g.Select(x => x.od.Price - x.od.OriginalPrice).Sum() - g.Sum(y => y.o.ValueCoupon),
                                orders = g.Select(x => x.o.Id).Distinct().Count(),
                                quantitysold = g.Sum(x => x.od.Quantity)
                            })
                            .ToListAsync();

            return Json(chartDataRangeDay);
        }

        [HttpPost]
        [Route("SelectFilterDate")]
        public async Task<IActionResult> SelectFilterDate(string filterdate)
        {
            var chartData = new List<StatisticalViewModel>();
            var today = DateTime.Today;
            var month = DateTime.Today.Month;
            var year = DateTime.Today.Year;
            if (filterdate == "last_month")
            {
                chartData = await getDataByMonth(month-1, year);
                return Json(chartData);
            }
            if (filterdate == "this_month")
            {
                chartData = await getDataByMonth(month, year);
                return Json(chartData);
            }
            if (filterdate == "last_year")
            {
                chartData = await getDataByYear(year-1);
                return Json(chartData);
            }

            if (filterdate == "this_year")
            {
                chartData = await getDataByYear(year);
                return Json(chartData);
              
            }
            return Json(chartData);
        }
     


        [Route("getDataByMonth")]
        public async Task<List<StatisticalViewModel>> getDataByMonth(int month, int year)
        {
            var chartDataYear = await _dataContext.Orders
                .Where(o => o.Status == 4 && o.CreatedDate.Year == year && o.CreatedDate.Month == month)
                .SelectMany(o => o.OrderDetails.Where(od => od.Status != 0), (o, od) => new { o, od })
                .GroupBy(x => x.o.CreatedDate.Day)
                .Select(g => new StatisticalViewModel
                {
                    date = g.Key.ToString(),
                    revenue = g.Select(x => x.o.TotalPrice).Distinct().Sum(),
                    profit = g.Select(x => x.od.Price - x.od.OriginalPrice).Sum() - g.Sum(y => y.o.ValueCoupon),
                    orders = g.Select(x => x.o.Id).Distinct().Count(),
                    quantitysold = g.Sum(x => x.od.Quantity)
                })
                .ToListAsync();
            return chartDataYear;
        }

        [Route("getDataByYear")]
        public async Task<List<StatisticalViewModel>> getDataByYear(int year)
        {
            var chartDataYear= await _dataContext.Orders
                .Where(o => o.Status == 4 && o.CreatedDate.Year == year)
                .SelectMany(o => o.OrderDetails.Where(od => od.Status != 0), (o, od) => new { o, od })
                .GroupBy(x => x.o.CreatedDate.Month)
                .Select(g => new StatisticalViewModel
                {
                    date = g.Key.ToString(),
                    revenue = g.Select(x => x.o.TotalPrice).Distinct().Sum(),
                    profit = g.Select(x => x.od.Price - x.od.OriginalPrice).Sum() - g.Sum(y => y.o.ValueCoupon),
                    orders = g.Select(x => x.o.Id).Distinct().Count(),
                    quantitysold = g.Sum(x => x.od.Quantity)
                })
                .ToListAsync();
            return chartDataYear;
        }

        [HttpPost]
        [Route("GetChartData")]
        public async Task<IActionResult> GetChartData()
        {
            var year = DateTime.Today.Year;
            var chartData = await getDataByYear(year);
            return Json(chartData);
        }



        [HttpPost]
        [Route("GetChartBrand")]
        public async Task<IActionResult> GetChartBrand()
        {

            var productsSoldByBrand = await _dataContext.Brands
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
                       .Select(g => new NameAndValueVM
                       {
                           label = g.Key,
                           value = g.Sum(x => x.od.Quantity)
                       })
                       .ToListAsync();

            var sum = productsSoldByBrand.Sum(n => n.value);
            //foreach (var item in productsSoldByBrand)
            //{
            //    item.value = item.value * 100 / sum;
            //}
            int s = 0;
            for (int i = 0; i < productsSoldByBrand.Count; i++)
            {
                if (i == productsSoldByBrand.Count - 1)
                {
                    productsSoldByBrand[i].value = 100 - s;
                }
                else
                {
                    int v = productsSoldByBrand[i].value * 100 / sum;
                    productsSoldByBrand[i].value = v;
                    s += v;
                }
            }
            return Json(productsSoldByBrand);
        }


        [HttpPost]
        [Route("GetChartCategories")]
        public async Task<IActionResult> GetChartCategories()
        {
            var productsSoldByCategory = await _dataContext.Categories
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
                                 .Select(g => new NameAndValueVM
                                 {
                                     label = g.Key,
                                     value = g.Sum(x => x.od.Quantity)
                                 })
                                 .ToListAsync();
            var sum = productsSoldByCategory.Sum(n => n.value);
            int s = 0;
            for (int i = 0; i < productsSoldByCategory.Count; i++)
            {
                if (i == productsSoldByCategory.Count - 1)
                {
                    productsSoldByCategory[i].value = 100 - s;
                }
                else
                {
                    int v = productsSoldByCategory[i].value * 100 / sum;
                    productsSoldByCategory[i].value = v;
                    s += v;
                }
            }
            return Json(productsSoldByCategory);
        }

        [HttpPost]
        [Route("GetChartStock")]
        public async Task<IActionResult> GetChartStock()
        {
            List<StockViewModel> stockViewModels = new List<StockViewModel> ();
            int year = DateTime.Today.Year;
            int month = DateTime.Today.Month;
            for (int i = 1; i <= month; i++)
            {

                var stockIn = await _dataContext.ReceivingStocks
                            .Where(r => r.DateReceive.Year == year && r.DateReceive.Month == i && r.Status != 0)
                            .SumAsync(x => x.Quantity);

                var stockOut = await _dataContext.Orders.Where(o => o.Status == 4 && o.CreatedDate.Year == year && o.CreatedDate.Month == i)
                                        .SelectMany(o => o.OrderDetails)
                                        .Where(od => od.Status != 0)
                                        .SumAsync(od => (int?)od.Quantity) ?? 0;

                StockViewModel stockViewModel = new StockViewModel 
                { 
                    month = i,
                    stockIn = stockIn,
                    stockOut = stockOut,
                };
                stockViewModels.Add(stockViewModel);
            }
            return Json(stockViewModels.OrderBy(s => s.month));
        }

        [HttpPost]
        [Route("GetChartStockFilter")]
        public async Task<IActionResult> GetChartStockFilter(string filterbarchart)
        {
            List<StockViewModel> stockViewModels = new List<StockViewModel>();
            int year = DateTime.Today.Year;
            int month = DateTime.Today.Month;
            if(filterbarchart == "this_year")
            {
                year = DateTime.Today.Year;
            }
            if(filterbarchart == "last_year")
            {
                year = year - 1;
                month = 12;
            }

            for (int i = 1; i <= month; i++)
            {

                var stockIn = await _dataContext.ReceivingStocks
                            .Where(r => r.DateReceive.Year == year && r.DateReceive.Month == i && r.Status != 0)
                            .SumAsync(x => x.Quantity);

                var stockOut = await _dataContext.Orders.Where(o => o.Status == 4 && o.CreatedDate.Year == year && o.CreatedDate.Month == i)
                                        .SelectMany(o => o.OrderDetails)
                                        .Where(od => od.Status != 0)
                                        .SumAsync(od => (int?)od.Quantity) ?? 0;

                StockViewModel stockViewModel = new StockViewModel
                {
                    month = i,
                    stockIn = stockIn,
                    stockOut = stockOut,
                };
                stockViewModels.Add(stockViewModel);
            }
            return Json(stockViewModels.OrderBy(s => s.month));
        }



    }
}
