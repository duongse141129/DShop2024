using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.Repository;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IO;


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
		public  IActionResult Index()
		{
			List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY) ?? new List<CartItemModel>();
			InformationDelivery info = HttpContext.Session.GetJson<InformationDelivery>(DShopConst.INFO_CUSTOMER_DELIVERY) ?? new InformationDelivery();
			List<CouponModel> coupouns = HttpContext.Session.GetJson<List<CouponModel>>(DShopConst.COUPONS_CUSTOMER_APPPLY) ?? new List<CouponModel>();

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

		public async Task GetValueByPercentCoupon()
		{
			List<CouponModel> coupouns = HttpContext.Session.GetJson<List<CouponModel>>(DShopConst.COUPONS_CUSTOMER_APPPLY) ?? new List<CouponModel>();
			List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY) ?? new List<CartItemModel>();
			if (coupouns.Count > 0)
			{
				foreach (CouponModel couponModel in coupouns)
				{
					var coupon = await _dataContext.Coupons.Include(p => p.Promotion).FirstOrDefaultAsync(c => c.Id == couponModel.Id);
					if (coupon.Promotion.CategoryCouponName == DShopConst.PERCENTAGE_DISCOUNT)
					{
						decimal subtotal = cartItems.Sum(c => c.Quantity * c.Price);
						var val = coupon.Value * subtotal / 100000;
						var CellVal = Math.Ceiling(val);
						couponModel.Value = CellVal * 1000;					
					}
				}
				HttpContext.Session.SetJson(DShopConst.COUPONS_CUSTOMER_APPPLY, coupouns);
			}

		}

		public async Task<ActionResult> AddToCart(int? Id) {
            if (Id == null)
            {
                return NotFound();
            }
            ProductModel product = await _dataContext.Products
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (product == null)
            {
                return NotFound();
            }
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY) ?? new List<CartItemModel>();
			CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();
			if(cartItem == null)
			{
				cart.Add(new CartItemModel(product));
			}
			else
			{
				if (product.Stock <= cartItem.Quantity)
				{
					TempData[DShopConst.TEMPDATA_ERROR] = $" Item {product.ProductName} only has {product.Stock} left";
					
				}
				else
				{
					cartItem.Quantity += 1;
					TempData[DShopConst.TEMPDATA_SUCCESS] = $" Add Item {product.ProductName} to cart successfully";
				}
				
				
			}
			HttpContext.Session.SetJson(DShopConst.CART_KEY, cart);
			await GetValueByPercentCoupon();
			TempData[DShopConst.TEMPDATA_SUCCESS] = $" Add Item {product.ProductName} to cart successfully";
			return Redirect(Request.Headers["Referer"].ToString());
		
		}

		public async Task<ActionResult> Increase(int? Id)
		{
            if (Id == null)
            {
                return NotFound();
            }
            ProductModel product = await _dataContext.Products
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (product == null)
            {
                return NotFound();
            }

			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY) ?? new List<CartItemModel>();
			CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();
			if (product.Stock <= cartItem.Quantity)
			{
				TempData[DShopConst.TEMPDATA_ERROR] = $" Item {product.ProductName} only has {product.Stock} left";
				HttpContext.Session.SetJson(DShopConst.CART_KEY, cart);
				return RedirectToAction("Index");
			}

			if (cartItem.Quantity >= 1 && product.Stock > cartItem.Quantity)
			{
				++cartItem.Quantity;
			}
			HttpContext.Session.SetJson(DShopConst.CART_KEY, cart);
			await GetValueByPercentCoupon();
			return RedirectToAction("Index");

		}

		public async Task<ActionResult> Decrease(int? Id)
		{
            if (Id == null)
            {
                return NotFound();
            }
            ProductModel product = await _dataContext.Products
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (product == null)
            {
                return NotFound();
            }

            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY) ?? new List<CartItemModel>();
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
				HttpContext.Session.Remove(DShopConst.CART_KEY);
			}
			HttpContext.Session.SetJson(DShopConst.CART_KEY, cart);
			await GetValueByPercentCoupon();
			return RedirectToAction("Index");
		}

		public async Task<ActionResult> Remove(int? Id)
		{
            if (Id == null)
            {
                return NotFound();
            }
            ProductModel product = await _dataContext.Products
							.FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (product == null)
            {
                return NotFound();
            }

            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY) ?? new List<CartItemModel>();
			CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();
			cart.RemoveAll(p => p.ProductId == Id);

			if (cart.Count == 0)
			{
				HttpContext.Session.Remove(DShopConst.CART_KEY);
			}
			HttpContext.Session.SetJson(DShopConst.CART_KEY, cart);
			await GetValueByPercentCoupon();
			return RedirectToAction("Index");
		}

		public ActionResult Clear()
		{
			HttpContext.Session.Remove(DShopConst.CART_KEY);
			return RedirectToAction("Index");
		}




		[HttpPost]
		[Route("GetShipping")]
		public async Task<ActionResult> GetShipping(InformationDelivery informationDelivery)
		{

			decimal shipppingPrice = 50000;
			if (ModelState.IsValid)
			{

				var existingShipping = await _dataContext.Shippings
											.FirstOrDefaultAsync(x => x.Province == informationDelivery.tinh);

				if (existingShipping != null)
				{
					shipppingPrice = existingShipping.Price;
				}

				List<CouponModel> coupouns = HttpContext.Session.GetJson<List<CouponModel>>(DShopConst.COUPONS_CUSTOMER_APPPLY) ?? new List<CouponModel>();
				if (coupouns.Count > 0)
				{
					foreach (var item in coupouns)
					{
						if (item.Promotion.CategoryCouponName.Equals(DShopConst.FREE_SHIPPING) || item.Promotion.CategoryCouponName.Equals(DShopConst.NEW_CUSTOMER))
						{
							informationDelivery.ShippingCost = 0;
							HttpContext.Session.SetJson(DShopConst.INFO_CUSTOMER_DELIVERY, informationDelivery);
							return Ok(new { success = true, message = "Get shipping successful" });
							
						}
					}
				}

				informationDelivery.ShippingCost = shipppingPrice;
				HttpContext.Session.SetJson(DShopConst.INFO_CUSTOMER_DELIVERY, informationDelivery);
				return Ok(new { success = true, message = "Get shipping successful" });
			}
			return Ok(new { success = false, message = "Get shipping fail. Please fill all inputs." });
		}


		[HttpPost]
		public async Task<ActionResult> GetCoupon( string couponCode)
		{
			if(couponCode == null)
			{
				return Ok(new { success = false, message = "Please enter your coupon code to apply coupon" });
			}
            

            var validCoupon = await _dataContext.Coupons
									.Include(p => p.Promotion)
									.FirstOrDefaultAsync(x => x.CouponCode == couponCode && x.Quantity >=1 && x.Status != 0);
			
			
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
						return Ok(new { success = false, message = "You have already used this coupon" });
					}

					List<CouponModel> coupouns = HttpContext.Session.GetJson<List<CouponModel>>(DShopConst.COUPONS_CUSTOMER_APPPLY) ?? new List<CouponModel>();
					if ( coupouns.Count > 0 )
					{
						foreach (var item in coupouns)
						{
							if (item.CouponCode == couponCode)
							{
								return Ok(new { success = false, message = "You have actived a coupon in this order" });
							}
						}
					}
					
					if (validCoupon.Promotion.CategoryCouponName.Equals(DShopConst.SUB_SUMTOTAL_DISCOUNT))
					{
						validCoupon.Value = validCoupon.Value;
                    }
                    if (validCoupon.Promotion.CategoryCouponName.Equals(DShopConst.PERCENTAGE_DISCOUNT))
                    {
                        List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY);
						if(cartItems.Count > 0 )
						{
                            decimal subtotal = cartItems.Sum(c => c.Quantity * c.Price);
							var val = validCoupon.Value * subtotal / 100000;
							var CellVal = Math.Ceiling(val);
							validCoupon.Value = CellVal*1000;
                        }
                    }

                    if (validCoupon.Promotion.CategoryCouponName.Equals(DShopConst.FREE_SHIPPING))
                    {
                        InformationDelivery info = HttpContext.Session.GetJson<InformationDelivery>(DShopConst.INFO_CUSTOMER_DELIVERY);
                        if(info != null)
						{
							info.ShippingCost = 0;
                            HttpContext.Session.SetJson(DShopConst.INFO_CUSTOMER_DELIVERY, info);
							validCoupon.Value = 0;
                        }
                    }

					if (validCoupon.Promotion.CategoryCouponName.Equals(DShopConst.NEW_CUSTOMER))
					{
						var codeCustomer = validCoupon.CouponCode.Split('_')[1];
						if (user.UserName.ToUpper().Equals(codeCustomer))
						{
							InformationDelivery info = HttpContext.Session.GetJson<InformationDelivery>(DShopConst.INFO_CUSTOMER_DELIVERY);
							if (info != null)
							{
								info.ShippingCost = 0;
								HttpContext.Session.SetJson(DShopConst.INFO_CUSTOMER_DELIVERY, info);
								validCoupon.Value = 0;
							}
						}
						else
						{
							return Ok(new { success = false, message = "This coupon code does not belong to you." });
						}					
					}


					coupouns.Add(validCoupon);
					HttpContext.Session.SetJson(DShopConst.COUPONS_CUSTOMER_APPPLY, coupouns);
					return Ok(new { success = true, message = "Apply coupon successfully" });

				}
				return Ok(new { success = false, message = "Coupon has expried" });
			}
			return Ok(new { success = false, message = "Coupon hasn't existed" });

		}




	}
}
