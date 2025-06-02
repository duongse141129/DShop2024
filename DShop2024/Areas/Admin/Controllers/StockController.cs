using DShop2024.EnumData;
using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
    public class StockController : Controller
	{
		private readonly DShopContext _context;
        private readonly string sidebar = "stockIn";
        public StockController(DShopContext context)
		{
			_context = context;
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
	}
}
