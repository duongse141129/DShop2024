using DShop2024.EnumData;
using DShop2024.Models;
using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
                                                  .Include(c => c.PaymentMethod)
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

        public async Task<IActionResult> CancelOder(int? id)
        {
            try
            {
                if(id == null)
                {
                    return NotFound();
                }
                var order = await _context.Orders.Where(o => o.Status != 0).FirstOrDefaultAsync( o => o.Id == id );
                if( order == null)
                {
                    return NotFound();
                }
                if( order.Status > 1)
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "The order cannot be canceled. Because the order has been processed.";
                    return RedirectToAction("Index");
                }
                if(order.Status == 1)
                {
                    var listOrderDetail = await _context.OrderDetails.Where(d => d.OrderId == order.Id).ToListAsync();
                    if (listOrderDetail.Count > 0)
                    {
                        foreach (var item in listOrderDetail)
                        {
                            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
                            product.Stock += item.Quantity;
                            _context.Products.Update(product);
                            await _context.SaveChangesAsync();
                        }
                    }

                    var user = await _userManager.GetUserAsync(this.User);

                    order.Status = 0;
                    order.UserIdUpdate = user.Id;
                    order.DateUpdate = DateTime.Now;
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Cancel order successful.";
                    return RedirectToAction("Index");
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Cancel order fail " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
