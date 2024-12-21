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
            var order = await _context.Orders.Where(p => p.Status != 0).Include(u => u.User).OrderByDescending(o => o.Id).ToListAsync();
            return View(order);
        }

        public async Task<IActionResult> ViewOrder(int Id )
        {
            {
                var order = await _context.Orders.Include(o => o.User)
                                                  .Include(od => od.OrderDetails)
                                                  .ThenInclude(p => p.Product)
                                                  .FirstOrDefaultAsync(o => o.Id == Id);
                decimal total = 0;
                foreach (var o in order.OrderDetails)
                {
                    total += o.Price * o.Quantity;
                }

                ViewBag.GrandTotal = total;
	
				return View(order);
            }
        }
		public async Task<IActionResult> ViewOrderDetail(int Id)
		{
			{
				List<OrderDetailModel> orderDetails = await _context.OrderDetails.Include(p => p.Product)
                                                                                    .Include(od => od.Order)
                                                                                    .ThenInclude(u  => u.User)
                                                                                    .Where(od => od.OrderId == Id)
                                                                                    .ToListAsync();

				return View(orderDetails);
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

		public async Task<IActionResult> Delete(int Id)
        {
            {
                OrderModel order = await _context.Orders.FindAsync(Id);
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                TempData["success"] = "Remove product success";
                return RedirectToAction("Index");
            }
        }
    }
}
