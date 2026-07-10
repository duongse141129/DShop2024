using AutoMapper;
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
        private readonly IMapper _mapper;

        public CartController(DShopContext context, UserManager<AppUserModel> userManager, IMapper mapper)
		{
			_context = context;
            _userManager = userManager;
            _mapper = mapper;
        }
		public async  Task<IActionResult> Index()
		{
            var messages = await ValidateCartPrice();
            if (messages.Any())
            {
                TempData[DShopConst.TEMPDATA_ERROR] = string.Join(", ", messages);
                return RedirectToAction("Index");
            }
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY) ?? new List<CartItemModel>();
			decimal sumPirceItemsCart = cartItems.Sum(s => s.Quantity * s.Price);
			CartItemViewModel cartItemViewModel = new CartItemViewModel
			{
				CartItems = cartItems,
				SumPriceItemsCart = sumPirceItemsCart,
			};
			return View(cartItemViewModel);
		}

        private async Task<List<string>> ValidateCartPrice()
        {
            var messages = new List<string>();
            var cartItems = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY);
            if (cartItems == null || !cartItems.Any())
                return messages;

            var now = DateTime.Now;
            for (int i = cartItems.Count - 1; i >= 0; i--)
            {
                var item = cartItems[i];
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);
                if (product == null || product.Status == 0)
                {
                    messages.Add($"The product '{item.ProductName}' is no longer available.");
                    cartItems.RemoveAll(p => p.ProductId == item.ProductId);
                }
                else
                {
                    var activeSale = await _context.Sales
                        .FirstOrDefaultAsync(s =>
                            s.ProductId == item.ProductId &&
                            s.Status != 0 &&
                            s.SaleStartDate <= now &&
                            s.SaleEndDate >= now);

                    decimal oldPrice = item.Price;
                    decimal currentPrice = activeSale?.SalePrice ?? product.Price;

                    if (oldPrice != currentPrice)
                    {
                        if (oldPrice < product.Price && activeSale == null)
                        {
                            messages.Add(
                                $"The promotion for '{item.ProductName}' has ended.");
                        }
                        else
                        {
                            messages.Add(
                                $"The price of '{item.ProductName}' has changed from " +
                                $"{oldPrice:N0} VND to {currentPrice:N0} VND.");
                        }

                        item.Price = currentPrice;
                    }
                }
            }
            HttpContext.Session.SetJson(DShopConst.CART_KEY, cartItems);
            return messages;
        }


        [HttpPost]
        public async Task<ActionResult> AddQuantityToCart( int quantity,int? productId)
        {
            if (productId == null)
            {
                return NotFound();
            }
            if (quantity < 1)
            {
                return Ok(new { success = false, Message = $" Input quantity >= 1" });
            }
            ProductModel product = await _context.Products.Include(p => p.Sales)
                .FirstOrDefaultAsync(m => m.Id == productId && m.Status != 0);
            if (product == null)
            {
                return NotFound();
            }
            if (product.Stock == 0)
            {
                return Ok(new { success = false, Message = $" Item {product.ProductName} is out of stock" });
            }
            if (product.Stock < quantity)
            {
                return Ok(new { success = false, Message = $" Item {product.ProductName} only has  {product.Stock} quantities left" });
            }
            var now = DateTime.Now;
            var activeSale = product.Sales?.FirstOrDefault(s =>s.Status != 0 && s.SaleStartDate <= now &&  s.SaleEndDate >= now);
            decimal finalPrice = activeSale != null ? activeSale.SalePrice : product.Price;

            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY) ?? new List<CartItemModel>();
            CartItemModel cartItem = cart.Where(c => c.ProductId == productId).FirstOrDefault();
            int currentInCart = cartItem?.Quantity ?? 0;
            if (product.Stock < (currentInCart + quantity))
            {
                return Ok(new { success = false, Message = $"Sản phẩm {product.ProductName} chỉ còn {product.Stock} sản phẩm." });
            }

            if (cartItem == null)
            {
                cart.Add(new CartItemModel(product, quantity, finalPrice));
            }
            else
            {
                cartItem.Quantity += quantity;
                cartItem.Price = finalPrice; 
            }
            HttpContext.Session.SetJson(DShopConst.CART_KEY, cart);
            await UpdateCouponValuesInSession();
            return Ok(new { success = true, Message = $" Add Item {product.ProductName} to cart successfully" });
        }

        public ActionResult Clear()
        {
            HttpContext.Session.Remove(DShopConst.CART_KEY);
            return RedirectToAction("Index");
        }

        private async Task UpdateCouponValuesInSession()
        {
            var coupons = HttpContext.Session.GetJson<List<CouponItem>>(DShopConst.COUPONS_CUSTOMER_APPPLY);
            if (coupons == null || !coupons.Any()) return;

            var cartItems = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY) ?? new List<CartItemModel>();
            decimal subtotal = cartItems.Sum(c => c.Quantity * c.Price);

            foreach (var coupon in coupons)
            {
                if (coupon.Promotion == DShopConst.PERCENTAGE_DISCOUNT)
                {
                    var dbCoupon = await _context.Coupons.Where(c => c.CouponCode == coupon.CouponCode).FirstOrDefaultAsync();
                    var val = subtotal * dbCoupon.Value  / 100000;
                    coupon.Value = Math.Ceiling(val) * 1000;
                }
            }
            HttpContext.Session.SetJson(DShopConst.COUPONS_CUSTOMER_APPPLY, coupons);
        }

        private void CheckMinimumAmount()
        {
            var coupons = HttpContext.Session.GetJson<List<CouponItem>>(DShopConst.COUPONS_CUSTOMER_APPPLY);
            if (coupons == null || !coupons.Any()) return;
            List<string> removedCodes = new List<string>();
            var cartItems = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY) ?? new List<CartItemModel>();
            decimal subtotal = cartItems.Sum(c => c.Quantity * c.Price);

            coupons.RemoveAll(coupon =>
            {
                bool UnCondition = subtotal < coupon.MinimumAmount;
                if (UnCondition)
                {
                    removedCodes.Add(coupon.CouponCode);
                }
                return UnCondition;
            });

            HttpContext.Session.SetJson(DShopConst.COUPONS_CUSTOMER_APPPLY, coupons);
            if (removedCodes.Count == 0)
                return;
            TempData[DShopConst.TEMPDATA_ERROR] = $"Coupon {string.Join(", ", removedCodes)} were removed because the minimum order value was not met.";
        }

        public async Task<ActionResult> Increase(int? Id)
        {
            if (Id == null) return NotFound();

            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (product == null) return NotFound();

            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY) ?? new List<CartItemModel>();
            CartItemModel cartItem = cart.FirstOrDefault(c => c.ProductId == Id);

            if (product.Stock <= cartItem.Quantity)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = $"Product '{product.ProductName}' only has {product.Stock} items left in stock.";
                return RedirectToAction("Index");
            }

            cartItem.Quantity++;
            HttpContext.Session.SetJson(DShopConst.CART_KEY, cart);

            await UpdateCouponValuesInSession();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Decrease(int? Id)
        {
            if (Id == null) return NotFound();

            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY) ?? new List<CartItemModel>();
            CartItemModel cartItem = cart.FirstOrDefault(c => c.ProductId == Id);

            if (cartItem != null)
            {
                if (cartItem.Quantity > 1) cartItem.Quantity--;
                else cart.Remove(cartItem);
            }

            if (!cart.Any()) HttpContext.Session.Remove(DShopConst.CART_KEY);
            else HttpContext.Session.SetJson(DShopConst.CART_KEY, cart);

            await UpdateCouponValuesInSession();
            CheckMinimumAmount();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Remove(int? Id)
        {
            if (Id == null) return NotFound();

            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY) ?? new List<CartItemModel>();
            cart.RemoveAll(p => p.ProductId == Id);

            if (!cart.Any()) HttpContext.Session.Remove(DShopConst.CART_KEY);
            else HttpContext.Session.SetJson(DShopConst.CART_KEY, cart);

            await UpdateCouponValuesInSession();
            CheckMinimumAmount();
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
        public async Task<ActionResult> GetCoupon(string couponCode)
        {
            InformationDelivery info = HttpContext.Session.GetJson<InformationDelivery>(DShopConst.INFO_CUSTOMER_DELIVERY);
            if (info == null)
            {
                return Ok(new { success = false, message = "Please provide delivery information and calculate shipping costs first." });
            }

            if (string.IsNullOrEmpty(couponCode))
                return Ok(new { success = false, message = "Please enter a coupon code." });

            var validCoupon = await _context.Coupons
                .Include(p => p.Promotion)
                .FirstOrDefaultAsync(x => x.CouponCode == couponCode);

            if (validCoupon == null )
                return Ok(new { success = false, message = "Coupon code does not exist." });

            if (validCoupon.Status == 0)
                return Ok(new { success = false, message = "Coupon has been removed." });

            if (validCoupon.Quantity <= 0)
                return Ok(new { success = false, message = "Coupon is out of usage limit." });

            if (validCoupon.DateStart.Date > DateTime.Today)
                return Ok(new { success = false, message = "The coupon is not active yet." });

            if (validCoupon.DateExpired.Date < DateTime.Today)
                return Ok(new { success = false, message = "The coupon has expired." });

            if (!CheckConditionAmuont(validCoupon.MinimumAmount))
                return Ok(new { success = false, message = "The coupon conditions are not met." });


            var user = await _userManager.GetUserAsync(this.User);
            var checkUsed = await _context.CouponRedemptions.FirstOrDefaultAsync(u => u.UserId == user.Id && u.CouponId == validCoupon.Id);

            if (checkUsed?.Status == 0)
                return Ok(new { success = false, message = "You had already used this code. You were allowed to use each code only once." });

            var currentCoupons = HttpContext.Session.GetJson<List<CouponItem>>(DShopConst.COUPONS_CUSTOMER_APPPLY) ?? new List<CouponItem>();

            if (currentCoupons.Any(c => c.CouponCode == couponCode))
                return Ok(new { success = false, message = "You have already applied this coupon." });


            if (validCoupon.Promotion.CategoryCouponName.Equals(DShopConst.SUB_SUMTOTAL_DISCOUNT))
            {
                validCoupon.Value = validCoupon.Value;
            }
            if (validCoupon.Promotion.CategoryCouponName.Equals(DShopConst.PERCENTAGE_DISCOUNT))
            {
                List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY) ?? new List<CartItemModel>();
                decimal subtotal = cartItems.Sum(c => c.Quantity * c.Price);
                var val = subtotal * validCoupon.Value / 100000;
                validCoupon.Value = Math.Ceiling(val) * 1000;
            }

            if (validCoupon.Promotion.CategoryCouponName.Equals(DShopConst.FREE_SHIPPING))
            {
                var checkFreeShipping = currentCoupons.Any(c => c.Promotion == DShopConst.FREE_SHIPPING || c.Promotion == DShopConst.NEW_CUSTOMER);
                if (checkFreeShipping)
                {
                    return Ok(new { success = false, message = "You already have free shipping applied. Please save this code for your next purchase." });
                }
                info.ShippingCost = 0;
                validCoupon.Value = 0;
                HttpContext.Session.SetJson(DShopConst.INFO_CUSTOMER_DELIVERY, info);
            }

            if (validCoupon.Promotion.CategoryCouponName.Equals(DShopConst.NEW_CUSTOMER))
            {
                var codeCustomer = validCoupon.CouponCode.Split('_')[1];
                if (!user.UserName.ToUpper().Equals(codeCustomer))
                {
                    return Ok(new { success = false, message = "This coupon does not belong to your account." });
                }
                info.ShippingCost = 0;
                validCoupon.Value = validCoupon.Value;
                HttpContext.Session.SetJson(DShopConst.INFO_CUSTOMER_DELIVERY, info);
            }
            CouponItem couponDto = _mapper.Map<CouponItem>(validCoupon);
            currentCoupons.Add(couponDto);
            HttpContext.Session.SetJson(DShopConst.COUPONS_CUSTOMER_APPPLY, currentCoupons);
            return Ok(new { success = true, message = "Coupon applied successfully." });
        }


        [HttpPost]
        public async Task<ActionResult> UseMyInformation(bool checkInfo)
        {
            if (!checkInfo)
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

        private bool CheckConditionAmuont(decimal minimumAmount)
        {
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY) ?? new List<CartItemModel>();
            if (cartItems.Count == 0) return false;
            decimal sumPirceItemsCart = cartItems.Sum(s => s.Quantity * s.Price);
            if (sumPirceItemsCart >= minimumAmount)
                return true;
            return false;
        }
    }
}
