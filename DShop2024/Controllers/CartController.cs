using DShop2024.EnumData;
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
		private readonly DShopContext _context;
        private readonly UserManager<AppUserModel> _userManager;

        public CartController(DShopContext context, UserManager<AppUserModel> userManager)
		{
			_context = context;
            _userManager = userManager;
        }
		public  IActionResult Index()
		{
			List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY) ?? new List<CartItemModel>();
			decimal sumPirceItemsCart = cartItems.Sum(s => s.Quantity * s.Price);
			CartItemViewModel cartItemViewModel = new CartItemViewModel
			{
				CartItems = cartItems,
				SumPriceItemsCart = sumPirceItemsCart,
			};
			return View(cartItemViewModel);
		}

		private async Task GetValueByPercentCoupon()
		{
			List<CouponModel> coupouns = HttpContext.Session.GetJson<List<CouponModel>>(DShopConst.COUPONS_CUSTOMER_APPPLY) ?? new List<CouponModel>();
			List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY) ?? new List<CartItemModel>();
			if (coupouns.Count > 0)
			{
				foreach (CouponModel couponModel in coupouns)
				{
					var coupon = await _context.Coupons.Include(p => p.Promotion).FirstOrDefaultAsync(c => c.Id == couponModel.Id);
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

        [HttpPost]
        public async Task<ActionResult> AddQuantityToCart([FromForm] int quantity, [FromForm] int? productId)
        {
            if (productId == null)
            {
                return NotFound();
            }
            if (quantity < 1)
            {
                return Ok(new { success = false, Message = $" Input quantity >= 1" });
            }
            ProductModel product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == productId && m.Status != 0);
            if (product == null)
            {
                return NotFound();
            }
            if (product.Stock == 0)
            {
                return Ok(new { success = false, Message =  $" Item {product.ProductName} is out of ourder" });
            }
            if (product.Stock < quantity)
            {
                return Ok(new { success = false, Message =  $" Item {product.ProductName} only has  {product.Stock} quantities left" });
            }

            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY) ?? new List<CartItemModel>();
            CartItemModel cartItem = cart.Where(c => c.ProductId == productId).FirstOrDefault();
            if (cartItem == null)
            {
                product.Stock = quantity;
                cart.Add(new CartItemModel(product));
            }
            else
            {
                if (product.Stock < cartItem.Quantity + quantity)
                {
                    return Ok(new { success = false, Message = $"Item {product.ProductName} only has {product.Stock} left" });
                }
                else
                {
                    cartItem.Quantity += quantity;
                }
            }
            HttpContext.Session.SetJson(DShopConst.CART_KEY, cart);
            await GetValueByPercentCoupon();
            return Ok(new { success = true, Message = $" Add Item {product.ProductName} to cart successfully" });
        }


        public async Task<ActionResult> Increase(int? Id)
		{
            if (Id == null)
            {
                return NotFound();
            }
            ProductModel product = await _context.Products
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
            ProductModel product = await _context.Products
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
            ProductModel product = await _context.Products
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

			decimal shipppingPrice = DShopConst.DEFAULT_SHIPPING_COST;
			if (ModelState.IsValid)
			{

				var existingShipping = await _context.Shippings
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
            
            var validCoupon = await _context.Coupons
									.Include(p => p.Promotion)
									.FirstOrDefaultAsync(x => x.CouponCode == couponCode);
					
			if(validCoupon != null)
			{
				if(validCoupon.Status == 0)
				{
					return Ok(new { success = false, message = "Coupon code has been deleted" });
				}
                if (validCoupon.Quantity == 0)
                {
                    return Ok(new { success = false, message = "Coupon code is out of stock" });
                }

                TimeSpan remainingTime = validCoupon.DateExpired.Date - DateTime.Today.Date;				
				TimeSpan continueTime = validCoupon.DateStart.Date - DateTime.Today.Date;
				if(continueTime.Days > 0)
				{
					return Ok(new { success = false, message = "Can't use this coupon now. Too soon" });
				}
				int daysRemaining = remainingTime.Days;
				if(daysRemaining >= 0)
				{
					var user = await _userManager.GetUserAsync(this.User);

                    var CheckUsed = await _context.CouponRedemptions.FirstOrDefaultAsync(u => u.UserId == user.Id && u.CouponId == validCoupon.Id);

					if (CheckUsed == null)
					{
						CouponRedemptionModel couponRedemption = new CouponRedemptionModel { UserId = user.Id, CouponId = validCoupon.Id, Status = 1 };
						await _context.CouponRedemptions.AddAsync(couponRedemption);
						await _context.SaveChangesAsync();
					}
					if (CheckUsed != null && CheckUsed.Status == 2 )
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
                    List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY) ?? new List<CartItemModel>();
                    decimal sumPirceItemsCart = cartItems.Sum(s => s.Quantity * s.Price);
					if( sumPirceItemsCart > 0 && cartItems.Count > 0)
					{
						if(sumPirceItemsCart < validCoupon.MinimumAmount)
						{
                            return Ok(new { success = false, message = "You do not satisfy the minimum purchase amount. Please purchase additional items." });
                        }
					}

                    if (validCoupon.Promotion.CategoryCouponName.Equals(DShopConst.SUB_SUMTOTAL_DISCOUNT))
					{
						validCoupon.Value = validCoupon.Value;
                    }
                    if (validCoupon.Promotion.CategoryCouponName.Equals(DShopConst.PERCENTAGE_DISCOUNT))
                    {
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
							if(info.ShippingCost == 0)
							{
                                return Ok(new { success = false, message = "You got free shipping. Save this code for next time." });
                            }
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
                                if (info.ShippingCost == 0)
                                {
                                    return Ok(new { success = false, message = "You got free shipping. Save this code for next time." });
                                }
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


        [HttpPost]
        public async Task<ActionResult> UseMyInformation(bool checkInfo)
        {
			if(!checkInfo)
			{
                InformationDelivery informationDelivery = new InformationDelivery();
                HttpContext.Session.SetJson(DShopConst.INFO_CUSTOMER_DELIVERY, informationDelivery);
                return Ok(new { success = false, message = "Remove shipping information." });
            }
			try
			{
                var user = await _userManager.GetUserAsync(this.User);
                if (String.IsNullOrEmpty(user.PhoneNumber))
                {
                    return Ok(new { success = false, message = "Go to profile and add your phone number" });
                }
                if (String.IsNullOrEmpty(user.Address))
                {
                    return Ok(new { success = false, message = "Go to profile and add your address" });
                }
                string[] address = user.Address.Split("_");
                InformationDelivery info = new InformationDelivery();
                info.tinh = address[address.Count() - 1];
                info.quan = address[address.Count() - 2];
                info.phuong = address[address.Count() - 3];
                info.Street = address[address.Count() - 4];
                info.PhoneDelivery = user.PhoneNumber;
                info.Consignee = user.UserName;
                info.IsUseMyInfo = true;
                var result = await GetShipping(info);
                if (result != null)
                {
                    return Ok(new { success = true, message = "Use information successful to get shipping successful" });
                }
                return Ok(new { success = false, message = "Use information to get shipping fail." });
            }
			catch (Exception ex)
			{
                return Ok(new { success = false, message = "Use information to get shipping fail. " + ex.Message });
            }
        }

    }
}
