using DShop2024.Models;

namespace DShop2024.ViewModels
{
	public class CartItemViewModel
	{
		public List<CartItemModel> CartItems { get; set; }
		public decimal GrandTotal { get; set; }

		public CouponModel CouponApply { get; set; }
		public InformationDelivery InfoDelivery { get; set; } 
	}
}
