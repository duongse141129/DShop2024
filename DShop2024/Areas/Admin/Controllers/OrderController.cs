using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly DShopContext _context;

        public OrderController(DShopContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var order = await _context.Orders.Where(p => p.Status != -1).Include(u => u.User).OrderByDescending(o => o.Id).ToListAsync();
            return View(order);
        }

        public async Task<IActionResult> ViewOrder(int Id )
        {
            {
                var order = await _context.Orders.Include(o => o.User)
                                                  .Include(od => od.OrderDetails)
                                                  .ThenInclude(p => p.Product)
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
                    TempData["success"] = "Update Status order successful";
                    return RedirectToAction("ViewOrder", "Order", new {order.Id});

                }
                catch (Exception ex)
                {
                    TempData["error"] = "Update status order fail " + ex.Message;
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
                    order.Status = 0;
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Cancle order successful";
                    return RedirectToAction("ViewOrder", "Order", new { order.Id });

                }
                catch (Exception ex)
                {
                    TempData["error"] = "Cancle order fail " + ex.Message;
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
                    return StatusCode(500, "Error");
                }

			}
		}

		
    }
}
