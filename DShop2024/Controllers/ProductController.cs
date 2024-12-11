using Microsoft.AspNetCore.Mvc;

namespace DShop2024.Controllers
{
	public class ProductController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Details()
		{
			return View();
		}
	}
}
