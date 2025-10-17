using DShop2024.Models;

namespace DShop2024.ViewModels
{
	public class ProductDetailViewModel
	{
		public ProductModel ProductDetail { get; set; }

		public double Point { get; set; }

		public List<RatingModel> listRating { get; set; }

		public bool IsOrder {  get; set; }	
		public bool IsFeedback{  get; set; }	

		public RatingModel Feedback { get; set; }

		public bool IsInWishlist { get; set; }
		public bool IsInCompare { get; set; }

        public List<string> ExistingImages { get; set; } = new();
    }
}
 