using DShop2024.Services.Recommend;
using Microsoft.AspNetCore.Mvc;

namespace DShop2024.Repository.Components
{
    public class RecommendProductViewComponent : ViewComponent
    {
        private readonly IRecommendationService _rec;

        public RecommendProductViewComponent(IRecommendationService rec)
        {
            _rec = rec;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var recomnededProducts = await _rec.GetRecommendedProductsAsync();
            return View(recomnededProducts);

        }
    }
}
