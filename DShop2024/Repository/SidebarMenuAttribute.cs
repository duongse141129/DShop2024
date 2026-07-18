using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DShop2024.Repository 
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method,AllowMultiple = false, Inherited = true)]
    public class SidebarMenuAttribute : ActionFilterAttribute
    {
        private readonly Enum _menu;
        private readonly Enum? _subMenu;

        public SidebarMenuAttribute(object menu, object? subMenu = null)
        {
            if (menu is not Enum menuEnum)
                throw new ArgumentException("menu must be an Enum");

            _menu = menuEnum;

            if (subMenu != null)
            {
                if (subMenu is not Enum subMenuEnum)
                    throw new ArgumentException("subMenu must be an Enum");

                _subMenu = subMenuEnum;
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.Controller is Controller controller)
            {
                controller.ViewBag.sidebar = _menu;
                controller.ViewBag.subMenu = _subMenu;
            }

            base.OnActionExecuting(context);
        }
    }

}

