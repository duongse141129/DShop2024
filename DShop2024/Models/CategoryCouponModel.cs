using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
	[Table("CategoryCoupon")]
	public class CategoryCouponModel
	{
		[Key]
		public int Id { get; set; }
		[Required(ErrorMessage = "Category coupon name can not null")]
		public string CategoryCouponName { get; set; }

		public int Status { get; set; }
	}
}
