using DShop2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;




namespace DShop2024.Repository.Components
{
    public class RecentlyViewedProductViewComponent : ViewComponent
    {

        public RecentlyViewedProductViewComponent()
        {
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var rvproduct = Request.Cookies["RecentlyViewedProducts"];
            List<ProductViewModel> recentlyViewedProducts;
            if (rvproduct == null)
            {
                recentlyViewedProducts = new List<ProductViewModel>();
            }
            else
            {
                recentlyViewedProducts = JsonConvert.DeserializeObject<List<ProductViewModel>>(rvproduct);
            }
            return View(recentlyViewedProducts.OrderByDescending(p => p.ViewAt).Take(8));

        }
    }
}
