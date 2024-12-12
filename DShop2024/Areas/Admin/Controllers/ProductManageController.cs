using Microsoft.AspNetCore.Mvc;

namespace DShop2024.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class ProductManageController : Controller
	{
		private readonly DShopContext _dataContext;

		public ProductManageController(DShopContext context)
		{
			_dataContext = context;
		}
		public IActionResult Index()
		{
			return View();
		}
	}
}
