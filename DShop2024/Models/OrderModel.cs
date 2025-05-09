using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
	[Table("Order")]
	public class OrderModel
	{
		public int Id { get; set; }
		public string OrderCode { get; set; }
		public string UserId { get; set; }
		public DateTime CreatedDate { get; set; }
		public string PaymentMethod { get; set; }
		public int Status { get; set; }
		public decimal TotalPrice { get; set; }

		[Required, MaxLength(300, ErrorMessage = "Address Delivery is required ")]
		public string AddressDelivery { get; set; }
		[Required, MaxLength(100, ErrorMessage = "Phone Delivery is required ")]
		public string PhoneDelivery { get; set; }
		[Required, MaxLength(100, ErrorMessage = "Consignee is required ")]
		public string Consignee { get; set; }

		public decimal ShippingCost { get; set; }
		public decimal ValueCoupon { get; set; }

		[ForeignKey("UserId")]
		public virtual AppUserModel User { get; set; }

		public virtual ICollection<OrderDetailModel> OrderDetails { get; set; }
		public virtual ICollection<OrderCouponsModel> OrderCoupons { get; set; }
	}
}
