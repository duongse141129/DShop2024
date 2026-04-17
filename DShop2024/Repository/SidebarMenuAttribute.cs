using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DShop2024.Repository
{
    public class SidebarMenuAttribute : ActionFilterAttribute
    {
        private readonly Enum _menu;
        public SidebarMenuAttribute(object menu)
        {
            if (menu is Enum enumValue)
            {
                _menu = enumValue;
            }
            else
            {
                throw new ArgumentException("Value must be an Enum");
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.Controller is Controller controller)
            {
                controller.ViewBag.sidebar = _menu;
            }
        }
    }
}
