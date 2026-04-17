using Microsoft.AspNetCore.Mvc;

namespace DShop2024.Controllers
{
	public class AjaxContentController : Controller
	{
		public IActionResult QuantityCart()
		{
			return ViewComponent("Cart");
		}
        public IActionResult CountContact()
        {
            return ViewComponent("Contact");
        }
        public IActionResult ReloadNotificationMessage()
        {
            return ViewComponent("NotificationMessage");
        }
        public IActionResult RecieveNotificationMessage()
        {
            return ViewComponent("NotifyReceiveMessage");
        }

    }
}
