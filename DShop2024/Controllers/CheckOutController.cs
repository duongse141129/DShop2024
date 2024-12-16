using DShop2024.Models;
using DShop2024.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DShop2024.Controllers
{
	public class CheckOutController : Controller
	{
		private readonly DShopContext _dataContext;

		private readonly UserManager<AppUserModel> _userManager;

		public CheckOutController(DShopContext context, UserManager<AppUserModel> userManager)
		{
			_dataContext = context;
			_userManager = userManager;
		}

		[Authorize]
		public async Task<IActionResult> CheckOut()
		{
			var userEmail = User.FindFirstValue(ClaimTypes.Email);
			if(userEmail == null)
			{
				return RedirectToAction("Login", "Account");
			}
			var user = await _userManager.GetUserAsync(this.User);
			var orderCode = Guid.NewGuid().ToString();
			var order = new OrderModel();
			order.OrderCode = orderCode;
			order.UserId = user.Id;	
			order.CreatedDate = DateTime.Now;
			order.PaymentMethod = "COD";
			order.Status = 1;
			await _dataContext.Orders.AddAsync(order);
			await _dataContext.SaveChangesAsync();
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
			foreach (var item in cart)
			{
				var orderDetail = new OrderDetailModel
				{
					OrderId = order.Id,
					ProductId = item.ProductId,
					Price = item.Price,
					Quantity = item.Quantity,
					Status = 1
				};
				await _dataContext.OrderDetails.AddAsync(orderDetail);
				await _dataContext.SaveChangesAsync();
			}
			HttpContext.Session.Remove("Cart");
			TempData["success"] = "CheckOut successful";


			return RedirectToAction("Index", "Cart");
		}
	}
}
