using AutoMapper;
using DShop2024.EnumData;
using DShop2024.Repository;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Dashboard")]
	[Authorize(Roles = RoleName.Administrator )]
    [SidebarMenu(Menu.Admin.Dashboard)]
    public class DashboardController : Controller
    {
        private readonly DShopContext _context;
        private readonly IMapper _mapper;

        public DashboardController(DShopContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var orderStatusCounts = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();
            int countCancelOrder = orderStatusCounts.FirstOrDefault(x => x.Status == 0)?.Count ?? 0;
            int countNewOrder = orderStatusCounts.FirstOrDefault(x => x.Status == 1)?.Count ?? 0;
            int countAcceptedOrder = orderStatusCounts.FirstOrDefault(x => x.Status == 2)?.Count ?? 0;
            int countDeliveryOrder = orderStatusCounts.FirstOrDefault(x => x.Status == 3)?.Count ?? 0;
            int countCompletedOrder = orderStatusCounts.FirstOrDefault(x => x.Status == 4)?.Count ?? 0;
            int countOrder = orderStatusCounts.Where(x => x.Status != 0).Sum(x => x.Count);

            var countProduct = await _context.Products.Where(p => p.Status != 0 && p.Stock > 0).CountAsync();
            var countUser = await _context.Users.Where(p => p.Status != 0).CountAsync();
            var countQuantityReturn = await _context.ReturnDetails.Where(p => p.Return.Status != 0).SumAsync(re => re.Quantity);

            string currentYear = DateTime.Now.Year.ToString();
            ViewBag.StatisticFilterOptions = new SelectList(DateFilterConstantData.GetDynamicStatisticsOptions(), "Value", "Text", currentYear);
            ViewBag.ImportExportFilterOptions = new SelectList(DateFilterConstantData.GetDynamicImportExportOptions(), "Value", "Text", currentYear);

            var now = DateTime.Now;
            var bestSaleProducts = await _context.Products
                                        .Where(p => p.Status != 0)
                                        .Select(p => new ProductViewModel
                                        {
                                            Id = p.Id,
                                            ProductName = p.ProductName,
                                            MainImage = p.MainImage,
                                            Price = p.Price,
                                            OriginalPrice = p.OriginalPrice,
                                            Stock = p.Stock,
                                            BrandName = p.Brand.BrandName,
                                            CategoryName = p.Category.CategoryName,

                                            QuantitySold = p.OrderDetails.Where(od => od.Order.Status != 0).Sum(od => (int?)od.Quantity) ?? 0,
                                            AveragePoint = p.Ratings.Where(r => r.Status != 0).Average(r => (double?)r.Star) ?? 0,

                                            SalePrice = p.Sales.Where(s => s.Status != 0
                                                                     && now >= s.SaleStartDate
                                                                     && now <= s.SaleEndDate)
                                                            .Select(s => (decimal?)s.SalePrice)
                                                            .FirstOrDefault(),
                                            IsOnSale = p.Sales.Any(s => s.Status != 0 &&
                                                                now >= s.SaleStartDate &&
                                                                now <= s.SaleEndDate)
                                        })
                                        .Where(p => p.QuantitySold > 0)
                                        .OrderByDescending(p => p.QuantitySold)
                                        .Take(5)
                                        .ToListAsync();

            var listCustomer = await (from u in _context.Users
                                      join ur in _context.UserRoles on u.Id equals ur.UserId
                                      join r in _context.Roles on ur.RoleId equals r.Id
                                      where r.Name == RoleName.Customer && u.Status != 0
                                      select u)
                                      .Take(20)
                                      .ToListAsync();

            var listContact = await _context.Contacts
                .Include(u => u.User)
                .Where(c => c.Status != 0)
                .OrderBy(c => c.Status)
                .ThenByDescending(d => d.DateSent)
                .Take(10)
                .ToListAsync();

            var today = DateTime.Today;
            var listCoupon = await _context.Coupons 
                .Where(c => c.Status != 0 && c.Quantity > 0)
                .Where(c => c.DateExpired.Date >= today && c.DateStart <= today)
                .OrderByDescending(c => c.Status)
                .Take(10)
                .ToListAsync();
            var listAvailableCouponVM = _mapper.Map<List<CouponViewModel>>(listCoupon);

            var dataDashboard = new DataDashboardViewModel
            {
                CountProduct = countProduct,
                CountUser = countUser,
                CountOrder = countOrder,
                CountCancelOrder = countCancelOrder,
                CountNewOrder = countNewOrder,
                CountAcceptedOrder = countAcceptedOrder,
                CountDeliveryOrder = countDeliveryOrder,
                CountCompletedOrder = countCompletedOrder,
                CountQuantityReturn = countQuantityReturn,
                BestSaleProducts = bestSaleProducts,
                ListCustomer = listCustomer,
                ListContact = listContact,
                ListAvailableCouponVM = listAvailableCouponVM
            };
            return View(dataDashboard);
        }


        //"If the customer returns the goods, the shop has to cover the shipping cost itself."
        [HttpPost]
        [Route("SubmitFilterDate")]
        public async Task<IActionResult> SubmitFilterDate(string dateStart, string dateEnd)
        {

            if (!DateTime.TryParse(dateStart, out DateTime dateStartSelect) ||
                        !DateTime.TryParse(dateEnd, out DateTime dateEndSelect))
            {
                return Json(new { success = false, message = "Invalid date format." });
            }

            if (dateEndSelect < dateStartSelect)
            {
                return Json(new { success = false, message = "Start date must be earlier than or equal to end date." });
            }

            if ((dateEndSelect - dateStartSelect).TotalDays > 31)
            {
                return Json(new { success = false, message = "Date range cannot exceed 31 days." });
            }
            var chartDataRangeDay = await _context.Orders
                    .Where(o => o.Status == 4 && o.CreatedDate.Date >= dateStartSelect.Date && o.CreatedDate.Date <= dateEndSelect.Date)
                    .GroupBy(o => new { o.CreatedDate.Date })
                    .Select(g => new StatisticalViewModel
                    {
                        Id = g.Key.Date.Day,
                        date = g.Key.Date.ToShortDateString(),
                        orders = g.Count(),
                        quantitysold = g.SelectMany(o => o.OrderDetails).Sum(od => od.Quantity),

                        revenue = g.Sum(o => o.GrandTotal)
                                  - (g.SelectMany(o => o.Returns)
                                       .Where(r => r.Status == 3)
                                       .Sum(r => (decimal?)r.TotalRefundAmount) ?? 0)
                                  - (g.SelectMany(o => o.Returns)
                                       .Where(r => r.Status == 3)
                                       .Sum(r => (decimal?)r.ReturnShippingCost) ?? 0),

                        profit = g.SelectMany(o => o.OrderDetails)
                                    .Sum(od => (od.Price - od.OriginalPrice) * od.Quantity)
                                 - g.Sum(o => o.ValueCoupon)
                                 - (g.SelectMany(o => o.Returns)
                                      .Where(p => p.Status == 3)
                                      .SelectMany(r => r.ReturnDetails)
                                      .Sum(rd => (decimal?)((rd.PricePerUnit - rd.OriginalPricePerUnit) * rd.Quantity)) ?? 0)
                                 - (g.SelectMany(o => o.Returns)
                                      .Where(r => r.Status == 3)
                                      .Sum(r => (decimal?)r.ReturnShippingCost) ?? 0)
                    })
                    .OrderBy(x => x.Id)
                    .ToListAsync();
            return Json(new { success = true, data = chartDataRangeDay });
        }

        [Route("getDataByMonth")]
        public async Task<List<StatisticalViewModel>> getDataByMonth(int month, int year)
        {
            var chartDataYear = await _context.Orders
                    .Where(o => o.Status == 4 && o.CreatedDate.Year == year && o.CreatedDate.Month == month)
                    .GroupBy(o => new { o.CreatedDate.Day })
                    .Select(g => new StatisticalViewModel
                    {
                        Id = g.Key.Day,
                        date = g.Key.Day.ToString("D2") + "/" + month + "/" + year,
                        orders = g.Count(),
                        quantitysold = g.SelectMany(o => o.OrderDetails).Sum(od => od.Quantity),

                        revenue = g.Sum(o => o.GrandTotal)
                                  - (g.SelectMany(o => o.Returns)
                                       .Where(r => r.Status == 3)
                                       .Sum(r => (decimal?)r.TotalRefundAmount) ?? 0)
                                  - (g.SelectMany(o => o.Returns)
                                       .Where(r => r.Status == 3)
                                       .Sum(r => (decimal?)r.ReturnShippingCost) ?? 0),

                        profit = g.SelectMany(o => o.OrderDetails)
                                    .Sum(od => (od.Price - od.OriginalPrice) * od.Quantity)
                                 - g.Sum(o => o.ValueCoupon)
                                 - (g.SelectMany(o => o.Returns)
                                      .Where(p => p.Status == 3)
                                      .SelectMany(r => r.ReturnDetails)
                                      .Sum(rd => (decimal?)((rd.PricePerUnit - rd.OriginalPricePerUnit) * rd.Quantity)) ?? 0)
                                 - (g.SelectMany(o => o.Returns)
                                      .Where(r => r.Status == 3)
                                      .Sum(r => (decimal?)r.ReturnShippingCost) ?? 0)
                    })
                    .OrderBy(x => x.Id)
                    .ToListAsync();
            return chartDataYear;
        }

        [Route("getDataByYear")]
        public async Task<List<StatisticalViewModel>> getDataByYear(int year)
        {
            var chartDataYear = await _context.Orders
                    .Where(o => o.Status == 4 && o.CreatedDate.Year == year)
                    .GroupBy(o => new { o.CreatedDate.Month })
                    .Select(g => new StatisticalViewModel
                    {
                        Id = g.Key.Month,
                        date = g.Key.Month.ToString("D2") + "/" + year,
                        orders = g.Count(),
                        quantitysold = g.SelectMany(o => o.OrderDetails).Sum(od => od.Quantity),

                        revenue = g.Sum(o => o.GrandTotal)
                                  - (g.SelectMany(o => o.Returns)
                                       .Where(r => r.Status == 3)
                                       .Sum(r => (decimal?)r.TotalRefundAmount) ?? 0)
                                  - (g.SelectMany(o => o.Returns)
                                       .Where(r => r.Status == 3)
                                       .Sum(r => (decimal?)r.ReturnShippingCost) ?? 0),

                        profit = g.SelectMany(o => o.OrderDetails)
                                    .Sum(od => (od.Price - od.OriginalPrice) * od.Quantity)
                                 - g.Sum(o => o.ValueCoupon)
                                 - (g.SelectMany(o => o.Returns)
                                      .Where(p => p.Status == 3)
                                      .SelectMany(r => r.ReturnDetails)
                                      .Sum(rd => (decimal?)((rd.PricePerUnit - rd.OriginalPricePerUnit) * rd.Quantity)) ?? 0)
                                 - (g.SelectMany(o => o.Returns)
                                      .Where(r => r.Status == 3)
                                      .Sum(r => (decimal?)r.ReturnShippingCost) ?? 0)

                    })
                    .OrderBy(x => x.Id)
                    .ToListAsync();
            return chartDataYear;
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
                chartData = await getDataByMonth(month - 1, year);
                return Json(chartData);
            }
            if (filterdate == "this_month")
            {
                chartData = await getDataByMonth(month, year);
                return Json(chartData);
            }
            year = Convert.ToInt32(filterdate);
            chartData = await getDataByYear(year);
            return Json(chartData);
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
            var productsSoldByBrand = await _context.Brands
                       .Where(b => b.Status != 0)
                       .Join(_context.Products.Where(p => p.Status != 0),
                       b => b.Id,
                       p => p.BrandId,
                       (b, p) => new { b, p })
                       .Join(_context.OrderDetails,
                       bp => bp.p.Id,
                       od => od.ProductId,
                       (bp, od) => new { bp.b, bp.p, od })
                       .Join(_context.Orders.Where(o => o.Status != 0),
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
            var productsSoldByCategory = await _context.Categories
                                 .Where(c => c.Status != 0)
                                 .Join(_context.Products.Where(p => p.Status != 0),
                                 b => b.Id,
                                 p => p.CategoryId,
                                 (b, p) => new { b, p })
                                 .Join(_context.OrderDetails,
                                 bp => bp.p.Id,
                                 od => od.ProductId,
                                 (bp, od) => new { bp.b, bp.p, od })
                                 .Join(_context.Orders.Where(o => o.Status != 0),
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

                var stockIn = await _context.ReceivingStocks
                            .Where(r => r.DateReceive.Year == year && r.DateReceive.Month == i && r.Status != 0)
                            .SumAsync(x => x.Quantity);

                var stockOut = await _context.Orders.Where(o => o.Status == 4 && o.CreatedDate.Year == year && o.CreatedDate.Month == i)
                                        .SelectMany(o => o.OrderDetails)
                                        .SumAsync(od => (int?)od.Quantity) ?? 0;
                var returnItems = await _context.Orders.Where(o => o.Status == 4 && o.CreatedDate.Year == year && o.CreatedDate.Month == i)
                    .SelectMany(o => o.Returns).Where(r => r.Status == 3 )
                    .SelectMany(o => o.ReturnDetails)
                    .SumAsync(rt => (int?)rt.Quantity) ?? 0;

                StockViewModel stockViewModel = new StockViewModel 
                { 
                    month = i,
                    stockIn = stockIn,
                    stockOut = stockOut - returnItems,
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
            int year = Convert.ToInt32(filterbarchart); ;
            int month = DateTime.Today.Month;
            if (year != DateTime.Now.Year)
            {
                month = 12;
            }

            for (int i = 1; i <= month; i++)
            {

                var stockIn = await _context.ReceivingStocks
                            .Where(r => r.DateReceive.Year == year && r.DateReceive.Month == i && r.Status != 0)
                            .SumAsync(x => x.Quantity);

                var stockOut = await _context.Orders.Where(o => o.Status == 4 && o.CreatedDate.Year == year && o.CreatedDate.Month == i)
                                        .SelectMany(o => o.OrderDetails)
                                        .SumAsync(od => (int?)od.Quantity) ?? 0;

                var returnItems = await _context.Orders.Where(o => o.Status == 4 && o.CreatedDate.Year == year && o.CreatedDate.Month == i)
                                    .SelectMany(o => o.Returns).Where(r => r.Status == 3)
                                    .SelectMany(o => o.ReturnDetails)
                                    .SumAsync(rt => (int?)rt.Quantity) ?? 0;

                StockViewModel stockViewModel = new StockViewModel
                {
                    month = i,
                    stockIn = stockIn,
                    stockOut = stockOut - returnItems,
                };
                stockViewModels.Add(stockViewModel);
            }
            return Json(stockViewModels.OrderBy(s => s.month));
        }



    }
}
