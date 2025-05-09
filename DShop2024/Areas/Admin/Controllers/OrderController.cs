using DShop2024.EnumData;
using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using static DShop2024.EnumData.Product;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
	public class OrderController : Controller
    {
        private readonly DShopContext _context;

        public OrderController(DShopContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string searchOrderCode = "", [FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 10)
        {
            IQueryable<OrderModel> listOrder =  _context.Orders.Include(u => u.User).OrderByDescending(o => o.Id);
            var count = await listOrder.CountAsync();
            if(count > 0)
            {
                if (!String.IsNullOrEmpty(searchOrderCode))
                {
                    listOrder = listOrder.Where(c => c.OrderCode == searchOrderCode);
                }
            }
            ViewBag.searchOrderCode = searchOrderCode;
            int totalOrder = listOrder.Count();
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

        public async Task<IActionResult> ViewOrder(int Id )
        {
            {
                var order = await _context.Orders.Include(o => o.User)
                                                  .Include(od => od.OrderDetails)
                                                  .ThenInclude(p => p.Product)
                                                  .Include(c => c.OrderCoupons)
                                                  .ThenInclude(c => c.Coupon)
                                                  .FirstOrDefaultAsync(o => o.Id == Id);

	        
				return View(order);
            }
        }
 
        public async Task<IActionResult> UpdateStatusOrder(int orderId)
        {
            {
                var order = await _context.Orders.FirstOrDefaultAsync(od => od.Id == orderId);

                if (order == null)
                {
                    return NotFound();
                }
                try
                {
                    if(order.Status != 4)
                    {
                        order.Status += 1;
                    }                    
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Update Status order successful";
                    return RedirectToAction("ViewOrder", "Order", new {order.Id});

                }
                catch (Exception ex)
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "Update status order fail " + ex.Message;
                    return RedirectToAction("ViewOrder", "Order", new { order.Id });
                }

            }
        }

        public async Task<IActionResult> CancelOrder(int orderId)
        {
            {
                var order = await _context.Orders.FirstOrDefaultAsync(od => od.Id == orderId);

                if (order == null)
                {
                    return NotFound();
                }
                try
                {

                    var listOrderDetail = await _context.OrderDetails.Where(d => d.OrderId == orderId).ToListAsync();
                    if(listOrderDetail.Count > 0)
                    {
						foreach (var item in listOrderDetail)
						{
                            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
                            product.Stock += item.Quantity;
                            _context.Products.Update(product);
                            await _context.SaveChangesAsync();
						}
					}


                    order.Status = 0;
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
        }




        [HttpPost]
        [Route("UpdateOrder")]
        public async Task<IActionResult> UpdateOrder(int orderId, int status)
		{
			{
                var order = await _context.Orders.FirstOrDefaultAsync(od => od.Id == orderId);

				if(order == null){
                    return NotFound();
                }
                order.Status = status;
                try
                {
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();
                    return Ok(new{success = true, message ="Update Order status successful"});

                }
                catch (Exception ex)
                {
                    return StatusCode(500, DShopConst.TEMPDATA_ERROR);
                }

			}
		}

		
    }
}
