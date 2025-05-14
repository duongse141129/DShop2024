using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DShop2024.Models
{
	[Table("OrderCoupons")]
	public class OrderCouponsModel
	{
		[Key]
		public int Id { get; set; }
		public int OrderId { get; set; }
		public int CouponId { get; set; }

		public int Status { get; set; }

		[ForeignKey("OrderId")]
		public virtual OrderModel Order { get; set; }

		[ForeignKey("CouponId")]
		public virtual CouponModel Coupon { get; set; }
	}
}
