using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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
                var order = await _context.Orders.Include(o => o.User).Include(od => od.OrderDetails).ThenInclude(p => p.Product).FirstOrDefaultAsync(o => o.Id == Id);
                //order.OrderDetails = await _context.OrderDetails.Include(o => o.Product)
                //                                                .Where(od => od.OrderId == orderId)
                //                                                .Where(od => od.Status != 0)
                //                                                .ToListAsync();
                
                return View(order);
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
