using DShop2024.Models;
using DShop2024.Models.Vnpay;
using DShop2024.Repository;
using DShop2024.Services.Momo;
using DShop2024.Services.Vnpay;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace DShop2024.Controllers
{
	public class CheckOutController : Controller
	{
		private readonly DShopContext _dataContext;

		private readonly UserManager<AppUserModel> _userManager;
		private readonly IEmailSender _emailSender;
		private readonly IMomoService _momoService;
		private readonly IVnPayService _vnPayService;
		public CheckOutController(DShopContext context, UserManager<AppUserModel> userManager, IEmailSender emailSender, IMomoService momoService, IVnPayService vnPayService)
		{
			_dataContext = context;
			_userManager = userManager;
			_emailSender = emailSender;
			_momoService = momoService;
			_vnPayService = vnPayService;

		}

		public async Task<string> CheckAllStock()
		{
			List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
			string productOutOfStock = "";
			foreach (var item in cartItems)
			{
				var product = await _dataContext.Products
								   .Where(p => p.Id == item.ProductId)
								   .FirstOrDefaultAsync();
				if (item.Quantity > product.Stock)
				{
					productOutOfStock += item.ProductName + " have "+ product.Stock + " left in stock," + "\n";
					
				}
			}
			return productOutOfStock;
		}

		[Authorize]
		public async Task<IActionResult> CheckOut(string payment)
		{
			if(string.IsNullOrEmpty(payment))
			{
				TempData["error"] = "Error payment";
				return RedirectToAction("Index", "Cart");
			}
			string checkStock = await CheckAllStock();
			if(!string.IsNullOrEmpty(checkStock))
			{
				TempData["error"] = checkStock;
				return RedirectToAction("Index", "Cart");
			}

			List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
			InformationDelivery info = HttpContext.Session.GetJson<InformationDelivery>("InfoCustomerDelivery");
			CouponModel coupoun = HttpContext.Session.GetJson<CouponModel>("CouponCustomerApply");
			var user = await _userManager.GetUserAsync(this.User);
			if (cartItems.Count == 0)
			{
				TempData["error"] = "Cart is empty";
				return RedirectToAction("Index", "Cart");
			}
			if(info == null)
			{
				TempData["error"] = "Infomation delivery is null";
				return RedirectToAction("Index", "Cart");
			}
			if (coupoun == null)
			{
				coupoun = new CouponModel();
			}

			var totalPrice = cartItems.Sum(s => s.Quantity * s.Price);
			int amount = Convert.ToInt32(totalPrice + info.ShippingCost - coupoun.Value);
			if (payment == "MOMO")
			{
				OrderInfoModel momoPayment = new OrderInfoModel { 
					FullName = user.UserName,
					OrderInfo = "Thanh toán qua Momo Payment tại DShop2024",
					Amount = amount.ToString()
				};
				return RedirectToAction("CreatePaymentMomo", "Payment", new { momoPayment.FullName, momoPayment.OrderInfo, momoPayment.Amount });
			}
			if(payment == "VNPAY")
			{
				PaymentInformationModel vnpayPayment = new PaymentInformationModel {
					Name = user.UserName,
					Amount = amount,
					OrderDescription= "Thanh toan qua Vnpay tai DShop2024",
					OrderType = "other"
				};
				return RedirectToAction("CreatePaymentUrlVnpay", "Payment", new { vnpayPayment.Name, vnpayPayment.Amount, vnpayPayment.OrderDescription, vnpayPayment.OrderType });
			}

			if (payment == "COD")
			{
				string orderCode = Guid.NewGuid().ToString();
				return RedirectToAction("SaveOrder", "CheckOut", new { paymentMethod = "COD", orderCode = orderCode});

			}
				
			return RedirectToAction("Index", "Cart");
		}


		public async Task<IActionResult> SaveOrder(string paymentMethod, string orderCode)
		{
			try
			{

				List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
				InformationDelivery info = HttpContext.Session.GetJson<InformationDelivery>("InfoCustomerDelivery");
				CouponModel coupoun = HttpContext.Session.GetJson<CouponModel>("CouponCustomerApply");
				var user = await _userManager.GetUserAsync(this.User);

				var order = new OrderModel();
				order.OrderCode = orderCode;
				order.UserId = user.Id;
				order.CreatedDate = DateTime.Now;
				order.PaymentMethod = paymentMethod;
				order.Status = 1;
				order.Consignee = info.Consignee;
				order.ShippingCost = info.ShippingCost;
				order.PhoneDelivery = info.PhoneDelivery;
				order.AddressDelivery = $" {info.Street} - {info.phuong} - {info.quan} - {info.tinh} ";
				order.TotalPrice = cartItems.Sum(s => s.Quantity * s.Price);
				if (coupoun != null)
				{
					order.ValueCoupon = coupoun.Value;
				}
				await _dataContext.Orders.AddAsync(order);
				await _dataContext.SaveChangesAsync();


				foreach (var item in cartItems)
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
				HttpContext.Session.Remove("InfoCustomerDelivery");
				HttpContext.Session.Remove("CouponCustomerApply");

				//var receiver = "dacclone577777@gmail.com";
				//var subject = "Order successful";
				//var message = "Thanks for order.";
				//await _emailSender.SendEmailAsync(receiver, subject, message);

				TempData["success"] = "CheckOut successful 11111111111111111111111111111111111111111111111111111111111111";
				return RedirectToAction("Index", "Home");
			}
			catch (Exception ex)
			{
				TempData["error"] = "CheckOut fail " +ex.Message;
				return RedirectToAction("Index", "Cart");
			}

		}



		[HttpGet]
		public IActionResult PaymentCallBack(MomoInfoModel model)
		{   
			var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query); 
			var requestQuery = HttpContext.Request.Query;

			var isSuccess = requestQuery["errorCode"];
			if(isSuccess == "0")
			{
				string orderCode = requestQuery["orderId"];
				return RedirectToAction("SaveOrder", "CheckOut", new { paymentMethod = "MOMO", orderCode = orderCode });		
			}
			TempData["error"] = "Momo transaction canceled";
			return RedirectToAction("Index", "Cart");



		}

		[HttpGet]
		public IActionResult PaymentCallbackVnpay()
		{
			var response =  _vnPayService.PaymentExecute(Request.Query);
			if (response.VnPayResponseCode == "00")
			{
				var orderCode = response.OrderId;
				return RedirectToAction("SaveOrder", "CheckOut", new { paymentMethod = "VNpay", orderCode = orderCode });
			}
			TempData["error"] = "VNpay transaction canceled";
			return RedirectToAction("Index", "Cart");
	
		}
	
	}
}
