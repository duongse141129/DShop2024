using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
	[Table("CouponRedemption")]
	public class CouponRedemptionModel
	{
		[Key]
		public int Id { get; set; }

		public string UserId { get; set; }
		public int CouponId { get; set; }

		public int Status { get; set; }


		[ForeignKey("CouponId")]
		public virtual CouponModel Coupon { get; set; }

		[ForeignKey("UserId")]
		public virtual AppUserModel User { get; set; }
	}
}
