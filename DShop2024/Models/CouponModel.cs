using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
	[Table("Coupon")]
	public class CouponModel
	{
		[Key]
		public int Id { get; set; }
		[Required, MaxLength(100, ErrorMessage = "Coupon code can not null")]
		public string CouponCode { get; set; }

		[Required( ErrorMessage = "Coupon name can not null")]
		public string CouponName { get; set; }
		[Required(ErrorMessage = "Value can not null ")]
		[Range(0, int.MaxValue, ErrorMessage = "Value > {1}")]
		public decimal Value { get; set; }
		public string? Description { get; set; }
		public DateTime DateStart { get; set; }
		public DateTime DateExpired{ get; set; }
		public int Quantity { get; set; }
		public int Status { get; set; }

		public int PromotionId { get; set; }

		[ForeignKey("PromotionId")]
		public virtual PromotionModel Promotion { get; set; }



	}
}
