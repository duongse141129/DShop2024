using DShop2024.Hubs;
using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly DShopContext _context;
        private readonly UserManager<AppUserModel> _userManager;

        public OrderController(DShopContext context, UserManager<AppUserModel> userManager)
        {
            _context = context;
            _userManager = userManager;

        }
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 2)
        {
            var user = await _userManager.GetUserAsync(this.User);
            IQueryable<OrderModel> listOrder = _context.Orders.Include(o => o.User)
                                                  .Include(od => od.OrderDetails)
                                                  .ThenInclude(p => p.Product)
                                                  .Include(c => c.OrderCoupons)
                                                  .ThenInclude(c => c.Coupon)
                                                  .Where(o => o.UserId == user.Id)
                                                  .OrderByDescending(o => o.CreatedDate);

            int totalOrder = listOrder.Count();
            if (pagesSize <= 0)
                pagesSize = 10;
            int countPages = (int)Math.Ceiling((double)totalOrder / 2);

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

            var orders = await listOrder.Skip((currentPage - 1) * pagesSize)
                        .Take(pagesSize).ToListAsync();
            ViewBag.pagingModel = pagingModel;
            return View(orders);
        }
    }
}
