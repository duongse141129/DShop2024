using DShop2024.EnumData;
using DShop2024.Models;
using Microsoft.AspNetCore.Mvc;

namespace DShop2024.Repository.Components
{
    public class CartViewComponent : ViewComponent
    {

        public CartViewComponent()
        {
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>(DShopConst.CART_KEY) ?? new List<CartItemModel>();
            var sumQuantity = cartItems.Sum(x => x.Quantity);
            return View(sumQuantity);

        }
    }
}
