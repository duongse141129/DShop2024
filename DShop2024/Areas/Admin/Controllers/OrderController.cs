using DShop2024.EnumData;
using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
    public class OrderController : Controller
    {
        private readonly DShopContext _context;
        private readonly UserManager<AppUserModel> _userManager;

        public OrderController(DShopContext context, UserManager<AppUserModel> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(string searchOrderCode = "", [FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 10)
        {
            ViewBag.sidebar = Menu.Admin.Order;

            var countCancelOrder = _context.Orders.Where(p => p.Status == 0).Count();
            var countNewOrder = _context.Orders.Where(p => p.Status == 1).Count();
            var countAcceptedOrder = _context.Orders.Where(p => p.Status == 2).Count();
            var countDeliveryOrder = _context.Orders.Where(p => p.Status == 3).Count();
            var countCompletedOrder = _context.Orders.Where(p => p.Status == 4).Count();

            ViewBag.countCancelOrder = countCancelOrder;
            ViewBag.countNewOrder = countNewOrder;
            ViewBag.countAcceptedOrder = countAcceptedOrder;
            ViewBag.countDeliveryOrder = countDeliveryOrder;
            ViewBag.countCompletedOrder = countCompletedOrder;


            IQueryable<OrderModel> listOrder = _context.Orders.Include(u => u.User).Include( p =>p.PaymentMethod).OrderByDescending(o => o.CreatedDate);
            var count = await listOrder.CountAsync();
            if (count > 0)
            {
                if (!String.IsNullOrEmpty(searchOrderCode))
                {
                    listOrder = listOrder.Where(c => c.OrderCode == searchOrderCode);
                }
            }
            ViewBag.searchOrderCode = searchOrderCode;
            int totalOrder = listOrder.Count();
            ViewBag.totalOrder = totalOrder;
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
                    pagesSize = pagesSize,
                    searchOrderCode = searchOrderCode
                })
            };

            var orders = await listOrder.Skip((currentPage - 1) * pagesSize)
                        .Take(pagesSize).ToListAsync();

            ViewBag.pagingModel = pagingModel;
            return View(orders);
        }

        public async Task<IActionResult> ViewOrder(int? Id)
        {
            ViewBag.sidebar = Menu.Admin.Order;
            if (Id == null)
            {
                return NotFound();
            }
            var order = await _context.Orders.Include(o => o.User)
                                                  .Include(od => od.OrderDetails)
                                                  .ThenInclude(p => p.Product)
                                                  .Include(c => c.OrderCoupons)
                                                  .ThenInclude(c => c.Coupon)
                                                  .Include(c => c.UpdateBy)
                                                  .Include(c => c.PaymentMethod)
                                                  .FirstOrDefaultAsync(o => o.Id == Id);
            return View(order);

        }

        public async Task<IActionResult> UpdateStatusOrder(int? orderId)
        {
            ViewBag.sidebar = Menu.Admin.Order;
            if (orderId == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FirstOrDefaultAsync(od => od.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }
            try
            {
                var user = await _userManager.GetUserAsync(this.User);
                if (order.Status != 4)
                {
                    order.Status += 1;
                }
                order.DateUpdate = DateTime.Now;
                order.UserIdUpdate = user.Id;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Update Status order successful";
                return RedirectToAction("ViewOrder", "Order", new { order.Id });

            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Update status order fail " + ex.Message;
                return RedirectToAction("ViewOrder", "Order", new { order.Id });
            }


        }

        public async Task<IActionResult> CancelOrder(int? orderId)
        {
            ViewBag.sidebar = Menu.Admin.Order;
            if (orderId == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FirstOrDefaultAsync(od => od.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }
            if (order.Status == 0)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Deleted order";
                return RedirectToAction("Index");
            }
            if (order.Status == 4)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Cannot cancel completed order ";
                return RedirectToAction("Index");
            }
            try
            {

                var listOrderDetail = await _context.OrderDetails.Where(d => d.OrderId == orderId).ToListAsync();
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
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Cancle order successful";
                return RedirectToAction("ViewOrder", "Order", new { order.Id });

            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Cancle order fail " + ex.Message;
                return RedirectToAction("ViewOrder", "Order", new { order.Id });
            }


        }

        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> GetPaymentMethods()
        {
            ViewBag.sidebar = Menu.Admin.Order;
            var payments = await _context.Payments.ToListAsync();
            return View(payments);

        }



        [HttpPost]
        public async Task<IActionResult> SetStatusPaymentMethod(int idPayment, int status)
        {
            ViewBag.sidebar = Menu.Admin.Order;
            try
            {
                var payment = await _context.Payments.FindAsync(idPayment);
                payment.Status = status;
                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, Message = "Set active payment successful" });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Message = ex.Message });
            }

        }


        //public async Task<IActionResult> SetOrder()
        //{
        //    ViewBag.sidebar = Menu.Admin.Order;
        //    try
        //    {
        //        var orders = await _context.Orders.ToListAsync();
        //        foreach (var item in orders)
        //        {
        //            var payment = await _context.Payments.FirstOrDefaultAsync(s => s.PaymentName == item.PaymentMethod);
        //            item.PaymentId = payment.Id;    
        //            _context.Orders.Update(item);
        //            await _context.SaveChangesAsync();
        //        }
        //        TempData[DShopConst.TEMPDATA_SUCCESS] = "success ";
        //        return RedirectToAction("GetPaymentMethods");
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData[DShopConst.TEMPDATA_ERROR] = "fail " + ex.Message;
        //        return RedirectToAction("GetPaymentMethods");
        //    }

        //}

    }
}
