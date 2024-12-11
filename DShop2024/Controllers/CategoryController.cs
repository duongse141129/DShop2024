using Microsoft.AspNetCore.Mvc;

namespace DShop2024.Controllers
{
	public class CategoryController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
