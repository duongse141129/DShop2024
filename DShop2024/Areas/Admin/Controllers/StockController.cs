using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.ViewModels;
using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
    public class StockController : Controller
	{
		private readonly DShopContext _context;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly string sidebar = "stockIn";

        public StockController(DShopContext context, UserManager<AppUserModel> userManager)
		{
			_context = context;
            _userManager = userManager;
        }
		public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 10)
		{
            ViewBag.sidebar = sidebar;
            IQueryable<ReceivingStockModel> listStockIn =  _context.ReceivingStocks
                            .Where(s => s.Status != 0)
                            .Include(p => p.Product)
                            .Include(p => p.User)
                                     .OrderByDescending(o => o.DateReceive);
            int totalOrder = listStockIn.Count();
            if (pagesSize <= 0)
                pagesSize = 10;
            int countPages = (int)Math.Ceiling((double)totalOrder / 10);

            if (currentPage > countPages)
                currentPage = countPages;
            if (currentPage < 1)
                currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new
                {
                    p = pageNumber,
                    pagesSize = pagesSize
                })
            };

            var stockIns = await listStockIn.Skip((currentPage - 1) * pagesSize)
                        .Take(pagesSize).ToListAsync();

            ViewBag.pagingModel = pagingModel;
            return View(stockIns);
		}

        [HttpPost]
        public async Task<IActionResult> SearchByProductId(int productId)
        {
            ViewBag.sidebar = sidebar;

            var product = await _context.Products.FirstOrDefaultAsync( p => p.Id == productId);
            if (product != null)
            {
                if(product.Status == 0)
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "This product has been removed.";
                    return RedirectToAction("Index");
                }

                return RedirectToAction("AddQuantity", "ProductManage",new { area = "Admin", Id = product.Id });
            }
            TempData[DShopConst.TEMPDATA_ERROR] = "Not found" ;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public  IActionResult ImportFromFIle()
        {
            ViewBag.sidebar = sidebar;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImportFromFile(IFormFile file)
        {
            ViewBag.sidebar = sidebar;     
            if (file != null && file.Length > 0)
            {
                var uploadDirectory = $"{Directory.GetCurrentDirectory()}\\wwwroot\\Uploads";

                if (!Directory.Exists(uploadDirectory))
                {
                    Directory.CreateDirectory(uploadDirectory);
                }

                var filePath = Path.Combine(uploadDirectory, file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
         
                using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    var excelData = new List<NameAndValueVM>();
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {

                        do
                        {
                            while (reader.Read())
                            {
                                try
                                {
                                    var rowData = new NameAndValueVM();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {

                                        if (i == 0)
                                        {
                                            rowData.label = reader.GetString(i);
                                        }
                                        if (i == 1)
                                        {
                                            rowData.value = int.Parse(reader.GetValue(i).ToString());
                                        }

                                    }
                                    var product = await _context.Products.AnyAsync(p => p.ProductName.ToUpper().Equals(rowData.label.ToString().ToUpper()));
                                    if (product)
                                    {
                                        var item = excelData.FirstOrDefault(p => p.label.ToUpper().Equals( rowData.label.ToUpper()));
                                        if(item != null)
                                        {
                                            item.value += rowData.value;
                                        }
                                        else
                                        {
                                            excelData.Add(rowData);
                                        }
                                    }
                                }
                                catch (FormatException)
                                {
                                }

                            }
                        } while (reader.NextResult());
                    }
                    return View(excelData);
                }
            }
            return View();
        }

        [HttpPost]
        public  async Task<IActionResult> Save( List<NameAndValueVM> items)
        {
            ViewBag.sidebar = sidebar;
            var user = await _userManager.GetUserAsync(this.User);
            try
            {
                foreach (var item in items)
                {
                    var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductName.ToUpper().Equals(item.label.ToString().ToUpper()));
                    if(product != null)
                    {
                        product.Stock += item.value;
                        _context.Products.Update(product);

                        ReceivingStockModel receivingStock = new ReceivingStockModel() { 
                            DateReceive = DateTime.Now ,
                            ProductId = product.Id,
                            UserId = user.Id,
                            Quantity = item.value,
                            Status = 1
                        };
                        await _context.ReceivingStocks.AddAsync(receivingStock);
                        await _context.SaveChangesAsync();
                    }
                }
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Save from file successful";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Save from file fail "+ ex.Message;
                return RedirectToAction("Index");
            }
        }

        #region


        //public static DateTime GetRandomDateTime(DateTime startDate, DateTime endDate)
        //{
        //    Random random = new Random();

        //    TimeSpan dateRange = endDate - startDate;
        //    int totalDays = (int)dateRange.TotalDays;

        //    int randomDays = random.Next(totalDays);

        //    int randomHours = random.Next(24);
        //    int randomMinutes = random.Next(60);
        //    int randomSeconds = random.Next(60);

        //    DateTime randomDate = startDate.AddDays(randomDays)
        //                                  .AddHours(randomHours)
        //                                  .AddMinutes(randomMinutes)
        //                                  .AddSeconds(randomSeconds);
        //    return randomDate;
        //}

        //public async Task<IActionResult> CreateRating()
        //{
        //    try
        //    {
        //        var customers = await (from u in _context.Users
        //                               join ur in _context.UserRoles on u.Id equals ur.UserId
        //                               join r in _context.Roles on ur.RoleId equals r.Id
        //                               where r.Name == RoleName.Customer && u.Status != 0
        //                               orderby u.Id descending
        //                               select  u).ToListAsync();

        //        foreach (var customer in customers)
        //        {               
        //            Random random = new Random();
        //            DateTime dateStart = new DateTime(2025, 1, 1);
        //            DateTime dateEnd = new DateTime(2025, 2, 25);
        //            int i;

        //            for (i = 32; i < 82; i++)
        //            {
        //                var checkOrder = await (from o in _context.Orders
        //                                        join od in _context.OrderDetails on o.Id equals od.OrderId
        //                                        where o.UserId == customer.Id && od.ProductId == i && o.Status == 4
        //                                        select o).FirstOrDefaultAsync();

        //                if (checkOrder != null)
        //                {
        //                    RatingModel rating = new RatingModel();
        //                    int star = random.Next(3, 6);
        //                    rating.Star = star;
        //                    if (star == 3)
        //                    {
        //                        rating.Comment = "Decent backpack, but nothing special";
        //                    }
        //                    else if (star == 4)
        //                    {
        //                        rating.Comment = "Great quality, just a bit heavy";
        //                    }
        //                    else if (star == 5)
        //                    {
        //                        rating.Comment = "Excellent. This backpack exceeded my expectations.";
        //                    }
        //                    rating.UserId = customer.Id;
        //                    rating.RatingDateTime = GetRandomDateTime(dateStart, dateEnd); ;
        //                    rating.ProductId = i;
        //                    rating.Status = 1;
        //                    await _context.Ratings.AddAsync(rating);
        //                    await _context.SaveChangesAsync();
        //                }
        //            }
        //        }
        //        TempData[DShopConst.TEMPDATA_SUCCESS] = "success";
        //        return RedirectToAction("Index");
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData[DShopConst.TEMPDATA_ERROR] = "error "+ ex.Message;
        //        return RedirectToAction("Index");
        //    }
        //}

        #endregion
    }
}
