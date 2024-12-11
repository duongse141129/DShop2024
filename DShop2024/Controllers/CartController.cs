using Microsoft.AspNetCore.Mvc;

namespace DShop2024.Controllers
{
	public class CartController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult CheckOut()
		{
			return View("~/Views/CheckOut/Index.cshtml");
		}
	}
}
