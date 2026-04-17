using DShop2024.Models;

namespace DShop2024.ViewModels
{
    public class ProductDetailManageViewModel
    {
        public ProductModel ProductDetail { get; set; }

        public double Point { get; set; } = 0;

        public List<RatingViewModel> listRating { get; set; } = new List<RatingViewModel>();

        public bool IsOrder { get; set; } = false;
        public bool IsFeedback { get; set; } = false;

        public RatingModel Feedback { get; set; } = new RatingModel();

        public bool IsInWishlist { get; set; } = false;
        public bool IsInCompare { get; set; } = false;

        public List<string> ExistingImages { get; set; } = new();
    }
}
