using DShop2024.Models;
using DShop2024.Repository;
using DShop2024.Services.Momo;
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
		private readonly IEmailSender _emailSender;
		private readonly IMomoService _momoService;

		public CheckOutController(DShopContext context, UserManager<AppUserModel> userManager, IEmailSender emailSender, IMomoService momoService )
		{
			_dataContext = context;
			_userManager = userManager;
			_emailSender = emailSender;
			_momoService = momoService;

		}

		[Authorize]
		public async Task<IActionResult> CheckOut()
		{
			var userEmail = User.FindFirstValue(ClaimTypes.Email);
			if (userEmail == null)
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
				var product = await _dataContext.Products.FindAsync(item.ProductId);
				product.Stock -= item.Quantity;
				_dataContext.Products.Update(product);

				await _dataContext.OrderDetails.AddAsync(orderDetail);
				await _dataContext.SaveChangesAsync();
			}
			HttpContext.Session.Remove("Cart");

			//var receiver = "dacclone577777@gmail.com";
			//var subject = "Order successful";
			//var message = "Thanks for order.";
			//await _emailSender.SendEmailAsync(receiver, subject, message);

			TempData["success"] = "CheckOut successful";
			return RedirectToAction("Index", "Cart");
		}


		[HttpGet]
		public async Task<IActionResult> PaymentCallBack(MomoInfoModel model)
		{
			var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query); var requestQuery =
			HttpContext.Request.Query;
			if (requestQuery["resultCode"] != 0) //|
			{
				var newMomoInsert = new MomoInfoModel
				{
					OrderId = requestQuery["orderId"],
					FullName = User.FindFirstValue(ClaimTypes.Email),
					Amount = decimal.Parse(requestQuery["Amount"]),
					OrderInfo = requestQuery["orderInfo"],
					DatePaid = DateTime.Now
				};

				_dataContext.Add(newMomoInsert);
				await _dataContext.SaveChangesAsync();
			}
			else
			{

				TempData["success"] = "Đã hủy giao dịch Momo.";
				return RedirectToAction("Index", "Cart");
				//var checkoutResult = await Checkout (requestQuery["orderId"]); return View(response);
				

			}
			return View(response);
		}
	}
}
