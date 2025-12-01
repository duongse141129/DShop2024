using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.Models.Vnpay;
using DShop2024.Repository;
using DShop2024.Services.Momo;
using DShop2024.Services.Vnpay;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


namespace DShop2024.Controllers
{
	[Authorize]
	public class CheckOutController : Controller
	{
		private readonly DShopContext _context;

		private readonly UserManager<AppUserModel> _userManager;
		private readonly IEmailSender _emailSender;
		private readonly IMomoService _momoService;
		private readonly IVnPayService _vnPayService;
        public CheckOutController(DShopContext context, UserManager<AppUserModel> userManager, IEmailSender emailSender, IMomoService momoService, IVnPayService vnPayService)
		{
			_context = context;
			_userManager = userManager;
			_emailSender = emailSender;
			_momoService = momoService;
			_vnPayService = vnPayService;

		}

		public async Task<IActionResult> Index()
		{
			List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY) ?? new List<CartItemModel>();
			InformationDelivery info = HttpContext.Session.GetJson<InformationDelivery>(DShopConst.INFO_CUSTOMER_DELIVERY) ?? new InformationDelivery();
			List<CouponModel> coupouns = HttpContext.Session.GetJson<List<CouponModel>>(DShopConst.COUPONS_CUSTOMER_APPPLY) ?? new List<CouponModel>();

			var patments = await _context.Payments.Where(p => p.Status != 0).ToListAsync();

            if (cartItems.Count == 0)
			{
				return RedirectToAction("Index", "Cart");
            }
            string checkStock = await CheckAllStock();
            if (!string.IsNullOrEmpty(checkStock))
            {
                TempData[DShopConst.TEMPDATA_ERROR] = checkStock;
                return RedirectToAction("Index", "Cart");
            }


            decimal shippingPrice = 0;
			if (info.ShippingCost != 0)
			{
				shippingPrice = info.ShippingCost;
			}

			decimal sumPirceItemsCart = cartItems.Sum(s => s.Quantity * s.Price);
			decimal sumCouponValue = coupouns.Sum(s => s.Value);
			decimal grandTotal = sumPirceItemsCart + shippingPrice - sumCouponValue;
			if (grandTotal < 0)
			{
				grandTotal = 0;
			}

			CartItemViewModel cartItemViewModel = new CartItemViewModel
			{
				CartItems = cartItems,
				SumPriceItemsCart = sumPirceItemsCart,
				SumCouponValue = sumCouponValue,
				GrandTotal = grandTotal,
				CouponsApply = coupouns,
				InfoDelivery = info,
				Payments = patments
			};


            return View(cartItemViewModel);
		}

		private async Task<string> CheckAllStock()
		{
			List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY);
			string productOutOfStock = "";
			foreach (var item in cartItems)
			{
				var product = await _context.Products
								   .Where(p => p.Id == item.ProductId)
								   .FirstOrDefaultAsync();
				if (item.Quantity > product.Stock)
				{
					productOutOfStock += item.ProductName + " have "+ product.Stock + " left in stock," ;
					
				}
			}
			return productOutOfStock;
		}

		public async Task<IActionResult> CheckOut(string payment)
		{
			if (string.IsNullOrEmpty(payment))
			{
				TempData[DShopConst.TEMPDATA_ERROR] = "Please select payment method";
				return RedirectToAction("Index");
			}
			string checkStock = await CheckAllStock();
			if (!string.IsNullOrEmpty(checkStock))
			{
				TempData[DShopConst.TEMPDATA_ERROR] = checkStock;
				return RedirectToAction("Index");
			}

			List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY);
			InformationDelivery info = HttpContext.Session.GetJson<InformationDelivery>(DShopConst.INFO_CUSTOMER_DELIVERY);
			List<CouponModel> coupouns = HttpContext.Session.GetJson<List<CouponModel>>(DShopConst.COUPONS_CUSTOMER_APPPLY) ?? new List<CouponModel>();
			var user = await _userManager.GetUserAsync(this.User);
			if (cartItems.Count == 0)
			{
				TempData[DShopConst.TEMPDATA_ERROR] = "Cart is empty";
				return RedirectToAction("Index", "Cart");
			}
			if (info == null || String.IsNullOrEmpty(info.tinh) || String.IsNullOrEmpty(info.quan) || String.IsNullOrEmpty(info.phuong) || String.IsNullOrEmpty(info.Street) || String.IsNullOrEmpty(info.PhoneDelivery) || String.IsNullOrEmpty(info.Consignee))
			{
				TempData[DShopConst.TEMPDATA_ERROR] = "Please fill out all fields of information delivery to checkout.";
				return RedirectToAction("Index", "CheckOut");
			}
			foreach (var coupon in coupouns)
			{
				var cp = await _context.Coupons.Include(p => p.Promotion).FirstOrDefaultAsync(c => c.Id == coupon.Id);
				if(cp != null )
				{
                    int quantityCoupon = cp.Quantity;
                    var dateNow = DateTime.Today.Date;
                    if (cp.Status == 0 || cp.Quantity <= 0 || dateNow > cp.DateExpired.Date)
                    {
						if(cp.Status == 0)
						{
                            TempData[DShopConst.TEMPDATA_ERROR] = $"Coupon {coupon.CouponCode} have been removed. Do you still want to checkout?";
                        }
                        if (cp.Quantity <= 0)
                        {
                            TempData[DShopConst.TEMPDATA_ERROR] = $"Coupon {coupon.CouponCode} is out of stock. Do you still want to checkout?";
                        }
                        if (dateNow > cp.DateExpired.Date)
                        {
                            TempData[DShopConst.TEMPDATA_ERROR] = $"Coupon {coupon.CouponCode} was expired. Do you still want to checkout?";
                        }

                        coupouns.Remove(coupon);
                        HttpContext.Session.SetJson(DShopConst.COUPONS_CUSTOMER_APPPLY, coupouns);     
						
						if(cp.Promotion.CategoryCouponName == DShopConst.FREE_SHIPPING || cp.Promotion.CategoryCouponName == DShopConst.NEW_CUSTOMER)
						{
							return RedirectToAction("GetShipping", "Cart", new { informationDelivery = info });
						}

                        return RedirectToAction("Index", "Cart");
                    }

				}
			}

			var totalPrice = cartItems.Sum(s => s.Quantity * s.Price);
			int amount = Convert.ToInt32(totalPrice + info.ShippingCost - coupouns.Sum(c => c.Value) );
			if (payment == PaymentEnumData.MOMO)
			{
				OrderInfoModel momoPayment = new OrderInfoModel { 
					FullName = user.UserName,
					OrderInfo = "Momo Payment at DShop2024",
					Amount = amount.ToString()
				};
				return RedirectToAction("CreatePaymentMomo", "Payment", new { momoPayment.FullName, momoPayment.OrderInfo, momoPayment.Amount });
			}
			if(payment == PaymentEnumData.VNPAY)
			{
				PaymentInformationModel vnpayPayment = new PaymentInformationModel {
					Name = user.UserName,
					Amount = amount,
					OrderDescription= "Vnpay payment at DShop2024",
					OrderType = "other"
				};
				return RedirectToAction("CreatePaymentUrlVnpay", "Payment", new { vnpayPayment.Name, vnpayPayment.Amount, vnpayPayment.OrderDescription, vnpayPayment.OrderType });
			}

			if (payment == PaymentEnumData.COD)
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

				List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY);
				InformationDelivery info = HttpContext.Session.GetJson<InformationDelivery>(DShopConst.INFO_CUSTOMER_DELIVERY);
				List<CouponModel> coupouns = HttpContext.Session.GetJson<List<CouponModel>>(DShopConst.COUPONS_CUSTOMER_APPPLY) ?? new List<CouponModel>();
				var user = await _userManager.GetUserAsync(this.User);
				var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentName == paymentMethod);

				var order = new OrderModel();
				order.OrderCode = orderCode;
				order.UserId = user.Id;
				order.CreatedDate = DateTime.Now;
				order.DateUpdate = DateTime.Now;
				order.UserIdUpdate = user.Id;
				order.PaymentId = payment.Id;
				order.Status = 1;
				order.Consignee = info.Consignee;
				order.ShippingCost = info.ShippingCost;
				order.PhoneDelivery = info.PhoneDelivery;
				order.AddressDelivery = $" {info.Street}_{info.phuong}_{info.quan}_{info.tinh} ";
				if (coupouns != null)
				{
					order.ValueCoupon = coupouns.Sum(c => c.Value);
				}
				var grandTotal = cartItems.Sum(s => s.Quantity * s.Price) + info.ShippingCost - coupouns.Sum(c => c.Value);
				if (grandTotal < 0)
				{
					grandTotal = 0;
				}
				order.TotalPrice = grandTotal;
				await _context.Orders.AddAsync(order);
				await _context.SaveChangesAsync();


				foreach (var item in cartItems)
				{
					var orderDetail = new OrderDetailModel
					{
						OrderId = order.Id,
						ProductId = item.ProductId,
						Price = item.Price,
						Quantity = item.Quantity,
						OriginalPrice = item.OriginalPrice,
						Status = 1


					};
					var product = await _context.Products.FindAsync(item.ProductId);
					product.Stock -= item.Quantity;
					_context.Products.Update(product);

					await _context.OrderDetails.AddAsync(orderDetail);
					await _context.SaveChangesAsync();
				}

				foreach (var item in coupouns)
				{
					CouponRedemptionModel couponRedemption = await _context.CouponRedemptions.FirstOrDefaultAsync(cr => cr.UserId == user.Id && cr.CouponId == item.Id );
					couponRedemption.Status = 2;
                    CouponModel couponModel = await _context.Coupons.Include(p => p.Promotion).FirstOrDefaultAsync(c => c.Id == item.Id);
                    couponModel.Quantity -= 1;
					OrderCouponsModel orderCoupons = new OrderCouponsModel { OrderId = order.Id, CouponId = item.Id, Status = 1 };

					if(couponModel.Promotion.CategoryCouponName == DShopConst.NEW_CUSTOMER)
					{
						couponModel.Status = 0;
					}

					_context.Coupons.Update(couponModel);
					_context.CouponRedemptions.Update(couponRedemption);
					await _context.OrderCouponss.AddAsync(orderCoupons);
					await _context.SaveChangesAsync();
				}

				var orderSendGmail = await _context.Orders
													.Include(u => u.User)
													.Include(od => od.OrderDetails)
													.ThenInclude(p => p.Product)
													.Where(o => o.Id == order.Id)
													.FirstOrDefaultAsync();
                var infoShop = await _context.InformationShops.FirstOrDefaultAsync();
                await _emailSender.SendEmailOrder(order, infoShop);			

				HttpContext.Session.Remove(DShopConst.CART_KEY);
				HttpContext.Session.Remove(DShopConst.INFO_CUSTOMER_DELIVERY);
				HttpContext.Session.Remove(DShopConst.COUPONS_CUSTOMER_APPPLY);


				TempData[DShopConst.TEMPDATA_SUCCESS] = "Checkout successful. Thank you for shopping at the DShop2024. ";
				return RedirectToAction("Index", "Home");
			}
			catch (Exception ex)
			{
				TempData[DShopConst.TEMPDATA_ERROR] = "CheckOut fail " +ex.Message;
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
			TempData[DShopConst.TEMPDATA_ERROR] = "Momo transaction canceled";
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
			TempData[DShopConst.TEMPDATA_ERROR] = "VNpay transaction canceled";
			return RedirectToAction("Index", "Cart");
	
		}
	
	}
}
