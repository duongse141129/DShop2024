using DShop2024.Services.Recommend;
using Microsoft.AspNetCore.Mvc;

namespace DShop2024.Repository.Components
{
    public class RecentlyViewedProductViewComponent : ViewComponent
    {
        private readonly IRecommendationService _rec;

        public RecentlyViewedProductViewComponent(IRecommendationService rec)
        {
            _rec = rec;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var recentlyViewedIdProducts = await _rec.GetRecentlyViewedProductsAsync();
            return View(recentlyViewedIdProducts.Take(8));
        }
    }
}
