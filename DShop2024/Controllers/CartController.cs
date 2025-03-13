using DShop2024.Models;
using DShop2024.Repository;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


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

			InformationDelivery info = HttpContext.Session.GetJson<InformationDelivery>("InfoCustomerDelivery") ?? new InformationDelivery();

			CouponModel coupoun = HttpContext.Session.GetJson<CouponModel>("CouponCustomerApply") ?? new CouponModel();

			decimal shippingPrice = 0;
			
			if (info.ShippingCost != 0)
			{
				shippingPrice = info.ShippingCost;
			}
			//var couponCode = Request.Cookies["CouponTitle"];

			CartItemViewModel cartItemViewModel = new CartItemViewModel { 
				CartItems = cartItems,
				GrandTotal = cartItems.Sum( s => s.Quantity* s.Price),
				CouponApply = coupoun,
				InfoDelivery= info
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
									.FirstOrDefaultAsync(x => x.CouponCode == couponValue && x.Quantity >=1);
			
			
			if(validCoupon != null)
			{
				TimeSpan remainingTime = validCoupon.DateExpired - DateTime.Now;
				int daysRemaining = remainingTime.Days;
				if(daysRemaining >= 0)
				{

					HttpContext.Session.SetJson("CouponCustomerApply", validCoupon);
					return Ok(new { success = true, message = "Apply coupon successfully" });
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
