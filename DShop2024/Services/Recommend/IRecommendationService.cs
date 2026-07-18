using DShop2024.ViewModels;

namespace DShop2024.Services.Recommend
{
    public interface IRecommendationService
    {
        Task<List<ProductViewModel>> GetRecommendedProductsAsync(int topN = 10);
        void AddRecentlyViewedProductAsync(int backpackId);
        Task<List<ProductViewModel>> GetRecentlyViewedProductsAsync();
    }
}
