using DShop2024.ViewModels;

namespace DShop2024.Services.Recommend
{
    public interface IRecommendationService
    {
        Task<IEnumerable<ProductViewModel>> RecommendForRecentlyViewedAsync(int topN = 10, double popularityWeight = 1.3);
        void AddRecentlyViewedProductAsync(int backpackId);
        Task<List<ProductViewModel>> GetRecentlyViewedProductsAsync();
    }
}
