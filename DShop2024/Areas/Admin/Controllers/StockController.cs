using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.Repository;
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
    [SidebarMenu(Menu.Admin.Inventory, SubMenu.Inventory.StockIn)]
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
            
            IQueryable<ReceivingStockModel> listStockIn =  _context.ReceivingStocks
                            .Where(s => s.Status != 0)
                            .Include(p => p.Product)
                            .Include(p => p.User)
                                     .OrderByDescending(o => o.DateReceive);
            int totalStock = await listStockIn.CountAsync();
            if (pagesSize <= 0)
                pagesSize = 10;
            int countPages = (int)Math.Ceiling((double)totalStock / 10);

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
            
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ImportFromFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return View();
            }

            var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "importStocks");
            if (!Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory);
            }

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{DateTime.Now:yyyyMMdd_HHmmssfff}_stock{extension}";
            var filePath = Path.Combine(uploadDirectory, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var productNames = (await _context.Products
                            .Select(p => p.ProductName)
                            .ToListAsync())
                            .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var excelData = new List<NameAndValueVM>();

            using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                do
                {
                    if (!reader.Read())
                        continue;
                    while (reader.Read())
                    {
                        var productName = reader.GetValue(0)?.ToString()?.Trim();

                        if (string.IsNullOrWhiteSpace(productName))
                            continue;

                        if (!int.TryParse(reader.GetValue(1)?.ToString(), out int quantity))
                            continue;

                        if (!productNames.Contains(productName))
                            continue;

                        var item = excelData.FirstOrDefault(x => x.label.Equals(productName, StringComparison.OrdinalIgnoreCase));

                        if (item != null)
                        {
                            item.value += quantity;
                        }
                        else
                        {
                            excelData.Add(new NameAndValueVM
                            {
                                label = productName,
                                value = quantity
                            });
                        }
                    }

                } while (reader.NextResult());
            }
            return View(excelData);
        }


        [HttpPost]
        public  async Task<IActionResult> Save( List<NameAndValueVM> items)
        {
            
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
