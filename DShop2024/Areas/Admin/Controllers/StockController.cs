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

        public StockController(DShopContext context, UserManager<AppUserModel> userManager)
		{
			_context = context;
            _userManager = userManager;
        }
		public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 10)
		{
            ViewBag.sidebar = Menu.Admin.Stock;
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
            ViewBag.sidebar = Menu.Admin.Stock;

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
            ViewBag.sidebar = Menu.Admin.Stock;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImportFromFile(IFormFile file)
        {
            ViewBag.sidebar = Menu.Admin.Stock;     
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
            ViewBag.sidebar = Menu.Admin.Stock;
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
    }
}
