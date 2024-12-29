using DShop2024.Models;
using DShop2024.Repository;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DShop2024.Controllers
{
	public class CartController : Controller
	{
		private readonly DShopContext _dataContext;

		public CartController(DShopContext context)
		{
			_dataContext = context;
		}
		public IActionResult Index()
		{
			List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

			var shippingPriceCookie = Request.Cookies["shipppingPrice"];
			decimal shippingPrice = 0;

			var couponCode = Request.Cookies["CouponTitle"];

			if(shippingPriceCookie != null)
			{
				var shippingPriceJson = shippingPriceCookie;
				shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
			}
			
			
			CartItemViewModel cartItemViewModel = new CartItemViewModel { 
				CartItems = cartItems,
				GrandTotal = cartItems.Sum( s => s.Quantity* s.Price),
				ShippingCost = shippingPrice,
                CouponCode = couponCode
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

			//TempData["success"] = $" Add Item {product.ProductName} to cart successfully";
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


		public async Task<ActionResult> GetShipping(ShippingModel shippingModel, string tinh, string quan, string phuong)
		{
			// default 50k
			decimal shipppingPrice = 50000;

			var existingShipping = await _dataContext.Shippings
										.FirstOrDefaultAsync(x => x.City == tinh && 
															x.District == quan 
															&& x.Ward == phuong);

			if(existingShipping != null)
			{
				shipppingPrice = existingShipping.Price;
			}

			var shippingPriceJson = JsonConvert.SerializeObject(shipppingPrice);
			try
			{
				var cookieOptionss = new CookieOptions
				{
					HttpOnly = true,
					Expires = DateTime.UtcNow.AddMinutes(30),
					Secure = true
				};
				Response.Cookies.Append("shipppingPrice", shippingPriceJson, cookieOptionss);
			}
			catch (Exception ex)
			{

				return Json(new { ex .Message});
			}
			return Json(new { shipppingPrice });
		}

        public ActionResult DeleteShipping()
        {
			Response.Cookies.Delete("shipppingPrice");
            return RedirectToAction("Index");
        }

		[HttpPost]
		public async Task<ActionResult> GetCoupon(CouponModel couponModel, string couponValue)
		{
			var validCoupon = await _dataContext.Coupons
									.FirstOrDefaultAsync(x => x.CouponName == couponValue && x.Quantity >=1);
			string couponTitle = validCoupon.CouponName + " | " + validCoupon?.Description;
			
			if(validCoupon != null)
			{
				TimeSpan remainingTime = validCoupon.DateExpired - DateTime.Now;
				int daysRemaining = remainingTime.Days;
				if(daysRemaining <= 0)
				{
					try
					{
						var cookieOptions = new CookieOptions
						{
							HttpOnly = true,
							Expires= DateTime.UtcNow.AddMinutes(30),
							Secure = true,
							SameSite = SameSiteMode.Strict, // kiểm tra tương thích trình duyệt
						};

						Response.Cookies.Append("CouponTitle", couponTitle, cookieOptions);
						return Ok( new {success = true, message = "Apply coupon successfully"});
					}
					catch (Exception ex)
					{

						return Ok(new { success = false, message = "Apply coupon fail: "+ ex.Message });
					}
				}
				
				else
				{
					return Ok(new { success = false, message = "Coupon has expried" });
				}
			}

			return Ok(new { success = false, message = "Coupon hasn't existed" });

		}




	}
}
