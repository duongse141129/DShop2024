using DShop2024.Models;

namespace DShop2024.ViewModels
{
	public class ProductDetailViewModel 
	{
		public ProductModel ProductDetail { get; set; }

		public double Point { get; set; } = 0;

		public List<RatingModel> listRating { get; set; } = new List<RatingModel>();

		public bool IsOrder {  get; set; } = false;
		public bool IsFeedback { get; set; } = false;

		public RatingModel Feedback { get; set; } = new RatingModel();

		public bool IsInWishlist { get; set; } = false ;
		public bool IsInCompare { get; set; } = false;

        public List<string> ExistingImages { get; set; } = new();
    }
}
 