using DShop2024.Models;

namespace DShop2024.ViewModels
{
	public class CartItemViewModel  
	{
		public List<CartItemModel> CartItems { get; set; }
		public decimal SumPriceItemsCart { get; set; }
		public decimal SumCouponValue { get; set; }
		public decimal GrandTotal { get; set; }

		public List<CouponModel> CouponsApply { get; set; }
		public InformationDelivery InfoDelivery { get; set; }

        public List<PaymentModel> Payments { get; set; }
    }
}
