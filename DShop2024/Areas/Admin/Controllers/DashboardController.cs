using AutoMapper;
using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Dashboard")]
	[Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
	public class DashboardController : Controller
    {
        private readonly DShopContext _dataContext;

        private readonly IMapper _mapper;
        private readonly string sidebar = "dashboard";

        public DashboardController(DShopContext context, IMapper mapper)
        {
            _dataContext = context;
            _mapper = mapper;
        }
        public async Task<IActionResult> Index()
        {
            ViewBag.sidebar = sidebar;
            var countProduct = _dataContext.Products.Where(p => p.Status != 0 && p.Stock > 0).Count();
            var countUser = _dataContext.Users.Where(p => p.Status != 0).Count();
            var countOrder = _dataContext.Orders.Where(p => p.Status != 0).Count();
        
            var countCancelOrder = _dataContext.Orders.Where(p => p.Status == 0).Count();
            var countNewOrder = _dataContext.Orders.Where(p => p.Status == 1).Count();
            var countAcceptedOrder = _dataContext.Orders.Where(p => p.Status == 2).Count();
            var countDeliveryOrder = _dataContext.Orders.Where(p => p.Status == 3).Count();
            var countCompletedOrder = _dataContext.Orders.Where(p => p.Status == 4).Count();

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
                            .Select(g => new ProductViewModel
                            {
                                Id = g.Key.Id,
                                ProductName = g.Key.ProductName,
                                Image = g.Key.Image,
                                OriginalPrice = g.Key.OriginalPrice,
                                Price = g.Key.Price,
                                QuantitySold = g.Sum(x => x.od.Quantity)
                            })
                            .OrderByDescending(x => x.QuantitySold)
                            .Take(5)
                            .ToListAsync();


            var listCustomer = await (from u in _dataContext.Users
                                      join ur in _dataContext.UserRoles on u.Id equals ur.UserId
                                      join r in _dataContext.Roles on ur.RoleId equals r.Id
                                      where r.Name == RoleName.Customer && u.Status != 0
                                      select u).ToListAsync();

            var listContact = await _dataContext.Contacts.Include(u => u.User).Where(c => c.Status != 0)
                                                                                .OrderBy(c => c.Status)
                                                                                .ThenByDescending(d => d.DateSent)
                                                                                .ToListAsync();

            var listCoupon = await _dataContext.Coupons
                                    .Where(c => c.Status != 0 && c.Quantity > 0)
                                    .Where(c => c.DateExpired.Date >= DateTime.Today.Date && c.DateStart <= DateTime.Today.Date)
                                    .OrderByDescending(c => c.Status)
                                    .ToListAsync();
            var listAvailableCouponVM = _mapper.Map<List<CouponViewModel>>(listCoupon);

            DataDashboardViewModel dataDashboard = new DataDashboardViewModel 
            { 
                CountProduct = countProduct,
                CountUser = countUser,
                CountOrder = countOrder,
                CountCancelOrder = countCancelOrder,
                CountNewOrder = countNewOrder,
                CountAcceptedOrder = countAcceptedOrder,
                CountDeliveryOrder = countDeliveryOrder,
                CountCompletedOrder = countCompletedOrder,
                BestSaleProducts = bestSaleProducts,
                ListCustomer = listCustomer,
                ListContact = listContact,
                ListAvailableCouponVM = listAvailableCouponVM
            };
            return View(dataDashboard);
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
                       .OrderByDescending( b => b.value)
                       .ToListAsync();          
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
                                 .OrderByDescending(b => b.value)
                                 .ToListAsync();
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
