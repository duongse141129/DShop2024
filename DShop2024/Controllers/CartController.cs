using DShop2024.Models;
using DShop2024.Repository;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace DShop2024.Controllers
{
	public class CartController : Controller
	{
		private readonly DShopContext _dataContext;
        private readonly UserManager<AppUserModel> _userManager;

        public CartController(DShopContext context, UserManager<AppUserModel> userManager)
		{
			_dataContext = context;
            _userManager = userManager;
        }
		public IActionResult Index()
		{
			List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
			InformationDelivery info = HttpContext.Session.GetJson<InformationDelivery>("InfoCustomerDelivery") ?? new InformationDelivery();
			List<CouponModel> coupouns = HttpContext.Session.GetJson<List<CouponModel>>("CouponCustomerApply") ?? new List<CouponModel>();

			decimal shippingPrice = 0;
			if (info.ShippingCost != 0)
			{
				shippingPrice = info.ShippingCost;
			}
			decimal sumPirceItemsCart = cartItems.Sum(s => s.Quantity * s.Price);
			decimal sumCouponValue = coupouns.Sum(s => s.Value);
			decimal grandTotal = sumPirceItemsCart + shippingPrice - sumCouponValue;
			if(grandTotal < 0)
			{
				grandTotal = 0;
			}

			CartItemViewModel cartItemViewModel = new CartItemViewModel {
				CartItems = cartItems,
				SumPriceItemsCart = sumPirceItemsCart,
				SumCouponValue = sumCouponValue,
				GrandTotal = grandTotal,
				CouponsApply = coupouns,
				InfoDelivery = info
			};

			return View(cartItemViewModel);
		}

		public IActionResult CheckOut()
		{
			return View("~/Views/CheckOut/Index.cshtml");
		}

		public async Task<ActionResult> AddToCart(int Id) {
			ProductModel product = await _dataContext.Products.FindAsync(Id);

			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
			CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();
			if(cartItem == null)
			{
				cart.Add(new CartItemModel(product));
			}
			else
			{
				if (product.Stock <= cartItem.Quantity)
				{
					TempData["error"] = $" Item {product.ProductName} only has {product.Stock} left";
					
				}
				else
				{
					cartItem.Quantity += 1;
					TempData["success"] = $" Add Item {product.ProductName} to cart successfully";
				}
				
				
			}
			HttpContext.Session.SetJson("Cart",cart);

			TempData["success"] = $" Add Item {product.ProductName} to cart successfully";
			return Redirect(Request.Headers["Referer"].ToString());
		
		}

		public async Task<ActionResult> Increase(int Id)
		{
			ProductModel product = await _dataContext.Products.FindAsync(Id);

			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
			CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();
			if (product.Stock <= cartItem.Quantity)
			{
				TempData["error"] = $" Item {product.ProductName} only has {product.Stock} left";
				HttpContext.Session.SetJson("Cart", cart);
				return RedirectToAction("Index");
			}

			if (cartItem.Quantity >= 1 && product.Stock > cartItem.Quantity)
			{
				++cartItem.Quantity;
			}
			HttpContext.Session.SetJson("Cart", cart);
			return RedirectToAction("Index");

		}

		public ActionResult Decrease(int Id)
		{
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
			CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();
			if(cartItem.Quantity >1)
			{
				--cartItem.Quantity;
			}
			else
			{
				cart.RemoveAll(p => p.ProductId == Id);
			}

			if(cart.Count == 0)
			{
				HttpContext.Session.Remove("Cart");
			}
			HttpContext.Session.SetJson("Cart", cart);
			return RedirectToAction("Index");
		}

		public ActionResult Remove(int Id)
		{
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
			CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();
			cart.RemoveAll(p => p.ProductId == Id);

			if (cart.Count == 0)
			{
				HttpContext.Session.Remove("Cart");
			}
			HttpContext.Session.SetJson("Cart", cart);
			return RedirectToAction("Index");
		}

		public ActionResult Clear()
		{
			HttpContext.Session.Remove("Cart");
			return RedirectToAction("Index");
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> GetShipping(InformationDelivery informationDelivery)
		{
			decimal shipppingPrice = 50000;			
			if(ModelState.IsValid)
			{
				var existingShipping = await _dataContext.Shippings
											.FirstOrDefaultAsync(x => x.City == informationDelivery.tinh &&
																x.District == informationDelivery.quan
																&& x.Ward == informationDelivery.phuong);

				if (existingShipping != null)
				{
					shipppingPrice = existingShipping.Price;
				}

				informationDelivery.ShippingCost = shipppingPrice;
				HttpContext.Session.SetJson("InfoCustomerDelivery", informationDelivery);
				return RedirectToAction("Index");
			}
			return RedirectToAction("Index");
		}


		[HttpPost]
		public async Task<ActionResult> GetCoupon( string couponValue)
		{
			if(couponValue == null)
			{
				return Ok(new { success = false, message = "Please enter your coupon code to apply coupon" });
			}
            

            var validCoupon = await _dataContext.Coupons
									.FirstOrDefaultAsync(x => x.CouponCode == couponValue && x.Quantity >=1 && x.Status != 0);
			
			
			if(validCoupon != null)
			{
				TimeSpan remainingTime = validCoupon.DateExpired.Date - DateTime.Today.Date;				
				TimeSpan continueTime = validCoupon.DateStart.Date - DateTime.Today.Date;
				if(continueTime.Days > 0)
				{
					return Ok(new { success = false, message = "Can't use this coupon now too soon" });
				}
				int daysRemaining = remainingTime.Days;
				if(daysRemaining >= 0)
				{
					var user = await _userManager.GetUserAsync(this.User);

                    var CheckUsed = await _dataContext.CouponRedemptions.FirstOrDefaultAsync(u => u.UserId == user.Id && u.CouponId == validCoupon.Id);

					if (CheckUsed == null)
					{
						CouponRedemptionModel couponRedemption = new CouponRedemptionModel { UserId = user.Id, CouponId = validCoupon.Id, status = 1 };
						await _dataContext.CouponRedemptions.AddAsync(couponRedemption);
						await _dataContext.SaveChangesAsync();
					}
					if (CheckUsed != null && CheckUsed.status == 2 )
					{
						return Ok(new { success = false, message = "You have used this coupon" });
					}

					List<CouponModel> coupouns = HttpContext.Session.GetJson<List<CouponModel>>("CouponCustomerApply") ?? new List<CouponModel>();
					if ( coupouns.Count > 0 )
					{
						foreach (var item in coupouns)
						{
							if (item.CouponCode == couponValue)
							{
								return Ok(new { success = false, message = "You have actived a coupon in this order" });
							}
						}
					}

					coupouns.Add(validCoupon);
					HttpContext.Session.SetJson("CouponCustomerApply", coupouns);
					return Ok(new { success = true, message = "Apply coupon successfully" });

					//if (coupouns == null)
					//{
					//	coupouns = new List<CouponModel>();
					//	coupouns.Add(validCoupon);
					//	HttpContext.Session.SetJson("CouponCustomerApply", coupouns);
					//	return Ok(new { success = true, message = "Apply coupon successfully" });
					//}
					//coupouns.Add(validCoupon);
					//HttpContext.Session.SetJson("CouponCustomerApply", coupouns);
					//return Ok(new { success = true, message = "Apply coupon successfully" });

				}
				return Ok(new { success = false, message = "Coupon has expried" });
			}
			return Ok(new { success = false, message = "Coupon hasn't existed" });

		}




	}
}
